# 04 — UI Control Inventory — Open lab system (Final Independent Audit)

> جرد كامل لكل عناصر التحكم في كل شاشة، مع المصدر (Repo Property / PDF reference).

---

## 0.1 LoginView — جرد التحكم الكامل

ملف: `Open_lab/Views/LoginView.xaml` (207 سطر)
ViewModel: `Open_lab/ViewModels/LoginViewModel.cs` (151 سطر)

### العناصر المرئية بترتيبها في XAML

| # | النوع | الـ Binding / النص الثابت | الموقع | السطر | الغرض |
|---|---|---|---|---|---|
| 1 | TextBlock | "تسجيل الدخول" | Row 0 — Header | ~117 | عنوان رئيسي بـ FontSize=26 FontWeight=ExtraBold |
| 2 | TextBlock | "Open Lab System" | Row 0 — Subtitle | ~122 | شعار النظام |
| 3 | TextBlock | "اسم المستخدم" | Row 1 — Label | ~131 | لافتة فوق حقل الاسم |
| 4 | TextBox | `{Binding Username, UpdateSourceTrigger=PropertyChanged}` | Row 1 | ~132 | إدخال اسم المستخدم |
| 5 | TextBlock | "كلمة المرور" | Row 1 — Label | ~135 | لافتة فوق حقل المرور |
| 6 | PasswordBox | `behaviors:PasswordBoxAssistant.BindPassword=True` + `BoundPassword={Binding Password, Mode=TwoWay}` | Row 1 — Grid | ~138 | إدخال آمن (مخفي عندما IsPasswordVisible=True) |
| 7 | TextBox | `{Binding Password}` | Row 1 — Grid (overlay) | ~153 | إظهار كلمة المرور كنص عندما IsPasswordVisible=True |
| 8 | Button (Eye) | `Command={Binding TogglePasswordVisibilityCommand}` Style=EyeButtonStyle | داخل Grid PasswordBox | ~170 | إظهار/إخفاء كلمة المرور (أيقونة `&#xE723;` ⇄ `&#xE7B3;`) |
| 9 | TextBlock | "تذكر بياناتي" | Row 1 — Footer | ~187 | لافتة CheckBox |
| 10 | CheckBox | `IsChecked={Binding RememberMe}` FlowDirection=LeftToRight | Row 1 — Footer | ~189 | تذكر اسم المستخدم |
| 11 | TextBlock | `{Binding StatusMessage}` | Row 2 | ~194 | رسالة خطأ / حالة (Foreground=#D13438) |
| 12 | Button | Content="دخول" IsDefault=True `Command={Binding LoginCommand}` | Row 3 | ~202 | تنفيذ تسجيل الدخول |

### Properties المرتبطة في LoginViewModel
- `Username : string` (TwoWay, default empty)
- `Password : string` (TwoWay, default empty)
- `RememberMe : bool` (TwoWay, default false إلا إذا كان هناك remembered username)
- `StatusMessage : string` (OneWay readonly — private set)
- `IsPasswordVisible : bool` (TwoWay)
- `IsBusy : bool` (OneWay readonly — private set, يعطّل LoginCommand)

### Commands
- `LoginCommand : ICommand` — async، يُنفّذ `LoginAsync` ⇒ `_authService.ValidateCredentialsAsync` ⇒ `_onLoginSuccess()`
- `TogglePasswordVisibilityCommand : ICommand` — تبديل قيمة `IsPasswordVisible`

### الموارد المُعرَّفة محلياً في LoginView.xaml
- `BoolToVis` — BooleanToVisibilityConverter
- `ModernInputField` — Style لـ TextBox/PasswordBox (Height=40, FontSize=14, Border CornerRadius=6, IsFocused ⇒ BorderBrush=#106EBE BorderThickness=2)
- `LoginButtonStyle` — Background=#106EBE, Height=45, IsMouseOver=#005A9E, IsPressed=#004578, IsEnabled=False ⇒ #C8C8C8
- `EyeButtonStyle` — Width=35, Transparent, أيقونة `&#xE723;` FontFamily=Segoe MDL2 Assets

### Status: ✅ مكتملة بالكامل (Live)

---

## 0.2 MainWindow — جرد التحكم الكامل

ملف: `Open_lab/Views/MainWindow.xaml` (209 سطر)
ViewModel: `Open_lab/ViewModels/MainViewModel.cs` (464 سطر)

### Row 0 — شريط الأدوات العلوي (Border Height=90)

UniformGrid Columns=12 FlowDirection=RightToLeft، Visibility=`{Binding IsToolbarVisible, Converter={StaticResource BoolToVisibility}}`

| # | النوع | GroupName | أيقونة Segoe MDL2 | النص | الـ Command |
|---|---|---|---|---|---|
| 1 | RadioButton | TopModules | `&#xE716;` | المرضى | NavigateToPatientsCommand |
| 2 | RadioButton | TopModules | `&#xE90F;` | أدوات | NavigateToToolsCommand |
| 3 | RadioButton | TopModules | `&#xE70B;` | ورقة عمل | NavigateToWorksheetCommand |
| 4 | RadioButton | TopModules | `&#xE8C7;` | حسابات | NavigateToAccountsCommand |
| 5 | RadioButton | TopModules | `&#xE9D2;` | احصاليات | NavigateToStatisticsCommand |
| 6 | RadioButton | TopModules | `&#xE716;` | المستخدمين | NavigateToUsersCommand |
| 7 | RadioButton | TopModules | `&#xE7F0;` | بيانات النظام | NavigateToSystemDataCommand |
| 8 | RadioButton | TopModules | `&#xE713;` | اعدادات | NavigateToSettingsCommand |
| 9 | RadioButton | TopModules | `&#xE77B;` | الموظفين | NavigateToEmployeesCommand |
| 10 | RadioButton | TopModules | `&#xE946;` | هل تعلم | NavigateToDidYouKnowCommand |
| 11 | RadioButton | TopModules | `&#xE946;` | نبذة | NavigateToAboutCommand |
| 12 | Button | (مفرد) | `&#xE8BB;` | خروج | LogoutCommand |

**ملاحظة:** أيقونتي «المرضى» و«المستخدمين» متطابقتان (`E716`)، وأيقونتي «هل تعلم» و«نبذة» متطابقتان (`E946`).

### Row 1 — منطقة المحتوى

| النوع | الـ Binding | الغرض |
|---|---|---|
| Border | Background=#F7FAF7 | حاوية بيضاء |
| ContentControl | `Content={Binding CurrentView}` | عرض الـ View النشطة |

### Row 2 — شريط الحالة (Border Padding=14,0)

DockPanel LastChildFill=False, StackPanel DockPanel.Dock=Right Orientation=Horizontal

| # | النوع | الـ Binding / النص | الغرض |
|---|---|---|---|
| 1 | TextBlock | `&#xE77B;` FontFamily=Segoe MDL2 | أيقونة شخص |
| 2 | TextBlock | "اسم المستخدم: " | لافتة |
| 3 | TextBlock | `{Binding CurrentUser}` | اسم المستخدم الحالي |
| 4 | TextBlock | "آخر دخول: " | لافتة |
| 5 | TextBlock | `{Binding LastLoginDate}` | تاريخ آخر دخول |
| 6 | TextBlock | "قاعدة البيانات: " | لافتة |
| 7 | TextBlock | "متصل" (نص ثابت) Foreground=#BDE5C8 | حالة DB (مكتوبة حرفياً — ليست Binding) |
| 8 | TextBlock | "اليوم: " | لافتة |
| 9 | TextBlock | `{Binding CurrentDate}` | التاريخ الحالي |

### Properties في MainViewModel للـ MainWindow

| Property | النوع | Read/Write | الوصف |
|---|---|---|---|
| CurrentView | object | OneWay (private set) | الـ View النشطة |
| CurrentViewModel | BaseViewModel | OneWay readonly | proxy لـ _navigationService.CurrentViewModel |
| CurrentUser | string | OneWay (private set) | اسم المستخدم في StatusBar |
| LastLoginDate | string | OneWay (private set) | تاريخ آخر دخول |
| CurrentDate | string | OneWay (private set) | "yyyy/MM/dd" |
| ActiveModule | string | OneWay (private set) | اسم الموديول النشط (للـ logic) |
| IsToolbarVisible | bool | OneWay (private set) | يخفي/يُظهر الـ Toolbar (يُخفى في PlaceholderView) |
| IsLoggedIn | bool | OneWay (private set) | يفعّل/يعطّل كل Navigate Commands |

### Commands — 50 إجمالاً (راجع الملف 03 للتفاصيل الكاملة)

### Status: ✅ مكتملة بالكامل

---

## 0.3 WelcomeView — جرد التحكم

ملف: `Open_lab/Views/WelcomeView.xaml` (84 سطر)
ViewModel: `Open_lab/ViewModels/WelcomeViewModel.cs` (17 سطر)

| # | النوع | الموقع | اللون | الغرض |
|---|---|---|---|---|
| 1 | TextBlock "MS SQL SERVER" | Top-Left | #4F5F5C | علامة DB |
| 2 | Ellipse 260×260 | Canvas (130,25) | #1F4D3A Opacity=0.96 | دائرة شعار كبيرة |
| 3 | TextBlock "☘" FontSize=78 | Canvas (214,91) | #DDEFE4 | شعار البرسيم |
| 4 | Ellipse 170×170 | Canvas (64,145) | #2C6B4F | دائرة "Open" |
| 5 | TextBlock "Open" FontSize=30 | Canvas (106,203) | White | نص |
| 6 | Ellipse 150×150 | Canvas (255,145) | #3E7E58 | دائرة "Lab" |
| 7 | TextBlock "Lab" FontSize=30 | Canvas (310,197) | White | نص |
| 8 | Ellipse 120×120 | Canvas (205,220) | #5C9A68 | دائرة "Sys" |
| 9 | TextBlock "Sys" FontSize=25 | Canvas (247,260) | White | نص |
| 10 | TextBlock "Open Lab System" | Bottom-Right | #41624C | تذييل |

**Properties في WelcomeViewModel:**
- `ModuleName : string` (read-only) — يُمرَّر من المُنشئ، لا يُعرض في الـ XAML الحالية

### Status: ✅ Static visual (لا Bindings فعّالة بين XAML والـ ViewModel)

---

## 0.4 PlaceholderView — جرد التحكم

ملف: `Open_lab/Views/Shared/PlaceholderView.xaml` (40 سطر)
Code-behind: `Open_lab/Views/Shared/PlaceholderView.xaml.cs` (25 سطر)
**لا ViewModel.** يستخدم `DataContext=this` مع Properties في code-behind.

| # | النوع | الـ Binding | الغرض |
|---|---|---|---|
| 1 | TextBlock | `{Binding FunctionTitle}` | عنوان الوظيفة (FontSize=30 Bold Foreground=#203B2B) |
| 2 | TextBlock | "نافذة مؤقتة للوظيفة، وسيتم استكمال محتواها لاحقاً." | نص ثابت |
| 3 | Button | Content="رجوع" `Command={Binding BackCommand}` | العودة للـ Hub السابق (Background=#234B67, White, 150×42) |

**Properties في PlaceholderView.xaml.cs:**
- `FunctionTitle : string` (get only)
- `BackCommand : ICommand?` (get only)

### Status: ✅ مكتمل (Anti-Pattern من حيث MVVM)

---

## 1.1 PatientModuleView — جرد التحكم

ملف: `Open_lab/Views/Patients/PatientModuleView.xaml` (123 سطر)
ViewModel: `Open_lab/ViewModels/Patients/PatientModuleViewModel.cs` (27 سطر)

### Row 0 — Info Card (215 px)

| # | النوع | المحتوى | اللون / الحجم |
|---|---|---|---|
| 1 | Ellipse 126×126 | شعار قطرة دم | Fill=#E7F2F4 Stroke=#B5CDD2 StrokeThickness=2 |
| 2 | TextBlock "AB-" | داخل الدائرة | Foreground=#A71921 FontSize=27 Bold |
| 3 | TextBlock `&#xE91D;` | أيقونة دم Segoe MDL2 | Foreground=#376C78 FontSize=54 |
| 4 | Path M55,150 L205,75 | سهم سحب | Stroke=#2C6B4F StrokeThickness=8 RoundCaps |
| 5 | Path (مثلث) | رأس السهم | Fill=#C0392B |
| 6 | TextBlock "من خلال هذه النافذة يمكنك عمل الآتي:" | Header | Foreground=#D56E13 FontSize=20 Bold |
| 7-10 | 4 TextBlocks | bullets | Foreground=#111 FontSize=16 |

### Row 1 — Buttons Grid (2×2)

| # | الـ Command | النص | أيقونة Segoe MDL2 |
|---|---|---|---|
| 1 | OpenAddPatientCommand | "اضافة وتعديل بيانات المرضى" | `&#xE95E;` |
| 2 | OpenEnterResultsCommand | "ادخال نتائج التحاليل" | `&#xE70B;` |
| 3 | OpenDeliverResultsCommand | "تسليم نتائج المرضى" | `&#xE7C3;` |
| 4 | OpenSearchPatientCommand | "بحث عن مريض" | `&#xE721;` |

### Status: 🟡 Hub موجود، 4 placeholders

---

## 2.1 SystemDataModuleView — جرد التحكم

ملف: `Open_lab/Views/SystemData/SystemDataModuleView.xaml` (118 سطر)
ViewModel: `Open_lab/ViewModels/SystemData/SystemDataModuleViewModel.cs` (50 سطر)

### Row 0 — Info Card

| # | النوع | المحتوى |
|---|---|---|
| 1 | Rectangle 150×120 | Fill=#C6B79A Stroke=#7D6E55 (خلفية الأرشيف) |
| 2-4 | 3 Rectangle 132×26 | رفوف الأرشيف Fill=#E7D8B8 |
| 5 | TextBlock `&#xE8B7;` | أيقونة كتاب Foreground=#5B4E3B FontSize=42 |
| 6 | TextBlock "من خلال هذه النافذة..." | Header |
| 7-10 | 4 TextBlocks | bullets |

### Row 1 — Buttons (14 إجمالاً)

| # | الـ Command | النص | اللون |
|---|---|---|---|
| 1 | OpenTestDataCommand | "بيانات التحاليل" | #8DB600 |
| 2 | OpenBarcodeTypesCommand | "Barcode Types" | #8DB600 |
| 3 | OpenCultureAntibioticsCommand | "Culture Antibiotics" | #8DB600 |
| 4 | OpenTestGroupsCommand | "مجموعات التحاليل" | #8DB600 |
| 5 | OpenTestUnitsCommand | "Test Units" | #8DB600 |
| 6 | OpenTestCommentsCommand | "Test Comments" | #8DB600 |
| 7 | OpenLabBranchesCommand | "Lab. branches" | #8DB600 |
| 8 | OpenPatientTitlesCommand | "القاب وتعريفات المرضى" | #8DB600 |
| 9 | OpenCustomGroupsCommand | "Custom Groups" | #8DB600 |
| 10 | OpenWorkGroupsLogCommand | "مجموعات العمل (Log)" | #8DB600 |
| 11 | OpenPrintPriceListCommand | "طباعة قائمة اسعار التحاليل" | #8DB600 |
| 12 | OpenLabEquipmentCommand | "أجهزة ومعدات المعمل" | #87CEEB (مميَّز) |
| 13 | OpenExternalEntitiesCommand | "الجهات الخارجية والمعدل" | #FF8C00 (برتقالي) |
| 14 | OpenExternalPriceListsCommand | "قائمة أسعار التحاليل للجهات" | #FF8C00 |

### Status: 🟡 Hub موجود، 14 placeholders

---

## 3.1 AccountsModuleView — جرد التحكم

ملف: `Open_lab/Views/Accounts/AccountsModuleView.xaml` (113 سطر)
ViewModel: `Open_lab/ViewModels/Accounts/AccountsModuleViewModel.cs` (27 سطر)

| # | الـ Command | النص | أيقونة |
|---|---|---|---|
| 1 | OpenInventoryCommand | "الجرد وحساب الدرج" | `&#xE72B;` |
| 2 | OpenExternalSamplesCommand | "العينات المرسلة للخارج" | `&#xE8C0;` |
| 3 | OpenCashTransactionCommand | "صرف وإيداع نقدية" | `&#xE825;` |
| 4 | OpenCompanyAccountsCommand | "حساب شركات ومندوبين" | `&#xE8D5;` |

Hero icon: `&#xE825;` (خزينة Foreground=#376C78 FontSize=64)

### Status: 🟡 Hub موجود، 4 placeholders

---

## 4.1 WorksheetModuleView — جرد التحكم

ملف: `Open_lab/Views/Worksheet/WorksheetModuleView.xaml` (97 سطر)
ViewModel: `Open_lab/ViewModels/Worksheet/WorksheetModuleViewModel.cs` (21 سطر)

| # | الـ Command | النص | أيقونة |
|---|---|---|---|
| 1 | OpenWorksheetByPatientCommand | "ورقة عمل بأسماء المرضى" | `&#xE9F9;` |
| 2 | OpenWorksheetByTestCommand | "ورقة عمل بأسماء التحاليل (Log)" | `&#xE9F9;` |

Hero icon: `&#xE81C;` (FontSize=64)

### Status: 🟡 Hub موجود، 2 placeholders

---

## 5.1 StatisticsModuleView — جرد التحكم

ملف: `Open_lab/Views/Statistics/StatisticsModuleView.xaml` (127 سطر)
ViewModel: `Open_lab/ViewModels/Statistics/StatisticsModuleViewModel.cs` (29 سطر)

| # | الـ Command | النص | أيقونة |
|---|---|---|---|
| 1 | OpenPatientCountStatCommand | "احصاليات وفقاً لعدد المرضى" | `&#xE9D2;` |
| 2 | OpenTestCountStatCommand | "احصاليات وفقاً لعدد التحاليل" | `&#xE9D2;` |
| 3 | OpenBranchStatCommand | "احصاليات خاصة بفروع المعمل" | `&#xE9D2;` |
| 4 | OpenExternalSamplesStatCommand | "احصاليات العينات المرسلة" | `&#xE9D2;` |
| 5 | OpenWorkPerformanceStatCommand | "احصاليات تقيم ومتابعة العمل" | `&#xE9D2;` |
| 6 | OpenResultsMonitorCommand | "متابعة ومراقبة النتائج" | `&#xE9D2;` |

Hero icon: `&#xE9D2;` (FontSize=64) — كل الأيقونات الستة متطابقة

### Status: 🟡 Hub موجود، 6 placeholders

---

## 6.1 SettingsModuleView — جرد التحكم

ملف: `Open_lab/Views/Settings/SettingsModuleView.xaml` (96 سطر)
ViewModel: `Open_lab/ViewModels/Settings/SettingsModuleViewModel.cs` (19 سطر)

| # | الـ Command | النص | أيقونة |
|---|---|---|---|
| 1 | OpenSystemSettingsCommand | "اعدادات النظام" | `&#xE713;` |
| 2 | OpenDatabaseMaintenanceCommand | "Database Maintenance" | `&#xE7B7;` |

Hero icon: `&#xE713;` (ترس FontSize=64)

### Status: 🟡 Hub موجود، 2 placeholders

---

## 7.1 ToolsModuleView — جرد التحكم

ملف: `Open_lab/Views/Tools/ToolsModuleView.xaml` (152 سطر)
ViewModel: `Open_lab/ViewModels/Tools/ToolsModuleViewModel.cs` (33 سطر)

| # | الـ Command | النص | أيقونة |
|---|---|---|---|
| 1 | OpenTestLibraryCommand | "مكتبة التحاليل" | `&#xEA86;` |
| 2 | OpenStopwatchCommand | "ساعة التوقيت Stopwatch" | `&#xE916;` |
| 3 | OpenRequirementsListCommand | "قائمة المطلوبات والمشتريات" | `&#xE7BF;` |
| 4 | OpenImageLibraryCommand | "مكتبة الصور" | `&#xEB9F;` |
| 5 | OpenUnitConverterCommand | "محول وحدات نتائج التحاليل" | `&#xE8EF;` |
| 6 | OpenAppointmentsCommand | "نونة المواعيد" | `&#xE787;` |
| 7 | OpenAbbreviationsDictionaryCommand | "قاموس لاختصارات" | `&#xE8D2;` |
| 8 | OpenCalculatorCommand | "الآلة الحاسبة" | `&#xED1E;` |
| 9 | OpenPhoneDirectoryCommand | "دليل الهاتف" | `&#xE77B;` |

Hero icon: `&#xED43;` (مفتاح ربط FontSize=64)
ModuleButtonStyle: 200×65 (الأصغر بعد SystemData)

### Status: 🟡 Hub موجود، 9 placeholders

---

## 8.1 UsersModuleView — جرد التحكم

ملف: `Open_lab/Views/Users/UsersModuleView.xaml` (113 سطر)
ViewModel: `Open_lab/ViewModels/Users/UsersModuleViewModel.cs` (27 سطر)

| # | الـ Command | النص | أيقونة |
|---|---|---|---|
| 1 | OpenCreateUsersCommand | "انشاء مستخدمين" | `&#xEE40;` |
| 2 | OpenChangePasswordCommand | "تغيير كلمة المرور" | `&#xE8D7;` |
| 3 | OpenAttendanceCommand | "الحضور والإنصراف" | `&#xE7BE;` |
| 4 | OpenLoginDetectorCommand | "Login detector" | `&#xE722;` |

Hero icon: `&#xE77B;` (شخص FontSize=64)

### Status: 🟡 Hub موجود، 4 placeholders

---

## ViewModels بدون View XAML — جرد Properties و Commands

### PatientRegistrationViewModel
**Properties:** PatientId, LabId, FullName, Gender, BirthDate, Phone, Address, ChronicDiseases, Allergies, Medications, MedicalNotes, StatusMessage, Results (ObservableCollection&lt;Patient&gt;), Referrals (ObservableCollection&lt;Referral&gt;), SelectedReferral, SelectedPatient
**Commands:** SaveCommand, NewCommand, GenerateLabIdCommand, LoadByLabIdCommand, SearchCommand, DeleteCommand

### PatientTestsSelectionViewModel
**Properties:** LabId, PatientName, PatientId, VisitId, StatusMessage, SelectedAvailableTest, SelectedVisitTest, SelectedAccountType, SelectedReferral, SearchText, SelectedCustomGroup, TotalAmount, AvailableTests, FilteredAvailableTests, SelectedTests, Referrals, AccountTypes, CustomGroups
**Commands:** LoadPatientCommand, CreateVisitCommand, AddTestCommand, AddCustomGroupCommand, RemoveTestCommand, RefreshTestsCommand

### PatientBillingViewModel
**Properties:** VisitId, Total, Discount, Paid, EditPaymentAmount, NetTotal, Balance, NewChargeAmount, NewChargeDescription, StatusMessage, Reason, PaymentMethod, SelectedPayment, Payments, AdditionalCharges
**Commands:** LoadVisitCommand, SaveInvoiceCommand, AddPaymentCommand, EditPaymentCommand, DeletePaymentCommand, AddChargeCommand, SettleAccountCommand, PrintInvoiceCommand

### PatientBillingByDateViewModel
**Properties:** PatientId, FromDate, ToDate, TotalInvoiced, TotalPaid, Balance, IsBusy, Invoices, Payments
**Commands:** SearchCommand

### ResultsEntryViewModel
**Properties:** DateFrom, DateTo, SelectedVisitTest, StatusMessage, MedicalHistorySummary, HasMedicalAlerts, VisitTests, ResultItems
**Commands:** LoadVisitTestsCommand, SaveResultsCommand, VerifyResultsCommand, ReopenResultsCommand

### ReportViewerViewModel
**Properties:** VisitId, StatusMessage, Report, PreviewPdfPath, PreviewPdfUri, PreviewContent (computed), Tests
**Commands:** LoadReportCommand, PrintCommand, ReprintCommand

### PatientSearchViewModel
**Properties:** Name, Phone, LabId, Date, StatusMessage, SelectedPatient, Patients, Visits
**Commands:** SearchCommand

### PatientHistoryViewModel
**Properties:** LabId, From, To, StatusMessage, History, Visits
**Commands:** LoadHistoryCommand, PrintHistoryCommand

### DashboardViewModel
**Properties:** PatientCount, VisitCount, TestCount
**Commands:** (لا توجد — تتحمَّل بشكل تلقائي عبر `_ = LoadAsync()`)

### TestCatalogViewModel
**Properties:** Tests, Groups, SampleTypes, Units
**Commands:** SaveCommand, NewCommand, DeleteCommand, ReloadCommand, GenerateBarcodeCommand

### ReferenceRangesViewModel
**Properties:** Tests, Ranges
**Commands:** LoadCommand, SaveCommand, DeleteCommand

### TestCommentsViewModel
**Properties:** Tests, Comments
**Commands:** LoadCommand, SaveCommand, DeleteCommand

### PriceListsViewModel
**Properties:** PriceLists, Items, Tests, Referrals
**Commands:** LoadCommand, SaveListCommand, UpdateListCommand, AddItemCommand, UpdateItemCommand, DeleteItemCommand, PrintListCommand

### CustomGroupsViewModel
**Properties:** Groups, GroupItems, Tests
**Commands:** SaveGroupCommand, AddItemCommand, DeleteItemCommand

### ReferralsViewModel
**Properties:** Referrals
**Commands:** SaveCommand, DeleteCommand

### UsersPermissionsViewModel
**Properties:** Users, Roles, Permissions
**Commands:** SaveUserCommand, DeleteUserCommand, SaveRoleCommand, DeleteRoleCommand, SaveRolePermissionsCommand, AssignRoleCommand, UnassignRoleCommand, ReloadCommand (8 Commands)

### StatisticsViewModel
**Properties:** Genders, Referrals (StatisticsReferralLookup), ByGender, ByReferral, MonthlyAnalysis, TopTests, YearlySamples, UserProductivity
**Commands:** LoadCommand, PrintCommand

### SystemSettingsViewModel
**Properties:** Settings
**Commands:** SaveProfileCommand, ReloadCommand, SaveRawSettingCommand, DeleteRawSettingCommand, ChangeMasterPasswordCommand

### BackupRestoreViewModel
**Properties:** BackupFiles
**Commands:** BackupCommand, RestoreCommand, LoadBackupsCommand, ConfigureScheduleCommand, DisableScheduleCommand, RefreshScheduleCommand

### AttendanceLogViewModel
**Properties:** Logs, Breaks
**Commands:** LoadLogsCommand, ClockInCommand, ClockOutCommand, StartBreakCommand, EndBreakCommand, LoadDailySummaryCommand, RefreshOpenLogCommand

### AccountsTreasuryViewModel
**Properties:** Payments, ByUser, ByReferral, ByBranch, ByDoctor
**Commands:** LoadCommand, DailyCommand, WeeklyCommand, MonthlyCommand, PrintCommand

### DeliveryViewModel
**Properties:** Visits
**Commands:** SearchCommand, DeliverCommand, ReopenCommand

### SampleCollectionViewModel
**Properties:** Items
**Commands:** LoadCommand, MarkCollectedCommand, MarkExternalCollectedCommand, MarkSeparatedCommand, MarkNotCollectedCommand, RefreshSampleStatusCommand

### CultureSensitivityViewModel (الأكبر — 548 سطر)
**Properties:** Cultures, Antibiotics, LinkedAntibiotics, VisitTests, ResultRows, AllowedSensitivities (6 collections)
**Commands:** LoadCommand, AddCultureCommand, DeleteCultureCommand, AddAntibioticCommand, DeleteAntibioticCommand, LinkCommand, UnlinkCommand, LoadVisitTestsCommand, SaveResultCommand, PrintCultureReportCommand (10 Commands)

### ReceiptPrintingViewModel
**Properties:** TestItems
**Commands:** LoadCommand, PrintReceiptCommand, PrintBarcodeCommand

### CombinedReportViewModel
**Properties:** Tests
**Commands:** LoadCommand, MoveUpCommand, MoveDownCommand, SaveReportOrderCommand

### BlankReportViewModel
**Commands:** LoadCommand, PrintBlankCommand

### ConstantsViewModel
**Properties:** Items (Setting)
**Commands:** SeedDefaultsCommand, ReloadCommand, SaveCommand, DeleteCommand

### CompareWithHistoryViewModel
**Properties:** Tests, HistoryResults
**Commands:** LoadPatientCommand, LoadHistoryCommand, ClearCommand

### GroupWorksheetViewModel
**Properties:** Groups, CustomGroups, Rows
**Commands:** LoadGroupsCommand, LoadWorksheetCommand, PrintCommand

### TestClassificationLogViewModel
**Properties:** Items (ReagentConsumptionReport)
**Commands:** LoadCommand, PrintCommand

### ExternalLabManagementViewModel
**Properties:** Referrals, PendingQueue, Manifests, SettlementHistory, SelectedQueueIds
**Commands:** LoadReferralsCommand, LoadQueueCommand, LoadManifestsCommand, CreateManifestCommand, UpdateStatusCommand, LoadSettlementCommand, CreateSettlementCommand, EnterExternalResultCommand, PrintExternalReportCommand (9 Commands)

### AttendanceReportViewModel
**Properties:** Users, ReportRows, PayrollRows
**Commands:** LoadUsersCommand, GenerateReportCommand, GeneratePayrollSummaryCommand, ClearFilterCommand

### ContractInvoiceViewModel
**Properties:** Referrals, PendingInvoices, ContractInvoices
**Commands:** LoadReferralsCommand, LoadPendingCommand, CreateInvoiceCommand, LoadHistoryCommand, SettleSelectedCommand

### UserActivityLogViewModel
**Properties:** Items
**Commands:** LoadCommand

### SystemUsageMonitorViewModel
**Properties:** Sessions
**Commands:** RefreshCommand

### PhysicianViewModel
**Properties:** Physicians, PriceLists
**Commands:** LoadPhysiciansCommand, SaveCommand, NewCommand, SearchCommand
**ملاحظة:** ليس له NavigationTarget، فقد يكون يُستخدم داخلياً من VM آخر.

### SettingsViewModel
**Properties:** Settings (SystemSetting), AvailablePrinters
**Commands:** LoadSettingsCommand, SavePrinterSettingsCommand, SaveMarginSettingsCommand, RefreshPrintersCommand

---

## ShellWindow.xaml (Dead Code) — جرد التحكم

ملف: `Open_lab/Shell/ShellWindow.xaml` (74 سطر)
**غير مستخدم** — `grep -rn "ShellWindow"` يُرجع فقط `ShellWindow.xaml.cs` (التعريف).

### الـ 38 زر التنقل الموجودة في Sidebar
الـ Sidebar يحوي `ScrollViewer Background=#2B2B2B Width=240`، و Visibility مربوطة بـ `{Binding IsLoggedIn, Converter={StaticResource BoolToVisibility}}` — لكن المُحوِّل BoolToVisibility غير معرَّف في Window.Resources الخاصة بـ ShellWindow! يعتمد على Application.Resources.

| القسم | الأزرار |
|---|---|
| الأساسية (11) | NavigateDashboardCommand, NavigatePatientRegistrationCommand, NavigatePatientTestsCommand, NavigatePatientBillingCommand, NavigatePatientBillingByDateCommand, NavigateResultsEntryCommand, NavigateReportViewerCommand, NavigateCombinedReportCommand, NavigateBlankReportCommand, NavigateReceiptPrintingCommand, NavigateCultureSensitivityCommand |
| بحث وتاريخ (3) | NavigatePatientSearchCommand, NavigatePatientHistoryCommand, NavigateCompareWithHistoryCommand |
| ورقة العمل (4) | NavigateWorkSheetByPatientCommand, NavigateWorkSheetByTestCommand, NavigateGroupWorksheetCommand, NavigateTestClassificationLogCommand |
| الكتالوج (6) | NavigateTestCatalogCommand, NavigateReferenceRangesCommand, NavigateTestCommentsCommand, NavigatePriceListsCommand, NavigateCustomGroupsCommand, NavigateReferralsCommand |
| العمليات (6) | NavigateAttendanceLogCommand, NavigateAccountsTreasuryCommand, NavigateDeliveryCommand, NavigateSampleCollectionCommand, NavigateExternalLabManagementCommand, NavigateContractInvoiceCommand |
| الإدارة (8) | NavigateUsersPermissionsCommand, NavigateUserActivityLogCommand, NavigateSystemUsageMonitorCommand, NavigateAttendanceReportCommand, NavigateStatisticsCommand, NavigateSystemSettingsCommand, NavigateBackupRestoreCommand, NavigateConstantsCommand |
| الإجراءات | LogoutCommand |

**ContentControl:** `<ContentControl Content="{Binding CurrentViewModel}" />` — لاحظ أنه يربط CurrentViewModel وليس CurrentView (يختلف عن MainWindow).

---

## Resources / Converters المرجعية الإجمالية

### في App.xaml
- `BoolToVisibility` — `BooleanToVisibilityConverter`
- `InverseBooleanConverter` — مُعرَّف في ViewModels/InverseBooleanConverter.cs
- `InverseBoolToVisibilityConverter` — مُعرَّف في نفس الملف

### في LoginView.xaml (محلياً)
- `BoolToVis` — BooleanToVisibilityConverter (إعادة تعريف محلية!)
- `ModernInputField` Style
- `LoginButtonStyle`
- `EyeButtonStyle`

### في MainWindow.xaml (محلياً)
- `TopToolbarBrush` — LinearGradientBrush
- `ToolbarRadioButtonStyle`
- `ToolbarExitButtonStyle`
- `ToolbarIconStyle`
- `ToolbarTextStyle`

### في كل ModuleView محلياً
- `ModuleButtonStyle` (إعادة تعريف في كل ملف — لا يوجد ResourceDictionary مشترك)

### Behaviors
- `PasswordBoxAssistant` — Attached Properties: BoundPassword (TwoWay), BindPassword (bool toggle), IsUpdating (internal)

---
