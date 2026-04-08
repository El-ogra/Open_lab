using System.Windows.Input;

namespace Open_lab.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private BaseViewModel _currentViewModel;
        private readonly NavigationService _navigationService;

        public MainViewModel()
        {
            _navigationService = new NavigationService(SetCurrentViewModel);
            _currentViewModel = new HomeViewModel();
            NavigateHomeCommand = new RelayCommand(_ => _navigationService.Navigate(new HomeViewModel()));
        }

        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            private set => SetProperty(ref _currentViewModel, value);
        }

        public ICommand NavigateHomeCommand { get; }

        private void SetCurrentViewModel(BaseViewModel viewModel)
        {
            CurrentViewModel = viewModel;
        }
    }
}
