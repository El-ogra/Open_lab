using System.Collections.Generic;
using System.Threading.Tasks;
using Open_lab.Models;

namespace Open_lab.Services
{
    /// <summary>
    /// Gap 4.8 — Print Blank Report Architecture Fix.
    /// Dedicated service responsible for building and printing a "blank" report
    /// (patient/visit demographics + the list of requested tests without results).
    /// The ViewModel must depend on this service instead of building the report
    /// content itself and calling PrintService directly.
    /// </summary>
    public interface IBlankReportService
    {
        /// <summary>
        /// Builds the textual lines that compose the blank report for a visit.
        /// Returns null when the visit cannot be found.
        /// </summary>
        Task<IReadOnlyList<string>?> BuildBlankReportAsync(int visitId);

        /// <summary>
        /// Builds the blank report and dispatches it to the underlying print service.
        /// Returns false when the visit cannot be found or printing is unavailable.
        /// </summary>
        Task<bool> PrintBlankReportAsync(int visitId);
    }
}
