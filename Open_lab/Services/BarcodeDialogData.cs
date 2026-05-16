using System;
using System.Collections.Generic;

namespace Open_lab.Services
{
    /// <summary>
    /// بيانات نافذة الباركود — مطابقة لطبيعة ملصقات النظام المرجعي:
    /// كل ملصق يعرض: اسم المريض + الجنس + السن + كود الباركود + اسم العينة + التاريخ.
    /// </summary>
    public class BarcodeDialogData
    {
        public string PatientName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public int? Age { get; set; }
        public string AgeUnit { get; set; } = "Years";
        public DateTime BarcodeDate { get; set; } = DateTime.Now;

        public string CaseCode { get; set; } = string.Empty;
        public string FileCode { get; set; } = string.Empty;
        public string LabCode { get; set; } = string.Empty;

        /// <summary>
        /// قائمة بأسماء العينات/التحاليل التي يجب طباعة ملصق لكل منها
        /// (مثال: "Serum Fasting", "CBC", "Urine", "ESR" ...).
        /// </summary>
        public List<string> SampleLabels { get; set; } = new();
    }
}
