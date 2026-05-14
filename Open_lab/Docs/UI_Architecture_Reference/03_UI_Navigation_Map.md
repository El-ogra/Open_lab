# 03 — UI Navigation Map — Open lab system (Final Independent Audit)

> خريطة التنقل الكاملة المُحقَّقة من الكود الفعلي. الكوميت: `011f15c`.

---

## 1) النمط المعماري للتنقل (المؤكَّد من الكود)

### المبدأ الأساسي
- **حاوية المحتوى الوحيدة:** `<ContentControl Content="{Binding CurrentView}" />` في `MainWindow.xaml` السطر 190 (داخل Border في Grid.Row=1)
- **آلية التطابق:** WPF يبحث عن `DataTemplate` المسجَّلة في `App.xaml` تحت `Application.Resources` بناءً على نوع `CurrentView`
- **عدد DataTemplates:** **11 فقط** في App.xaml
- **مصدر `CurrentView`:** Property في `MainViewModel`، تُحدَّث برمجياً من ميثودز خاصة في نفس الفئة

### الطبقات المعمارية للتنقل
1. **MainViewModel.CurrentView** ← يُحدَّث من `NavigateToXxxModule()` / `OpenPlaceholder()` / `NavigateTopModule()`
2. **NavigationService.CurrentViewModel** ← Property مستقلة عن CurrentView، تُحدَّث من `NavigationService.Navigate(target)` الذي ينشئ ViewModel عبر `IViewModelFactory`
3. **ViewModelFactory.Create(target, onLoginSuccess?)** ← switch على `NavigationTarget` يُرجع `BaseViewModel`

### ملاحظة حرجة جداً (مكتشفة بالفحص)
**Disconnect بين الطبقتين:**
- `MainViewModel.CurrentView` ⇒ تستخدم لعرض WelcomeView و ModuleView (PatientModule, AccountsModule, ...) و PlaceholderView
- `NavigationService.CurrentViewModel` ⇒ تُحدَّث بـ ViewModelFactory عند استدعاء `NavigateTo(target)` لكنها **لا تربط بـ CurrentView**
- النتيجة: عند ضغط زر `NavigatePatientRegistrationCommand` (مثلاً)، تُنشأ `PatientRegistrationViewModel` لكنها تذهب لـ `NavigationService.CurrentViewModel`، بينما `MainViewModel.CurrentView` يبقى كما هو
- `MainViewModel.CurrentViewModel` يعرَّف كـ `_navigationService.CurrentViewModel` (مجرد proxy للقراءة) لكن هذه Property غير مربوطة بأي شيء في XAML

### ما هو غير مستخدم في الكود (مؤكد بالـ grep)
- ❌ `Frame` / `NavigationWindow` — لا توجد إطلاقاً
- ❌ `Window.Show()` / `Window.ShowDialog()` خارج App.xaml.cs
- ❌ `Prism Region` / `IRegionManager`
- ❌ أي حاوي IoC خارجي (Caliburn / ReactiveUI / MVVMLight)
- ❌ `KeyBinding` / `InputBindings` — 0 نتائج في الكود (لا اختصارات لوحة مفاتيح فعلية)
- ❌ `Window.Close()` خارج App.xaml.cs

---

## 2) NavigationTarget Enum — 40 قيمة

ملف: `Open_lab/ViewModels/NavigationTarget.cs`

