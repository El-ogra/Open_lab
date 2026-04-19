using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    public interface ISampleTrackingService
    {
        Task<SampleCollection?> GetSampleStatusAsync(int visitTestId);
        Task UpdateSeparationStatusAsync(int visitTestId, bool isSeparated);
        Task<List<SampleCollection>> GetPendingTrackingSamplesAsync();
    }
}
