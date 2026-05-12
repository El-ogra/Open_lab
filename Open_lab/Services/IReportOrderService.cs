using System.Collections.Generic;
using System.Threading.Tasks;

namespace Open_lab.Services
{
    /// <summary>
    /// Gap 4.5 — Arrange Report Order Persistence.
    /// Persists the order in which VisitTests appear in a Visit's printed report.
    /// </summary>
    public interface IReportOrderService
    {
        /// <summary>
        /// Saves the report ordering for a given visit. The list contains VisitTestIds
        /// in the desired display order.
        /// </summary>
        Task SaveReportOrderAsync(int visitId, IReadOnlyList<int> orderedVisitTestIds);

        /// <summary>
        /// Returns the persisted ordered VisitTestIds for a visit, or an empty list when
        /// no custom order has been saved yet.
        /// </summary>
        Task<IReadOnlyList<int>> GetReportOrderAsync(int visitId);
    }
}
