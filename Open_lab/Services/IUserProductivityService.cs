using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Open_lab.Services
{
    public class UserPerformanceRow
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int CompletedTestsCount { get; set; }
    }

    public interface IUserProductivityService
    {
        Task<List<UserPerformanceRow>> GetUserPerformanceAsync(DateTime from, DateTime to);
    }
}
