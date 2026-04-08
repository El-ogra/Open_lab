using System;
using System.Windows.Input;
using Open_lab.Data;

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

            LogoutCommand = new RelayCommand(_ => ShowLogin());

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

        public ICommand LogoutCommand { get; }

        private void ShowLogin()
        {
            IsLoggedIn = false;
            CurrentViewModel = new LoginViewModel(_dbFactory, OnLoginSuccess);
        }

        private void OnLoginSuccess()
        {
            IsLoggedIn = true;
            NavigateDashboard();
        }

        private void NavigateDashboard()
        {
            CurrentViewModel = new DashboardViewModel(_dbFactory);
        }

        private void NavigatePatientRegistration()
        {
            CurrentViewModel = new PatientRegistrationViewModel(_dbFactory);
        }

        private void NavigatePatientTests()
        {
            CurrentViewModel = new PatientTestsSelectionViewModel(_dbFactory);
        }

        private void NavigatePatientBilling()
        {
            CurrentViewModel = new PatientBillingViewModel(_dbFactory);
        }

        private void NavigateResultsEntry()
        {
            CurrentViewModel = new ResultsEntryViewModel(_dbFactory);
        }

        private void NavigateReportViewer()
        {
            CurrentViewModel = new ReportViewerViewModel(_dbFactory);
        }

        private void NavigatePatientSearch()
        {
            CurrentViewModel = new PatientSearchViewModel(_dbFactory);
        }

        private void NavigatePatientHistory()
        {
            CurrentViewModel = new PatientHistoryViewModel(_dbFactory);
        }

        private void NavigateWorkSheetByPatient()
        {
            CurrentViewModel = new WorkSheetByPatientViewModel(_dbFactory);
        }

        private void NavigateWorkSheetByTest()
        {
            CurrentViewModel = new WorkSheetByTestViewModel(_dbFactory);
        }

        private void NavigateTestCatalog()
        {
            CurrentViewModel = new TestCatalogViewModel(_dbFactory);
        }

        private void NavigateReferenceRanges()
        {
            CurrentViewModel = new ReferenceRangesViewModel(_dbFactory);
        }

        private void NavigateTestComments()
        {
            CurrentViewModel = new TestCommentsViewModel(_dbFactory);
        }

        private void NavigatePriceLists()
        {
            CurrentViewModel = new PriceListsViewModel(_dbFactory);
        }

        private void NavigateCustomGroups()
        {
            CurrentViewModel = new CustomGroupsViewModel(_dbFactory);
        }

        private void NavigateReferrals()
        {
            CurrentViewModel = new ReferralsViewModel(_dbFactory);
        }

        private void NavigateUsersPermissions()
        {
            CurrentViewModel = new UsersPermissionsViewModel(_dbFactory);
        }

        private void NavigateStatistics()
        {
            CurrentViewModel = new StatisticsViewModel(_dbFactory);
        }

        private void NavigateSystemSettings()
        {
            CurrentViewModel = new SystemSettingsViewModel(_dbFactory);
        }

        private void NavigateBackupRestore()
        {
            CurrentViewModel = new BackupRestoreViewModel(_dbFactory);
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
        }
    }
}


