# 09 — UI Screen Prompts — Open lab system (Final Independent Audit)

> مطالبات تنفيذ مستقلة وكاملة لكل شاشة في Open lab system. كل مطالبة قابلة للتنفيذ مباشرة بواسطة وكيل تنفيذ XAML بدون مرجع خارجي.

---

## القراءات الإلزامية قبل البدء بأي مطالبة

كل مهندس/وكيل تنفيذ يجب أن يقرأ بالترتيب قبل البدء:
1. `07_UI_Visual_Style_Guide.md` — لاستخراج ResourceDictionary (الألوان، الفُرَش، الـ Styles)
2. `08_UI_Window_Contracts.md` — للحصول على x:Class والمسار والـ Bindings الحقيقية لكل View
3. `04_UI_Control_Inventory.md` — جرد التحكم لكل شاشة
4. `05_UI_Interaction_Rules.md` — قواعد التفاعل (Click, Hover, Focus, Validation)
5. `06_UI_Keyboard_Shortcuts.md` — الاختصارات المُقترحة (F-Keys + Ctrl + Esc)
6. `10_UI_Agent_Master_Prompt.md` — القواعد العامة (MVVM، أنماط، لا code-behind)

---

## ترتيب الإنتاج الإلزامي (Production Order)

| # | الشاشة | الحالة الحالية | الاعتمادية |
|---|---|---|---|
| 0 | Shell (MainWindow) | ✅ موجود | لا شيء |
| 1 | LoginView | ✅ موجود | لا شيء |
| 2 | WelcomeView | ✅ موجود | لا شيء |
| 3 | PlaceholderView | ✅ موجود | لا شيء |
| 4 | 8 Hub Views (Patients, SystemData, Accounts, Worksheet, Statistics, Settings, Tools, Users) | ✅ موجودة | لا شيء |
| 5 | PatientRegistrationView | ❌ مطلوب إنشاؤها | PatientRegistrationViewModel (موجود) + DataTemplate في App.xaml |
| 6 | PatientTestsSelectionView | ❌ | PatientTestsSelectionViewModel + DataTemplate |
| 7 | PatientBillingView | ❌ | PatientBillingViewModel |
| 8 | PatientBillingByDateView | ❌ | PatientBillingByDateViewModel |
| 9 | ResultsEntryView | ❌ | ResultsEntryViewModel |
| 10 | ReportViewerView | ❌ | ReportViewerViewModel |
| 11 | PatientSearchView | ❌ | PatientSearchViewModel |
| 12 | PatientHistoryView | ❌ | PatientHistoryViewModel |
| 13 | DashboardView | ❌ | DashboardViewModel |
| 14 | TestCatalogView | ❌ | TestCatalogViewModel |
| 15 | ReferenceRangesView | ❌ | ReferenceRangesViewModel |
| 16 | TestCommentsView | ❌ | TestCommentsViewModel |
| 17 | PriceListsView | ❌ | PriceListsViewModel |
| 18 | CustomGroupsView | ❌ | CustomGroupsViewModel |
| 19 | ReferralsView | ❌ | ReferralsViewModel |
| 20 | UsersPermissionsView | ❌ | UsersPermissionsViewModel |
| 21 | StatisticsView | ❌ | StatisticsViewModel |
| 22 | SystemSettingsView | ❌ | SystemSettingsViewModel |
| 23 | BackupRestoreView | ❌ | BackupRestoreViewModel |
| 24 | AttendanceLogView | ❌ | AttendanceLogViewModel |
| 25 | AccountsTreasuryView | ❌ | AccountsTreasuryViewModel |
| 26 | DeliveryView | ❌ | DeliveryViewModel |
| 27 | SampleCollectionView | ❌ | SampleCollectionViewModel |
| 28 | CultureSensitivityView | ❌ | CultureSensitivityViewModel (الأكبر) |
| 29 | ReceiptPrintingView | ❌ | ReceiptPrintingViewModel |
| 30 | CombinedReportView | ❌ | CombinedReportViewModel |
| 31 | BlankReportView | ❌ | BlankReportViewModel |
| 32 | ConstantsView | ❌ | ConstantsViewModel |
| 33 | CompareWithHistoryView | ❌ | CompareWithHistoryViewModel |
| 34 | GroupWorksheetView | ❌ | GroupWorksheetViewModel |
| 35 | TestClassificationLogView | ❌ | TestClassificationLogViewModel |
| 36 | ExternalLabManagementView | ❌ | ExternalLabManagementViewModel |
| 37 | AttendanceReportView | ❌ | AttendanceReportViewModel |
| 38 | ContractInvoiceView | ❌ | ContractInvoiceViewModel |
| 39 | UserActivityLogView | ❌ | UserActivityLogViewModel |
| 40 | SystemUsageMonitorView | ❌ | SystemUsageMonitorViewModel |

