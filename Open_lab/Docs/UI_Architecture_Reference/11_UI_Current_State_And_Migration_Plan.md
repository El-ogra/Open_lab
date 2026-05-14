# 11 — UI Current State And Migration Plan — Open lab system (Final Independent Audit)

> الحالة الحالية الفعلية + خطة الهجرة الكاملة من Placeholder-first إلى View-first.
> الفرع: `Fi5ve`، الكوميت: `011f15c` («بعد حذف الشغل القديم»).

---

# القسم الأول — الوضع الحالي الفعلي

## 1.1 البنية الحالية الكاملة لمجلد المشروع

```
Open_lab/
├── App.xaml                              ✅ يحوي 11 DataTemplate + 3 Converters
├── App.xaml.cs                           ✅ يبني ServiceProvider ويعرض LoginWindow برمجياً
├── App.config                            ✅
├── Open_lab.csproj                       ✅
├── AssemblyInfo.cs                       ✅
├── Behaviors/
│   └── PasswordBoxAssistant.cs           ✅ AttachedProperty للـ PasswordBox binding
├── Shell/
│   ├── ShellWindow.xaml                  ⚠️ موجود لكن غير مستخدم كـ Shell (Sidebar 38 زراً)
│   └── ShellWindow.xaml.cs               ⚠️ غير مستخدم — لا مرجع له في App.xaml.cs
├── Data/
│   ├── AuditInterceptor.cs               ✅
│   ├── OpenLabDbContext.cs               ✅
│   └── OpenLabDbContextFactory.cs        ✅
├── Migrations/                           ✅ 17 ملف Migration (EF Core)
├── Models/                               ✅ Entities + Physician + ReferenceRangeResult + ReportModels + SystemSetting
├── Services/                             ✅ 94 ملف (47 Interface + 47 Implementation)
│   ├── PermissionCodes.cs               ✅ 22 صلاحية محددة
│   ├── MainWindowLayoutService.cs       ✅ يضبط أبعاد النافذة بعد Login
│   └── ... (92 ملف خدمة أخرى)
├── Views/                                ✅ 12 ملف XAML
│   ├── LoginView.xaml                    ✅ منفذة كاملة (207 سطر)
│   ├── LoginView.xaml.cs                 ✅
│   ├── MainWindow.xaml                   ✅ منفذة كاملة (209 سطر)
│   ├── MainWindow.xaml.cs                ✅
│   ├── WelcomeView.xaml                  ✅ Static visual (84 سطر)
│   ├── WelcomeView.xaml.cs               ✅
│   ├── Shared/
│   │   ├── PlaceholderView.xaml          ✅ مشتركة (40 سطر)
│   │   └── PlaceholderView.xaml.cs       ⚠️ Anti-MVVM (DataContext=this)
│   ├── Patients/PatientModuleView.xaml   🟡 Hub فقط (123 سطر)
│   ├── Accounts/AccountsModuleView.xaml  🟡 Hub فقط (113 سطر)
│   ├── SystemData/SystemDataModuleView.xaml  🟡 Hub فقط (118 سطر)
│   ├── Worksheet/WorksheetModuleView.xaml  🟡 Hub فقط (97 سطر)
│   ├── Statistics/StatisticsModuleView.xaml  🟡 Hub فقط (127 سطر)
│   ├── Settings/SettingsModuleView.xaml  🟡 Hub فقط (96 سطر)
│   ├── Tools/ToolsModuleView.xaml        🟡 Hub فقط (152 سطر)
│   └── Users/UsersModuleView.xaml        🟡 Hub فقط (113 سطر)
└── ViewModels/                           ✅ 53 ملف
    ├── BaseViewModel.cs                  ✅ INotifyPropertyChanged
    ├── RelayCommand.cs                   ✅
    ├── AppSession.cs                     ✅ Static session container
    ├── PermissionToggle.cs               ✅
    ├── UiModels.cs                       ✅ DTOs مشتركة
    ├── InverseBooleanConverter.cs        ✅
    ├── NavigationTarget.cs               ✅ 40 قيمة
    ├── NavigationService.cs              ✅
    ├── ViewModelFactory.cs               ✅
    ├── INavigationService.cs             ✅
    ├── IViewModelFactory.cs              ✅
    ├── LoginViewModel.cs                 ✅ (151 سطر)
    ├── MainViewModel.cs                  ✅ (464 سطر) — 50 Command
    ├── WelcomeViewModel.cs               ✅ (17 سطر)
    ├── HomeViewModel.cs                  ✅ (8 سطر)
    ├── DashboardViewModel.cs             🟠 ViewModel only (45 سطر)
    ├── Patients/PatientModuleViewModel.cs  🟡 Hub (27 سطر)
    ├── Accounts/AccountsModuleViewModel.cs  🟡 Hub (27 سطر)
    ├── SystemData/SystemDataModuleViewModel.cs  🟡 Hub (50 سطر)
    ├── Worksheet/WorksheetModuleViewModel.cs  🟡 Hub (21 سطر)
    ├── Statistics/StatisticsModuleViewModel.cs  🟡 Hub (29 سطر)
    ├── Settings/SettingsModuleViewModel.cs  🟡 Hub (19 سطر)
    ├── Tools/ToolsModuleViewModel.cs     🟡 Hub (33 سطر)
    ├── Users/UsersModuleViewModel.cs     🟡 Hub (27 سطر)
    └── [37 Sub-ViewModel آخر] 🟠 كلها ViewModel only بدون View
```

