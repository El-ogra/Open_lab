using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Open_lab.Services
{
    public class ReagentConsumptionReport
    {
        public string ReagentName { get; set; } = string.Empty;
        public decimal TotalConsumed { get; set; }
        public string Unit { get; set; } = string.Empty;
        public int TestCount { get; set; }
    }

    public interface ITestClassificationService
    {
        Task<List<ReagentConsumptionReport>> GetConsumptionReportAsync(DateTime from, DateTime to);
    }
}
