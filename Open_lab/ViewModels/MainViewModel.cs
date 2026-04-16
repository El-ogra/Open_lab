using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Data;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private BaseViewModel _currentViewModel = new HomeViewModel();
        private bool _isLoggedIn;
        private readonly Func<OpenLabDbContext> _dbFactory;

        public MainViewModel()
        {
            _dbFactory = () => new OpenLabDbContextFactory().CreateDbContext(Array.Empty<string>());

            NavigateDashboardCommand = new RelayCommand(_ => NavigateDashboard(), _ => IsLoggedIn);
            NavigatePatientRegistrationCommand = new RelayCommand(_ => NavigatePatientRegistration(), _ => IsLoggedIn);
            NavigatePatientTestsCommand = new RelayCommand(_ => NavigatePatientTests(), _ => IsLoggedIn);
            NavigatePatientBillingCommand = new RelayCommand(_ => NavigatePatientBilling(), _ => IsLoggedIn);
            NavigateResultsEntryCommand = new RelayCommand(_ => NavigateResultsEntry(), _ => IsLoggedIn);
            NavigateReportViewerCommand = new RelayCommand(_ => NavigateReportViewer(), _ => IsLoggedIn);

            NavigatePatientSearchCommand = new RelayCommand(_ => NavigatePatientSearch(), _ => IsLoggedIn);
            NavigatePatientHistoryCommand = new RelayCommand(_ => NavigatePatientHistory(), _ => IsLoggedIn);
            NavigateWorkSheetByPatientCommand = new RelayCommand(_ => NavigateWorkSheetByPatient(), _ => IsLoggedIn);
            NavigateWorkSheetByTestCommand = new RelayCommand(_ => NavigateWorkSheetByTest(), _ => IsLoggedIn);
            NavigateTestCatalogCommand = new RelayCommand(_ => NavigateTestCatalog(), _ => IsLoggedIn);
            NavigateReferenceRangesCommand = new RelayCommand(_ => NavigateReferenceRanges(), _ => IsLoggedIn);
            NavigateTestCommentsCommand = new RelayCommand(_ => NavigateTestComments(), _ => IsLoggedIn);
            NavigatePriceListsCommand = new RelayCommand(_ => NavigatePriceLists(), _ => IsLoggedIn);
            NavigateCustomGroupsCommand = new RelayCommand(_ => NavigateCustomGroups(), _ => IsLoggedIn);
            NavigateReferralsCommand = new RelayCommand(_ => NavigateReferrals(), _ => IsLoggedIn);
            NavigateUsersPermissionsCommand = new RelayCommand(_ => NavigateUsersPermissions(), _ => IsLoggedIn);
            NavigateStatisticsCommand = new RelayCommand(_ => NavigateStatistics(), _ => IsLoggedIn);
            NavigateSystemSettingsCommand = new RelayCommand(_ => NavigateSystemSettings(), _ => IsLoggedIn);
            NavigateBackupRestoreCommand = new RelayCommand(_ => NavigateBackupRestore(), _ => IsLoggedIn);
            NavigateAttendanceLogCommand = new RelayCommand(_ => NavigateAttendanceLog(), _ => IsLoggedIn);
            NavigateAccountsTreasuryCommand = new RelayCommand(_ => NavigateAccountsTreasury(), _ => IsLoggedIn);
            NavigateSampleCollectionCommand = new RelayCommand(_ => NavigateSampleCollection(), _ => IsLoggedIn);
            NavigateCultureSensitivityCommand = new RelayCommand(_ => NavigateCultureSensitivity(), _ => IsLoggedIn);
            NavigateReceiptPrintingCommand = new RelayCommand(_ => NavigateReceiptPrinting(), _ => IsLoggedIn);
            NavigateCombinedReportCommand = new RelayCommand(_ => NavigateCombinedReport(), _ => IsLoggedIn);
            NavigateBlankReportCommand = new RelayCommand(_ => NavigateBlankReport(), _ => IsLoggedIn);

            LogoutCommand = new RelayCommand(async _ => await LogoutAsync());

            ShowLogin();
        }

        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            private set => SetProperty(ref _currentViewModel, value);
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
        public ICommand NavigateSampleCollectionCommand { get; }
        public ICommand NavigateCultureSensitivityCommand { get; }
        public ICommand NavigateReceiptPrintingCommand { get; }
        public ICommand NavigateCombinedReportCommand { get; }
        public ICommand NavigateBlankReportCommand { get; }

        public ICommand LogoutCommand { get; }

        private void ShowLogin()
        {
            IsLoggedIn = false;
            CurrentViewModel = new LoginViewModel(CreateAuthService(), CreateAdminSetupService(), CreateAttendanceService(), OnLoginSuccess);
        }

        private void OnLoginSuccess()
        {
            IsLoggedIn = true;
            NavigateDashboard();
        }

        private void NavigateDashboard()
        {
            CurrentViewModel = new DashboardViewModel(CreateDashboardService());
        }

        private void NavigatePatientRegistration()
        {
            CurrentViewModel = new PatientRegistrationViewModel(CreatePatientService());
        }

        private void NavigatePatientTests()
        {
            CurrentViewModel = new PatientTestsSelectionViewModel(CreatePatientService(), CreateVisitService(), CreateTestCatalogService());
        }

        private void NavigatePatientBilling()
        {
            CurrentViewModel = new PatientBillingViewModel(CreateInvoiceService());
        }

        private void NavigateResultsEntry()
        {
            CurrentViewModel = new ResultsEntryViewModel(CreateResultsService());
        }

        private void NavigateReportViewer()
        {
            CurrentViewModel = new ReportViewerViewModel(CreateReportService());
        }

        private void NavigatePatientSearch()
        {
            CurrentViewModel = new PatientSearchViewModel(CreatePatientSearchService());
        }

        private void NavigatePatientHistory()
        {
            CurrentViewModel = new PatientHistoryViewModel(CreatePatientService(), CreateReportService());
        }

        private void NavigateWorkSheetByPatient()
        {
            CurrentViewModel = new WorkSheetByPatientViewModel(CreateWorksheetService());
        }

        private void NavigateWorkSheetByTest()
        {
            CurrentViewModel = new WorkSheetByTestViewModel(CreateWorksheetService());
        }

        private void NavigateTestCatalog()
        {
            CurrentViewModel = new TestCatalogViewModel(CreateTestCatalogService());
        }

        private void NavigateReferenceRanges()
        {
            CurrentViewModel = new ReferenceRangesViewModel(CreateTestCatalogService());
        }

        private void NavigateTestComments()
        {
            CurrentViewModel = new TestCommentsViewModel(CreateTestCatalogService());
        }

        private void NavigatePriceLists()
        {
            CurrentViewModel = new PriceListsViewModel(CreateTestCatalogService());
        }

        private void NavigateCustomGroups()
        {
            CurrentViewModel = new CustomGroupsViewModel(CreateTestCatalogService());
        }

        private void NavigateReferrals()
        {
            CurrentViewModel = new ReferralsViewModel(CreateTestCatalogService());
        }

        private void NavigateUsersPermissions()
        {
            CurrentViewModel = new UsersPermissionsViewModel(CreateUserAdminService());
        }

        private void NavigateStatistics()
        {
            CurrentViewModel = new StatisticsViewModel(CreateStatisticsService());
        }

        private void NavigateSystemSettings()
        {
            CurrentViewModel = new SystemSettingsViewModel(CreateSystemSettingsService());
        }

        private void NavigateBackupRestore()
        {
            CurrentViewModel = new BackupRestoreViewModel(CreateBackupRestoreService());
        }

        private void NavigateAttendanceLog()
        {
            CurrentViewModel = new AttendanceLogViewModel(CreateAttendanceService());
        }

        private void NavigateAccountsTreasury()
        {
            CurrentViewModel = new AccountsTreasuryViewModel(CreateAccountsTreasuryService());
        }

        private void NavigateSampleCollection()
        {
            CurrentViewModel = new SampleCollectionViewModel(CreateSampleCollectionService());
        }

        private void NavigateCultureSensitivity()
        {
            CurrentViewModel = new CultureSensitivityViewModel(CreateCultureSensitivityService());
        }

        private void NavigateReceiptPrinting()
        {
            CurrentViewModel = new ReceiptPrintingViewModel(CreateReceiptService());
        }

        private void NavigateCombinedReport()
        {
            CurrentViewModel = new CombinedReportViewModel(CreateReportService());
        }

        private void NavigateBlankReport()
        {
            CurrentViewModel = new BlankReportViewModel(CreateReportService());
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
                await CreateAttendanceService().CloseAsync(AppSession.AttendanceLogId);
            }
            catch
            {
            }
            finally
            {
                AppSession.AttendanceLogId = 0;
            }
        }

        private IAuthService CreateAuthService() => new AuthService(_dbFactory());
        private IAdminSetupService CreateAdminSetupService() => new AdminSetupService(_dbFactory());
        private IAttendanceService CreateAttendanceService() => new AttendanceService(_dbFactory());
        private IDashboardService CreateDashboardService() => new DashboardService(_dbFactory());
        private IPatientService CreatePatientService() => new PatientService(_dbFactory());
        private IVisitService CreateVisitService() => new VisitService(_dbFactory());
        private IInvoiceService CreateInvoiceService() => new InvoiceService(_dbFactory());
        private IResultsService CreateResultsService() => new ResultsService(_dbFactory());
        private IReportService CreateReportService() => new ReportService(_dbFactory());
        private IPatientSearchService CreatePatientSearchService() => new PatientSearchService(_dbFactory());
        private IWorksheetService CreateWorksheetService() => new WorksheetService(_dbFactory());
        private ITestCatalogService CreateTestCatalogService() => new TestCatalogService(_dbFactory());
        private IUserAdminService CreateUserAdminService() => new UserAdminService(_dbFactory());
        private IStatisticsService CreateStatisticsService() => new StatisticsService(_dbFactory());
        private ISystemSettingsService CreateSystemSettingsService() => new SystemSettingsService(_dbFactory());
        private IBackupRestoreService CreateBackupRestoreService() => new BackupRestoreService(_dbFactory());
        private IAccountsTreasuryService CreateAccountsTreasuryService() => new AccountsTreasuryService(_dbFactory());
        private ISampleCollectionService CreateSampleCollectionService() => new SampleCollectionService(_dbFactory());
        private ICultureSensitivityService CreateCultureSensitivityService() => new CultureSensitivityService(_dbFactory());
        private IReceiptService CreateReceiptService() => new ReceiptService(_dbFactory());

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
            (NavigateSampleCollectionCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateCultureSensitivityCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateReceiptPrintingCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateCombinedReportCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateBlankReportCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
