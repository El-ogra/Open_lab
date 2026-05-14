# 10 — UI Agent Master Prompt — Open lab system (Final Independent Audit)

> دليل التشغيل الموحَّد لأي وكيل تنفيذ ينتج XAML لـ Open lab system. كل قاعدة ملزمة بدون استثناء.

---

## 0) الهوية والنطاق

- **النظام:** Open lab system
- **الإطار:** WPF + .NET 8
- **النمط المعماري:** MVVM صارم (ViewModel-First)
- **التنقل:** `ContentControl + DataTemplate` عبر `MainViewModel.CurrentView`
- **اللغة الأساسية:** العربية، RTL
- **مصدر الحقيقة:** ViewModels الفعلية في فرع `Fi5ve` كوميت `011f15cff5a7f9898bddcc70b134f4258979125e`
- **المرجعان البصريان:** `real lab system help.pdf` و `RLS_Learn.pdf`
- **رسالة الكوميت:** «بعد حذف الشغل القديم»

---

## 1) قواعد MVVM الصارمة

### 1.1 لا code-behind
- `*.xaml.cs` لكل View تحتوي حصراً:
  ```csharp
  public partial class XxxView : UserControl
  {
      public XxxView() { InitializeComponent(); }
  }
  ```
- ❌ ممنوع: Event handlers (Click, SelectionChanged, KeyDown, MouseDoubleClick, …)
- ❌ ممنوع: DependencyProperty مخصصة في View
- ❌ ممنوع: تعديل `DataContext` من code-behind
- ❌ ممنوع: استدعاءات Services مباشرة من code-behind
- ❌ ممنوع: استدعاءات Application.Current.MainWindow.X من code-behind
- ✅ مسموح فقط للحالة الاستثنائية: PlaceholderView الحالي يحقن `FunctionTitle` و`BackCommand` عبر constructor (نمط قائم لا يُعمَّم على أي View جديد)
- ✅ مسموح: `using` directives في code-behind لاستيراد namespaces

### 1.2 DataContext
- يأتي من `App.xaml` `DataTemplate` بشكل تلقائي عبر `ContentControl Content="{Binding CurrentView}"`
- ❌ لا تكتب `DataContext = new XxxViewModel()` في View
- ❌ لا تكتب `<UserControl.DataContext>` في XAML
- ✅ يحقن من خلال DataTemplate في App.xaml
- استثناء: PlaceholderView (نمط قائم)

### 1.3 Bindings
- كل Binding يجب أن يطابق Property موجودة فعلاً في الـ ViewModel المرجعي (راجع الملف 08 — Window Contracts)
- ❌ لا تخمن أسماء Properties
- ❌ لا تستخدم x:Name إلا للضرورة القصوى (وحتى ذلك مرغوب تجنبه)
- ❌ لا تستخدم RelativeSource إلا للضرورة
- ✅ Bindings كلها OneWay افتراضياً إلا إذا كانت TwoWay منطقياً (TextBox.Text, PasswordBox, CheckBox.IsChecked, ComboBox.SelectedItem)

### 1.4 Commands
- كل تفاعل (Click, Submit, Refresh) يُربط بـ ICommand
- ❌ لا تستخدم Click="…" أو أي event handler
- ✅ استخدم `<Button Command="{Binding XxxCommand}" />`
- ✅ KeyBinding لاختصارات لوحة المفاتيح: `<KeyBinding Key="F9" Command="{Binding SaveCommand}" />`

---

## 2) قواعد الـ XAML

### 2.1 بنية الـ UserControl الجديد
```xml
<UserControl x:Class="Open_lab.Views.{Subfolder}.{ViewName}"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:behaviors="clr-namespace:Open_lab.Behaviors"
             FlowDirection="RightToLeft">
    <UserControl.InputBindings>
        <!-- KeyBindings المطلوبة -->
    </UserControl.InputBindings>
    <UserControl.Resources>
        <!-- Resources محلية -->
    </UserControl.Resources>
    <Grid>
        <!-- المحتوى -->
    </Grid>
</UserControl>
```

