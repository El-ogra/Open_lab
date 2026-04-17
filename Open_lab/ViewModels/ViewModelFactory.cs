using System;
using Open_lab.Data;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class ViewModelFactory : IViewModelFactory
    {
        private readonly Func<OpenLabDbContext> _dbFactory;

        public ViewModelFactory(Func<OpenLabDbContext>? dbFactory = null)
        {
            _dbFactory = dbFactory ?? (() => new OpenLabDbContextFactory().CreateDbContext(Array.Empty<string>()));
        }

        public BaseViewModel Create(NavigationTarget target, Action? onLoginSuccess = null)
        {
            return target switch
            {
                NavigationTarget.Home => new HomeViewModel(),
                NavigationTarget.Login => CreateLoginViewModel(onLoginSuccess),
                NavigationTarget.Dashboard => new DashboardViewModel(CreateDashboardService()),
                NavigationTarget.PatientRegistration => new PatientRegistrationViewModel(CreatePatientService()),
                NavigationTarget.PatientTestsSelection => new PatientTestsSelectionViewModel(CreatePatientService(), CreateVisitService(), CreateTestCatalogService(), CreateInvoiceService()),
                NavigationTarget.PatientBilling => new PatientBillingViewModel(CreateInvoiceService()),
                NavigationTarget.ResultsEntry => new ResultsEntryViewModel(CreateResultsService()),
                NavigationTarget.ReportViewer => new ReportViewerViewModel(CreateReportService(), CreatePrintService()),
                NavigationTarget.PatientSearch => new PatientSearchViewModel(CreatePatientSearchService()),
                NavigationTarget.PatientHistory => new PatientHistoryViewModel(CreatePatientService(), CreateReportService(), CreatePrintService()),
                NavigationTarget.WorkSheetByPatient => new WorkSheetByPatientViewModel(CreateWorksheetService(), CreatePrintService()),
                NavigationTarget.WorkSheetByTest => new WorkSheetByTestViewModel(CreateWorksheetService(), CreatePrintService()),
                NavigationTarget.TestCatalog => new TestCatalogViewModel(CreateTestCatalogService(), CreateBarcodeService()),
                NavigationTarget.ReferenceRanges => new ReferenceRangesViewModel(CreateTestCatalogService()),
                NavigationTarget.TestComments => new TestCommentsViewModel(CreateTestCatalogService()),
                NavigationTarget.PriceLists => new PriceListsViewModel(CreateTestCatalogService(), CreatePrintService()),
                NavigationTarget.CustomGroups => new CustomGroupsViewModel(CreateTestCatalogService()),
                NavigationTarget.Referrals => new ReferralsViewModel(CreateTestCatalogService()),
                NavigationTarget.UsersPermissions => new UsersPermissionsViewModel(CreateUserAdminService()),
                NavigationTarget.Statistics => new StatisticsViewModel(CreateStatisticsService(), CreatePrintService()),
                NavigationTarget.SystemSettings => new SystemSettingsViewModel(CreateSystemSettingsService()),
                NavigationTarget.BackupRestore => new BackupRestoreViewModel(CreateBackupRestoreService()),
                NavigationTarget.AttendanceLog => new AttendanceLogViewModel(CreateAttendanceService()),
                NavigationTarget.AccountsTreasury => new AccountsTreasuryViewModel(CreateAccountsTreasuryService(), CreatePrintService()),
                NavigationTarget.Delivery => new DeliveryViewModel(CreateDeliveryService()),
                NavigationTarget.SampleCollection => new SampleCollectionViewModel(CreateSampleCollectionService()),
                NavigationTarget.CultureSensitivity => new CultureSensitivityViewModel(CreateCultureSensitivityService()),
                NavigationTarget.ReceiptPrinting => new ReceiptPrintingViewModel(CreateReceiptService(), CreatePrintService(), CreateBarcodeService()),
                NavigationTarget.CombinedReport => new CombinedReportViewModel(CreateReportService()),
                NavigationTarget.BlankReport => new BlankReportViewModel(CreateReportService()),
                NavigationTarget.Constants => new ConstantsViewModel(CreateConstantsService()),
                _ => throw new ArgumentOutOfRangeException(nameof(target), target, "Unsupported navigation target.")
            };
        }

        public IAttendanceService CreateAttendanceService() => new AttendanceService(_dbFactory());

        private BaseViewModel CreateLoginViewModel(Action? onLoginSuccess)
        {
            if (onLoginSuccess is null)
            {
                throw new InvalidOperationException("Login navigation requires a success callback.");
            }

            return new LoginViewModel(
                CreateAuthService(),
                CreateAuthorizationService(),
                CreateAdminSetupService(),
                CreateAttendanceService(),
                CreateUserPreferenceService(),
                onLoginSuccess);
        }

        private IAuthService CreateAuthService() => new AuthService(_dbFactory());
        private IAuthorizationService CreateAuthorizationService() => new AuthorizationService(_dbFactory());
        private IAdminSetupService CreateAdminSetupService() => new AdminSetupService(_dbFactory());
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
        private IDeliveryService CreateDeliveryService() => new DeliveryService(_dbFactory());
        private ISampleCollectionService CreateSampleCollectionService() => new SampleCollectionService(_dbFactory());
        private ICultureSensitivityService CreateCultureSensitivityService() => new CultureSensitivityService(_dbFactory());
        private IReceiptService CreateReceiptService() => new ReceiptService(_dbFactory());
        private IConstantsService CreateConstantsService() => new ConstantsService(_dbFactory());
        private IUserPreferenceService CreateUserPreferenceService() => new UserPreferenceService();
        private IPrintService CreatePrintService() => new PrintService();
        private IBarcodeService CreateBarcodeService() => new BarcodeService();
    }
}