---

## القاعدة الموحَّدة لكل مطالبة

كل XAML جديد يجب أن:
1. **يبدأ بـ `<UserControl x:Class="Open_lab.Views.{Subfolder}.{ViewName}" FlowDirection="RightToLeft">`**
2. **لا يحوي code-behind** سوى `InitializeComponent()` في constructor الافتراضي
3. **لا يستخدم event handlers** (Click, SelectionChanged, KeyDown) — كل التفاعل عبر Commands و Bindings
4. **يستخدم ResourceDictionary الموحَّد** (المُقترح في الملف 07)
5. **يحوي `<UserControl.InputBindings>`** لـ KeyBindings المطلوبة (راجع الملف 06)
6. **يتطابق Bindings مع الـ ViewModel الفعلي** المذكور في الملف 08
7. **يدعم RTL** بطبيعته، مع استثناءات `FlowDirection=LeftToRight` للأيقونات والشعارات

---

## Prompt #1 — PatientRegistrationView

### الهدف
إنشاء UserControl يربط مع `PatientRegistrationViewModel` لتسجيل بيانات المرضى الجدد، مع تعديل المسجَّلين، وعرض نتائج البحث، وحفظ التاريخ الطبي.

### المعطيات
- **x:Class:** `Open_lab.Views.Patients.PatientRegistrationView`
- **ViewModel:** `Open_lab.ViewModels.PatientRegistrationViewModel`
- **Bindings الـ Properties:** PatientId, LabId, FullName, Gender, BirthDate, Phone, Address, ChronicDiseases, Allergies, Medications, MedicalNotes, StatusMessage, SelectedPatient, SelectedReferral, Results, Referrals
- **Bindings الـ Commands:** SaveCommand, NewCommand, GenerateLabIdCommand, LoadByLabIdCommand, SearchCommand, DeleteCommand

### التخطيط المطلوب
- Grid بـ 4 صفوف:
  - Row 0 (Auto): شريط أدوات علوي يحوي:
    - حقل LabId (TextBox) + زر «توليد» (GenerateLabIdCommand) + زر «تحميل» (LoadByLabIdCommand)
    - زر «جديد F1» (NewCommand) + زر «حفظ F9» (SaveCommand) + زر «حذف F12» (DeleteCommand)
  - Row 1 (Auto, ~300px): نموذج الإدخال — Grid 2 أعمدة × 6 صفوف:
    - Col 0/Row 0: «اسم المريض» + TextBox(FullName)
    - Col 1/Row 0: «جهة الإحالة» + ComboBox(SelectedReferral, ItemsSource=Referrals, DisplayMemberPath=Name)
    - Col 0/Row 1: «النوع» + ComboBox(Gender) با خيارات ذكر/أنثى
    - Col 1/Row 1: «تاريخ الميلاد» + DatePicker(BirthDate)
    - Col 0/Row 2: «الهاتف» + TextBox(Phone)
    - Col 1/Row 2: «العنوان» + TextBox(Address)
    - Row 3 colspan=2: عنوان «التاريخ الطبي»
    - Row 4 cols separate: 4 TextBoxes متعددة الأسطر (ChronicDiseases, Allergies, Medications, MedicalNotes)
  - Row 2 (Auto, ~30px): TextBlock StatusMessage بلون #D13438
  - Row 3 (*): DataGrid يعرض Results (Patient list)
    - Columns: LabId, FullName, Gender, BirthDate, Phone
    - SelectedItem={Binding SelectedPatient}
