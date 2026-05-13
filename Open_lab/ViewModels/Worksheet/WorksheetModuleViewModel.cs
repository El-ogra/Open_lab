using System;
using System.Windows.Input;
using Open_lab.ViewModels;

namespace Open_lab.ViewModels.Worksheet
{
    public class WorksheetModuleViewModel : BaseViewModel
    {
        private readonly Action<string> _openPlaceholder;

        public WorksheetModuleViewModel(Action<string> openPlaceholder)
        {
            _openPlaceholder = openPlaceholder ?? throw new ArgumentNullException(nameof(openPlaceholder));

            OpenWorksheetByPatientCommand = new RelayCommand(_ => _openPlaceholder("ورقة عمل بأسماء المرضى"));
            OpenWorksheetByTestCommand = new RelayCommand(_ => _openPlaceholder("ورقة عمل بأسماء التحاليل (Log)"));
        }

        public ICommand OpenWorksheetByPatientCommand { get; }
        public ICommand OpenWorksheetByTestCommand { get; }
    }
}
