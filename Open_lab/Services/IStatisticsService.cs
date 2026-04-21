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

    public sealed class MonthlyAnalysisRow
    {
        public int Month { get; init; }
        public string MonthName { get; init; } = string.Empty;
        public int VisitCount { get; init; }
        public int TestCount { get; init; }
        public decimal Revenue { get; init; }
    }

    public sealed class TopTestRow
    {
        public string TestName { get; init; } = string.Empty;
        public int DemandCount { get; init; }
        public decimal TotalRevenue { get; init; }
    }

    public sealed class YearlySampleRow
    {
        public int Year { get; init; }
        public int SamplesCount { get; init; }
    }

    public interface IStatisticsService
    {
        Task<List<StatisticsReferralLookup>> GetReferralsAsync();
        Task<StatisticsSnapshot> GetSnapshotAsync(DateTime from, DateTime to, string? gender, int? referralId);
        Task<List<MonthlyAnalysisRow>> GetMonthlyAnalysisAsync(int year);
        Task<List<TopTestRow>> GetTop10TestsAsync(DateTime from, DateTime to);
        Task<List<YearlySampleRow>> GetSampleCountPerYearAsync(int yearsBack = 5);
    }
}