- **InputBindings:**
  - F1 ⇒ NewCommand
  - F9 ⇒ SaveCommand
  - F12 ⇒ DeleteCommand
- **اللون والـ Style:** استخدام Hub.Bg=#25333E كخلفية أصلية، Card بـ Background=White، أزرار بـ #8DB600

### معايير القبول
- يجب أن يعمل الـ DataContext افتراضياً (لأن DataTemplate ستُضاف في App.xaml)
- لا code-behind غير InitializeComponent
- جميع الـ Bindings تطابق Properties الموجودة فعلاً

---

## Prompt #2 — PatientTestsSelectionView

### الهدف
شاشة اختيار التحاليل للمريض، تحوي قائمتين (متاحة/مختارة) مع إضافة/إزالة سريعة.

### المعطيات
- **x:Class:** `Open_lab.Views.Patients.PatientTestsSelectionView`
- **Bindings:** LabId, PatientName, PatientId, VisitId, StatusMessage, SelectedAvailableTest, SelectedVisitTest, SelectedAccountType, SelectedReferral, SearchText, SelectedCustomGroup, TotalAmount, AvailableTests, FilteredAvailableTests, SelectedTests, Referrals, AccountTypes, CustomGroups
- **Commands:** LoadPatientCommand, CreateVisitCommand, AddTestCommand, AddCustomGroupCommand, RemoveTestCommand, RefreshTestsCommand

### التخطيط
- Grid بـ 3 صفوف:
  - Row 0 (Auto): Header — LabId TextBox + زر «تحميل المريض» (LoadPatientCommand) + TextBlock(PatientName) + ComboBox(AccountTypes) + ComboBox(Referrals)
  - Row 1 (*): Grid 3 أعمدة:
    - Col 0: قائمة AvailableTests (DataGrid مع SearchBox SearchText)
    - Col 1: عمود الأزرار «إضافة →», «إزالة ←», «إضافة مجموعة» (AddCustomGroupCommand)
    - Col 2: قائمة SelectedTests (DataGrid)
  - Row 2 (Auto): Footer — TextBlock «الإجمالي: » + TextBlock(TotalAmount) + زر «إنشاء زيارة» (CreateVisitCommand)

### معايير القبول
- DoubleClick على Test في AvailableTests ⇒ AddTestCommand (يمكن إضافته عبر `<DataGrid.InputBindings>` أو `<DataGrid.RowStyle>` مع EventToCommand)
- F5 ⇒ RefreshTestsCommand

---

## Prompt #3 — PatientBillingView

### الهدف
شاشة حساب المريض — تحوي تفاصيل الفاتورة + المدفوعات + الرسوم الإضافية.

### المعطيات
- **x:Class:** `Open_lab.Views.Patients.PatientBillingView`
- **Bindings:** VisitId, Total, Discount, Paid, EditPaymentAmount, NetTotal, Balance, NewChargeAmount, NewChargeDescription, StatusMessage, Reason, PaymentMethod, SelectedPayment, Payments, AdditionalCharges
- **Commands:** LoadVisitCommand, SaveInvoiceCommand, AddPaymentCommand, EditPaymentCommand, DeletePaymentCommand, AddChargeCommand, SettleAccountCommand, PrintInvoiceCommand

### التخطيط
- Grid 3 صفوف:
  - Row 0: Header (VisitId TextBox + LoadVisitCommand + PrintInvoiceCommand)
  - Row 1 (Auto): شبكة 2 أعمدة:
    - Col 0: حقول مالية (Total, Discount, Paid, NetTotal, Balance) — Total و NetTotal و Balance ReadOnly
    - Col 1: DataGrid Payments + EditPaymentAmount + Add/Edit/Delete Buttons
  - Row 2 (*): DataGrid AdditionalCharges + حقل NewChargeDescription + حقل NewChargeAmount + AddChargeCommand
- **Footer:** زر SettleAccountCommand + زر SaveInvoiceCommand
- **InputBindings:**
  - Enter Enter ⇒ SettleAccountCommand
  - Ctrl+S ⇒ SaveInvoiceCommand
  - Ctrl+P ⇒ PrintInvoiceCommand

