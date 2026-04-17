using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IViewModelFactory _viewModelFactory;
        private bool _isLoggedIn;

        public MainViewModel()
        {
            _viewModelFactory = new ViewModelFactory();
            _navigationService = new NavigationService(_viewModelFactory);
            _navigationService.PropertyChanged += OnNavigationServicePropertyChanged;

            NavigateDashboardCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.Dashboard), _ => IsLoggedIn);
            NavigatePatientRegistrationCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.PatientRegistration), _ => CanNavigate(PermissionCodes.PatientsView));
            NavigatePatientTestsCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.PatientTestsSelection), _ => CanNavigate(PermissionCodes.VisitsView));
            NavigatePatientBillingCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.PatientBilling), _ => CanNavigate(PermissionCodes.AccountsView));
            NavigateResultsEntryCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.ResultsEntry), _ => CanNavigate(PermissionCodes.ResultsView));
            NavigateReportViewerCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.ReportViewer), _ => CanNavigate(PermissionCodes.ReportsView));

            NavigatePatientSearchCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.PatientSearch), _ => CanNavigate(PermissionCodes.PatientsView));
            NavigatePatientHistoryCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.PatientHistory), _ => CanNavigate(PermissionCodes.PatientsView));
            NavigateWorkSheetByPatientCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.WorkSheetByPatient), _ => CanNavigate(PermissionCodes.TestsView));
            NavigateWorkSheetByTestCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.WorkSheetByTest), _ => CanNavigate(PermissionCodes.TestsView));
            NavigateTestCatalogCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.TestCatalog), _ => CanNavigate(PermissionCodes.TestsEdit));
            NavigateReferenceRangesCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.ReferenceRanges), _ => CanNavigate(PermissionCodes.TestsEdit));
            NavigateTestCommentsCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.TestComments), _ => CanNavigate(PermissionCodes.TestsEdit));
            NavigatePriceListsCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.PriceLists), _ => CanNavigate(PermissionCodes.TestsEdit));
            NavigateCustomGroupsCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.CustomGroups), _ => CanNavigate(PermissionCodes.TestsEdit));
            NavigateReferralsCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.Referrals), _ => CanNavigate(PermissionCodes.TestsEdit));
            NavigateUsersPermissionsCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.UsersPermissions), _ => CanNavigate(PermissionCodes.UsersView));
            NavigateStatisticsCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.Statistics), _ => CanNavigate(PermissionCodes.StatisticsView));
            NavigateSystemSettingsCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.SystemSettings), _ => CanNavigate(PermissionCodes.SettingsView));
            NavigateBackupRestoreCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.BackupRestore), _ => CanNavigate(PermissionCodes.BackupRestore));
            NavigateAttendanceLogCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.AttendanceLog), _ => CanNavigate(PermissionCodes.UsersView));
            NavigateAccountsTreasuryCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.AccountsTreasury), _ => CanNavigate(PermissionCodes.AccountsView));
            NavigateDeliveryCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.Delivery), _ => CanNavigate(PermissionCodes.DeliveryView));
            NavigateSampleCollectionCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.SampleCollection), _ => CanNavigate(PermissionCodes.TestsView));
            NavigateCultureSensitivityCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.CultureSensitivity), _ => CanNavigate(PermissionCodes.TestsView));
            NavigateReceiptPrintingCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.ReceiptPrinting), _ => CanNavigate(PermissionCodes.AccountsView));
            NavigateCombinedReportCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.CombinedReport), _ => CanNavigate(PermissionCodes.ReportsView));
            NavigateBlankReportCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.BlankReport), _ => CanNavigate(PermissionCodes.ReportsView));
            NavigateConstantsCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.Constants), _ => CanNavigate(PermissionCodes.ConstantsView));

            LogoutCommand = new RelayCommand(async _ => await LogoutAsync(), _ => IsLoggedIn);

            ShowLogin();
        }

        public BaseViewModel CurrentViewModel => _navigationService.CurrentViewModel;

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            private set
            {
                if (SetProperty(ref _isLoggedIn, value))
                {
                    RaiseNavigationCanExecuteChanged();
                }
            }
        }

        public ICommand NavigateDashboardCommand { get; }
        public ICommand NavigatePatientRegistrationCommand { get; }
        public ICommand NavigatePatientTestsCommand { get; }
        public ICommand NavigatePatientBillingCommand { get; }
        public ICommand NavigateResultsEntryCommand { get; }
        public ICommand NavigateReportViewerCommand { get; }
        public ICommand NavigatePatientSearchCommand { get; }
        public ICommand NavigatePatientHistoryCommand { get; }
        public ICommand NavigateWorkSheetByPatientCommand { get; }
        public ICommand NavigateWorkSheetByTestCommand { get; }
        public ICommand NavigateTestCatalogCommand { get; }
        public ICommand NavigateReferenceRangesCommand { get; }
        public ICommand NavigateTestCommentsCommand { get; }
        public ICommand NavigatePriceListsCommand { get; }
        public ICommand NavigateCustomGroupsCommand { get; }
        public ICommand NavigateReferralsCommand { get; }
        public ICommand NavigateUsersPermissionsCommand { get; }
        public ICommand NavigateStatisticsCommand { get; }
        public ICommand NavigateSystemSettingsCommand { get; }
        public ICommand NavigateBackupRestoreCommand { get; }
        public ICommand NavigateAttendanceLogCommand { get; }
        public ICommand NavigateAccountsTreasuryCommand { get; }
        public ICommand NavigateDeliveryCommand { get; }
        public ICommand NavigateSampleCollectionCommand { get; }
        public ICommand NavigateCultureSensitivityCommand { get; }
        public ICommand NavigateReceiptPrintingCommand { get; }
        public ICommand NavigateCombinedReportCommand { get; }
        public ICommand NavigateBlankReportCommand { get; }
        public ICommand NavigateConstantsCommand { get; }
        public ICommand LogoutCommand { get; }

        private void ShowLogin()
        {
            AppSession.Clear();
            IsLoggedIn = false;
            _navigationService.Navigate(NavigationTarget.Login, () => _ = OnLoginSuccessAsync());
        }

        private async Task OnLoginSuccessAsync()
        {
            IsLoggedIn = true;
            NavigateTo(NavigationTarget.Dashboard);
            await Task.CompletedTask;
        }

        private void NavigateTo(NavigationTarget target)
        {
            _navigationService.Navigate(target);
        }

        private bool CanNavigate(string permissionCode)
        {
            return IsLoggedIn && AppSession.HasPermission(permissionCode);
        }

        private async Task LogoutAsync()
        {
            await CloseAttendanceAsync();
            ShowLogin();
        }

        private async Task CloseAttendanceAsync()
        {
            if (AppSession.AttendanceLogId <= 0)
            {
                return;
            }

            try
            {
                await _viewModelFactory.CreateAttendanceService().CloseAsync(AppSession.AttendanceLogId);
            }
            catch
            {
            }
            finally
            {
                AppSession.AttendanceLogId = 0;
            }
        }

        private void OnNavigationServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(INavigationService.CurrentViewModel))
            {
                OnPropertyChanged(nameof(CurrentViewModel));
            }
        }

        private void RaiseNavigationCanExecuteChanged()
        {
            (NavigateDashboardCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigatePatientRegistrationCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigatePatientTestsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigatePatientBillingCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateResultsEntryCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateReportViewerCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigatePatientSearchCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigatePatientHistoryCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateWorkSheetByPatientCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateWorkSheetByTestCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateTestCatalogCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateReferenceRangesCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateTestCommentsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigatePriceListsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateCustomGroupsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateReferralsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateUsersPermissionsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateStatisticsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateSystemSettingsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateBackupRestoreCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateAttendanceLogCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateAccountsTreasuryCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateDeliveryCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateSampleCollectionCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateCultureSensitivityCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateReceiptPrintingCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateCombinedReportCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateBlankReportCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateConstantsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (LogoutCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