```csharp
namespace Open_lab.ViewModels
{
    public enum NavigationTarget
    {
        Home,                       // 0
        Login,                      // 1
        Dashboard,                  // 2
        PatientRegistration,        // 3
        PatientTestsSelection,      // 4
        PatientBilling,             // 5
        PatientBillingByDate,       // 6
        ResultsEntry,               // 7
        ReportViewer,               // 8
        PatientSearch,              // 9
        PatientHistory,             // 10
        WorkSheetByPatient,         // 11
        WorkSheetByTest,            // 12
        TestCatalog,                // 13
        ReferenceRanges,            // 14
        TestComments,               // 15
        PriceLists,                 // 16
        CustomGroups,               // 17
        Referrals,                  // 18
        UsersPermissions,           // 19
        Statistics,                 // 20
        SystemSettings,             // 21
        BackupRestore,              // 22
        AttendanceLog,              // 23
        AccountsTreasury,           // 24
        Delivery,                   // 25
        SampleCollection,           // 26
        CultureSensitivity,         // 27
        ReceiptPrinting,            // 28
        CombinedReport,             // 29
        BlankReport,                // 30
        Constants,                  // 31
        CompareWithHistory,         // 32
        GroupWorksheet,             // 33
        TestClassificationLog,      // 34
        ExternalLabManagement,      // 35
        AttendanceReport,           // 36
        ContractInvoice,            // 37
        UserActivityLog,            // 38
        SystemUsageMonitor          // 39
    }
}
```

**العدد الإجمالي:** 40 قيمة (محسوبة بـ `grep -E "^\s+[A-Z]" NavigationTarget.cs | wc -l`)

---

## 3) ViewModelFactory.Create — جدول التحويل الكامل

ملف: `Open_lab/ViewModels/ViewModelFactory.cs`

| NavigationTarget | ViewModel المُنشأ | طريقة الإنشاء |
|---|---|---|
| Home | HomeViewModel | CreateViewModel\<T\>() عبر ActivatorUtilities |
| Login | LoginViewModel | CreateLoginViewModel(onLoginSuccess) — يستخدم ActivatorUtilities + onLoginSuccess (Action) |
| Dashboard | DashboardViewModel | DI |
| PatientRegistration | PatientRegistrationViewModel | DI |
| PatientTestsSelection | PatientTestsSelectionViewModel | DI |
| PatientBilling | PatientBillingViewModel | DI |
| PatientBillingByDate | PatientBillingByDateViewModel | DI |
| ResultsEntry | ResultsEntryViewModel | DI |
| ReportViewer | ReportViewerViewModel | DI |
| PatientSearch | PatientSearchViewModel | DI |
| PatientHistory | PatientHistoryViewModel | DI |
| WorkSheetByPatient | WorkSheetByPatientViewModel | DI |
| WorkSheetByTest | WorkSheetByTestViewModel | DI |
| TestCatalog | TestCatalogViewModel | DI |
| ReferenceRanges | ReferenceRangesViewModel | DI |
| TestComments | TestCommentsViewModel | DI |
| PriceLists | PriceListsViewModel | DI |
| CustomGroups | CustomGroupsViewModel | DI |
| Referrals | ReferralsViewModel | DI |
| UsersPermissions | UsersPermissionsViewModel | DI |
| Statistics | StatisticsViewModel | DI |
| SystemSettings | SystemSettingsViewModel | DI |
| BackupRestore | BackupRestoreViewModel | DI |
| AttendanceLog | AttendanceLogViewModel | DI |
| AccountsTreasury | AccountsTreasuryViewModel | DI |
| Delivery | DeliveryViewModel | DI |
| SampleCollection | SampleCollectionViewModel | DI |
| CultureSensitivity | CultureSensitivityViewModel | DI |
| ReceiptPrinting | ReceiptPrintingViewModel | DI |
| CombinedReport | CombinedReportViewModel | DI |
| BlankReport | BlankReportViewModel | DI |
| Constants | ConstantsViewModel | DI |
| CompareWithHistory | CompareWithHistoryViewModel | DI |
| GroupWorksheet | GroupWorksheetViewModel | DI |
| TestClassificationLog | TestClassificationLogViewModel | DI |
| ExternalLabManagement | ExternalLabManagementViewModel | DI |
| AttendanceReport | AttendanceReportViewModel | DI |
| ContractInvoice | ContractInvoiceViewModel | DI |
| UserActivityLog | UserActivityLogViewModel | DI |
| SystemUsageMonitor | SystemUsageMonitorViewModel | DI |

**ملاحظة:** كل ViewModel من هذه الـ 40 ينشأ، **لكن** لا توجد DataTemplate في App.xaml لـ 37 منها (الـ 3 الموجودة فقط: Login, Main, Welcome).

