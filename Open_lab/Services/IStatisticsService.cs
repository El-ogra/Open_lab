using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.ViewModels;

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

    public sealed class StatisticsReferralLookup
    {
        public int? ReferralId { get; init; }
        public string Name { get; init; } = string.Empty;
    }

    public sealed class StatisticsSnapshot
    {
        public StatisticsSummary Summary { get; init; } = new();
        public List<StatisticsGenderRow> ByGender { get; init; } = new();
        public List<StatisticsReferralRow> ByReferral { get; init; } = new();
    }

    public interface IStatisticsService
    {
        Task<List<StatisticsReferralLookup>> GetReferralsAsync();
        Task<StatisticsSnapshot> GetSnapshotAsync(DateTime from, DateTime to, string? gender, int? referralId);
    }
}