### 2.2 FlowDirection
- **القاعدة الجذرية:** كل UserControl/Window يحوي `FlowDirection="RightToLeft"`
- **استثناءات LTR محلية (إلزامية):**
  - كل TextBlock يحوي أيقونة Segoe MDL2 Assets ⇒ `FlowDirection="LeftToRight"`
  - كل حقل إدخال يحوي نص إنجليزي (LabId, Username, Password) ⇒ `FlowDirection="LeftToRight"`
  - كل Canvas/Grid يحوي أشكال رسومية (شعارات، رسومات هندسية) ⇒ `FlowDirection="LeftToRight"`

### 2.3 Resources Discipline
- **محلي للـ View:** ضع Styles خاصة بـ View واحد في `UserControl.Resources`
- **مشترك:** يجب نقله إلى App.xaml أو ResourceDictionary مستقلة
- **لا تكرر** نفس الـ Style في 8 ملفات (مثل ModuleButtonStyle الحالي)
- استخدم `StaticResource` للموارد الثابتة، `DynamicResource` فقط عند الحاجة لتبديل ثيم
- اسم المفاتيح يتبع PascalCase أو Dot.Notation (Primary.Dark، Hub.Button)

### 2.4 Grid Discipline
- استخدم `Grid.RowDefinitions` و`Grid.ColumnDefinitions` صراحة
- اقرأ Height: `Auto` للمحتوى المعتمد على المحتوى، `*` للمساحة المتاحة، رقم ثابت للأبعاد المحددة
- استخدم `Grid.Row="0"` و`Grid.Column="0"` صراحة (لا اعتماد على الافتراضي)

### 2.5 الاتساق مع المصمم
- استخدم نفس الـ Palette (راجع الملف 07)
- استخدم نفس الـ FontSize ladder (12, 13, 14, 16, 20, 24, 26, 30, 64)
- استخدم نفس CornerRadius (3, 5, 6, 8)
- استخدم نفس Spacing (Margin 8/10/15/20)

---

## 3) قواعد الـ DataTemplate

### 3.1 إضافة في App.xaml
كل ViewModel جديد لديه View يجب إضافة DataTemplate في App.xaml:
```xml
<DataTemplate DataType="{x:Type vm:XxxViewModel}">
    <views:XxxView />
</DataTemplate>
```

### 3.2 Namespace Aliases
- vm = `clr-namespace:Open_lab.ViewModels`
- views = `clr-namespace:Open_lab.Views`
- patientsVm = `clr-namespace:Open_lab.ViewModels.Patients`
- patientsViews = `clr-namespace:Open_lab.Views.Patients` (يجب إضافته إن لم يكن موجوداً)
- (نفس النمط لـ Accounts, SystemData, Worksheet, Statistics, Settings, Tools, Users)

### 3.3 الترتيب
- DataTemplates للـ ViewModels الأساسية (Login, Main, Welcome) أولاً
- ثم Module ViewModels
- ثم Sub-ViewModels (PatientRegistration, ResultsEntry, …)

---

## 4) قواعد الـ Navigation

### 4.1 لا تنشئ Window جديدة
- ❌ ممنوع `new Window { Content = … }` خارج App.xaml.cs
- ❌ ممنوع `Window.ShowDialog()` لشاشات وظيفية
- ✅ كل شاشة UserControl تُستضاف داخل MainWindow's ContentControl

### 4.2 آلية التنقل المعتمدة
لإضافة Navigate لـ View جديدة:
1. تأكد أن `NavigationTarget` enum يحوي القيمة (40 قيمة موجودة بالفعل)
2. تأكد أن `ViewModelFactory.Create(target)` يبني ViewModel (موجود لكل الـ 40)
3. **أضف الجديد:** ربط `MainViewModel.CurrentView` بالـ ViewModel الناتج من Factory
   - حالياً: NavigationService.Navigate تنشئ ViewModel لكن لا تربطها بـ CurrentView
   - المطلوب: تعديل MainViewModel.NavigateTo لـ `CurrentView = _navigationService.CurrentViewModel`

