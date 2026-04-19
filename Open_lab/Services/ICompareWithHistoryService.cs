using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Open_lab.Services
{
    public class HistoricalResult
    {
        public DateTime VisitDate { get; set; }
        public int VisitId { get; set; }
        public string ParameterName { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string? Flag { get; set; }
    }

    public interface ICompareWithHistoryService
    {
        Task<List<HistoricalResult>> GetLastResultsAsync(int patientId, int testId, int count = 3);
    }
}
