using System.Collections.Generic;

namespace Open_lab.Services
{
    public class BarcodeDialogData
    {
        public string PatientName { get; set; } = string.Empty;
        public string CaseCode { get; set; } = string.Empty;
        public string FileCode { get; set; } = string.Empty;
        public string LabCode { get; set; } = string.Empty;
        public List<string> SampleLabels { get; set; } = new();
    }
}