---

## Prompt #4 — ResultsEntryView

### الهدف
شاشة إدخال نتائج التحاليل — الأكثر استخداماً يومياً.

### المعطيات
- **x:Class:** `Open_lab.Views.Patients.ResultsEntryView`
- **Bindings:** DateFrom, DateTo, SelectedVisitTest, StatusMessage, MedicalHistorySummary, HasMedicalAlerts, VisitTests, ResultItems
- **Commands:** LoadVisitTestsCommand, SaveResultsCommand, VerifyResultsCommand, ReopenResultsCommand

### التخطيط
- Grid 3 أعمدة:
  - Col 0 (300px): DateFrom/DateTo + LoadVisitTestsCommand + DataGrid VisitTests (SelectedItem=SelectedVisitTest)
  - Col 1 (*): DataGrid ResultItems (EditMode) + Footer مع SaveResultsCommand + VerifyResultsCommand + ReopenResultsCommand
  - Col 2 (250px): MedicalHistorySummary panel + لون أحمر لو HasMedicalAlerts=true
- **InputBindings:**
  - F8 ⇒ VerifyResultsCommand
  - F9 ⇒ SaveResultsCommand
  - Enter داخل خلية ResultItems ⇒ الانتقال للخلية التالية

---

## Prompt #5 — ReportViewerView

### الهدف
عارض التقارير — يحوي PDF Preview + Print.

### المعطيات
- **x:Class:** `Open_lab.Views.Patients.ReportViewerView`
- **Bindings:** VisitId, StatusMessage, Report, PreviewPdfPath, PreviewPdfUri, Tests
- **Commands:** LoadReportCommand, PrintCommand, ReprintCommand

### التخطيط
- Grid 2 صفوف:
  - Row 0 (Auto): VisitId TextBox + LoadReportCommand + PrintCommand + ReprintCommand
  - Row 1 (*): شبكة 2 أعمدة:
    - Col 0 (250px): DataGrid Tests (VisitTestReportItem)
    - Col 1 (*): WebBrowser أو System.Windows.Controls.Frame يعرض PreviewPdfUri
- **InputBindings:** Ctrl+P ⇒ PrintCommand

---

## Prompt #6 — PatientSearchView

### الهدف
شاشة البحث عن مريض.

### المعطيات
- **x:Class:** `Open_lab.Views.Patients.PatientSearchView`
- **Bindings:** Name, Phone, LabId, Date, StatusMessage, SelectedPatient, Patients, Visits
- **Commands:** SearchCommand

### التخطيط
- Grid 3 صفوف:
  - Row 0: Header — TextBoxes (Name, Phone, LabId) + DatePicker(Date) + SearchCommand
  - Row 1: DataGrid Patients (SelectedItem=SelectedPatient)
  - Row 2: DataGrid Visits (Bound ل Visits — يُحمَّل عند تغيير SelectedPatient)
- **InputBindings:** F5 ⇒ SearchCommand

---

## Prompt #7 — PatientHistoryView

### الهدف
عرض تاريخ المريض الزمني.

### المعطيات
- **x:Class:** `Open_lab.Views.Patients.PatientHistoryView`
- **Bindings:** LabId, From, To, StatusMessage, History, Visits
- **Commands:** LoadHistoryCommand, PrintHistoryCommand

### التخطيط
- Grid 3 صفوف:
  - Row 0: LabId + DatePicker(From) + DatePicker(To) + LoadHistoryCommand + PrintHistoryCommand
  - Row 1: ملخص (Border يعرض History.PatientName, RegistrationDate, etc.)
  - Row 2: DataGrid Visits

---

## Prompt #8 — DashboardView

### الهدف
لوحة المعلومات الرئيسية — عرض إحصاءات سريعة.

### المعطيات
- **x:Class:** `Open_lab.Views.DashboardView`
- **Bindings:** PatientCount, VisitCount, TestCount
- **Commands:** لا توجد

### التخطيط
- Grid مع 3 خانات شبكية (KPI Cards) كل خانة تعرض:
  - أيقونة Segoe MDL2
  - عنوان (المرضى / الزيارات / التحاليل)
  - رقم كبير {Binding PatientCount} … إلخ