---

## 4) MainViewModel.Commands — 50 Command كاملة

ملف: `Open_lab/ViewModels/MainViewModel.cs` (464 سطر)

محسوبة بـ `grep -cE "public (ICommand|RelayCommand)" MainViewModel.cs` = **50**

### 4.1 Commands الخاصة بـ NavigationTarget (38 Command)
كلها تستدعي `NavigateTo(NavigationTarget.Xxx)` ⇒ `_navigationService.Navigate(target)` ⇒ تنشئ ViewModel جديدة لكن لا تظهر في UI (لا DataTemplate).

| Command | NavigationTarget | CanExecute (Permission) |
|---|---|---|
| NavigateDashboardCommand | Dashboard | IsLoggedIn |
| NavigatePatientRegistrationCommand | PatientRegistration | PatientsView |
| NavigatePatientTestsCommand | PatientTestsSelection | VisitsView |
| NavigatePatientBillingCommand | PatientBilling | AccountsView |
| NavigatePatientBillingByDateCommand | PatientBillingByDate | AccountsView |
| NavigateResultsEntryCommand | ResultsEntry | ResultsView |
| NavigateReportViewerCommand | ReportViewer | ReportsView |
| NavigatePatientSearchCommand | PatientSearch | PatientsView |
| NavigatePatientHistoryCommand | PatientHistory | PatientsView |
| NavigateWorkSheetByPatientCommand | WorkSheetByPatient | TestsView |
| NavigateWorkSheetByTestCommand | WorkSheetByTest | TestsView |
| NavigateTestCatalogCommand | TestCatalog | TestsEdit |
| NavigateReferenceRangesCommand | ReferenceRanges | TestsEdit |
| NavigateTestCommentsCommand | TestComments | TestsEdit |
| NavigatePriceListsCommand | PriceLists | TestsEdit |
| NavigateCustomGroupsCommand | CustomGroups | TestsEdit |
| NavigateReferralsCommand | Referrals | TestsEdit |
| NavigateUsersPermissionsCommand | UsersPermissions | UsersView |
| NavigateStatisticsCommand | Statistics | StatisticsView |
| NavigateSystemSettingsCommand | SystemSettings | SettingsView |
| NavigateBackupRestoreCommand | BackupRestore | BackupRestore |
| NavigateAttendanceLogCommand | AttendanceLog | UsersView |
| NavigateAccountsTreasuryCommand | AccountsTreasury | AccountsView |
| NavigateDeliveryCommand | Delivery | DeliveryView |
| NavigateSampleCollectionCommand | SampleCollection | TestsView |
| NavigateCultureSensitivityCommand | CultureSensitivity | TestsView |
| NavigateReceiptPrintingCommand | ReceiptPrinting | AccountsView |
| NavigateCombinedReportCommand | CombinedReport | ReportsView |
| NavigateBlankReportCommand | BlankReport | ReportsView |
| NavigateConstantsCommand | Constants | ConstantsView |
| NavigateCompareWithHistoryCommand | CompareWithHistory | ResultsView |
| NavigateGroupWorksheetCommand | GroupWorksheet | TestsView |
| NavigateTestClassificationLogCommand | TestClassificationLog | TestsView |
| NavigateExternalLabManagementCommand | ExternalLabManagement | TestsView |
| NavigateAttendanceReportCommand | AttendanceReport | UsersView |
| NavigateContractInvoiceCommand | ContractInvoice | AccountsView |
| NavigateUserActivityLogCommand | UserActivityLog | UsersView |
| NavigateSystemUsageMonitorCommand | SystemUsageMonitor | UsersView |

### 4.2 Commands الخاصة بفتح الـ Hubs (8 Commands)
كلها تستدعي `NavigateToXxxModule()` التي تضبط `CurrentView = new XxxModuleViewModel(OpenPlaceholder)`. هذه هي الـ Commands الفعّالة في الـ UI لأن DataTemplates للـ Hubs مسجَّلة.

