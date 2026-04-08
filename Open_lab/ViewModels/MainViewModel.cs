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

        private void RaiseNavigationCanExecuteChanged()
        {
            (NavigateDashboardCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigatePatientRegistrationCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigatePatientTestsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigatePatientBillingCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateResultsEntryCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (NavigateReportViewerCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