---

## Prompt #9 — TestCatalogView (Master-Detail)

### الهدف
كتالوج التحاليل — قائمة على اليمين + نموذج تفاصيل + قائمة Sample Types و Units.

### المعطيات
- **Bindings:** Tests, Groups, SampleTypes, Units
- **Commands:** SaveCommand, NewCommand, DeleteCommand, ReloadCommand, GenerateBarcodeCommand

### التخطيط
- Grid 2 أعمدة:
  - Col 0 (300px): DataGrid Tests
  - Col 1 (*): نموذج تفاصيل (TestName, ReportName, Group ComboBox, SampleType ComboBox, Unit ComboBox, Price)
- **Footer:** New, Save, Delete, Reload, GenerateBarcode
- **InputBindings:** F1=New, F9=Save, F12=Delete

---

## Prompt #10 — ReferenceRangesView

### الهدف
إدارة القيم المرجعية لكل تحليل.

### التخطيط
- Grid 2 أعمدة:
  - Col 0: DataGrid Tests
  - Col 1: DataGrid Ranges (EditMode) — Columns: MinValue, MaxValue, AgeFrom, AgeTo, Gender, LowComment, HighComment, IsCritical
- **Commands:** LoadCommand, SaveCommand, DeleteCommand

---

## Prompt #11 — TestCommentsView

نفس نمط ReferenceRangesView لكن DataGrid Comments بدلاً من Ranges.

---

## Prompt #12 — PriceListsView

### الهدف
إدارة قوائم الأسعار للعقود والإحالات.

### التخطيط
- Grid 3 صفوف:
  - Row 0: ListBox PriceLists + زر LoadCommand
  - Row 1: نموذج تفاصيل PriceList + SaveListCommand + UpdateListCommand
  - Row 2: DataGrid Items + ComboBox(Tests) + ComboBox(Referrals) + AddItemCommand + UpdateItemCommand + DeleteItemCommand + PrintListCommand

---

## Prompt #13 — CustomGroupsView

### التخطيط
- Grid 3 أعمدة:
  - Col 0: DataGrid Groups
  - Col 1: DataGrid GroupItems
  - Col 2: ListBox Tests متاحة + AddItemCommand
- **Commands:** SaveGroupCommand, AddItemCommand, DeleteItemCommand

---

## Prompt #14 — ReferralsView

### التخطيط
- DataGrid Referrals (Editable) + SaveCommand + DeleteCommand
- **Columns:** Name, ReferralType (ComboBox: None/Doctor/Lab/Insurance), Phone, Address, Commission, PriceList

---

## Prompt #15 — UsersPermissionsView

### الهدف
إدارة المستخدمين والصلاحيات.

### التخطيط (الأكثر تعقيداً)
- TabControl 3 تبويبات:
  - Tab 1 «المستخدمون»: DataGrid Users + SaveUserCommand + DeleteUserCommand + ComboBox Roles + AssignRoleCommand + UnassignRoleCommand
  - Tab 2 «الأدوار»: DataGrid Roles + SaveRoleCommand + DeleteRoleCommand
  - Tab 3 «الصلاحيات»: ListBox Permissions (PermissionToggle) + SaveRolePermissionsCommand + ReloadCommand

---

## Prompt #16 — StatisticsView

### التخطيط
- TabControl 8 تبويبات (واحدة لكل ObservableCollection):
  - ByGender / ByReferral / MonthlyAnalysis / TopTests / YearlySamples / UserProductivity
  - كل تبويبة DataGrid
- **Header:** DatePicker From/To + ComboBox Genders + ComboBox Referrals + LoadCommand + PrintCommand

---

## Prompt #17 — SystemSettingsView

### التخطيط
- Grid:
  - Row 0: Form لـ Profile Settings + SaveProfileCommand
  - Row 1: DataGrid Settings (Raw key-value) + SaveRawSettingCommand + DeleteRawSettingCommand + ReloadCommand
- **Action Bar:** ChangeMasterPasswordCommand (يفتح Dialog منفصل)

---

