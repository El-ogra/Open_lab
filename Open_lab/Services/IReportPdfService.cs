using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    public interface IReportPdfService
    {
        Task<string> GenerateVisitReportPdfAsync(VisitReportData report);
    }
}
