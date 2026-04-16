using System;
using System.ComponentModel;

namespace Open_lab.ViewModels
{
    public interface INavigationService : INotifyPropertyChanged
    {
        BaseViewModel CurrentViewModel { get; }
        void Navigate(NavigationTarget target, Action? onLoginSuccess = null);
    }
}