| Command | الميثود | يضبط CurrentView إلى | ActiveModule |
|---|---|---|---|
| NavigateToPatientsCommand | NavigateToPatientsModule | new PatientModuleViewModel(OpenPlaceholder) | "المرضى" |
| NavigateToToolsCommand | NavigateToToolsModule | new ToolsModuleViewModel(OpenPlaceholder) | "أدوات" |
| NavigateToWorksheetCommand | NavigateToWorksheetModule | new WorksheetModuleViewModel(OpenPlaceholder) | "ورقة عمل" |
| NavigateToAccountsCommand | NavigateToAccountsModule | new AccountsModuleViewModel(OpenPlaceholder) | "حسابات" |
| NavigateToStatisticsCommand | NavigateToStatisticsModule | new StatisticsModuleViewModel(OpenPlaceholder) | "احصاليات" |
| NavigateToUsersCommand | NavigateToUsersModule | new UsersModuleViewModel(OpenPlaceholder) | "المستخدمين" |
| NavigateToSystemDataCommand | NavigateToSystemDataModule | new SystemDataModuleViewModel(OpenPlaceholder) | "بيانات النظام" |
| NavigateToSettingsCommand | NavigateToSettingsModule | new SettingsModuleViewModel(OpenPlaceholder) | "اعدادات" |

### 4.3 Commands «الوهمية» (3 Commands)
كلها تستدعي `NavigateTopModule(name)` التي تضبط `CurrentView = new WelcomeViewModel(moduleName)` فقط — أي يظهر شعار النظام مع ActiveModule المضبوط.

| Command | يضبط ActiveModule إلى |
|---|---|
| NavigateToEmployeesCommand | "الموظفين" |
| NavigateToDidYouKnowCommand | "هل تعلم" |
| NavigateToAboutCommand | "نبذة" |

### 4.4 LogoutCommand
- يستدعي `LogoutAsync` ⇒ `CloseAttendanceAsync` (إن وجد AttendanceLogId) ⇒ `_logoutRequested.Invoke()` ⇒ يعود إلى ShowLoginWindow
- `CanExecute = IsLoggedIn`

**المجموع: 38 + 8 + 3 + 1 = 50 ✓**

---

## 5) خريطة دفق التنقل الكاملة

