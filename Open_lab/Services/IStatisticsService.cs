using System;
using System.Threading.Tasks;

namespace Open_lab.Services
{
    public sealed class StatisticsSummary
    {
        public int VisitCount { get; init; }
        public int PatientCount { get; init; }
        public int TestCount { get; init; }
        public decimal TotalRevenue { get; init; }
        public decimal TotalPaid { get; init; }
    }

    public interface IStatisticsService
    {
        Task<StatisticsSummary> GetSummaryAsync(DateTime from, DateTime to);
    }
}
