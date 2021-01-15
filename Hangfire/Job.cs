using Hangfire.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hangfire
{
    public class Job
    {
        #region Property
        private readonly IEmployeeService _employeeService;
        #endregion

        #region Constructor
        public Job(IEmployeeService employeeService)
        {
            _employeeService = employeeService; 
        }
        #endregion

        #region Job Scheduler
        public async Task<bool> JobAsync()
        {
            var result = await _employeeService.InsertEmployeeAsync();
            return true;
        }
        #endregion
    }
}