```
App.OnStartup
    └─► ConfigureServices (DI Container)
    └─► ShowLoginWindow
            └─► new Window (400×550, NoResize, RTL, Title="تسجيل الدخول")
            │   ├─ Content = new LoginView()
            │   └─ DataContext = ActivatorUtilities.CreateInstance<LoginViewModel>(
            │                       _serviceProvider, 
            │                       new Action(() => OpenMainWindowAfterLogin(loginWindow)))
            └─► loginWindow.Show()
    
    [USER ENTERS CREDENTIALS]
    LoginViewModel.LoginCommand executes
        └─► _authService.ValidateCredentialsAsync(Username, Password)
        └─► if isAdmin: _adminSetupService.EnsureAdminAccessAsync
        └─► _authorizationService.GetPermissionCodesAsync
        └─► AppSession.UserId/Username/Permissions/IsAdmin set
        └─► _userPreferenceService.SetRememberedUsername (if RememberMe)
        └─► _attendanceService.CreateLoginAsync ⇒ AppSession.AttendanceLogId
        └─► _onLoginSuccess() — invokes OpenMainWindowAfterLogin
    
    App.OpenMainWindowAfterLogin
        └─► mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>()
        └─► new MainWindow { DataContext = mainViewModel }
        └─► mainViewModel.InitializeAfterLogin(username, lastLoginDate, onLogoutRequested)
        │     └─ CurrentUser, LastLoginDate, CurrentDate set
        │     └─ ActiveModule = ""
        │     └─ CurrentView = new WelcomeViewModel()
        │     └─ IsToolbarVisible = true
        │     └─ IsLoggedIn = true
        │     └─ _windowLayoutService.ApplyAppLayout() ⇒ Window 1100×700, CanResize
        └─► mainWindow.Show()
        └─► loginWindow.Close()
    
    [MainWindow displayed]
    ├─ Toolbar Row 0 (90px): 11 RadioButton + Logout Button
    ├─ Content Row 1 (*): ContentControl ⇒ WelcomeView (initial)
    └─ StatusBar Row 2 (30px): User info + DB status + Date
    
    [USER CLICKS A TOP RADIO]
    NavigateToPatientsCommand
        └─► NavigateToPatientsModule()
            └─ ActiveModule = "المرضى"
            └─ IsToolbarVisible = true
            └─ CurrentView = new PatientModuleViewModel(OpenPlaceholder)
        WPF reads CurrentView's type ⇒ matches DataTemplate ⇒ renders PatientModuleView
    
    [USER CLICKS A HUB BUTTON IN PatientModuleView]
    OpenAddPatientCommand (in PatientModuleViewModel)
        └─► _openPlaceholder("اضافة وتعديل بيانات المرضى")
            └─► MainViewModel.OpenPlaceholder(string functionTitle)
                └─ IsToolbarVisible = false
                └─ CurrentView = new PlaceholderView(functionTitle, new RelayCommand(_ => ReturnToActiveModule()))
        WPF renders PlaceholderView (no Template needed — it's directly a UserControl)
    
    [USER CLICKS «رجوع» IN PlaceholderView]
    BackCommand executes ⇒ ReturnToActiveModule()
        └─ IsToolbarVisible = true
        └─ switch (ActiveModule):
            "المرضى" ⇒ CurrentView = new PatientModuleViewModel(OpenPlaceholder)
            "بيانات النظام" ⇒ CurrentView = new SystemDataModuleViewModel(OpenPlaceholder)
            "حسابات" ⇒ CurrentView = new AccountsModuleViewModel(OpenPlaceholder)
            "ورقة عمل" ⇒ CurrentView = new WorksheetModuleViewModel(OpenPlaceholder)
            "احصاليات" ⇒ CurrentView = new StatisticsModuleViewModel(OpenPlaceholder)
            "اعدادات" ⇒ CurrentView = new SettingsModuleViewModel(OpenPlaceholder)
            "أدوات" ⇒ CurrentView = new ToolsModuleViewModel(OpenPlaceholder)
            "المستخدمين" ⇒ CurrentView = new UsersModuleViewModel(OpenPlaceholder)
            default ⇒ CurrentView = new WelcomeViewModel(ActiveModule)
    
    [USER CLICKS LOGOUT]
    LogoutCommand ⇒ LogoutAsync()
        └─► CloseAttendanceAsync (if AttendanceLogId > 0)
            └─ _attendanceService.CloseAsync(AppSession.AttendanceLogId)
            └─ AppSession.AttendanceLogId = 0
        └─► if _logoutRequested != null:
            └─ AppSession.Clear()
            └─ IsLoggedIn = false
            └─ ActiveModule = ""
            └─ IsToolbarVisible = true
            └─ CurrentView = new WelcomeViewModel()
            └─ _logoutRequested.Invoke() ⇒ App.ReturnToLogin
                └─ ShowLoginWindow() ⇒ تعود إلى الخطوة الأولى
                └─ mainWindow.Close()
```

---

## 6) خرائط الأزرار حسب الـ Module

### 6.1 PatientModule → 4 Placeholders
```
PatientModuleView
├─ [زر 1] OpenAddPatientCommand     → PlaceholderView("اضافة وتعديل بيانات المرضى")
├─ [زر 2] OpenEnterResultsCommand   → PlaceholderView("ادخال نتائج التحاليل")
├─ [زر 3] OpenDeliverResultsCommand → PlaceholderView("تسليم نتائج المرضى")
└─ [زر 4] OpenSearchPatientCommand  → PlaceholderView("بحث عن مريض")
```

