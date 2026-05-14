# 08 — UI Window Contracts — Open lab system (Final Independent Audit)

> عقود التطبيق لكل شاشة. كل اسم Command و Property مأخوذ **حرفياً** من ViewModels الفعلية في فرع `Fi5ve` كوميت `011f15c`.

---

## القاعدة الإلزامية

كل XAML View يجب أن:
1. يطابق `x:Class` المساحة المذكورة هنا حرفياً
2. لا يستخدم أي Binding غير مُدرج هنا
3. لا يفترض وجود Property غير موجودة في الـ ViewModel المذكور

---

## 0.1 LoginView Contract

- **نوع XAML:** `UserControl`
- **مساحة الاسم:** `Open_lab.Views`
- **x:Class:** `Open_lab.Views.LoginView`
- **ملف XAML:** `Open_lab/Views/LoginView.xaml`
- **ملف Code-behind:** `Open_lab/Views/LoginView.xaml.cs`
- **ViewModel:** `Open_lab.ViewModels.LoginViewModel`
- **DataContext:** يُحقن من `App.OnStartup.ShowLoginWindow` عبر `ActivatorUtilities.CreateInstance<LoginViewModel>(_serviceProvider, Action onLoginSuccess)`
- **Constructor:** `LoginViewModel(IAuthService, IAuthorizationService, IAdminSetupService, IAttendanceService, IUserPreferenceService, Action onLoginSuccess)`

### Properties

| Name | Type | Mode | Default |
|---|---|---|---|
| Username | string | TwoWay | string.Empty (أو remembered) |
| Password | string | TwoWay | string.Empty |
| RememberMe | bool | TwoWay | false (أو true لو وُجد remembered) |
| StatusMessage | string | OneWay (private set) | string.Empty |
| IsPasswordVisible | bool | TwoWay | false |
| IsBusy | bool | OneWay (private set) | false |

### Commands

| Name | Type | CanExecute |
|---|---|---|
| LoginCommand | ICommand (RelayCommand async) | `_ => !IsBusy` |
| TogglePasswordVisibilityCommand | ICommand (RelayCommand) | — (دائماً true) |

### Resources / Converters المطلوبة

- `BoolToVis` — BooleanToVisibilityConverter (محلية في UserControl.Resources)
- `ModernInputField` Style (محلية)
- `LoginButtonStyle` Style (محلية)
- `EyeButtonStyle` Style (محلية)
- `behaviors:PasswordBoxAssistant.BindPassword` + `BoundPassword` (من `Open_lab.Behaviors.PasswordBoxAssistant`)

### Bindings الحرفية المسموحة في XAML
- `{Binding Username, UpdateSourceTrigger=PropertyChanged}` ⇒ TextBox
- `{Binding Password, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}` ⇒ via PasswordBoxAssistant.BoundPassword
- `{Binding Password, UpdateSourceTrigger=PropertyChanged}` ⇒ TextBox (overlay)
- `{Binding IsPasswordVisible}` ⇒ DataTrigger (Style)
- `{Binding RememberMe}` ⇒ CheckBox.IsChecked
- `{Binding StatusMessage}` ⇒ TextBlock.Text
- `{Binding LoginCommand}` ⇒ Button.Command
- `{Binding TogglePasswordVisibilityCommand}` ⇒ Button.Command

---

## 0.2 MainWindow Contract

- **نوع XAML:** `Window`
- **مساحة الاسم:** `Open_lab.Views`
- **x:Class:** `Open_lab.Views.MainWindow`
- **ملف XAML:** `Open_lab/Views/MainWindow.xaml`
- **ملف Code-behind:** `Open_lab/Views/MainWindow.xaml.cs`
- **ViewModel:** `Open_lab.ViewModels.MainViewModel`
- **DataContext:** يُحقن من `App.OpenMainWindowAfterLogin`
- **Constructor:** `MainViewModel(INavigationService, IAttendanceService, IMainWindowLayoutService)`

### Properties Public في MainViewModel