### 4.3 Hub buttons إلى Sub-Views
في الوضع الحالي: كل Hub button يستدعي `_openPlaceholder(title)`.
**الانتقال المطلوب:**
- بدلاً من PlaceholderView، استدعِ `MainViewModel.NavigateXxxCommand`
- مثال: PatientModuleViewModel.OpenAddPatientCommand يجب أن يستدعي MainViewModel.NavigatePatientRegistrationCommand
- آلية الحقن: تمرير `MainViewModel` (أو NavigationService) عبر constructor الـ Module ViewModel

---

## 5) قواعد الـ Validation

### 5.1 لا IDataErrorInfo / INotifyDataErrorInfo (حالياً)
- الكود الحالي لا يستخدم هذه الـ Interfaces
- ✅ التحقق برمجياً في `Command.Execute` ⇒ تحديث `StatusMessage`

### 5.2 النمط المُعتمَد
```csharp
private async Task SaveAsync()
{
    if (string.IsNullOrWhiteSpace(FullName))
    {
        StatusMessage = "خطأ: يرجى إدخال اسم المريض.";
        return;
    }
    // … تنفيذ الحفظ
}
```

### 5.3 عرض الأخطاء
- TextBlock في الـ View مربوط بـ `{Binding StatusMessage}`
- Foreground=#D13438 (Login.Error.Red)
- TextWrapping=Wrap, MinHeight=18

---

## 6) قواعد الـ Async / Loading

### 6.1 IsBusy Pattern
- كل ViewModel يحتاج Loading state يجب أن يحوي:
  - Property `IsBusy : bool` (OneWay private set)
  - استدعاء `RaiseCanExecuteChanged` عند تغييرها
- استخدام في XAML: ProgressBar أو زر معطَّل عبر CanExecute

### 6.2 لا تستخدم `_ = LoadAsync()` في Constructor (Anti-Pattern موجود)
- في الكود الحالي: `_ = InitializeAsync()` في PatientRegistrationViewModel و DashboardViewModel
- المُعتمد مستقبلاً: استدعاء صريح من View بعد Loaded event، أو من MainViewModel بعد الـ Navigate

### 6.3 try/catch لكل Async
```csharp
try
{
    // عملية async
}
catch (Exception ex)
{
    StatusMessage = $"خطأ: {ex.Message}";
}
finally
{
    IsBusy = false;
}
```

---

## 7) قواعد الـ Permissions

### 7.1 PermissionCodes
- استخدام `Open_lab.Services.PermissionCodes` (22 صلاحية محددة)
- لا تكتب strings حرفية للصلاحيات

### 7.2 CanExecute
- كل Navigate Command يحوي CanExecute = `IsLoggedIn && AppSession.HasPermission(code)`
- الـ Edit/Delete Commands في الـ Sub-ViewModels تحوي `AppSession.HasPermission(PermissionCodes.XxxEdit)`

---

## 8) قواعد لوحة المفاتيح (KeyBindings)

### 8.1 المكان
- اختصارات عامة في `Window.InputBindings` (MainWindow)
- اختصارات محلية في `UserControl.InputBindings` (لكل View)

### 8.2 النمط الإلزامي
```xml
<UserControl.InputBindings>
    <KeyBinding Key="F1" Command="{Binding NewCommand}" />
    <KeyBinding Key="F9" Command="{Binding SaveCommand}" />
    <KeyBinding Key="F12" Command="{Binding DeleteCommand}" />
    <KeyBinding Key="S" Modifiers="Control" Command="{Binding SaveCommand}" />
    <KeyBinding Key="Escape" Command="{Binding BackCommand}" />
</UserControl.InputBindings>
```

### 8.3 الاختصارات الإلزامية (من PDFs)
- F1=New / F2=Patients / F3=Search / F4=ResultsEntry / F5=Refresh / F6=Delivery / F7=ExternalSamples
- F8=ToggleReviewed / F9=Save / F10=PrintBarcode / F11=PrintReceipt / F12=Delete/PrintPreview

---

## 9) قواعد الـ Styling

