using System;

namespace Open_lab.ViewModels
{
    public class NavigationService : BaseViewModel, INavigationService
    {
        private readonly IViewModelFactory _viewModelFactory;
        private BaseViewModel _currentViewModel;

        public NavigationService(IViewModelFactory viewModelFactory)
        {
            _viewModelFactory = viewModelFactory ?? throw new ArgumentNullException(nameof(viewModelFactory));
            _currentViewModel = _viewModelFactory.Create(NavigationTarget.Home);
        }

        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            private set => SetProperty(ref _currentViewModel, value);
        }

        public void Navigate(NavigationTarget target, Action? onLoginSuccess = null)
        {
            CurrentViewModel = _viewModelFactory.Create(target, onLoginSuccess);
        }
    }
}
