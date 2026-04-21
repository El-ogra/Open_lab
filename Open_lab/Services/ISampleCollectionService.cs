using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.ViewModels;

namespace Open_lab.Services
{
    public interface ISampleCollectionService
    {
        Task<List<SampleCollectionRow>> GetRowsAsync(DateTime from, DateTime to);
        Task MarkCollectedAsync(int visitTestId, int userId, bool isExternal = false, int? receivedBy = null);
        Task MarkSeparatedAsync(int visitTestId, string? separationType = null);
        Task MarkNotCollectedAsync(int visitTestId);
    }
}