| Name | Type | Mode |
|---|---|---|
| CurrentView | object | OneWay (private set) |
| CurrentViewModel | BaseViewModel | OneWay readonly (proxy) |
| CurrentUser | string | OneWay (private set) |
| LastLoginDate | string | OneWay (private set) |
| CurrentDate | string | OneWay (private set) |
| ActiveModule | string | OneWay (private set) |
| IsToolbarVisible | bool | OneWay (private set) |
| IsLoggedIn | bool | OneWay (private set) |

### Commands (50 إجمالاً) — الأنواع كلها ICommand

#### Top Navigation (3 + 8 + 1 = 12 Commands)
- NavigateToPatientsCommand
- NavigateToToolsCommand
- NavigateToWorksheetCommand
- NavigateToAccountsCommand
- NavigateToStatisticsCommand
- NavigateToUsersCommand
- NavigateToSystemDataCommand
- NavigateToSettingsCommand
- NavigateToEmployeesCommand (Welcome with "الموظفين")
- NavigateToDidYouKnowCommand (Welcome with "هل تعلم")
- NavigateToAboutCommand (Welcome with "نبذة")
- LogoutCommand (CanExecute = IsLoggedIn)

#### NavigationTarget Commands (38 Commands)
NavigateDashboardCommand, NavigatePatientRegistrationCommand, NavigatePatientTestsCommand, NavigatePatientBillingCommand, NavigatePatientBillingByDateCommand, NavigateResultsEntryCommand, NavigateReportViewerCommand, NavigatePatientSearchCommand, NavigatePatientHistoryCommand, NavigateWorkSheetByPatientCommand, NavigateWorkSheetByTestCommand, NavigateTestCatalogCommand, NavigateReferenceRangesCommand, NavigateTestCommentsCommand, NavigatePriceListsCommand, NavigateCustomGroupsCommand, NavigateReferralsCommand, NavigateUsersPermissionsCommand, NavigateStatisticsCommand, NavigateSystemSettingsCommand, NavigateBackupRestoreCommand, NavigateAttendanceLogCommand, NavigateAccountsTreasuryCommand, NavigateDeliveryCommand, NavigateSampleCollectionCommand, NavigateCultureSensitivityCommand, NavigateReceiptPrintingCommand, NavigateCombinedReportCommand, NavigateBlankReportCommand, NavigateConstantsCommand, NavigateCompareWithHistoryCommand, NavigateGroupWorksheetCommand, NavigateTestClassificationLogCommand, NavigateExternalLabManagementCommand, NavigateAttendanceReportCommand, NavigateContractInvoiceCommand, NavigateUserActivityLogCommand, NavigateSystemUsageMonitorCommand

### Methods Public
- `InitializeAfterLogin(string currentUser, string lastLoginDate, Action? onLogoutRequested = null)`

### Resources المعرَّفة محلياً في MainWindow.xaml
- `TopToolbarBrush` (LinearGradientBrush)
- `ToolbarRadioButtonStyle`
- `ToolbarExitButtonStyle`
- `ToolbarIconStyle`
- `ToolbarTextStyle`

### Bindings الحرفية المسموحة
- `{Binding IsToolbarVisible, Converter={StaticResource BoolToVisibility}}` ⇒ Border Visibility
- `{Binding CurrentView}` ⇒ ContentControl.Content
- `{Binding CurrentUser}`, `{Binding LastLoginDate}`, `{Binding CurrentDate}` ⇒ TextBlock.Text
- 12 Bindings لـ Commands الـ Toolbar

---

## 0.3 WelcomeView Contract

- **نوع XAML:** `UserControl`
- **مساحة الاسم:** `Open_lab.Views`
- **x:Class:** `Open_lab.Views.WelcomeView`
- **ViewModel:** `Open_lab.ViewModels.WelcomeViewModel`
- **Constructor:** `WelcomeViewModel()` أو `WelcomeViewModel(string moduleName)`

### Properties
- `ModuleName : string` (get only — readonly)

### Commands
- لا توجد

### Bindings
- لا Bindings فعّالة في XAML الحالي (كل النصوص ثابتة)

---

