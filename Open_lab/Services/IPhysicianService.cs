using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    /// <summary>
    /// Service for managing physicians (doctors).
    /// </summary>
    public interface IPhysicianService
    {
        /// <summary>
        /// Gets all physicians.
        /// </summary>
        Task<List<Physician>> GetAllAsync();

        /// <summary>
        /// Gets all active physicians.
        /// </summary>
        Task<List<Physician>> GetActiveAsync();

        /// <summary>
        /// Gets a physician by ID.
        /// </summary>
        Task<Physician?> GetByIdAsync(int physicianId);

        /// <summary>
        /// Creates a new physician.
        /// </summary>
        Task<Physician> CreateAsync(Physician physician);

        /// <summary>
        /// Updates an existing physician.
        /// </summary>
        Task<Physician> UpdateAsync(Physician physician);

        /// <summary>
        /// Deletes a physician.
        /// </summary>
        Task DeleteAsync(int physicianId);

        /// <summary>
        /// Searches physicians by name or specialty.
        /// </summary>
        Task<List<Physician>> SearchAsync(string searchTerm);
    }
}
