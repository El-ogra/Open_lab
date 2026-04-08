using System;

namespace Open_lab.ViewModels
{
    public class SelectedTestItem
    {
        public int VisitTestId { get; set; }
        public int TestId { get; set; }
        public string TestName { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public class VisitTestRow
    {
        public int VisitTestId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string TestName { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public string? Status { get; set; }
    }

    public class ResultEntryItem
    {
        public int ParameterId { get; set; }
        public string ParameterName { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string? Flag { get; set; }
        public string? Comment { get; set; }
    }

    public class WorkSheetPatientRow
    {
        public int VisitId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public int TestsCount { get; set; }
    }

    public class WorkSheetTestRow
    {
        public string TestName { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
