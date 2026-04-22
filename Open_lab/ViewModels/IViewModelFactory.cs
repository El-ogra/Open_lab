namespace Open_lab.ViewModels
{
    public interface IViewModelFactory
    {
        BaseViewModel Create(NavigationTarget target, System.Action? onLoginSuccess = null);
    }
}
