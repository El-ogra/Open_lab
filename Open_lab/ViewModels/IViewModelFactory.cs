using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public interface IViewModelFactory
    {
        BaseViewModel Create(NavigationTarget target, System.Action? onLoginSuccess = null);
        IAttendanceService CreateAttendanceService();
    }
}