## Prompt #18 — BackupRestoreView

### التخطيط
- Grid:
  - Row 0: BackupCommand + RestoreCommand + ListBox BackupFiles + LoadBackupsCommand
  - Row 1: ConfigureScheduleCommand + DisableScheduleCommand + RefreshScheduleCommand

---

## Prompt #19 — AttendanceLogView

### التخطيط
- Grid 2 أعمدة:
  - Col 0: DataGrid Logs + LoadLogsCommand + ClockInCommand + ClockOutCommand + RefreshOpenLogCommand
  - Col 1: DataGrid Breaks + StartBreakCommand + EndBreakCommand + LoadDailySummaryCommand

---

## Prompt #20 — AccountsTreasuryView

### التخطيط
- Grid 2 صفوف:
  - Row 0: Filter Bar + LoadCommand + DailyCommand + WeeklyCommand + MonthlyCommand + PrintCommand
  - Row 1: TabControl 5 تبويبات: Payments / ByUser / ByReferral / ByBranch / ByDoctor — كل تبويبة DataGrid

---

## Prompt #21 — DeliveryView

### التخطيط
- Grid:
  - Row 0: SearchCommand
  - Row 1: DataGrid Visits + DeliverCommand + ReopenCommand
- **InputBindings:** F6 = SearchCommand

---

## Prompt #22 — SampleCollectionView

### التخطيط
- DataGrid Items + LoadCommand + MarkCollectedCommand + MarkExternalCollectedCommand + MarkSeparatedCommand + MarkNotCollectedCommand + RefreshSampleStatusCommand

---

## Prompt #23 — CultureSensitivityView (الأكبر)

### التخطيط (5 Tabs)
- Tab 1 «الـ Cultures»: DataGrid Cultures + AddCultureCommand + DeleteCultureCommand
- Tab 2 «الـ Antibiotics»: DataGrid Antibiotics + AddAntibioticCommand + DeleteAntibioticCommand
- Tab 3 «الربط»: ListBox Cultures + ListBox Antibiotics + LinkCommand + UnlinkCommand + DataGrid LinkedAntibiotics
- Tab 4 «النتائج»: DataGrid VisitTests + DataGrid ResultRows + ComboBox AllowedSensitivities + SaveResultCommand + PrintCultureReportCommand
- **InputBindings:** F9 = SaveResultCommand

---

## Prompt #24 — ReceiptPrintingView

### التخطيط
- Grid:
  - Row 0: VisitId + LoadCommand
  - Row 1: DataGrid TestItems
  - Row 2: PrintReceiptCommand + PrintBarcodeCommand
- **InputBindings:** F10 = PrintBarcodeCommand, F11 = PrintReceiptCommand

---

## Prompt #25 — CombinedReportView

### التخطيط
- Grid:
  - Row 0: LoadCommand
  - Row 1: ListBox Tests (مع MoveUp/MoveDown buttons)
  - Row 2: SaveReportOrderCommand

---

## Prompt #26 — BlankReportView

### التخطيط
- Form بسيط: حقول المريض الإلزامية + LoadCommand + PrintBlankCommand

---

## Prompt #27 — ConstantsView

### التخطيط
- DataGrid Items + SeedDefaultsCommand + ReloadCommand + SaveCommand + DeleteCommand

---

## Prompt #28 — CompareWithHistoryView

### التخطيط
- Grid:
  - Row 0: LabId + LoadPatientCommand + LoadHistoryCommand + ClearCommand
  - Row 1: DataGrid Tests + DataGrid HistoryResults (مقارنة جنباً إلى جنب)

---

## Prompt #29 — GroupWorksheetView

### التخطيط
- Grid:
  - Row 0: ListBox Groups + ListBox CustomGroups + LoadGroupsCommand
  - Row 1: DataGrid Rows + LoadWorksheetCommand + PrintCommand

---

## Prompt #30 — TestClassificationLogView

### التخطيط
- DataGrid Items + LoadCommand + PrintCommand

---

## Prompt #31 — ExternalLabManagementView

