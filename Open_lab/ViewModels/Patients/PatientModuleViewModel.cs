using System;
using System.Windows.Input;
using Open_lab.ViewModels;

namespace Open_lab.ViewModels.Patients
{
    public class PatientModuleViewModel : BaseViewModel
    {
        public PatientModuleViewModel(Action<string> openPlaceholder)
        {
            if (openPlaceholder == null)
            {
                throw new ArgumentNullException(nameof(openPlaceholder));
            }

            OpenAddPatientCommand = new RelayCommand(_ => openPlaceholder("اضافة وتعديل بيانات المرضى"));
            OpenEnterResultsCommand = new RelayCommand(_ => openPlaceholder("ادخال نتائج التحاليل"));
            OpenDeliverResultsCommand = new RelayCommand(_ => openPlaceholder("تسليم نتائج المرضى"));
            OpenSearchPatientCommand = new RelayCommand(_ => openPlaceholder("بحث عن مريض"));
        }

        public PatientModuleViewModel(Action<NavigationTarget> navigate)
        {
            if (navigate == null)
            {
                throw new ArgumentNullException(nameof(navigate));
            }

            OpenAddPatientCommand = new RelayCommand(_ => navigate(NavigationTarget.PatientRegistration));
            OpenEnterResultsCommand = new RelayCommand(_ => navigate(NavigationTarget.ResultsEntry));
            OpenDeliverResultsCommand = new RelayCommand(_ => navigate(NavigationTarget.Delivery));
            OpenSearchPatientCommand = new RelayCommand(_ => navigate(NavigationTarget.PatientSearch));
        }

        public ICommand OpenAddPatientCommand { get; }
        public ICommand OpenEnterResultsCommand { get; }
        public ICommand OpenDeliverResultsCommand { get; }
        public ICommand OpenSearchPatientCommand { get; }
    }
}
