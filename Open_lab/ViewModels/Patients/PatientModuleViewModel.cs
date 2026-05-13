using System;
using System.Windows.Input;
using Open_lab.ViewModels;

namespace Open_lab.ViewModels.Patients
{
    public class PatientModuleViewModel : BaseViewModel
    {
        private readonly Action<string> _openPlaceholder;

        public PatientModuleViewModel(Action<string> openPlaceholder)
        {
            _openPlaceholder = openPlaceholder ?? throw new ArgumentNullException(nameof(openPlaceholder));

            OpenAddPatientCommand = new RelayCommand(_ => _openPlaceholder("اضافة وتعديل بيانات المرضى"));
            OpenEnterResultsCommand = new RelayCommand(_ => _openPlaceholder("ادخال نتائج التحاليل"));
            OpenDeliverResultsCommand = new RelayCommand(_ => _openPlaceholder("تسليم نتائج المرضى"));
            OpenSearchPatientCommand = new RelayCommand(_ => _openPlaceholder("بحث عن مريض"));
        }

        public ICommand OpenAddPatientCommand { get; }
        public ICommand OpenEnterResultsCommand { get; }
        public ICommand OpenDeliverResultsCommand { get; }
        public ICommand OpenSearchPatientCommand { get; }
    }
}
