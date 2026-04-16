using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    public class ReceiptData
    {
        public Visit Visit { get; set; } = null!;
        public Patient Patient { get; set; } = null!;
        public System.Collections.Generic.List<VisitTest> VisitTests { get; set; } = new();
        public Invoice? Invoice { get; set; }
    }

    public interface IReceiptService
    {
        Task<ReceiptData?> GetReceiptDataAsync(int visitId);
    }
}
