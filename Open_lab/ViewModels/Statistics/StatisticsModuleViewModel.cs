using System;
using System.Windows.Input;
using Open_lab.ViewModels;

namespace Open_lab.ViewModels.Statistics
{
    public class StatisticsModuleViewModel : BaseViewModel
    {
        private readonly Action<string> _openPlaceholder;

        public StatisticsModuleViewModel(Action<string> openPlaceholder)
        {
            _openPlaceholder = openPlaceholder ?? throw new ArgumentNullException(nameof(openPlaceholder));

            OpenPatientCountStatCommand = new RelayCommand(_ => _openPlaceholder("احصاليات وفقاً لعدد المرضى"));
            OpenTestCountStatCommand = new RelayCommand(_ => _openPlaceholder("احصاليات وفقاً لعدد التحاليل"));
            OpenBranchStatCommand = new RelayCommand(_ => _openPlaceholder("احصاليات خاصة بفروع المعمل"));
            OpenExternalSamplesStatCommand = new RelayCommand(_ => _openPlaceholder("احصاليات العينات المرسلة"));
            OpenWorkPerformanceStatCommand = new RelayCommand(_ => _openPlaceholder("احصاليات تقيم ومتابعة العمل"));
            OpenResultsMonitorCommand = new RelayCommand(_ => _openPlaceholder("متابعة ومراقبة النتائج"));
        }

        public ICommand OpenPatientCountStatCommand { get; }
        public ICommand OpenTestCountStatCommand { get; }
        public ICommand OpenBranchStatCommand { get; }
        public ICommand OpenExternalSamplesStatCommand { get; }
        public ICommand OpenWorkPerformanceStatCommand { get; }
        public ICommand OpenResultsMonitorCommand { get; }
    }
}
