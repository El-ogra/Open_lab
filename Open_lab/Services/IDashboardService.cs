using System.Threading.Tasks;

namespace Open_lab.Services
{
    public interface IDashboardService
    {
        Task<(int PatientCount, int VisitCount, int TestCount)> GetCountsAsync();
    }
}
