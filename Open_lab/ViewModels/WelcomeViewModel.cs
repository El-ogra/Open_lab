namespace Open_lab.ViewModels
{
    public class WelcomeViewModel : BaseViewModel
    {
        public WelcomeViewModel()
            : this(string.Empty)
        {
        }

        public WelcomeViewModel(string moduleName)
        {
            ModuleName = moduleName;
        }

        public string ModuleName { get; }
    }
}
