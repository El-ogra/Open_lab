using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    public interface IVisitService
    {
        Task<Visit?> GetByIdAsync(int visitId);
        Task<List<Visit>> GetByPatientIdAsync(int patientId);
        Task<Visit> CreateAsync(Visit visit);
        Task UpdateAsync(Visit visit);
        Task DeleteAsync(int visitId);
        Task<VisitTest> AddTestToVisitAsync(int visitId, int testId, decimal? overridePrice = null);
        Task RemoveVisitTestAsync(int visitTestId);
    }
}