## 0.4 PlaceholderView Contract (Anti-Pattern)

- **نوع XAML:** `UserControl`
- **مساحة الاسم:** `Open_lab.Views.Shared`
- **x:Class:** `Open_lab.Views.Shared.PlaceholderView`
- **ViewModel:** ❌ لا يوجد
- **DataContext:** `this` (في constructor)
- **Constructor:** `PlaceholderView(string functionTitle, ICommand? backCommand)` أو `PlaceholderView()` (default "وظيفة مؤقتة")

### Properties (في code-behind)
- `FunctionTitle : string` (get only)
- `BackCommand : ICommand?` (get only)

### Bindings
- `{Binding FunctionTitle}` ⇒ TextBlock.Text
- `{Binding BackCommand}` ⇒ Button.Command

---

## 1.1 PatientModuleView Contract

- **x:Class:** `Open_lab.Views.Patients.PatientModuleView`
- **ViewModel:** `Open_lab.ViewModels.Patients.PatientModuleViewModel`
- **Constructor:** `PatientModuleViewModel(Action<string> openPlaceholder)`

### Properties
- لا توجد

### Commands (4)
- OpenAddPatientCommand
- OpenEnterResultsCommand
- OpenDeliverResultsCommand
- OpenSearchPatientCommand

---

## 2.1 SystemDataModuleView Contract

- **x:Class:** `Open_lab.Views.SystemData.SystemDataModuleView`
- **ViewModel:** `Open_lab.ViewModels.SystemData.SystemDataModuleViewModel`
- **Constructor:** `SystemDataModuleViewModel(Action<string> openPlaceholder)`

### Commands (14)
OpenTestDataCommand, OpenBarcodeTypesCommand, OpenCultureAntibioticsCommand, OpenTestGroupsCommand, OpenTestUnitsCommand, OpenTestCommentsCommand, OpenLabBranchesCommand, OpenPatientTitlesCommand, OpenCustomGroupsCommand, OpenWorkGroupsLogCommand, OpenPrintPriceListCommand, OpenLabEquipmentCommand, OpenExternalEntitiesCommand, OpenExternalPriceListsCommand

---

## 3.1 AccountsModuleView Contract

- **x:Class:** `Open_lab.Views.Accounts.AccountsModuleView`
- **ViewModel:** `Open_lab.ViewModels.Accounts.AccountsModuleViewModel`
- **Constructor:** `AccountsModuleViewModel(Action<string> openPlaceholder)`

### Commands (4)
OpenInventoryCommand, OpenExternalSamplesCommand, OpenCashTransactionCommand, OpenCompanyAccountsCommand

---

## 4.1 WorksheetModuleView Contract

- **x:Class:** `Open_lab.Views.Worksheet.WorksheetModuleView`
- **ViewModel:** `Open_lab.ViewModels.Worksheet.WorksheetModuleViewModel`

### Commands (2)
OpenWorksheetByPatientCommand, OpenWorksheetByTestCommand

---

## 5.1 StatisticsModuleView Contract

- **x:Class:** `Open_lab.Views.Statistics.StatisticsModuleView`
- **ViewModel:** `Open_lab.ViewModels.Statistics.StatisticsModuleViewModel`

### Commands (6)
OpenPatientCountStatCommand, OpenTestCountStatCommand, OpenBranchStatCommand, OpenExternalSamplesStatCommand, OpenWorkPerformanceStatCommand, OpenResultsMonitorCommand

---

## 6.1 SettingsModuleView Contract

- **x:Class:** `Open_lab.Views.Settings.SettingsModuleView`
- **ViewModel:** `Open_lab.ViewModels.Settings.SettingsModuleViewModel`

### Commands (2)
OpenSystemSettingsCommand, OpenDatabaseMaintenanceCommand

---

## 7.1 ToolsModuleView Contract

- **x:Class:** `Open_lab.Views.Tools.ToolsModuleView`
- **ViewModel:** `Open_lab.ViewModels.Tools.ToolsModuleViewModel`

