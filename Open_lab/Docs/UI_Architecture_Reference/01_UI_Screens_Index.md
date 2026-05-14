# 01 — UI Screens Index — Open lab system (Final Independent Audit)

> فهرس كامل ونهائي لكل شاشات Open lab system، مبني على فحص نصي مباشر للكود الفعلي + المرجعين `real lab system help.pdf` و `RLS_Learn.pdf`.
>
> - **المستودع:** `https://github.com/El-ogra/Open_lab.git`
> - **الفرع:** `Fi5ve`
> - **الكوميت:** `011f15cff5a7f9898bddcc70b134f4258979125e` — رسالة الكوميت: «بعد حذف الشغل القديم»
> - **منهجية التحقق:** كل صف في الجداول مدعوم بمسار ملف فعلي + اسم class / property / command / x:Class.

---

## 0) مفاتيح حالة الشاشة في المستودع (Repo Status Legend)

| الرمز | الدلالة | المعنى التشغيلي |
|---|---|---|
| ✅ موجودة | XAML + code-behind + ViewModel كاملة وتعمل | يمكن فتحها وتُعرض فعلياً |
| 🟡 Hub فقط | ModuleView (الـ Hub) موجود ويعرض الأزرار، لكن وظائفه الفرعية تفتح PlaceholderView | الأزرار تعمل لكن تفتح شاشة مؤقتة |
| 🟠 ViewModel فقط | ViewModel موجود + قيمة NavigationTarget معرَّفة + ViewModelFactory.Create يبنيها — لكن **لا توجد View XAML مخصصة** ولا DataTemplate في App.xaml | الـ Command الموجه لها يعطّل CurrentView لكن لا تظهر واجهة (مجرد كائن ViewModel) |
| 🔴 مفقودة كلياً | لا XAML ولا ViewModel ولا NavigationTarget | غير موجودة في النظام |
| ⬛ Placeholder Hook | يفتح PlaceholderView عند الضغط على الزر في الـ Hub | شاشة بديلة مؤقتة عامة |

---

## 1) ملخص رقمي محقَّق من الكود (بأدلة)

