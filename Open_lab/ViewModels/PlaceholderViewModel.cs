using System.Windows.Input;

namespace Open_lab.ViewModels
{
    public class PlaceholderViewModel : BaseViewModel
    {
        public PlaceholderViewModel(string functionTitle, ICommand? backCommand)
        {
            FunctionTitle = functionTitle;
            BackCommand = backCommand;
        }

        public string FunctionTitle { get; }

        public ICommand? BackCommand { get; }
    }
}
