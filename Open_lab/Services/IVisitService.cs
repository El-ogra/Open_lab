using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    public interface IVisitService
    {
        Task<Visit?> GetByIdAsync(int visitId);
        Task<List<Visit>> GetByPatientIdAsync(int patientId);
        Task<List<VisitTest>> GetVisitTestsAsync(int visitId);
        Task<Visit> CreateAsync(Visit visit);
        Task UpdateAsync(Visit visit);
        Task DeleteAsync(int visitId);
        Task<VisitTest> AddTestToVisitAsync(int visitId, int testId, decimal? overridePrice = null);
        Task<List<VisitTest>> AddCustomGroupToVisitAsync(int visitId, int customGroupId);
        Task RemoveVisitTestAsync(int visitTestId);
    }
}