### 9.1 ResourceDictionary المركزية المُقترحة
أنشئ `Open_lab/Themes/AppResources.xaml` يحوي:
- كل الـ SolidColorBrushes
- LinearGradientBrush (TopToolbarBrush)
- ModuleButtonStyle موحَّد
- Hub.HeroCard Style
- Hub.HeroIcon Style
- DataGrid Default Style (مفقود حالياً)
- TextBox Default Style (موحَّد)

### 9.2 الإضافة إلى App.xaml
```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="Themes/AppResources.xaml" />
        </ResourceDictionary.MergedDictionaries>
        <!-- DataTemplates -->
    </ResourceDictionary>
</Application.Resources>
```

---

## 10) قواعد الـ DataGrid

### 10.1 الإعدادات الأساسية
- `AutoGenerateColumns="False"` (دائماً)
- `IsReadOnly="True"` (الافتراضي للقوائم العرض فقط)
- `SelectionMode="Single"`
- `SelectedItem="{Binding SelectedXxx}"`
- `ItemsSource="{Binding XxxCollection}"`

### 10.2 Columns
- استخدم `DataGridTextColumn` لـ Strings
- استخدم `DataGridCheckBoxColumn` لـ Bools
- استخدم `DataGridComboBoxColumn` للقوائم
- استخدم `DataGridTemplateColumn` للأشياء المخصصة

### 10.3 RTL في DataGrid
- DataGrid يدعم RTL تلقائياً إذا كان UserControl FlowDirection=RTL
- لكن Headers قد تحتاج تنسيق إضافي

---

## 11) قواعد الـ Anti-Patterns الممنوعة

### 11.1 الممنوع تماماً
- ❌ `DataContext = this` في code-behind (PlaceholderView الحالي مستثنى، لكن لا يُعمَّم)
- ❌ MessageBox.Show في ViewModels (استخدم StatusMessage أو IDialogService)
- ❌ `Window.Show()` / `Window.ShowDialog()` خارج App.xaml.cs
- ❌ Hardcoded strings (استخدم Resources للترجمة المستقبلية)
- ❌ Hardcoded colors (استخدم StaticResource من ResourceDictionary)
- ❌ Magic numbers (FontSize، Margin، Width) — استخدم Resources
- ❌ تعريف Style مكرر في عدة Views
- ❌ Bindings إلى Properties غير موجودة
- ❌ استدعاء Services من code-behind
- ❌ استخدام Singleton أو Static خارج AppSession و PermissionCodes

### 11.2 المسموح بتحفُّظ
- ⚠️ x:Name (فقط عند الحاجة القصوى — مثلاً PasswordBox للـ Behavior)
- ⚠️ ElementName binding (يكسر بعض المعمارية MVVM)
- ⚠️ Trigger في Style (مفضّل DataTrigger مع Binding)

---

## 12) قواعد الـ Logging و الـ Diagnostics

### 12.1 لا System.Diagnostics في XAML
- استخدام `Debug.WriteLine` في ViewModels فقط (نمط موجود في MainViewModel.CloseAttendanceAsync)

### 12.2 لا Trace
- لا تستخدم TraceListener أو TraceSwitch

---

## 13) قواعد الـ Testing

### 13.1 ViewModels Unit Testable
- جميع ViewModels يجب أن تكون قابلة للاختبار بدون UI
- استخدم Interfaces للـ Services (موجود في الكود الحالي)
- لا تعتمد على Application.Current في ViewModels (المُستثنى: MainWindowLayoutService يستخدمه — مقبول لأنه Service)

### 13.2 موجود فعلاً
- مشروع Open_lab.Tests يحوي اختبارات لمعظم ViewModels (55+ ملف اختبار)
- استخدم Moq لـ Services

---

## 14) قواعد الأولوية في التنفيذ

### المرحلة 1: البنية التحتية (1 يوم)
1. أنشئ `Themes/AppResources.xaml` بـ ResourceDictionary المركزية
2. ربطها في App.xaml
3. وحّد ModuleButtonStyle (إزالة 8 نسخ مكررة)

### المرحلة 2: ربط الـ Navigation الفعلي (نصف يوم)
1. تعديل MainViewModel.NavigateTo بحيث يربط `CurrentView = NavigationService.CurrentViewModel`
2. تعديل كل Module ViewModels بحيث تستدعي Navigate Commands بدلاً من _openPlaceholder