## 1.2 إحصاءات رقمية مؤكَّدة (محسوبة بالـ grep)

| المؤشر | القيمة | الإثبات |
|---|---:|---|
| إجمالي ملفات المشروع | 358 | `find . -type f -not -path './.git/*' \| wc -l` |
| ملفات XAML | 14 | App.xaml + ShellWindow.xaml + 12 ملف في Views/ |
| ملفات XAML تنفّذ Views فعلية | 12 | كل Views/ + Shared/PlaceholderView |
| ملفات .xaml.cs (code-behind) | 14 | كل XAML مقابل |
| ملفات .cs في ViewModels/ | 53 | `ls ViewModels/*.cs \| wc -l` |
| ملفات .cs في Services/ | 94 | يشمل Interfaces |
| Interfaces في Services/ | 47 | بادئة `I` |
| Implementation Classes في Services/ | 47 | بدون بادئة `I` (تقريباً، مع استثناءات مثل PasswordSecurity، PermissionCodes) |
| Migrations EF Core | 17 | في `Migrations/` |
| Models Entity Classes | 5 ملفات | يحوي عشرات Entities |
| ملفات اختبار في Open_lab.Tests/ | 55+ | `ls Open_lab.Tests/ViewModels/*.cs` |
| قيم NavigationTarget enum | **40** | `grep -E "^\s+[A-Z]" NavigationTarget.cs \| wc -l` = 40 |
| Commands في MainViewModel | **50** | `grep -cE "public (ICommand\|RelayCommand)" MainViewModel.cs` = 50 |
| DataTemplates في App.xaml | **11** | عددها يدوياً |
| ModuleView Hubs منفذة | **8** | Patients, SystemData, Accounts, Worksheet, Statistics, Settings, Tools, Users |
| Placeholder hooks في Module ViewModels | **45** | `grep -E '_openPlaceholder\("\|Create\("' الـ 8 ملفات = 45 (4+4+14+2+6+2+9+4) |
| Top-bar RadioButton | 11 | في MainWindow.xaml |
| Top-bar Button (Logout) | 1 | LogoutCommand |
| Sidebar buttons في ShellWindow | 39 | `grep "Button" ShellWindow.xaml` = 39 (38 Navigate + 1 Logout) |
| Navigate*Command في Sidebar | 38 | تطابق NavigationTarget |
| KeyBinding في كل المشروع | **0** | `grep -rn "KeyBinding" Open_lab/` = 0 |
| InputBindings في كل المشروع | **0** | `grep -rn "InputBindings" Open_lab/` = 0 |
| ContextMenu | **0** | `grep -rn "ContextMenu" Open_lab/Views Open_lab/Shell` = 0 |
| MessageBox.Show في ViewModels/Views | **0** | يتم استخدام StatusMessage بدلاً منها |

## 1.3 تحليل حالة الـ Views (12 View)

| Status | Count | الـ Views |
|---|:---:|---|
| ✅ Live & Wired | 4 | LoginView, MainWindow, WelcomeView, PlaceholderView |
| 🟡 Hub-only (Placeholder dispatchers) | 8 | PatientModuleView, AccountsModuleView, SystemDataModuleView, WorksheetModuleView, StatisticsModuleView, SettingsModuleView, ToolsModuleView, UsersModuleView |
| ⚠️ Dead | 1 | ShellWindow.xaml (غير مستخدم) |

## 1.4 تحليل حالة الـ ViewModels (53 ملف)

| Status | Count | التصنيف |
|---|:---:|---|
| ✅ Active & UI-bound | 11 | Login, Main, Welcome + 8 Module hubs |
| 🟠 Defined-but-orphaned (لا View، لا DataTemplate، لكن NavigationTarget موجود) | 37 | كل Sub-ViewModels (PatientRegistration, ResultsEntry, …) |
| 🛠 Infrastructure (لا View — أدوات داعمة) | 5 | BaseViewModel, RelayCommand, AppSession, NavigationService, ViewModelFactory |

## 1.5 تحليل DataTemplates في App.xaml (11 templates)

| ViewModel | View | Status |
|---|---|:---:|
| LoginViewModel | LoginView | ✅ |
| MainViewModel | MainWindow | ✅ |
| WelcomeViewModel | WelcomeView | ✅ |
| PatientModuleViewModel | PatientModuleView | ✅ |
| SystemDataModuleViewModel | SystemDataModuleView | ✅ |
| AccountsModuleViewModel | AccountsModuleView | ✅ |
| WorksheetModuleViewModel | WorksheetModuleView | ✅ |
| StatisticsModuleViewModel | StatisticsModuleView | ✅ |
| SettingsModuleViewModel | SettingsModuleView | ✅ |
| ToolsModuleViewModel | ToolsModuleView | ✅ |
| UsersModuleViewModel | UsersModuleView | ✅ |

⚠️ **37 DataTemplate ناقصة** لكل من: PatientRegistration, PatientTestsSelection, PatientBilling, PatientBillingByDate, ResultsEntry, ReportViewer, PatientSearch, PatientHistory, Dashboard, TestCatalog, ReferenceRanges, TestComments, PriceLists, CustomGroups, Referrals, UsersPermissions, Statistics, SystemSettings, BackupRestore, AttendanceLog, AccountsTreasury, Delivery, SampleCollection, CultureSensitivity, ReceiptPrinting, CombinedReport, BlankReport, Constants, CompareWithHistory, GroupWorksheet, TestClassificationLog, ExternalLabManagement, AttendanceReport, ContractInvoice, UserActivityLog, SystemUsageMonitor, Physician.

## 1.6 ما يعمل فعلياً

✅ **يعمل تماماً:**
- تسجيل الدخول وتوليد الجلسة (LoginView + LoginViewModel + Services)
- تطبيق ApplyLoginLayout (400×550) و ApplyAppLayout (1100×700)
- شريط الأدوات العلوي في MainWindow بـ 11 RadioButton + Logout
- التنقل بين الـ 8 Hubs (PatientModuleView, ...)
- التنقل بين «الموظفين» / «هل تعلم» / «نبذة» (WelcomeView مع ModuleName)
- 45 وظيفة Hub button تفتح PlaceholderView
- الرجوع من PlaceholderView إلى Hub المناسب
- Logout (CloseAttendanceAsync + return to login)
- AppSession + Permissions + AttendanceLog

## 1.7 ما لا يعمل / لم يُنفَّذ

❌ **غير منفذ في UI:**
- إضافة/تعديل/بحث المرضى (PatientRegistration / PatientSearch / PatientHistory)
- إدخال نتائج التحاليل (ResultsEntry)
- إدارة الفواتير (PatientBilling / PatientBillingByDate / ContractInvoice)
- التقارير (ReportViewer / CombinedReport / BlankReport)
- ورقة العمل (WorkSheetByPatient / WorkSheetByTest / GroupWorksheet)
- بيانات النظام (TestCatalog / ReferenceRanges / TestComments / PriceLists / CustomGroups / Referrals)
- المستخدمين والصلاحيات (UsersPermissions)
- الإحصائيات (Statistics)
- إعدادات النظام (SystemSettings / Constants / BackupRestore)
- الحضور (AttendanceLog / AttendanceReport)
- الخزينة (AccountsTreasury)
- التسليم (Delivery / SampleCollection)
- المزارع (CultureSensitivity)
- المعامل الخارجية (ExternalLabManagement)
- اللوحة المعلوماتية (Dashboard)
- سجل النشاط (UserActivityLog / SystemUsageMonitor)
- المقارنة بالتاريخ (CompareWithHistory)

❌ **مفقود في البنية:**
- ResourceDictionary مركزية (Styles مكررة 8 مرات)
- DataTemplates للـ 37 ViewModel
- KeyBindings (0 اختصارات)
- ContextMenu (0)
- MessageBox / Dialog Service
- ربط `CurrentView ← NavigationService.CurrentViewModel` في MainViewModel
- ShellWindow.xaml غير مستخدم (Dead Code)

## 1.8 ما محذوف (بعد كوميت «بعد حذف الشغل القديم»)

من سياق رسالة الكوميت، يُحتمل أن نسخاً سابقة من Views حُذفت. لكن لا يمكن التحقق من ذلك دون مراجعة الكوميت السابق `1c4841e`. الواضح من الكوميت الحالي هو **حالة طلَعة جديدة** (clean slate) للـ Views فقط دون الـ ViewModels.

## 1.9 ما يحتاج إعادة بناء فوراً

| الأولوية | البند | السبب |
|---|---|---|
| 🔴 حرجة | إصلاح Navigation Disconnect | NavigationService.CurrentViewModel لا يربط بـ MainViewModel.CurrentView ⇒ كل Navigate*Commands عاطلة في UI |
| 🔴 حرجة | إنشاء Views للوظائف الـ 45 أو ربطها بـ Navigate Commands بدلاً من Placeholder | المستخدم لا يستطيع فعل شيء فعلي حالياً |
| 🟡 متوسطة | ResourceDictionary مركزية | تكرار ModuleButtonStyle في 8 ملفات |
| 🟡 متوسطة | KeyBindings للاختصارات | 0 اختصار حالياً مقابل 12+ مطلوب |
| 🟢 منخفضة | حذف ShellWindow.xaml أو تفعيله | Dead Code يربك القارئ |
| 🟢 منخفضة | StatusBar «متصل» نص ثابت | يجب أن يكون Binding لحالة DB |

---

# القسم الثاني — خطة الهجرة الكاملة

## 2.1 المبدأ التوجيهي

**التحول من «Placeholder-first» إلى «View-first»:**
- الحالة الحالية: كل Hub button يفتح PlaceholderView. النظام يعمل لكن دون وظائف فعلية.
- الحالة المستهدفة: كل Hub button يفتح Sub-View حقيقية مربوطة بالـ ViewModel المناسبة.

## 2.2 المراحل الكاملة

### المرحلة 0 — التحضير (½ يوم)
- مراجعة هذا المستند + الملفات 01-10
- إنشاء فرع جديد للهجرة (مثلاً `migration/view-first`) — لاحقاً، خارج نطاق هذا التحقيق
- مراجعة المرجعين PDF
- تنفيذ unit tests الحالية للتأكد من خط الأساس

### المرحلة 1 — البنية التحتية (1 يوم)

**الخطوات:**
1. أنشئ `Open_lab/Themes/AppResources.xaml` بـ:
   - كل الـ Brushes (Primary.*, Hub.*, Login.*, Status.*)
   - LinearGradientBrush TopToolbarBrush
   - ModuleButtonStyle الموحَّد
   - ToolbarRadioButtonStyle (نقل من MainWindow.xaml)
   - ModernInputField Style (نقل من LoginView)
   - DataGrid Default Style (جديد)
2. ربطها في App.xaml عبر `<ResourceDictionary.MergedDictionaries>`
3. حذف Resources المكررة من الـ Views الثمانية

**معايير الإنجاز:**
- كل ModuleView يستخدم `Style="{StaticResource ModuleButtonStyle}"` من ResourceDictionary
- 8 ملفات XAML أقصر بـ ~40 سطراً لكل منها

### المرحلة 2 — ربط Navigation (½ يوم)

**الخطوات:**
1. عدِّل `MainViewModel.NavigateTo(NavigationTarget target)`:
   ```csharp
   private void NavigateTo(NavigationTarget target)
   {
       _navigationService.Navigate(target);
       CurrentView = _navigationService.CurrentViewModel;
       IsToolbarVisible = true;
   }
   ```
2. أو بديل: استبدال CurrentView بـ binding مباشر إلى CurrentViewModel في MainWindow.xaml
3. تأكد أن كل Navigate*Command يعمل وفعلاً يغير المحتوى

**معايير الإنجاز:**
- ضغط Navigate*Command (مثلاً NavigatePatientRegistrationCommand) يغير CurrentView إلى PatientRegistrationViewModel

### المرحلة 3 — Views الأساسية (5 أيام)

**ترتيب التنفيذ:**
1. DashboardView (KPI cards) — يوم
2. PatientRegistrationView — يوم
3. PatientSearchView + PatientHistoryView — يوم
4. ResultsEntryView — يوم
5. PatientBillingView + PatientBillingByDateView + PatientTestsSelectionView + ReportViewerView — يوم

**لكل View:**
- أنشئ XAML بمراجعة الملف 02 (Layout) + 04 (Inventory) + 08 (Contract)
- أضف DataTemplate في App.xaml
- اربط KeyBindings (راجع الملف 06)
- اختبر Smoke

### المرحلة 4 — Views بيانات النظام (3 أيام)

1. TestCatalogView + ReferenceRangesView — يوم
2. TestCommentsView + CustomGroupsView — يوم
3. PriceListsView + ReferralsView — يوم

### المرحلة 5 — Views العمليات (4 أيام)

1. AttendanceLogView + AttendanceReportView — يوم
2. AccountsTreasuryView + ContractInvoiceView — يوم
3. DeliveryView + SampleCollectionView + ReceiptPrintingView — يوم
4. ExternalLabManagementView + CultureSensitivityView — يوم (CultureSensitivity أكبر)

### المرحلة 6 — Views الإدارة (3 أيام)

1. UsersPermissionsView — يوم (الأكثر تعقيداً بعد Culture)
2. StatisticsView (TabControl 8 تبويبات) — يوم
3. SystemSettingsView + BackupRestoreView + ConstantsView — يوم

### المرحلة 7 — Views الإضافية (1 يوم)

- CombinedReportView + BlankReportView + CompareWithHistoryView + GroupWorksheetView + TestClassificationLogView + UserActivityLogView + SystemUsageMonitorView

### المرحلة 8 — تحويل Hub Buttons من Placeholders إلى Navigate (½ يوم)

**لكل Module ViewModel:**
- استبدل `_openPlaceholder(title)` بـ استدعاء Navigate Command المناسب من MainViewModel
- خيار 1: حقن MainViewModel عبر constructor الـ Module ViewModel
- خيار 2: حقن INavigationService وحقن خريطة من العنوان للـ Target

**مثال على PatientModuleViewModel:**
```csharp
public PatientModuleViewModel(INavigationService navigationService)
{
    OpenAddPatientCommand = new RelayCommand(_ => navigationService.Navigate(NavigationTarget.PatientRegistration));
    OpenEnterResultsCommand = new RelayCommand(_ => navigationService.Navigate(NavigationTarget.ResultsEntry));
    OpenDeliverResultsCommand = new RelayCommand(_ => navigationService.Navigate(NavigationTarget.Delivery));
    OpenSearchPatientCommand = new RelayCommand(_ => navigationService.Navigate(NavigationTarget.PatientSearch));
}
```

### المرحلة 9 — حذف Dead Code وتحسينات (½ يوم)

- حذف ShellWindow.xaml و ShellWindow.xaml.cs (أو تفعيلهما كبديل)
- حذف PlaceholderView (بعد توفر كل Views) أو الحفاظ عليها لـ legacy
- ربط TextBlock «متصل» في StatusBar بحالة DB حقيقية
- حذف أيقونات Toolbar المتكررة (\uE716 مكررة، \uE946 مكررة) أو تخصيص أيقونات فريدة

### المرحلة 10 — KeyBindings و الـ UX (½ يوم)

- إضافة KeyBindings للاختصارات الإلزامية في كل View (راجع 06)
- إضافة TabIndex للحقول حسب الأهمية
- إضافة ToolTip لكل زر مهم
- إضافة ProgressBar أو شعار تحميل لـ IsBusy

### المرحلة 11 — اختبارات (2 أيام)

- تشغيل اختبارات Open_lab.Tests الموجودة
- إضافة اختبارات لـ ViewModels المعدَّلة (Module ViewModels مع NavigationService)
- اختبار يدوي لكل سيناريو من PDF
- اختبار صلاحيات (Login مع أدوار مختلفة)

### المرحلة 12 — التوثيق والتسليم (1 يوم)

- تحديث `Open_lab/Docs/UI_Navigation_Specification.md`
- تحديث `Open_lab/Docs/Open_lab_Modules_Documentation.md`
- إضافة CHANGELOG.md

## 2.3 جدول زمني تقديري

| المرحلة | المدة | الـ Output |
|---|---|---|
| 0 — التحضير | 0.5 يوم | فهم البنية + قراءة المستندات |
| 1 — البنية التحتية | 1 يوم | ResourceDictionary مركزية |
| 2 — ربط Navigation | 0.5 يوم | Navigate*Commands تعمل |
| 3 — Views الأساسية | 5 أيام | 9 Views (Patient/Results/Reports) |
| 4 — Views بيانات النظام | 3 أيام | 6 Views (Catalog/Ranges/...) |
| 5 — Views العمليات | 4 أيام | 8 Views (Attendance/Treasury/...) |
| 6 — Views الإدارة | 3 أيام | 6 Views (Users/Stats/Settings) |
| 7 — Views الإضافية | 1 يوم | 7 Views (Reports/Compare/...) |
| 8 — Hub Navigation | 0.5 يوم | تحويل 8 Module VMs |
| 9 — Dead Code | 0.5 يوم | حذف ShellWindow + تنظيف |
| 10 — KeyBindings | 0.5 يوم | اختصارات F-Keys في كل View |
| 11 — اختبارات | 2 أيام | اختبارات شاملة |
| 12 — توثيق | 1 يوم | تحديث Docs |
| **الإجمالي** | **~23 يوم** | **40 View منفذة كاملة** |

## 2.4 المخاطر والـ Dependencies

### مخاطر تقنية
1. **بعض الـ Services قد تكون غير مكتملة** — يجب اختبار كل Service قبل ربطه بـ View
2. **مشروع EF Core Migrations** — إذا تعارضت Migrations الجديدة مع Views، قد تحتاج migrations إضافية
3. **PDF Preview في ReportViewerView** — WebBrowser في WPF قديم، قد تحتاج WebView2 (يحتاج Microsoft.Web.WebView2 nuget)
4. **CultureSensitivityView (548 سطر VM)** — الأكثر تعقيداً، قد يحتاج تقسيم إلى عدة Sub-Views

### مخاطر تنظيمية
1. **PDF غير كامل أحياناً** — قد لا توضح كل التفاصيل (مثل ترتيب الحقول، ألوان)
2. **مزامنة بين فِرق التطوير** — مع 40 View، تحتاج تقسيم العمل
3. **اختبار التوافق** — كل View جديد قد يكسر اختبارات قائمة

### Dependencies إلزامية
- .NET 8 SDK
- Microsoft.Extensions.DependencyInjection
- EF Core (موجود)
- Microsoft.Web.WebView2 (مُقترح للـ PDF Preview)

## 2.5 خريطة التحول النهائية

```
الحالة الحالية:
  MainWindow
    └─ [Toolbar] 11 RadioButton + Logout
    └─ [Content] CurrentView
        ├─ WelcomeView (initial / 3 fake modules)
        ├─ ModuleView (8 Hubs)
        │   └─ Button × N
        │       └─ PlaceholderView (شاشة موحَّدة "وظيفة قيد التطوير")
        │
        ↓ بعد الهجرة ↓
        
