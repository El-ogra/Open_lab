using System.Windows.Controls;
using System.Windows.Input;

namespace Open_lab.Views.Patients
{
    public partial class PatientRegistrationView : UserControl
    {
        public PatientRegistrationView()
        {
            InitializeComponent();
        }

        private void AvailableTestsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ViewModels.PatientRegistrationViewModel vm)
            {
                if (vm.AddSelectedTestCommand.CanExecute(null))
                {
                    vm.AddSelectedTestCommand.Execute(null);
                }
            }
        }
    }
}