### Commands (9)
OpenTestLibraryCommand, OpenStopwatchCommand, OpenRequirementsListCommand, OpenImageLibraryCommand, OpenUnitConverterCommand, OpenAppointmentsCommand, OpenAbbreviationsDictionaryCommand, OpenCalculatorCommand, OpenPhoneDirectoryCommand

---

## 8.1 UsersModuleView Contract

- **x:Class:** `Open_lab.Views.Users.UsersModuleView`
- **ViewModel:** `Open_lab.ViewModels.Users.UsersModuleViewModel`

### Commands (4)
OpenCreateUsersCommand, OpenChangePasswordCommand, OpenAttendanceCommand, OpenLoginDetectorCommand

---

## ViewModels بدون View — العقود الجاهزة (للتنفيذ المستقبلي)

### PatientRegistrationViewModel Contract
- **مسار:** `Open_lab/ViewModels/PatientRegistrationViewModel.cs` (383 سطر)
- **Constructors:**
  - `PatientRegistrationViewModel(IPatientService)`
  - `PatientRegistrationViewModel(IPatientService, ITestCatalogService?)`
- **Properties:** PatientId(int), LabId(string), FullName(string), Gender(string), BirthDate(DateTime?), Phone(string?), Address(string?), ChronicDiseases(string?), Allergies(string?), Medications(string?), MedicalNotes(string?), StatusMessage(string), SelectedPatient(Patient?), SelectedReferral(Referral?), Results(ObservableCollection&lt;Patient&gt;), Referrals(ObservableCollection&lt;Referral&gt;)
- **Commands:** SaveCommand, NewCommand, GenerateLabIdCommand, LoadByLabIdCommand, SearchCommand, DeleteCommand
- **Permission Codes المستخدمة:** PatientsView, PatientsEdit
- **Async Init:** `_ = InitializeAsync()` في constructor (يحمّل Referrals + يولد LabId)

### PatientTestsSelectionViewModel Contract
- **مسار:** `Open_lab/ViewModels/PatientTestsSelectionViewModel.cs` (428 سطر)
- **Constructor:** `(IPatientService, IVisitService, ITestCatalogService, IInvoiceService)`
- **Properties:** LabId, PatientName, PatientId, VisitId, StatusMessage, SelectedAvailableTest, SelectedVisitTest, SelectedAccountType ("Cash"/"Referral"), SelectedReferral, SearchText, SelectedCustomGroup, TotalAmount, AvailableTests, FilteredAvailableTests, SelectedTests, Referrals, AccountTypes, CustomGroups
- **Commands:** LoadPatientCommand, CreateVisitCommand, AddTestCommand, AddCustomGroupCommand, RemoveTestCommand, RefreshTestsCommand
- **Permission Codes:** PatientsView, VisitsEdit, TestsView

### PatientBillingViewModel Contract
- **مسار:** `Open_lab/ViewModels/PatientBillingViewModel.cs` (413 سطر)
- **Constructor:** `(IInvoiceService)`
- **Properties:** VisitId, Total, Discount, Paid, EditPaymentAmount, NetTotal, Balance, NewChargeAmount, NewChargeDescription, StatusMessage, Reason, PaymentMethod, SelectedPayment, Payments, AdditionalCharges
- **Commands:** LoadVisitCommand, SaveInvoiceCommand, AddPaymentCommand, EditPaymentCommand, DeletePaymentCommand, AddChargeCommand, SettleAccountCommand, PrintInvoiceCommand
- **Permission Codes:** AccountsView, AccountsEdit

### PatientBillingByDateViewModel Contract
- **مسار:** `Open_lab/ViewModels/PatientBillingByDateViewModel.cs` (109 سطر)
- **Constructor:** `(IInvoiceService)`
- **Properties:** PatientId, FromDate, ToDate, TotalInvoiced, TotalPaid, Balance, IsBusy, Invoices, Payments
- **Commands:** SearchCommand

### ResultsEntryViewModel Contract
- **مسار:** `Open_lab/ViewModels/ResultsEntryViewModel.cs` (302 سطر)
- **Constructors:**
  - `(IResultsService)`
  - `(IResultsService, IPatientService?)`
