using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hangfire.Services
{
   public interface IEmployeeService
    {
        Task<bool> InsertEmployeeAsync();
    }
}
