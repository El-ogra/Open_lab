using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Services;
using Open_lab.ViewModels.Patients;
using Open_lab.ViewModels.SystemData;
using Open_lab.Views.Shared;

namespace Open_lab.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IAttendanceService _attendanceService;
        private readonly IMainWindowLayoutService _windowLayoutService;
        private object _currentView = new WelcomeViewModel();
        private string _currentUser = string.Empty;
        private string _lastLoginDate = string.Empty;
        private string _currentDate = System.DateTime.Now.ToString("yyyy/MM/dd");
        private string _activeModule = string.Empty;
        private System.Action? _logoutRequested;
        private bool _isToolbarVisible = true;
        private bool _isLoggedIn;

        public MainViewModel(
            INavigationService navigationService,
            IAttendanceService attendanceService,
            IMainWindowLayoutService windowLayoutService)
        {
            _navigationService = navigationService ?? throw new System.ArgumentNullException(nameof(navigationService));
            _attendanceService = attendanceService ?? throw new System.ArgumentNullException(nameof(attendanceService));
            _windowLayoutService = windowLayoutService ?? throw new System.ArgumentNullException(nameof(windowLayoutService));
            _navigationService.PropertyChanged += OnNavigationServicePropertyChanged;

            NavigateDashboardCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.Dashboard), _ => IsLoggedIn);
            NavigatePatientRegistrationCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.PatientRegistration), _ => CanNavigate(PermissionCodes.PatientsView));
            NavigatePatientTestsCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.PatientTestsSelection), _ => CanNavigate(PermissionCodes.VisitsView));
            NavigatePatientBillingCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.PatientBilling), _ => CanNavigate(PermissionCodes.AccountsView));
            NavigatePatientBillingByDateCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.PatientBillingByDate), _ => CanNavigate(PermissionCodes.AccountsView));
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
            NavigateCompareWithHistoryCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.CompareWithHistory), _ => CanNavigate(PermissionCodes.ResultsView));
            NavigateGroupWorksheetCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.GroupWorksheet), _ => CanNavigate(PermissionCodes.TestsView));
            NavigateTestClassificationLogCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.TestClassificationLog), _ => CanNavigate(PermissionCodes.TestsView));
            NavigateExternalLabManagementCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.ExternalLabManagement), _ => CanNavigate(PermissionCodes.TestsView));
            NavigateAttendanceReportCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.AttendanceReport), _ => CanNavigate(PermissionCodes.UsersView));
            NavigateContractInvoiceCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.ContractInvoice), _ => CanNavigate(PermissionCodes.AccountsView));
            NavigateUserActivityLogCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.UserActivityLog), _ => CanNavigate(PermissionCodes.UsersView));
            NavigateSystemUsageMonitorCommand = new RelayCommand(_ => NavigateTo(NavigationTarget.SystemUsageMonitor), _ => CanNavigate(PermissionCodes.UsersView));

            NavigateToPatientsCommand = new RelayCommand(_ => NavigateToPatientsModule());
            NavigateToToolsCommand = new RelayCommand(_ => NavigateTopModule("أدوات"));
            NavigateToWorksheetCommand = new RelayCommand(_ => NavigateTopModule("ورقة عمل"));
            NavigateToAccountsCommand = new RelayCommand(_ => NavigateTopModule("حسابات"));
            NavigateToStatisticsCommand = new RelayCommand(_ => NavigateTopModule("احصاليات"));
            NavigateToUsersCommand = new RelayCommand(_ => NavigateTopModule("المستخدمين"));
            NavigateToSystemDataCommand = new RelayCommand(_ => NavigateToSystemDataModule());
            NavigateToSettingsCommand = new RelayCommand(_ => NavigateTopModule("اعدادات"));
            NavigateToEmployeesCommand = new RelayCommand(_ => NavigateTopModule("الموظفين"));
            NavigateToDidYouKnowCommand = new RelayCommand(_ => NavigateTopModule("هل تعلم"));
            NavigateToAboutCommand = new RelayCommand(_ => NavigateTopModule("نبذة"));
            LogoutCommand = new RelayCommand(async _ => await LogoutAsync(), _ => IsLoggedIn);

            ShowLogin();
        }

        public BaseViewModel CurrentViewModel => _navigationService.CurrentViewModel;

        public object CurrentView
        {
            get => _currentView;
            private set => SetProperty(ref _currentView, value);
        }

        public string CurrentUser
        {
            get => _currentUser;
            private set => SetProperty(ref _currentUser, value);
        }

        public string LastLoginDate
        {
            get => _lastLoginDate;
            private set => SetProperty(ref _lastLoginDate, value);
        }

        public string CurrentDate
        {
            get => _currentDate;
            private set => SetProperty(ref _currentDate, value);
        }

        public string ActiveModule
        {
            get => _activeModule;
            private set => SetProperty(ref _activeModule, value);
        }

        public bool IsToolbarVisible
        {
            get => _isToolbarVisible;
            private set => SetProperty(ref _isToolbarVisible, value);
        }

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
        public ICommand NavigatePatientBillingByDateCommand { get; }
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
        public ICommand NavigateCompareWithHistoryCommand { get; }
        public ICommand NavigateGroupWorksheetCommand { get; }
        public ICommand NavigateTestClassificationLogCommand { get; }
        public ICommand NavigateExternalLabManagementCommand { get; }
        public ICommand NavigateAttendanceReportCommand { get; }
        public ICommand NavigateContractInvoiceCommand { get; }
        public ICommand NavigateUserActivityLogCommand { get; }
        public ICommand NavigateSystemUsageMonitorCommand { get; }
        public ICommand NavigateToPatientsCommand { get; }
        public ICommand NavigateToToolsCommand { get; }
        public ICommand NavigateToWorksheetCommand { get; }
        public ICommand NavigateToAccountsCommand { get; }
        public ICommand NavigateToStatisticsCommand { get; }
        public ICommand NavigateToUsersCommand { get; }
        public ICommand NavigateToSystemDataCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }
        public ICommand NavigateToEmployeesCommand { get; }
        public ICommand NavigateToDidYouKnowCommand { get; }
        public ICommand NavigateToAboutCommand { get; }
        public ICommand LogoutCommand { get; }

        public void InitializeAfterLogin(string currentUser, string lastLoginDate, System.Action? onLogoutRequested = null)
        {
            CurrentUser = currentUser;
            LastLoginDate = lastLoginDate;
            CurrentDate = System.DateTime.Now.ToString("yyyy/MM/dd");
            ActiveModule = string.Empty;
            CurrentView = new WelcomeViewModel();
            IsToolbarVisible = true;
            _logoutRequested = onLogoutRequested;
            IsLoggedIn = true;
            _windowLayoutService.ApplyAppLayout();
        }

        private void ShowLogin()
        {
            AppSession.Clear();
            IsLoggedIn = false;
            _windowLayoutService.ApplyLoginLayout();

            _navigationService.Navigate(NavigationTarget.Login, () => _ = OnLoginSuccessAsync());
        }

        private async Task OnLoginSuccessAsync()
        {
            IsLoggedIn = true;
            _windowLayoutService.ApplyAppLayout();

            NavigateTo(NavigationTarget.Dashboard);
            await Task.CompletedTask;
        }

        private void NavigateTo(NavigationTarget target)
        {
            _navigationService.Navigate(target);
        }

        private void NavigateTopModule(string moduleName)
        {
            ActiveModule = moduleName;
            IsToolbarVisible = true;
            CurrentView = new WelcomeViewModel(moduleName);
        }

        private void NavigateToPatientsModule()
        {
            ActiveModule = "المرضى";
            IsToolbarVisible = true;
            CurrentView = new PatientModuleViewModel(OpenPlaceholder);
        }

        private void NavigateToSystemDataModule()
        {
            ActiveModule = "بيانات النظام";
            IsToolbarVisible = true;
            CurrentView = new SystemDataModuleViewModel(OpenPlaceholder);
        }

        private void OpenPlaceholder(string functionTitle)
        {
            IsToolbarVisible = false;
            CurrentView = new PlaceholderView(functionTitle, new RelayCommand(_ => ReturnToActiveModule()));
        }

        private void ReturnToActiveModule()
        {
            IsToolbarVisible = true;

            if (ActiveModule == "المرضى")
            {
                CurrentView = new PatientModuleViewModel(OpenPlaceholder);
                return;
            }

            if (ActiveModule == "بيانات النظام")
            {
                CurrentView = new SystemDataModuleViewModel(OpenPlaceholder);
                return;
            }

            CurrentView = new WelcomeViewModel(ActiveModule);
        }

        private bool CanNavigate(string permissionCode)
        {
            return IsLoggedIn && AppSession.HasPermission(permissionCode);
        }

        private async Task LogoutAsync()
        {
            await CloseAttendanceAsync();
            if (_logoutRequested != null)
            {
                AppSession.Clear();
                IsLoggedIn = false;
                ActiveModule = string.Empty;
                IsToolbarVisible = true;
                CurrentView = new WelcomeViewModel();
                _logoutRequested.Invoke();
                return;
            }

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
                await _attendanceService.CloseAsync(AppSession.AttendanceLogId);
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"Failed to close attendance log {AppSession.AttendanceLogId}: {ex}");
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
            (NavigatePatientBillingByDateCommand as RelayCommand)?.RaiseCanExecuteChanged();
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
            (NavigateCompareWithHistoryCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateGroupWorksheetCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateTestClassificationLogCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateExternalLabManagementCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateAttendanceReportCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateContractInvoiceCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateUserActivityLogCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateSystemUsageMonitorCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateToPatientsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateToToolsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateToWorksheetCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateToAccountsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateToStatisticsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateToUsersCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateToSystemDataCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateToSettingsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateToEmployeesCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateToDidYouKnowCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateToAboutCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (LogoutCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