### المرحلة 3: Views الأساسية (5 أيام)
- 9 Views أساسية: Dashboard, PatientRegistration, PatientSearch, PatientHistory, ResultsEntry, ReportViewer, PatientBilling, PatientBillingByDate, PatientTestsSelection

### المرحلة 4: Views الكتالوج (3 أيام)
- 6 Views: TestCatalog, ReferenceRanges, TestComments, PriceLists, CustomGroups, Referrals

### المرحلة 5: Views العمليات (4 أيام)
- 8 Views: AttendanceLog, AccountsTreasury, Delivery, SampleCollection, ExternalLabManagement, ContractInvoice, ReceiptPrinting, CultureSensitivity

### المرحلة 6: Views الإدارة (3 أيام)
- 6 Views: UsersPermissions, UserActivityLog, SystemUsageMonitor, AttendanceReport, Statistics, SystemSettings, BackupRestore, Constants

### المرحلة 7: Views الإضافية (يوم)
- 5 Views: CombinedReport, BlankReport, CompareWithHistory, GroupWorksheet, TestClassificationLog

### المرحلة 8: الاختبارات والتوثيق (2 أيام)
- تنفيذ الاختبارات الموجودة
- تحديث Docs/

---

## 15) قواعد الاتساق عبر اللغات

### 15.1 الأسماء العربية
- استخدم النص العربي بالضبط كما هو في PDFs
- لا تترجم Field labels بنفسك
- استخدم نفس الـ Strings الموجودة في Module ViewModels (مثلاً "اضافة وتعديل بيانات المرضى" — حتى لو فيها أخطاء إملائية)

### 15.2 الأسماء الإنجليزية
- استخدم PascalCase للـ Properties والـ Classes
- استخدم camelCase للـ private fields (مع _ prefix)
- استخدم UPPER_CASE للـ Constants

---

## 16) قائمة التحقق قبل تسليم XAML جديد

| ✓ | البند |
|---|---|
| ☐ | x:Class يطابق الـ namespace |
| ☐ | FlowDirection=RightToLeft على الجذر |
| ☐ | Code-behind يحوي فقط InitializeComponent |
| ☐ | لا DataContext في XAML أو code-behind |
| ☐ | جميع Bindings تطابق Properties موجودة في ViewModel الفعلي |
| ☐ | جميع Commands تطابق Commands موجودة في ViewModel الفعلي |
| ☐ | استخدام StaticResource من ResourceDictionary المركزية |
| ☐ | لا تكرار للـ Styles |
| ☐ | InputBindings لـ KeyBindings الإلزامية |
| ☐ | DataGrid AutoGenerateColumns=False |
| ☐ | لا حقول هاردكود (FontSize، Color، Margin، Width) خارج Resources |
| ☐ | لا event handlers (Click، SelectionChanged، KeyDown) |
| ☐ | DataTemplate مضافة في App.xaml |
| ☐ | اختبار Smoke: View تظهر دون أخطاء عند Navigate |
| ☐ | الاختصارات F-Keys مرتبطة بالـ Commands الصحيحة |
| ☐ | LTR محلي للأيقونات والشعارات والحقول الإنجليزية |

---

## 17) ملخص القواعد الذهبية

1. **MVVM صارم** — لا code-behind إلا InitializeComponent
2. **مصدر الحقيقة:** ViewModels في فرع Fi5ve كوميت 011f15c
3. **مرجع التصميم:** PDFs الثلاثة + الملف 07
4. **التنقل:** ContentControl + DataTemplate (لا Frame، لا NavigationWindow)
5. **اللغة:** عربي RTL مع استثناءات LTR محلية محددة
6. **الموارد:** ResourceDictionary مركزية، لا تكرار
7. **الاختصارات:** KeyBindings في كل View حسب الحاجة
8. **التحقق:** برمجياً في Commands، عرض StatusMessage
9. **Async:** try/catch/finally + IsBusy
10. **الصلاحيات:** PermissionCodes + AppSession.HasPermission

---
