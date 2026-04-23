using System;
using System.Collections.Generic;

namespace Open_lab.Models
{
    public class VisitReportData
    {
        public Visit Visit { get; set; } = null!;
        public Patient Patient { get; set; } = null!;
        public List<VisitTestReportItem> Tests { get; set; } = new();
        public Invoice? Invoice { get; set; }
    }

    public class VisitTestReportItem
    {
        public VisitTest VisitTest { get; set; } = null!;
        public Test Test { get; set; } = null!;
        public List<ResultValueReportItem> Results { get; set; } = new();
    }

    public class ResultValueReportItem
    {
        public ResultValue Result { get; set; } = null!;
        public string? PreviousValue { get; set; }
        public DateTime? PreviousDate { get; set; }
    }

    public class PatientHistoryReportData
    {
        public Patient Patient { get; set; } = null!;
        public List<VisitReportData> Visits { get; set; } = new();
    }

    /// <summary>
    /// Margin settings for printed documents (in centimeters).
    /// </summary>
    public class MarginSettings
    {
        public decimal LeftMargin { get; set; }
        public decimal RightMargin { get; set; }
        public decimal TopMargin { get; set; } = 1; // Default 1cm
        public decimal BottomMargin { get; set; } = 1; // Default 1cm
    }
}
