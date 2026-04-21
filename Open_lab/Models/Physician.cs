using System.Collections.Generic;

namespace Open_lab.Models
{
    /// <summary>
    /// Represents a physician/doctor who refers patients to the lab.
    /// Used for price resolution priority and commission tracking.
    /// </summary>
    public class Physician
    {
        public int PhysicianId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Specialty { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Optional price list assigned to this physician for priority pricing.
        /// </summary>
        public int? PriceListId { get; set; }

        /// <summary>
        /// Optional commission percentage for this physician.
        /// </summary>
        public decimal? CommissionPercentage { get; set; }

        public PriceList? PriceList { get; set; }
        public ICollection<Visit> Visits { get; set; } = new HashSet<Visit>();
    }
}