### التخطيط (5 Sections)
- Section 1: ListBox Referrals + LoadReferralsCommand
- Section 2: DataGrid PendingQueue + LoadQueueCommand + CreateManifestCommand
- Section 3: DataGrid Manifests + LoadManifestsCommand + UpdateStatusCommand
- Section 4: DataGrid SettlementHistory + LoadSettlementCommand + CreateSettlementCommand
- Section 5: EnterExternalResultCommand + PrintExternalReportCommand

---

## Prompt #32 — AttendanceReportView

### التخطيط
- Grid:
  - Row 0: ListBox Users + LoadUsersCommand + DatePicker From/To
  - Row 1: GenerateReportCommand + GeneratePayrollSummaryCommand + ClearFilterCommand
  - Row 2: TabControl: ReportRows / PayrollRows

---

## Prompt #33 — ContractInvoiceView

### التخطيط
- Grid:
  - Row 0: ListBox Referrals + LoadReferralsCommand
  - Row 1: DataGrid PendingInvoices + LoadPendingCommand
  - Row 2: CreateInvoiceCommand + LoadHistoryCommand + SettleSelectedCommand
  - Row 3: DataGrid ContractInvoices

---

## Prompt #34 — UserActivityLogView

### التخطيط
- DataGrid Items + LoadCommand

---

## Prompt #35 — SystemUsageMonitorView

### التخطيط
- DataGrid Sessions + RefreshCommand

---

## Prompt #36 — PatientBillingByDateView

### التخطيط
- Grid:
  - Row 0: PatientId TextBox + DatePicker FromDate + DatePicker ToDate + SearchCommand
  - Row 1: Totals (TotalInvoiced, TotalPaid, Balance)
  - Row 2: TabControl: Invoices DataGrid / Payments DataGrid

---

## Prompt #37 — Update App.xaml — إضافة DataTemplates للـ 37 ViewModel

### المطلوب
إضافة 37 DataTemplate جديدة في App.xaml تربط كل ViewModel بـ View XAML المُنشأ.

### النمط
```xml
<DataTemplate DataType="{x:Type vm:PatientRegistrationViewModel}">
    <patientsViews:PatientRegistrationView />
</DataTemplate>
```

### المخطوطات (Namespaces) المطلوبة
- xmlns:vm، patientsVm، accountsVm، إلخ (موجودة بالفعل)
- xmlns لـ Views الجديدة المُنشأة

---

## Prompt #38 — تصحيح MainViewModel.OpenPlaceholder

### السبب
بعد إضافة Views حقيقية، لن نحتاج PlaceholderView لكل وظيفة. يجب توجيه Hub buttons إلى Navigate*Command بدلاً من _openPlaceholder.

### المطلوب
تعديل كل Module ViewModels بحيث:
- بدلاً من `_openPlaceholder("اضافة وتعديل بيانات المرضى")`
- استخدم: `_navigationService.Navigate(NavigationTarget.PatientRegistration)`
- أو حقن `MainViewModel.NavigatePatientRegistrationCommand` عبر constructor

### بديل
تعديل MainViewModel.OpenPlaceholder ليتعرف على عنوان الوظيفة ويوجهها إلى NavigationTarget المناسب.

---

## معايير القبول العامة لكل Prompt

1. **لا code-behind** سوى constructor
2. **DataContext** يأتي من DataTemplate في App.xaml تلقائياً (لا تضبط DataContext في constructor)
3. **FlowDirection=RightToLeft** على المستوى الجذر
4. **استخدام Resources الموحَّدة** من الملف 07
5. **كل Binding يطابق** Property/Command موجودة فعلاً في الـ ViewModel المرجعي
6. **InputBindings** للاختصارات الإلزامية (راجع 06)
7. **Styles محلية** عند الحاجة فقط
8. **عدم تكرار** ModuleButtonStyle — يجب نقله إلى ResourceDictionary مشتركة

---

## ملخص

- **40 Prompt** يجب تنفيذها (8 Hubs موجودة، 32 View جديدة + DataTemplates + تعديل MainViewModel)
- **37 ViewModel موجود** بانتظار View
- **0 DataTemplate** جديدة مضافة حتى الآن
- **0 KeyBinding** موجود — كلها يجب إضافتها

---