- **Properties:** DateFrom, DateTo, SelectedVisitTest, StatusMessage, MedicalHistorySummary, HasMedicalAlerts, VisitTests, ResultItems
- **Commands:** LoadVisitTestsCommand, SaveResultsCommand, VerifyResultsCommand, ReopenResultsCommand
- **Permission Codes:** ResultsView, ResultsEdit

### ReportViewerViewModel Contract
- **مسار:** `Open_lab/ViewModels/ReportViewerViewModel.cs` (148 سطر)
- **Constructors:**
  - `(IReportService, IPrintService, IResultsService)`
  - `(IReportService, IPrintService, IResultsService, IReportPdfService?)`
- **Properties:** VisitId, StatusMessage, Report, PreviewPdfPath, PreviewContent (computed), PreviewPdfUri, Tests
- **Commands:** LoadReportCommand, PrintCommand, ReprintCommand

### PatientSearchViewModel Contract
- **مسار:** `Open_lab/ViewModels/PatientSearchViewModel.cs` (117 سطر)
- **Constructor:** `(IPatientSearchService)`
- **Properties:** Name, Phone, LabId, Date, StatusMessage, SelectedPatient, Patients, Visits
- **Commands:** SearchCommand

### PatientHistoryViewModel Contract
- **مسار:** `Open_lab/ViewModels/PatientHistoryViewModel.cs` (124 سطر)
- **Constructor:** `(IPatientService, IReportService, IPrintService)`
- **Properties:** LabId, From, To, StatusMessage, History, Visits
- **Commands:** LoadHistoryCommand, PrintHistoryCommand

### DashboardViewModel Contract
- **مسار:** `Open_lab/ViewModels/DashboardViewModel.cs` (45 سطر)
- **Constructor:** `(IDashboardService)`
- **Properties:** PatientCount, VisitCount, TestCount
- **Commands:** لا توجد
- **Auto-Load:** `_ = LoadAsync()` في constructor

### TestCatalogViewModel Contract
- **مسار:** `Open_lab/ViewModels/TestCatalogViewModel.cs` (348 سطر)
- **Properties:** Tests, Groups, SampleTypes, Units
- **Commands:** SaveCommand, NewCommand, DeleteCommand, ReloadCommand, GenerateBarcodeCommand

### ReferenceRangesViewModel Contract
- **Properties:** Tests, Ranges
- **Commands:** LoadCommand, SaveCommand, DeleteCommand

### TestCommentsViewModel Contract
- **Properties:** Tests, Comments
- **Commands:** LoadCommand, SaveCommand, DeleteCommand

### PriceListsViewModel Contract
- **Properties:** PriceLists, Items, Tests, Referrals
- **Commands:** LoadCommand, SaveListCommand, UpdateListCommand, AddItemCommand, UpdateItemCommand, DeleteItemCommand, PrintListCommand

### CustomGroupsViewModel Contract
- **Properties:** Groups, GroupItems, Tests
- **Commands:** SaveGroupCommand, AddItemCommand, DeleteItemCommand

### ReferralsViewModel Contract
- **Properties:** Referrals
- **Commands:** SaveCommand, DeleteCommand

### UsersPermissionsViewModel Contract
- **Properties:** Users, Roles, Permissions
- **Commands:** SaveUserCommand, DeleteUserCommand, SaveRoleCommand, DeleteRoleCommand, SaveRolePermissionsCommand, AssignRoleCommand, UnassignRoleCommand, ReloadCommand

### StatisticsViewModel Contract
- **Properties:** Genders, Referrals, ByGender, ByReferral, MonthlyAnalysis, TopTests, YearlySamples, UserProductivity
- **Commands:** LoadCommand, PrintCommand

### SystemSettingsViewModel Contract
- **Properties:** Settings
- **Commands:** SaveProfileCommand, ReloadCommand, SaveRawSettingCommand, DeleteRawSettingCommand, ChangeMasterPasswordCommand

