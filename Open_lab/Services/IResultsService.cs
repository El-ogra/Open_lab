using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    public interface IResultsService
    {
        Task<List<VisitTest>> GetVisitTestsByDateAsync(DateTime from, DateTime to);
        Task<VisitTest?> GetVisitTestByIdAsync(int visitTestId);
        Task<List<ResultValue>> GetResultsForVisitTestAsync(int visitTestId);
        Task<List<TestParameter>> GetParametersForTestAsync(int testId);
        Task SaveResultAsync(int visitTestId, int parameterId, string? value, string? flag, string? comment);
        Task VerifyVisitTestAsync(int visitTestId, int verifiedByUserId);
        Task ReopenVisitTestAsync(int visitTestId);
    }
}