المستقبل:
  MainWindow
    └─ [Toolbar] 11 RadioButton + Logout
    └─ [Content] CurrentView
        ├─ DashboardView (default after login)
        ├─ ModuleView (8 Hubs)
        │   └─ Button × N
        │       └─ Sub-View (PatientRegistrationView, ResultsEntryView, ...)
        │           └─ [InputBindings] F1=New, F9=Save, F12=Delete
        │           └─ [Bindings] ViewModel Properties + Commands
        ↓
        Total: 8 Hubs + 37+ Sub-Views = 45+ Views ✅ كاملة
```

## 2.6 معايير الإنجاز النهائية

- ☐ كل Hub button يفتح Sub-View حقيقية (لا PlaceholderView)
- ☐ كل Navigate*Command يعمل ويغير CurrentView
- ☐ كل ViewModel معرَّفة في NavigationTarget لها View و DataTemplate
- ☐ ResourceDictionary مركزية تحوي كل Styles المشتركة
- ☐ KeyBindings مطبَّقة في كل View حسب الملف 06
- ☐ ShellWindow.xaml محذوف أو مُفعَّل بوضوح
- ☐ كل Bindings تطابق Properties موجودة فعلاً
- ☐ Unit tests تمر بنجاح
- ☐ MS SQL Server connection status حقيقي في StatusBar
- ☐ Documentation محدَّثة

---