### BackupRestoreViewModel Contract
- **Properties:** BackupFiles
- **Commands:** BackupCommand, RestoreCommand, LoadBackupsCommand, ConfigureScheduleCommand, DisableScheduleCommand, RefreshScheduleCommand

### AttendanceLogViewModel Contract
- **Properties:** Logs, Breaks
- **Commands:** LoadLogsCommand, ClockInCommand, ClockOutCommand, StartBreakCommand, EndBreakCommand, LoadDailySummaryCommand, RefreshOpenLogCommand

### AccountsTreasuryViewModel Contract
- **Properties:** Payments, ByUser, ByReferral, ByBranch, ByDoctor
- **Commands:** LoadCommand, DailyCommand, WeeklyCommand, MonthlyCommand, PrintCommand

### DeliveryViewModel Contract
- **Properties:** Visits
- **Commands:** SearchCommand, DeliverCommand, ReopenCommand

### SampleCollectionViewModel Contract
- **Properties:** Items
- **Commands:** LoadCommand, MarkCollectedCommand, MarkExternalCollectedCommand, MarkSeparatedCommand, MarkNotCollectedCommand, RefreshSampleStatusCommand

### CultureSensitivityViewModel Contract (الأكبر — 548 سطر)
- **Properties:** Cultures, Antibiotics, LinkedAntibiotics, VisitTests, ResultRows, AllowedSensitivities
- **Commands:** LoadCommand, AddCultureCommand, DeleteCultureCommand, AddAntibioticCommand, DeleteAntibioticCommand, LinkCommand, UnlinkCommand, LoadVisitTestsCommand, SaveResultCommand, PrintCultureReportCommand

### ReceiptPrintingViewModel Contract
- **Properties:** TestItems
- **Commands:** LoadCommand, PrintReceiptCommand, PrintBarcodeCommand

### CombinedReportViewModel Contract
- **Properties:** Tests
- **Commands:** LoadCommand, MoveUpCommand, MoveDownCommand, SaveReportOrderCommand

### BlankReportViewModel Contract
- **Commands:** LoadCommand, PrintBlankCommand

### ConstantsViewModel Contract
- **Properties:** Items
- **Commands:** SeedDefaultsCommand, ReloadCommand, SaveCommand, DeleteCommand

### CompareWithHistoryViewModel Contract
- **Properties:** Tests, HistoryResults
- **Commands:** LoadPatientCommand, LoadHistoryCommand, ClearCommand

### GroupWorksheetViewModel Contract
- **Properties:** Groups, CustomGroups, Rows
- **Commands:** LoadGroupsCommand, LoadWorksheetCommand, PrintCommand

### TestClassificationLogViewModel Contract
- **Properties:** Items
- **Commands:** LoadCommand, PrintCommand

### ExternalLabManagementViewModel Contract
- **Properties:** Referrals, PendingQueue, Manifests, SettlementHistory, SelectedQueueIds
- **Commands:** LoadReferralsCommand, LoadQueueCommand, LoadManifestsCommand, CreateManifestCommand, UpdateStatusCommand, LoadSettlementCommand, CreateSettlementCommand, EnterExternalResultCommand, PrintExternalReportCommand

### AttendanceReportViewModel Contract
- **Properties:** Users, ReportRows, PayrollRows
- **Commands:** LoadUsersCommand, GenerateReportCommand, GeneratePayrollSummaryCommand, ClearFilterCommand

### ContractInvoiceViewModel Contract
- **Properties:** Referrals, PendingInvoices, ContractInvoices
- **Commands:** LoadReferralsCommand, LoadPendingCommand, CreateInvoiceCommand, LoadHistoryCommand, SettleSelectedCommand

### UserActivityLogViewModel Contract
- **Properties:** Items
- **Commands:** LoadCommand

### SystemUsageMonitorViewModel Contract
- **Properties:** Sessions
- **Commands:** RefreshCommand

### SettingsViewModel Contract
- **Properties:** Settings, AvailablePrinters
- **Commands:** LoadSettingsCommand, SavePrinterSettingsCommand, SaveMarginSettingsCommand, RefreshPrintersCommand