### 6.2 AccountsModule → 4 Placeholders
```
AccountsModuleView
├─ OpenInventoryCommand        → PlaceholderView("الجرد وحساب الدرج")
├─ OpenExternalSamplesCommand  → PlaceholderView("العينات المرسلة للخارج")
├─ OpenCashTransactionCommand  → PlaceholderView("صرف وإيداع نقدية")
└─ OpenCompanyAccountsCommand  → PlaceholderView("حساب شركات ومندوبين")
```

### 6.3 SystemDataModule → 14 Placeholders
```
SystemDataModuleView
├─ OpenTestDataCommand            → PlaceholderView("بيانات التحاليل")
├─ OpenBarcodeTypesCommand        → PlaceholderView("Barcode Types")
├─ OpenCultureAntibioticsCommand  → PlaceholderView("Culture Antibiotics")
├─ OpenTestGroupsCommand          → PlaceholderView("مجموعات التحاليل")
├─ OpenTestUnitsCommand           → PlaceholderView("Test Units")
├─ OpenTestCommentsCommand        → PlaceholderView("Test Comments")
├─ OpenLabBranchesCommand         → PlaceholderView("Lab. branches")
├─ OpenPatientTitlesCommand       → PlaceholderView("القاب وتعريفات المرضى")
├─ OpenCustomGroupsCommand        → PlaceholderView("Custom Groups")
├─ OpenWorkGroupsLogCommand       → PlaceholderView("مجموعات العمل (Log)")
├─ OpenPrintPriceListCommand      → PlaceholderView("طباعة قائمة اسعار التحاليل")
├─ OpenLabEquipmentCommand        → PlaceholderView("أجهزة ومعدات المعمل")
├─ OpenExternalEntitiesCommand    → PlaceholderView("الجهات الخارجية والمعدل")
└─ OpenExternalPriceListsCommand  → PlaceholderView("قائمة أسعار التحاليل للجهات")
```

### 6.4 WorksheetModule → 2 Placeholders
```
WorksheetModuleView
├─ OpenWorksheetByPatientCommand  → PlaceholderView("ورقة عمل بأسماء المرضى")
└─ OpenWorksheetByTestCommand     → PlaceholderView("ورقة عمل بأسماء التحاليل (Log)")
```

### 6.5 StatisticsModule → 6 Placeholders
```
StatisticsModuleView
├─ OpenPatientCountStatCommand     → PlaceholderView("احصاليات وفقاً لعدد المرضى")
├─ OpenTestCountStatCommand        → PlaceholderView("احصاليات وفقاً لعدد التحاليل")
├─ OpenBranchStatCommand           → PlaceholderView("احصاليات خاصة بفروع المعمل")
├─ OpenExternalSamplesStatCommand  → PlaceholderView("احصاليات العينات المرسلة")
├─ OpenWorkPerformanceStatCommand  → PlaceholderView("احصاليات تقيم ومتابعة العمل")
└─ OpenResultsMonitorCommand       → PlaceholderView("متابعة ومراقبة النتائج")
```

### 6.6 SettingsModule → 2 Placeholders
```
SettingsModuleView
├─ OpenSystemSettingsCommand       → PlaceholderView("اعدادات النظام")
└─ OpenDatabaseMaintenanceCommand  → PlaceholderView("Database Maintenance")
```

### 6.7 ToolsModule → 9 Placeholders
```
ToolsModuleView
├─ OpenTestLibraryCommand              → PlaceholderView("مكتبة التحاليل")
├─ OpenStopwatchCommand                → PlaceholderView("ساعة التوقيت Stopwatch")
├─ OpenRequirementsListCommand         → PlaceholderView("قائمة المطلوبات والمشتريات")
├─ OpenImageLibraryCommand             → PlaceholderView("مكتبة الصور")
├─ OpenUnitConverterCommand            → PlaceholderView("محول وحدات نتائج التحاليل")
├─ OpenAppointmentsCommand             → PlaceholderView("نونة المواعيد")
├─ OpenAbbreviationsDictionaryCommand  → PlaceholderView("قاموس لاختصارات")
├─ OpenCalculatorCommand               → PlaceholderView("الآلة الحاسبة")
└─ OpenPhoneDirectoryCommand           → PlaceholderView("دليل الهاتف")
```

