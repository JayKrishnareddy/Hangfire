using Hangfire.AppDbContext;
using Hangfire.Services;
using HangfireBasicAuthenticationFilter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hangfire
{
    public class Startup
    {
        private static IEmployeeService employeeService;
        private readonly Job jobscheduler = new Job(employeeService);
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hangfire", Version = "v1" });
            });

            #region Configure Connection String
            services.AddDbContext<EmployeeDbContext>(item => item.UseSqlServer(Configuration.GetConnectionString("myconn")));
            #endregion

            #region Configure Hangfire
            services.AddHangfire(c => c.UseSqlServerStorage(Configuration.GetConnectionString("myconn")));
            GlobalConfiguration.Configuration.UseSqlServerStorage(Configuration.GetConnectionString("myconn")).WithJobExpirationTimeout(TimeSpan.FromDays(7));
            #endregion

            #region Services Injection
            services.AddTransient<IEmployeeService, EmployeeService>();
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,IBackgroundJobClient backgroundJobClient, IRecurringJobManager recurringJobManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hangfire v1"));
            }

            #region Configure Hangfire
            app.UseHangfireServer();

            //Basic Authentication added to access the Hangfire Dashboard
            app.UseHangfireDashboard("/hangfire", new DashboardOptions()
            {
                AppPath = null,
                DashboardTitle = "Hangfire Dashboard",
                Authorization = new[]{
                new HangfireCustomBasicAuthenticationFilter{
                    User = Configuration.GetSection("HangfireCredentials:UserName").Value,
                    Pass = Configuration.GetSection("HangfireCredentials:Password").Value
                }
            },
                //Authorization = new[] { new DashboardNoAuthorizationFilter() },
                //IgnoreAntiforgeryToken = true
            }); ;
            #endregion

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            #region Job Scheduling Tasks
            //recurringJobManager.AddOrUpdate("Insert Employee : Runs Every 1 Min", () => jobscheduler.JobAsync(), "*/1 * * * *");
            #endregion
        }
    }
}
