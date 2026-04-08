using System;

namespace Open_lab.ViewModels
{
    public class NavigationService
    {
        private readonly Action<BaseViewModel> _navigate;

        public NavigationService(Action<BaseViewModel> navigate)
        {
            _navigate = navigate ?? throw new ArgumentNullException(nameof(navigate));
        }

        public void Navigate(BaseViewModel viewModel)
        {
            _navigate(viewModel);
        }
    }
}