### PhysicianViewModel Contract (لا NavigationTarget)
- **Properties:** Physicians, PriceLists
- **Commands:** LoadPhysiciansCommand, SaveCommand, NewCommand, SearchCommand

---

## ShellWindow.xaml Contract (Dead Code)

- **x:Class:** `Open_lab.Shell.ShellWindow`
- **ViewModel:** يتوقع MainViewModel (يستخدم نفس Bindings) لكن **غير منشأ في أي مكان**
- **ContentControl Content Binding:** `CurrentViewModel` (ليس CurrentView كما في MainWindow) — هذا تعارض
- **Resources المطلوبة:** `BoolToVisibility` — لكن غير مُعرَّفة في ShellWindow.Resources، تعتمد على Application.Resources

---

## Resources & Converters Inventory (الإجمالي)

### في App.xaml (Application.Resources)
- `BoolToVisibility` (System BooleanToVisibilityConverter)
- `InverseBooleanConverter` (Custom)
- `InverseBoolToVisibilityConverter` (Custom)
- 11 DataTemplates (LoginViewModel, MainViewModel, WelcomeViewModel + 8 ModuleViewModels)

### في كل XAML محلياً
- LoginView: 4 Styles + BoolToVis converter
- MainWindow: 5 Styles + 1 Brush
- 8 ModuleView XAML: 1 Style لكل (مكرر 8 مرات)
- PlaceholderView: 0 Resources
- WelcomeView: 0 Resources
- ShellWindow: 0 Resources

---

## Behaviors الـ Attached

### PasswordBoxAssistant (`Open_lab.Behaviors.PasswordBoxAssistant`)
- **AttachedProperties:**
  - `BoundPassword : string` (TwoWay default)
  - `BindPassword : bool` (toggle to attach handler)
  - `IsUpdating : bool` (internal)
- **الاستخدام:** فقط في LoginView على PasswordBox

---

## ملخص العقود الكامل

| الـ View | XAML موجود | x:Class | ViewModel | DataTemplate في App.xaml |
|---|:---:|---|---|:---:|
| LoginView | ✅ | Open_lab.Views.LoginView | LoginViewModel | ✅ |
| MainWindow | ✅ | Open_lab.Views.MainWindow | MainViewModel | ✅ |
| WelcomeView | ✅ | Open_lab.Views.WelcomeView | WelcomeViewModel | ✅ |
| PlaceholderView | ✅ | Open_lab.Views.Shared.PlaceholderView | — (self DataContext) | ❌ (يُنشأ مباشرة) |
| PatientModuleView | ✅ | Open_lab.Views.Patients.PatientModuleView | Patients.PatientModuleViewModel | ✅ |
| SystemDataModuleView | ✅ | Open_lab.Views.SystemData.SystemDataModuleView | SystemData.SystemDataModuleViewModel | ✅ |
| AccountsModuleView | ✅ | Open_lab.Views.Accounts.AccountsModuleView | Accounts.AccountsModuleViewModel | ✅ |
| WorksheetModuleView | ✅ | Open_lab.Views.Worksheet.WorksheetModuleView | Worksheet.WorksheetModuleViewModel | ✅ |
| StatisticsModuleView | ✅ | Open_lab.Views.Statistics.StatisticsModuleView | Statistics.StatisticsModuleViewModel | ✅ |
| SettingsModuleView | ✅ | Open_lab.Views.Settings.SettingsModuleView | Settings.SettingsModuleViewModel | ✅ |
| ToolsModuleView | ✅ | Open_lab.Views.Tools.ToolsModuleView | Tools.ToolsModuleViewModel | ✅ |
| UsersModuleView | ✅ | Open_lab.Views.Users.UsersModuleView | Users.UsersModuleViewModel | ✅ |
| ShellWindow (DEAD) | ✅ | Open_lab.Shell.ShellWindow | (يتوقع MainViewModel) | ❌ غير مستخدم |
| ~37 Sub-VM | ❌ | — | (كل واحد بمساحة ViewModels) | ❌ |

---