| المؤشر | العدد الفعلي | الإثبات (grep على كوميت `011f15c`) |
|---|---:|---|
| ملفات XAML الكلية | **14** | `find Open_lab -name "*.xaml" -not -path "*/Migrations/*"` ⇒ 14 |
| ملفات Views/*.xaml | 12 | App.xaml + Shell/ShellWindow.xaml + 12 = 14 |
| ملفات XAML داخل `Open_lab/Views/` | 12 | LoginView, MainWindow, WelcomeView, PlaceholderView + 8 Module Views |
| ملفات ViewModel في `Open_lab/ViewModels/` | **53** | `ls Open_lab/ViewModels/*.cs` (يشمل المساعدات والـ Module sub-ViewModels) |
| قيم Enum NavigationTarget | **40** | `grep -E "^\s+[A-Z]" NavigationTarget.cs \| wc -l` = 40 |
| Commands في MainViewModel (ICommand مُعلَنة) | **50** | `grep -cE "public (ICommand\|RelayCommand)" MainViewModel.cs` = 50 |
| استدعاءات `new RelayCommand(` في MainViewModel | 51 | يشمل 50 ICommand + 1 سطر إضافي داخل RaiseNavigationCanExecuteChanged (cast) |
| DataTemplates في App.xaml | **11** | LoginViewModel, MainViewModel, WelcomeViewModel + 8 ModuleViewModels |
| ModuleView Hubs (XAML موجود) | **8** | Patients, SystemData, Accounts, Worksheet, Statistics, Settings, Tools, Users |
| Placeholder Hooks (إجمالي) | **45** | `grep -E '_openPlaceholder\("\|Create\("' في الـ 8 Module ViewModels = 45 |
| RadioButton علوية في MainWindow toolbar | 11 | Patients, Tools, Worksheet, Accounts, Statistics, Users, SystemData, Settings, Employees, DidYouKnow, About |
| Button علوية إضافية (خروج Logout) | 1 | LogoutCommand |
| Sidebar buttons في ShellWindow.xaml | 38 | `grep -E "Command=.\{Binding Navigate" ShellWindow.xaml \| wc -l` = 38 |

---

## 2) بنية الشاشات الفعلية (المرئية للمستخدم)

### 2.0 شاشات النظام الأساسية (Shell / Bootstrap)

#### 0.1 LoginView — شاشة تسجيل الدخول
- **الاسم الرسمي:** «تسجيل الدخول — Open Lab System»
- **النوع التقني:** `UserControl` (يُستضاف داخل `Window` مخصصة في `App.OnStartup → ShowLoginWindow`)
- **مساحة الاسم:** `Open_lab.Views.LoginView`
- **x:Class:** `Open_lab.Views.LoginView`
- **ViewModel:** `Open_lab.ViewModels.LoginViewModel`
- **النوافذ الحاوية:** نافذة مخصصة بأبعاد 400×550، `ResizeMode=NoResize`, `WindowStartupLocation=CenterScreen`, `FlowDirection=RightToLeft`
- **الموديول:** Bootstrap (لا ينتمي إلى أي موديول من شريط الأدوات)
- **الغرض الوظيفي:** التحقق من بيانات المستخدم وتفعيل صلاحيات الجلسة قبل دخول MainWindow
- **نقطة الدخول:** `App.OnStartup` ⇒ `ShowLoginWindow` ⇒ تعرض LoginView داخل Window حاوية
- **نقطة الخروج:** بعد نجاح `LoginCommand`، يُستدعى `_onLoginSuccess` ⇒ `OpenMainWindowAfterLogin` ⇒ تُغلق نافذة LoginWindow ويظهر MainWindow
- **عناصر التحكم الأساسية:** TextBox للـ Username + PasswordBox/TextBox مزدوج للـ Password + Button العين للتبديل + CheckBox للتذكر + TextBlock للحالة + Button «دخول»
- **دورة الاستخدام:** Username → Password → (اختياري RememberMe) → Enter/زر دخول ⇒ Validate ⇒ AppSession.SetPermissions ⇒ AttendanceLogin
- **النوافذ التابعة:** لا توجد
- **الحالة:** ✅ موجودة بالكامل

#### 0.2 MainWindow — النافذة الرئيسية (Shell)
- **الاسم الرسمي:** «Open Lab System >> القائمة الرئيسية» (من Title attribute)
- **النوع التقني:** `Window`
- **مساحة الاسم:** `Open_lab.Views.MainWindow`
- **x:Class:** `Open_lab.Views.MainWindow`
- **ViewModel:** `Open_lab.ViewModels.MainViewModel`
- **النوافذ الحاوية:** Window مستقلة (1180×720, MinHeight=640, MinWidth=980)
- **الموديول:** Shell — يستضيف باقي الموديولات داخل `ContentControl Content="{Binding CurrentView}"`
- **الغرض الوظيفي:** عرض شريط أدوات علوي بـ 12 خانة (11 RadioButton + 1 Button Logout) + منطقة محتوى ديناميكية + شريط حالة سفلي بمعلومات الجلسة
- **نقطة الدخول:** من `App.OpenMainWindowAfterLogin` بعد نجاح Login
- **نقطة الخروج:** `LogoutCommand` ⇒ `CloseAttendanceAsync` + `_logoutRequested.Invoke()` ⇒ يُعاد عرض LoginWindow
- **عناصر التحكم الأساسية:** 11 RadioButton علوية (GroupName=TopModules) + Button Logout + ContentControl (Row 1) + Status bar (Row 2)
- **دورة الاستخدام:** اختيار RadioButton أعلى ⇒ تنفّذ NavigateTo*Module في MainViewModel ⇒ يتغير CurrentView ⇒ يظهر الـ Hub المختار
- **النوافذ التابعة:** كل Module Views + WelcomeView + PlaceholderView كلها داخل ContentControl
- **الحالة:** ✅ موجودة بالكامل

#### 0.3 WelcomeView — شاشة الترحيب
- **الاسم الرسمي:** «Open Lab System» (شاشة الشعار)
- **النوع التقني:** `UserControl`
- **مساحة الاسم:** `Open_lab.Views.WelcomeView`
- **ViewModel:** `Open_lab.ViewModels.WelcomeViewModel`
- **الغرض الوظيفي:** عرض شعار النظام (Canvas مع 4 دوائر متداخلة بألوان خضراء + نصوص Open/Lab/Sys/☘) عندما لا يوجد موديول نشط، أو عند الضغط على «الموظفين» / «هل تعلم» / «نبذة»
- **نقطة الدخول:** افتراضياً عند `InitializeAfterLogin`، أو بضغط أحد أزرار `NavigateToEmployeesCommand`/`NavigateToDidYouKnowCommand`/`NavigateToAboutCommand` التي تستدعي `NavigateTopModule(name)` ⇒ `CurrentView = new WelcomeViewModel(moduleName)`
- **عناصر التحكم:** Canvas (520×360) + 4 Ellipses + 4 TextBlocks + 2 TextBlocks زاويتين «MS SQL SERVER» + «Open Lab System»
- **النوافذ التابعة:** لا توجد
- **الحالة:** ✅ موجودة بالكامل

#### 0.4 PlaceholderView — النافذة المؤقتة المشتركة
- **الاسم الرسمي:** «نافذة مؤقتة للوظيفة، وسيتم استكمال محتواها لاحقاً»
- **النوع التقني:** `UserControl` (داخل `Open_lab.Views.Shared`)
- **x:Class:** `Open_lab.Views.Shared.PlaceholderView`
- **الـ ViewModel:** **بلا ViewModel** — تستخدم نمط self-DataContext في code-behind (`DataContext = this`)
- **الـ Constructor:** `PlaceholderView(string functionTitle, ICommand? backCommand)` — يُستدعى من `MainViewModel.OpenPlaceholder`
- **عناصر التحكم:** TextBlock للعنوان (Binding على `FunctionTitle`) + TextBlock نص ثابت + Button «رجوع» (Binding على `BackCommand`)
- **الغرض الوظيفي:** الشاشة البديلة الموحدة لكل وظيفة فرعية لم تُنفَّذ بعد (45 وظيفة)
- **نقطة الدخول:** من أي Hub button يستدعي `_openPlaceholder("..title..")` في الـ ModuleViewModel ⇒ `MainViewModel.OpenPlaceholder` ⇒ `IsToolbarVisible = false` + `CurrentView = new PlaceholderView(...)`
- **نقطة الخروج:** زر «رجوع» ⇒ `ReturnToActiveModule` ⇒ يعيد عرض ModuleView المناسب وفقاً لـ `ActiveModule`
- **سمات تميّز:** الشاشة الوحيدة التي تخفي شريط الأدوات العلوي (`IsToolbarVisible=false`)
- **الحالة:** ✅ موجودة بالكامل (لكنها بطبيعتها generic placeholder)

---

### 2.1 موديول المرضى (Patients Module)

#### 1.1 PatientModuleView — الـ Hub
- **الاسم:** «من خلال هذه النافذة يمكنك عمل الآتي...» (Patients Hub)
- **x:Class:** `Open_lab.Views.Patients.PatientModuleView`
- **ViewModel:** `Open_lab.ViewModels.Patients.PatientModuleViewModel`
- **التخطيط:** Grid سطرين: الأعلى Border معلوماتي 215 px (شعار قطرة دم + سهم + AB- + نص متعدد السطور)، الأسفل شبكة 2×2 من أزرار الوظائف (لون `#8DB600`)
- **الغرض:** نقطة دخول رئيسية لإدارة المرضى (إضافة/إدخال نتائج/تسليم/بحث)
- **نقطة الدخول:** `MainViewModel.NavigateToPatientsCommand` ⇒ `NavigateToPatientsModule` ⇒ `ActiveModule="المرضى"` + `CurrentView = new PatientModuleViewModel(OpenPlaceholder)`
- **نقطة الخروج:** اختيار موديول علوي آخر، أو ضغط زر فرعي يفتح PlaceholderView
- **الأزرار الـ 4:** `OpenAddPatientCommand`, `OpenEnterResultsCommand`, `OpenDeliverResultsCommand`, `OpenSearchPatientCommand`
- **النوافذ التابعة (4 placeholders):**
  - ⬛ «اضافة وتعديل بيانات المرضى»
  - ⬛ «ادخال نتائج التحاليل»
  - ⬛ «تسليم نتائج المرضى»
  - ⬛ «بحث عن مريض»
- **الحالة:** 🟡 Hub فقط — كل الوظائف الفرعية تفتح PlaceholderView

#### 1.2 PatientRegistration (ViewModel فقط)
- **x:Class:** ❌ لا يوجد View XAML
- **ViewModel:** `Open_lab.ViewModels.PatientRegistrationViewModel` (383 سطر — مكتمل)
- **NavigationTarget:** `PatientRegistration` (قيمة #4 في الـ enum)
- **في ViewModelFactory:** `CreateViewModel<PatientRegistrationViewModel>()` — يبنيها بـ DI
- **في MainViewModel:** `NavigatePatientRegistrationCommand` متاح + `CanExecute = PermissionCodes.PatientsView`
- **في App.xaml:** ❌ لا DataTemplate ⇒ حتى لو ضُبط CurrentViewModel، لن يظهر شيء (سيظهر النص الحرفي للنوع)
- **Properties موجودة:** LabId, FullName, Gender, BirthDate, Phone, Address, ChronicDiseases, Allergies, Medications, MedicalNotes, PatientId, StatusMessage, SelectedPatient, SelectedReferral, Results, Referrals
- **Commands موجودة:** SaveCommand, NewCommand, GenerateLabIdCommand, LoadByLabIdCommand, SearchCommand, DeleteCommand
- **الحالة:** 🟠 ViewModel فقط (بدون View XAML ولا DataTemplate)

#### 1.3 PatientTestsSelection (ViewModel فقط)
- **ViewModel:** `PatientTestsSelectionViewModel` (428 سطر)
- **NavigationTarget:** `PatientTestsSelection`
- **Properties:** LabId, PatientName, PatientId, VisitId, SearchText, SelectedAccountType, SelectedReferral, SelectedAvailableTest, SelectedVisitTest, SelectedCustomGroup, TotalAmount, StatusMessage, AvailableTests, FilteredAvailableTests, SelectedTests, Referrals, AccountTypes, CustomGroups
- **Commands:** LoadPatientCommand, CreateVisitCommand, AddTestCommand, AddCustomGroupCommand, RemoveTestCommand, RefreshTestsCommand
- **الحالة:** 🟠 ViewModel فقط

#### 1.4 PatientBilling (ViewModel فقط)
- **ViewModel:** `PatientBillingViewModel` (413 سطر)
- **NavigationTarget:** `PatientBilling`
- **Properties:** VisitId, Total, Discount, Paid, EditPaymentAmount, NetTotal, Balance, NewChargeAmount, NewChargeDescription, StatusMessage, Reason, PaymentMethod, SelectedPayment, Payments, AdditionalCharges
- **Commands:** LoadVisitCommand, SaveInvoiceCommand, AddPaymentCommand, EditPaymentCommand, DeletePaymentCommand, AddChargeCommand, SettleAccountCommand, PrintInvoiceCommand
- **الحالة:** 🟠 ViewModel فقط

#### 1.5 PatientBillingByDate (ViewModel فقط)
- **ViewModel:** `PatientBillingByDateViewModel` (109 سطر)
- **NavigationTarget:** `PatientBillingByDate`
- **Properties:** PatientId, FromDate, ToDate, TotalInvoiced, TotalPaid, Balance, IsBusy, Invoices, Payments
- **Commands:** SearchCommand
- **الحالة:** 🟠 ViewModel فقط

#### 1.6 ResultsEntry (ViewModel فقط)
- **ViewModel:** `ResultsEntryViewModel` (302 سطر)
- **NavigationTarget:** `ResultsEntry`
- **Properties:** DateFrom, DateTo, SelectedVisitTest, StatusMessage, MedicalHistorySummary, HasMedicalAlerts, VisitTests, ResultItems
- **Commands:** LoadVisitTestsCommand, SaveResultsCommand, VerifyResultsCommand, ReopenResultsCommand
- **الحالة:** 🟠 ViewModel فقط

#### 1.7 ReportViewer (ViewModel فقط)
- **ViewModel:** `ReportViewerViewModel` (148 سطر)
- **NavigationTarget:** `ReportViewer`
- **Properties:** VisitId, StatusMessage, Report, PreviewPdfPath, PreviewPdfUri, Tests
- **Commands:** LoadReportCommand, PrintCommand, ReprintCommand
- **الحالة:** 🟠 ViewModel فقط

#### 1.8 PatientSearch (ViewModel فقط)
- **ViewModel:** `PatientSearchViewModel` (117 سطر)
- **NavigationTarget:** `PatientSearch`
- **Properties:** Name, Phone, LabId, Date, StatusMessage, SelectedPatient, Patients, Visits
- **Commands:** SearchCommand
- **الحالة:** 🟠 ViewModel فقط

#### 1.9 PatientHistory (ViewModel فقط)
- **ViewModel:** `PatientHistoryViewModel` (124 سطر)
- **NavigationTarget:** `PatientHistory`
- **Properties:** LabId, From, To, StatusMessage, History, Visits
- **Commands:** LoadHistoryCommand, PrintHistoryCommand
- **الحالة:** 🟠 ViewModel فقط

---

### 2.2 موديول بيانات النظام (SystemData Module)

#### 2.1 SystemDataModuleView — الـ Hub
- **x:Class:** `Open_lab.Views.SystemData.SystemDataModuleView`
- **ViewModel:** `SystemDataModuleViewModel`
- **التخطيط:** Grid سطرين: الأعلى 215 px (شعار «أرشيف» مستطيلات بنية)، الأسفل شبكة 4×4: 4 أعمدة بعرض 220-230 والصف الرابع يحوي عمود StackPanel جانبي مع زرين برتقاليين
- **الأزرار الـ 14:**
  - OpenTestDataCommand → «بيانات التحاليل»
  - OpenBarcodeTypesCommand → «Barcode Types»
  - OpenCultureAntibioticsCommand → «Culture Antibiotics»
  - OpenTestGroupsCommand → «مجموعات التحاليل»
  - OpenTestUnitsCommand → «Test Units»
  - OpenTestCommentsCommand → «Test Comments»
  - OpenLabBranchesCommand → «Lab. branches»
  - OpenPatientTitlesCommand → «القاب وتعريفات المرضى»
  - OpenCustomGroupsCommand → «Custom Groups»
  - OpenWorkGroupsLogCommand → «مجموعات العمل (Log)»
  - OpenPrintPriceListCommand → «طباعة قائمة اسعار التحاليل»
  - OpenLabEquipmentCommand → «أجهزة ومعدات المعمل» (لون أزرق سماوي `#87CEEB`)
  - OpenExternalEntitiesCommand → «الجهات الخارجية والمعدل» (لون برتقالي `#FF8C00`)
  - OpenExternalPriceListsCommand → «قائمة أسعار التحاليل للجهات» (لون برتقالي)
- **الحالة:** 🟡 Hub فقط — كل الـ 14 وظيفة تفتح PlaceholderView

#### الـ ViewModels المرتبطة بالنظام (موجودة بدون View)
- 🟠 `TestCatalogViewModel` — Tests, Groups, SampleTypes, Units, SaveCommand, NewCommand, DeleteCommand, ReloadCommand, GenerateBarcodeCommand
- 🟠 `ReferenceRangesViewModel` — Tests, Ranges, LoadCommand, SaveCommand, DeleteCommand
- 🟠 `TestCommentsViewModel` — Tests, Comments, LoadCommand, SaveCommand, DeleteCommand
- 🟠 `PriceListsViewModel` — PriceLists, Items, Tests, Referrals, LoadCommand, SaveListCommand, UpdateListCommand, AddItemCommand, UpdateItemCommand, DeleteItemCommand, PrintListCommand
- 🟠 `CustomGroupsViewModel` — Groups, GroupItems, Tests, SaveGroupCommand, AddItemCommand, DeleteItemCommand
- 🟠 `ReferralsViewModel` — Referrals, SaveCommand, DeleteCommand

---

### 2.3 موديول الحسابات (Accounts Module)

#### 3.1 AccountsModuleView — الـ Hub
- **x:Class:** `Open_lab.Views.Accounts.AccountsModuleView`
- **ViewModel:** `AccountsModuleViewModel`
- **التخطيط:** Grid سطرين: الأعلى 215 px (شعار خزينة Segoe MDL2 \uE825)، الأسفل شبكة 2×2 من 4 أزرار
- **الأزرار الـ 4:**
  - OpenInventoryCommand → «الجرد وحساب الدرج»
  - OpenExternalSamplesCommand → «العينات المرسلة للخارج»
  - OpenCashTransactionCommand → «صرف وإيداع نقدية»
  - OpenCompanyAccountsCommand → «حساب شركات ومندوبين»
- **الحالة:** 🟡 Hub فقط — 4 وظائف فرعية كلها placeholders

#### الـ ViewModels المرتبطة بالحسابات
- 🟠 `AccountsTreasuryViewModel` — Payments, ByUser, ByReferral, ByBranch, ByDoctor, LoadCommand, DailyCommand, WeeklyCommand, MonthlyCommand, PrintCommand
- 🟠 `DeliveryViewModel` — Visits, SearchCommand, DeliverCommand, ReopenCommand
- 🟠 `ReceiptPrintingViewModel` — TestItems, LoadCommand, PrintReceiptCommand, PrintBarcodeCommand
- 🟠 `ContractInvoiceViewModel` — Referrals, PendingInvoices, ContractInvoices, LoadReferralsCommand, LoadPendingCommand, CreateInvoiceCommand, LoadHistoryCommand, SettleSelectedCommand

---

### 2.4 موديول ورقة العمل (Worksheet Module)

#### 4.1 WorksheetModuleView — الـ Hub
- **x:Class:** `Open_lab.Views.Worksheet.WorksheetModuleView`
- **ViewModel:** `WorksheetModuleViewModel`
- **التخطيط:** Grid سطرين: الأعلى 215 px (شعار جدول Segoe MDL2 \uE81C)، الأسفل صف واحد من زرين بعرض 250
- **الأزرار الـ 2:**
  - OpenWorksheetByPatientCommand → «ورقة عمل بأسماء المرضى»
  - OpenWorksheetByTestCommand → «ورقة عمل بأسماء التحاليل (Log)»
- **الحالة:** 🟡 Hub فقط — وظيفتان فرعيتان placeholders

#### الـ ViewModels المرتبطة بورقة العمل
- 🟠 `WorkSheetByPatientViewModel` — Rows, LoadCommand, PrintCommand
- 🟠 `WorkSheetByTestViewModel` — Rows, LoadCommand, PrintCommand
- 🟠 `GroupWorksheetViewModel` — Groups, CustomGroups, Rows, LoadGroupsCommand, LoadWorksheetCommand, PrintCommand
- 🟠 `CombinedReportViewModel` — Tests, LoadCommand, MoveUpCommand, MoveDownCommand, SaveReportOrderCommand
- 🟠 `BlankReportViewModel` — LoadCommand, PrintBlankCommand
- 🟠 `CultureSensitivityViewModel` — 6 collections + 11 commands (الأكبر بين ViewModels — 548 سطر)
- 🟠 `SampleCollectionViewModel` — Items, LoadCommand, MarkCollectedCommand, MarkExternalCollectedCommand, MarkSeparatedCommand, MarkNotCollectedCommand, RefreshSampleStatusCommand
- 🟠 `CompareWithHistoryViewModel` — Tests, HistoryResults, LoadPatientCommand, LoadHistoryCommand, ClearCommand
- 🟠 `TestClassificationLogViewModel` — Items, LoadCommand, PrintCommand
- 🟠 `ExternalLabManagementViewModel` — Referrals, PendingQueue, Manifests, SettlementHistory, SelectedQueueIds + 9 commands

---

### 2.5 موديول الإحصائيات (Statistics Module)

#### 5.1 StatisticsModuleView — الـ Hub
- **x:Class:** `Open_lab.Views.Statistics.StatisticsModuleView`
- **ViewModel:** `StatisticsModuleViewModel`
- **التخطيط:** Grid سطرين: الأعلى 215 px (شعار رسم بياني Segoe MDL2 \uE9D2)، الأسفل شبكة 2×3 من 6 أزرار
- **الأزرار الـ 6:**
  - OpenPatientCountStatCommand → «احصاليات وفقاً لعدد المرضى»
  - OpenTestCountStatCommand → «احصاليات وفقاً لعدد التحاليل»
  - OpenBranchStatCommand → «احصاليات خاصة بفروع المعمل»
  - OpenExternalSamplesStatCommand → «احصاليات العينات المرسلة»
  - OpenWorkPerformanceStatCommand → «احصاليات تقيم ومتابعة العمل»
  - OpenResultsMonitorCommand → «متابعة ومراقبة النتائج»
- **الحالة:** 🟡 Hub فقط — 6 وظائف placeholders

#### الـ ViewModels المرتبطة بالإحصائيات
- 🟠 `StatisticsViewModel` — 8 collections (Genders, Referrals, ByGender, ByReferral, MonthlyAnalysis, TopTests, YearlySamples, UserProductivity) + LoadCommand + PrintCommand

---

### 2.6 موديول الإعدادات (Settings Module)

#### 6.1 SettingsModuleView — الـ Hub
- **x:Class:** `Open_lab.Views.Settings.SettingsModuleView`
- **ViewModel:** `SettingsModuleViewModel`
- **التخطيط:** Grid سطرين: الأعلى 215 px (شعار ترس Segoe MDL2 \uE713)، الأسفل صف واحد من زرين
- **الأزرار الـ 2:**
  - OpenSystemSettingsCommand → «اعدادات النظام»
  - OpenDatabaseMaintenanceCommand → «Database Maintenance»
- **الحالة:** 🟡 Hub فقط — وظيفتان placeholders

#### الـ ViewModels المرتبطة بالإعدادات
- 🟠 `SettingsViewModel` — Settings, AvailablePrinters, LoadSettingsCommand, SavePrinterSettingsCommand, SaveMarginSettingsCommand, RefreshPrintersCommand
- 🟠 `SystemSettingsViewModel` — Settings + 5 commands
- 🟠 `ConstantsViewModel` — Items, SeedDefaultsCommand, ReloadCommand, SaveCommand, DeleteCommand
- 🟠 `BackupRestoreViewModel` — BackupFiles + 6 commands

---

### 2.7 موديول الأدوات (Tools Module)

#### 7.1 ToolsModuleView — الـ Hub
- **x:Class:** `Open_lab.Views.Tools.ToolsModuleView`
- **ViewModel:** `ToolsModuleViewModel`
- **التخطيط:** Grid سطرين: الأعلى 215 px (شعار أدوات Segoe MDL2 \uED43)، الأسفل شبكة 3×3 من 9 أزرار (200×65)
- **الأزرار الـ 9:**
  - OpenTestLibraryCommand → «مكتبة التحاليل»
  - OpenStopwatchCommand → «ساعة التوقيت Stopwatch»
  - OpenRequirementsListCommand → «قائمة المطلوبات والمشتريات»
  - OpenImageLibraryCommand → «مكتبة الصور»
  - OpenUnitConverterCommand → «محول وحدات نتائج التحاليل»
  - OpenAppointmentsCommand → «نونة المواعيد»
  - OpenAbbreviationsDictionaryCommand → «قاموس لاختصارات»
  - OpenCalculatorCommand → «الآلة الحاسبة»
  - OpenPhoneDirectoryCommand → «دليل الهاتف»
- **الحالة:** 🟡 Hub فقط — 9 وظائف placeholders

---

### 2.8 موديول المستخدمين (Users Module)

#### 8.1 UsersModuleView — الـ Hub
- **x:Class:** `Open_lab.Views.Users.UsersModuleView`
- **ViewModel:** `UsersModuleViewModel`
- **التخطيط:** Grid سطرين: الأعلى 215 px (شعار شخص Segoe MDL2 \uE77B)، الأسفل شبكة 2×2 من 4 أزرار
- **الأزرار الـ 4:**
  - OpenCreateUsersCommand → «انشاء مستخدمين»
  - OpenChangePasswordCommand → «تغيير كلمة المرور»
  - OpenAttendanceCommand → «الحضور والإنصراف»
  - OpenLoginDetectorCommand → «Login detector»
- **الحالة:** 🟡 Hub فقط — 4 وظائف placeholders

#### الـ ViewModels المرتبطة بالمستخدمين
- 🟠 `UsersPermissionsViewModel` — Users, Roles, Permissions + 8 commands (الأكثر تعقيداً بعد Culture)
- 🟠 `AttendanceLogViewModel` — Logs, Breaks + 7 commands
- 🟠 `AttendanceReportViewModel` — Users, ReportRows, PayrollRows + 4 commands
- 🟠 `UserActivityLogViewModel` — Items + LoadCommand
- 🟠 `SystemUsageMonitorViewModel` — Sessions + RefreshCommand

---

### 2.9 الموديولات «الافتراضية» (لا تفتح Hub فعلاً)

من الكود في MainViewModel، توجد 3 RadioButton في الـ Toolbar لا تفتح Hub حقيقي، بل تستدعي `NavigateTopModule(name)` الذي يضبط فقط `ActiveModule` و`CurrentView = new WelcomeViewModel(moduleName)`:

| الزر | الـ Command | الـ ActiveModule | يفتح |
|---|---|---|---|
| «الموظفين» | NavigateToEmployeesCommand | "الموظفين" | WelcomeView |
| «هل تعلم» | NavigateToDidYouKnowCommand | "هل تعلم" | WelcomeView |
| «نبذة» | NavigateToAboutCommand | "نبذة" | WelcomeView |

**الحالة:** 🔴 لا توجد Hub Views مخصصة — مجرد WelcomeView جانبي بـ ModuleName.

---

### 2.10 الشاشات الموجودة في NavigationTarget لكن غير مرتبطة بأي زر Hub

`NavigationTarget.Home` ⇒ `HomeViewModel` (8 أسطر فقط، يحوي Title وSubtitle نصيين). NavigationTarget.Login يعالجها `ViewModelFactory.CreateLoginViewModel(onLoginSuccess)`.

---

## 3) الجرد الإجمالي للحالة

| الفئة | العدد | النسبة |
|---|---:|---:|
| Views XAML كاملة (✅) | 12 | 100% من XAML الموجود |
| Module Hubs منفّذة (🟡) | 8 | كل الموديولات الفرعية الـ 8 |
| ViewModels بلا View (🟠) | ~37 | غالبية الـ NavigationTarget |
| Placeholder Hooks فعلية (⬛) | 45 | كل وظائف الـ 8 Hubs |
| Top-bar Buttons «وهمية» (🔴 Hub) | 3 | Employees / DidYouKnow / About |

---

## 4) ملاحظات تنفيذية حرجة

1. **ShellWindow.xaml غير مستخدم:** بحث `grep -rn "ShellWindow"` في الـ C# يعطي فقط `Shell/ShellWindow.xaml.cs` (التعريف الذاتي). لا توجد إشارة في `App.xaml.cs` ولا في أي مكان آخر. الـ Sidebar بـ 38 زراً يبقى Dead Code.
2. **DataTemplates الـ 11 في App.xaml:** فقط الـ ModuleViewModels الـ 8 + LoginViewModel + MainViewModel + WelcomeViewModel. أي ViewModel آخر يُمرَّر إلى CurrentView لن يُعرَض بشكل صحيح (سيظهر النوع كنص).
3. **MainWindowLayoutService:** يضبط أبعاد النافذة عند Login (400×550) و App (1100×700) — بينما MainWindow.xaml يُعرَّف بـ 1180×720. هذا تعارض يعمل عند الـ runtime لصالح الـ Service.
4. **PlaceholderView كسر MVVM:** هي الـ View الوحيدة التي تستخدم `DataContext = this` في constructor، وتحقن `FunctionTitle` و`BackCommand` كـ Properties محلية في الـ code-behind.
5. **LoginView داخل Window حاوية:** ليست Window مستقلة بل UserControl يُستضاف داخل Window بُنيت برمجياً في `App.OnStartup.ShowLoginWindow`.

---