### 6.8 UsersModule → 4 Placeholders
```
UsersModuleView
├─ OpenCreateUsersCommand     → PlaceholderView("انشاء مستخدمين")
├─ OpenChangePasswordCommand  → PlaceholderView("تغيير كلمة المرور")
├─ OpenAttendanceCommand      → PlaceholderView("الحضور والإنصراف")
└─ OpenLoginDetectorCommand   → PlaceholderView("Login detector")
```

### المجموع: 4+4+14+2+6+2+9+4 = **45 placeholders** ✓

---

## 7) ShellWindow Navigation (Dead Code)

ShellWindow.xaml يحوي Sidebar بـ 38 زر تنقل Bound إلى Commands في MainViewModel. **هذه النافذة غير مستخدمة إطلاقاً.** المجموعات داخلها:

| القسم | عدد الأزرار |
|---|---:|
| الأساسية | 11 (Dashboard, PatientRegistration, PatientTests, PatientBilling, PatientBillingByDate, ResultsEntry, ReportViewer, CombinedReport, BlankReport, ReceiptPrinting, CultureSensitivity) |
| بحث وتاريخ | 3 (PatientSearch, PatientHistory, CompareWithHistory) |
| ورقة العمل | 4 (WorkSheetByPatient, WorkSheetByTest, GroupWorksheet, TestClassificationLog) |
| الكتالوج | 6 (TestCatalog, ReferenceRanges, TestComments, PriceLists, CustomGroups, Referrals) |
| العمليات | 6 (AttendanceLog, AccountsTreasury, Delivery, SampleCollection, ExternalLabManagement, ContractInvoice) |
| الإدارة | 8 (UsersPermissions, UserActivityLog, SystemUsageMonitor, AttendanceReport, Statistics, SystemSettings, BackupRestore, Constants) |
| نظام | 1 (Logout) — لا يحسب ضمن Navigate Commands |
| **مجموع Navigate** | **38** |

ShellWindow.xaml يستخدم `<ContentControl Content="{Binding CurrentViewModel}" />` (وليس CurrentView). هذا يعني لو كان مستخدماً، لما عمل لأن CurrentViewModel لا توجد لها DataTemplates.

---

## 8) Navigation Failures Mapping

| Command | الحالة عند الضغط |
|---|---|
| NavigateToXxxModule (الـ 8) | ✅ يعمل ⇒ Hub يظهر فعلاً |
| NavigateToEmployees/DidYouKnow/About | ✅ يعمل ⇒ WelcomeView مع ModuleName |
| LogoutCommand | ✅ يعمل ⇒ يعود إلى Login |
| Navigate*Command (الـ 38 إلى NavigationTarget غير Login) | ⚠️ ينشئ ViewModel في NavigationService.CurrentViewModel، لكن لا يظهر شيء جديد في UI لأن: (أ) لا DataTemplate (ب) CurrentView لا يتغير |
| NavigateDashboardCommand | ⚠️ نفس المشكلة — يُستدعى تلقائياً في `OnLoginSuccessAsync` لكنه لا يُغيِّر CurrentView |
| Navigate to NavigationTarget.Login | ✅ يستخدم في `ShowLogin` لكن السلوك الحقيقي يأتي من App.xaml.cs |

---

## 9) خلاصة التنقل الفعلي

**النظام يعمل فقط على 3 مستويات تنقل:**
1. **Top Toolbar (11 RadioButton + 1 Button):** الوحيدة الفعّالة للتنقل بين الـ Hubs
2. **Hub Buttons داخل Module Views (45 زر):** كلها تفتح PlaceholderView واحد فقط
3. **PlaceholderView Back Button:** يعود للـ Hub المناسب

**لا يوجد:**
- شاشات داخلية لأي وظيفة محددة
- روابط مباشرة لـ ViewModels المخصصة (PatientRegistration, ResultsEntry, ...)
- اختصارات لوحة مفاتيح فعلية
- نوافذ Dialog منبثقة
- روابط Deep Linking
- History/Back stack

---
