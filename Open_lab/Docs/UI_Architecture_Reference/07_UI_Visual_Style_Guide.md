# 07 — UI Visual Style Guide — Open lab system (Final Independent Audit)

> دليل النمط البصري الكامل المُستخلَص من الكود الفعلي (XAML في فرع `Fi5ve` كوميت `011f15c`) + المرجعين PDF.

---

## 1) لوحة الألوان (Color Palette) — مأخوذة حرفياً من XAML

### الألوان الأساسية للهوية (Brand / Primary)

| الاسم المنطقي | الـ Hex | المصدر في الكود | الاستخدام الفعلي |
|---|---|---|---|
| Primary.Dark | `#0B2239` | MainWindow.xaml `TopToolbarBrush` GradientStop Offset=0 | بداية تدرج Toolbar العلوي + StatusBar |
| Primary.Mid | `#234B67` | MainWindow.xaml `TopToolbarBrush` GradientStop Offset=1 + PlaceholderView زر «رجوع» Background | نهاية التدرج + أزرار العودة |
| Primary.Hover | `#244F70` | MainWindow.xaml Trigger IsMouseOver على RadioButton + ExitButton | حالة Hover للأزرار العلوية |
| Primary.Active | `#4F7590` | MainWindow.xaml Trigger IsChecked على RadioButton + IsPressed على ExitButton | حالة الموديول النشط + ضغط زر الخروج |
| Primary.ActiveBorder | `#B7D6E8` | MainWindow.xaml Trigger IsChecked BorderBrush | حدود حالة Active للـ RadioButton العلوي |

### اللون الأخضر — لون أزرار الـ Hubs

| الاسم | الـ Hex | المصدر | الاستخدام |
|---|---|---|---|
| Hub.Button.Primary | `#8DB600` | كل ModuleView ModuleButtonStyle Background | لون كل أزرار الـ Hub في 8 Modules (Patients, SystemData, Accounts, Worksheet, Statistics, Settings, Tools, Users) |

### الألوان المميَّزة في SystemData

| الاسم | الـ Hex | المصدر | الاستخدام |
|---|---|---|---|
| SystemData.Equipment | `#87CEEB` | SystemDataModuleView Button «أجهزة ومعدات المعمل» Background | زر واحد فقط مميَّز بأزرق سماوي |
| SystemData.Equipment.Foreground | `#143347` | نفس الزر Foreground | نص داكن فوق الأزرق السماوي |
| SystemData.External.Orange | `#FF8C00` | الزرَّين «الجهات الخارجية والمعدل» و «قائمة أسعار التحاليل للجهات» | لون برتقالي مميَّز للأزرار الخارجية |

### ألوان LoginView (نظام Fluent بأزرق Microsoft)

| الاسم | الـ Hex | المصدر | الاستخدام |
|---|---|---|---|
| Login.AccentBlue | `#106EBE` | LoginButtonStyle Background + IsFocused/Hover BorderBrush | الزر الأساسي + حدود الحقل النشط |
| Login.AccentBlue.Hover | `#005A9E` | LoginButtonStyle Trigger IsMouseOver | حالة Hover |
| Login.AccentBlue.Pressed | `#004578` | LoginButtonStyle Trigger IsPressed | حالة الضغط |
| Login.Disabled.Gray | `#C8C8C8` | LoginButtonStyle Trigger IsEnabled=False | الزر معطَّل |
| Login.Border.Default | `#D1D1D1` | ModernInputField Border | حدود الحقل الافتراضية |
| Login.Title.DarkBlue | `#1B3B6F` | TextBlock «تسجيل الدخول» Foreground | عنوان رئيسي |
| Login.Subtitle.Blue | `#106EBE` | TextBlock «Open Lab System» Foreground | عنوان فرعي |
| Login.Label.Dark | `#323130` | TextBlocks لافتات «اسم المستخدم» و «كلمة المرور» | نصوص اللافتات |
| Login.Hint.Gray | `#605E5C` | TextBlocks ثانوية («تذكر بياناتي») + EyeButton أيقونة افتراضية | نصوص توضيحية |
| Login.Error.Red | `#D13438` | TextBlock StatusMessage Foreground | رسائل الخطأ |
| Login.Border.Subtle | `#F3F5F7` | Border الرئيسي للنموذج | حدود الـ Card الخارجية |

### ألوان الـ Hubs (Module Hubs)

| الاسم | الـ Hex | المصدر | الاستخدام |
|---|---|---|---|
| Hub.Background.Dark | `#25333E` | Grid Root لكل ModuleView | الخلفية الرئيسية للـ Hub |
| Hub.Card.Background | `#FAFAFA` | Border Row 0 Background | خلفية الـ Info Card العلوي |
| Hub.Card.Border | `#D5D8DA` | Border Row 0 BorderBrush | حدود الـ Card |
| Hub.Card.Title.Orange | `#D56E13` | TextBlock «من خلال هذه النافذة...» Foreground | عنوان «يمكنك عمل الآتي» في كل Hub |
| Hub.Card.Body.Black | `#111` | TextBlocks bullets في الـ Card | نقاط النص الأسود |
| Hub.Logo.Circle.Background | `#E7F2F4` | Ellipse 126×126 Fill في الـ Hubs | دائرة الشعار |
| Hub.Logo.Circle.Border | `#B5CDD2` | Ellipse Stroke | حدود الدائرة |
| Hub.Logo.Icon | `#376C78` | الأيقونات الكبيرة Segoe MDL2 Foreground | رمز الموديول |

### ألوان شعار PatientModuleView الخاصة

| الاسم | الـ Hex | الاستخدام |
|---|---|---|
| Patient.BloodGroup.Red | `#A71921` | TextBlock «AB-» (Foreground) |
| Patient.Syringe.Green | `#2C6B4F` | Path سهم سحب الدم (Stroke) |
| Patient.Needle.Red | `#C0392B` | Path مثلث رأس السهم (Fill) |

### ألوان شعار SystemDataModuleView الخاصة (شعار الأرشيف)

| الاسم | الـ Hex | الاستخدام |
|---|---|---|
| Archive.Wood.Background | `#C6B79A` | Rectangle 150×120 (Fill) |
| Archive.Wood.Stroke | `#7D6E55` | Rectangle و رفوف (Stroke) |
| Archive.Shelf.Background | `#E7D8B8` | 3 Rectangles 132×26 (Fill — رفوف داخلية) |
| Archive.Icon.Brown | `#5B4E3B` | TextBlock أيقونة كتاب (Foreground) |

### ألوان WelcomeView (Logo Canvas)

| الاسم | الـ Hex | الاستخدام |
|---|---|---|
| Welcome.Bg | `#F7FAF7` | Grid Background |
| Welcome.Ellipse.Largest | `#1F4D3A` | Ellipse 260×260 Fill |
| Welcome.Ellipse.Open | `#2C6B4F` | Ellipse 170×170 Fill ("Open") |
| Welcome.Ellipse.Lab | `#3E7E58` | Ellipse 150×150 Fill ("Lab") |
| Welcome.Ellipse.Sys | `#5C9A68` | Ellipse 120×120 Fill ("Sys") |
| Welcome.Leaf | `#DDEFE4` | TextBlock «☘» Foreground |
| Welcome.Top.Label | `#4F5F5C` | TextBlock «MS SQL SERVER» Foreground |
| Welcome.Bottom.Label | `#41624C` | TextBlock «Open Lab System» Foreground |

### ألوان شريط الحالة (StatusBar)

| الاسم | الـ Hex | الاستخدام |
|---|---|---|
| Status.Text.Light | `#DCECF6` | لافتات «اسم المستخدم: » «آخر دخول: » … |
| Status.Text.Value | `White` | قيم Bindings (CurrentUser, LastLoginDate, CurrentDate) |
| Status.DB.Connected | `#BDE5C8` | TextBlock «متصل» (ثابت) |

### ألوان PlaceholderView

| الاسم | الـ Hex | الاستخدام |
|---|---|---|
| Placeholder.Bg | `#F7FAF7` | Grid Background |
| Placeholder.Card.Bg | `White` | Border الداخلي Background |
| Placeholder.Card.Border | `#D7DFD8` | Border BorderBrush |
| Placeholder.Title | `#203B2B` | TextBlock العنوان Foreground |
| Placeholder.Body | `#3F4F46` | TextBlock الوصف Foreground |
| Placeholder.Button.Bg | `#234B67` | زر «رجوع» Background |

### ألوان منطقة المحتوى في MainWindow

| الاسم | الـ Hex | الاستخدام |
|---|---|---|
| Main.Content.Bg | `#F7FAF7` | Border Row 1 Background (خلف ContentControl) |

---

## 2) الفُرَش (Brushes) المُعرَّفة كموارد

### LinearGradientBrush — TopToolbarBrush

ملف: `MainWindow.xaml` السطر 12-15

```xml
<LinearGradientBrush x:Key="TopToolbarBrush" StartPoint="0,0" EndPoint="1,1">
    <GradientStop Color="#0B2239" Offset="0" />
    <GradientStop Color="#234B67" Offset="1" />
</LinearGradientBrush>
```

**التطبيق:** 
- Border Row 0 (Toolbar — Height=90)
- Border Row 2 (StatusBar — Height=30)

**اتجاه التدرج:** من أعلى يسار (0,0) ⇒ إلى أسفل يمين (1,1) — قطري عبر العنصر

---

## 3) الطباعة (Typography)

### الخطوط المستخدمة

| الخط | المصدر | الاستخدام |
|---|---|---|
| Segoe UI (الافتراضي) | تلقائياً | كل النصوص العربية والعنوانية |
| Segoe MDL2 Assets | `FontFamily="Segoe MDL2 Assets"` | كل الأيقونات في MainWindow Toolbar + Hubs + LoginView Eye Button + ModuleView Hero icons |

### أحجام الخط (FontSize) — مستخلَصة حرفياً

| الحجم | الاستخدام |
|---|---|
| 12 | LoginView Subtitle, StatusMessage |
| 13 | LoginView CheckBox label, MainWindow ToolbarTextStyle |
| 14 | LoginView ModernInputField (TextBox/PasswordBox), Tools Button text |
| 15 | SystemData Button text |
| 16 | LoginView LoginButtonStyle, ModuleView Button text, Welcome zwischen Labels, Hub bullet text, Settings/Worksheet/Statistics/Users buttons text |
| 18 | ShellWindow Logo |
| 20 | Hub Card Title «من خلال هذه النافذة...» |
| 24 | ToolbarIconStyle (أيقونات Segoe MDL2 في Toolbar) |
| 25 | WelcomeView TextBlock «Sys» |
| 26 | LoginView Title «تسجيل الدخول» |
| 27 | PatientModuleView Hero TextBlock «AB-» |
| 28 | Hub Buttons أيقونات Segoe MDL2 + ToolsModule (24) |
| 30 | WelcomeView «Open» و «Lab», PlaceholderView Title |
| 42 | SystemDataModuleView Hero icon |
| 54 | PatientModuleView Hero icon (Segoe MDL2 \uE91D) |
| 64 | معظم Hubs Hero icons (Accounts, Worksheet, Statistics, Settings, Tools, Users) |
| 78 | WelcomeView TextBlock «☘» |

### أوزان الخط (FontWeight)

| الوزن | الاستخدام |
|---|---|
| Bold | TextBlocks عنوان الـ Hub Info Card («من خلال هذه النافذة...»), WelcomeView TextBlocks «Open/Lab/Sys», PatientModuleView «AB-», PlaceholderView Title |
| ExtraBold | LoginView Title «تسجيل الدخول» |
| SemiBold | كل ModuleButtonStyle, LoginView Labels, Subtitle, StatusBar labels, RadioButton IsChecked Trigger |
| Regular (Normal) | معظم النصوص العادية، bullets |

---

## 4) الحدود والـ Corner Radius

### Corner Radius القياسية

| القيمة | المصدر |
|---|---|
| 3 | Hub Card Row 0 Border CornerRadius |
| 5 | ModuleButtonStyle Border CornerRadius + PlaceholderView Border |
| 6 | LoginView ModernInputField Border + LoginButtonStyle + ToolbarRadioButtonStyle |
| 8 | LoginView Outer Border CornerRadius |

### Border Thickness

| القيمة | المصدر |
|---|---|
| 1 | ModernInputField (default), Hub Card Border, PlaceholderView Border, LoginView Outer Border |
| 2 | ModernInputField IsFocused Trigger (Border زرقاء عند التركيز) + Hub Card Logo Ellipse Stroke |
| 8 | PatientModuleView Path سهم سحب الدم (StrokeThickness) |
| 0 | كل ModuleButtonStyle (بدون حدود) |

---

## 5) المسافات (Spacing — Margin, Padding)

### القيم المعيارية في الكود

| القيمة | الاستخدام |
|---|---|
| **Margin** | |
| 0,2,0,5 | ToolbarIconStyle |
| 3,8 | ToolbarRadioButtonStyle و ExitButtonStyle |
| 8 | SystemData/Tools ModuleButtonStyle Margin |
| 10 | معظم ModuleButtonStyle Margin |
| 15 | Settings/Worksheet ModuleButtonStyle Margin |
| 16 | ShellWindow TextBlocks Headings |
| 20 | Hub Card Border Margin |
| 24 | WelcomeView TextBlocks |
| 30,18 | SystemData Hub Buttons Grid Margin |
| **Padding** | |
| 4 | ToolbarRadioButtonStyle Border Padding |
| 10,0 | ModernInputField Padding |
| 12 | Hub Card Title Margin Bottom + معظم ModuleButtonStyle Border Padding |
| 14,0 | MainWindow StatusBar Border Padding |
| 22 | Hub Card Row 0 Border Padding |
| 30 | LoginView Outer Border Padding |
| 32 | PlaceholderView Border Padding |

---

## 6) الأبعاد (Dimensions)

### أبعاد النوافذ

| الـ Window | Width | Height | المصدر |
|---|---:|---:|---|
| Login Window (في App.xaml.cs) | 400 | 550 | برمجياً |
| MainWindow (في XAML) | 1180 | 720 | MainWindow.xaml attributes |
| MainWindow (بعد ApplyAppLayout) | 1100 | 700 | MainWindowLayoutService.cs |
| MinWidth / MinHeight لـ MainWindow | 980 | 640 | XAML |
| ShellWindow (غير مستخدم) | 1100 | 700 | XAML |

### أبعاد العناصر الأساسية

| العنصر | الأبعاد |
|---|---|
| Toolbar Row 0 (MainWindow) | Auto × 90 |
| StatusBar Row 2 (MainWindow) | * × 30 |
| Hub Card Row 0 | * × 215 |
| LoginView Card | 360 × Auto |
| PlaceholderView Card | 520 × MinHeight=210 |
| WelcomeView Canvas | 520 × 360 |
| LoginView Input (ModernInputField) | * × 40 |
| LoginView Button | * × 45 |
| LoginView Eye Button | 35 × Auto |
| PlaceholderView Back Button | 150 × 42 |

### أبعاد الأزرار في Hubs

| الـ Module | Width | Height | Margin |
|---|---:|---:|---:|
| PatientModuleView | 220 | 70 | 10 |
| AccountsModuleView | 220 | 70 | 10 |
| UsersModuleView | 220 | 70 | 10 |
| SettingsModuleView | 220 | 70 | 15 |
| WorksheetModuleView | 250 | 70 | 15 |
| StatisticsModuleView | 250 | 70 | 10 |
| ToolsModuleView | 200 | 65 | 8 |
| SystemDataModuleView | (Auto عرض الـ Grid Column) | 58 | 8 |

---

## 7) الأيقونات (Segoe MDL2 Assets Glyphs)

### دليل الـ Glyphs المستخدمة

| الـ Code | الـ Glyph | الاستخدام |
|---|---|---|
| `\uE713` | ⚙ Settings | MainWindow Toolbar «اعدادات», SettingsModuleView Hero+Button |
| `\uE716` | 👤 People | MainWindow Toolbar «المرضى» و «المستخدمين» |
| `\uE721` | 🔍 Zoom | PatientModuleView «بحث عن مريض» |
| `\uE722` | 🔑 Permissions | UsersModuleView «Login detector» |
| `\uE723` | 👁 RedEye | LoginView Eye Button (مغلق) |
| `\uE77B` | 👤 Contact | StatusBar أيقونة المستخدم + Toolbar «الموظفين» + UsersModuleView Hero + Tools «دليل الهاتف» |
| `\uE787` | 📅 Calendar | Tools «نونة المواعيد» |
| `\uE7B3` | 👁 RedEye (variant) | LoginView Eye Button (مفتوح) |
| `\uE7B7` | Database | Settings «Database Maintenance» |
| `\uE7BE` | Clock | Users «الحضور والإنصراف» |
| `\uE7BF` | Cart | Tools «قائمة المطلوبات والمشتريات» |
| `\uE7C3` | Page | PatientModuleView «تسليم نتائج المرضى» |
| `\uE7F0` | Cloud | MainWindow Toolbar «بيانات النظام» |
| `\uE81C` | Library | WorksheetModuleView Hero icon |
| `\uE825` | Money | MainWindow «حسابات» (لا — هي E8C7) — هذه AccountsModuleView Hero |
| `\uE8B7` | Folder | SystemDataModuleView Hero (داخل صورة الأرشيف) |
| `\uE8BB` | Cancel | MainWindow Toolbar «خروج» (LogoutCommand) |
| `\uE8C0` | Mail | AccountsModuleView «العينات المرسلة للخارج» |
| `\uE8C7` | Calculator | MainWindow Toolbar «حسابات» |
| `\uE8D2` | Reading | Tools «قاموس لاختصارات» |
| `\uE8D5` | Bank | AccountsModuleView «حساب شركات ومندوبين» |
| `\uE8D7` | Lock | Users «تغيير كلمة المرور» |
| `\uE8EF` | Calculator | Tools «محول وحدات نتائج التحاليل» |
| `\uE90F` | Wrench | MainWindow Toolbar «أدوات» |
| `\uE916` | Stopwatch | Tools «ساعة التوقيت» |
| `\uE91D` | Drop | PatientModuleView Hero (قطرة دم) |
| `\uE946` | Info | MainWindow Toolbar «هل تعلم» و «نبذة» |
| `\uE95E` | AddFriend | PatientModuleView «اضافة وتعديل بيانات المرضى» |
| `\uE9D2` | BarChart4 | MainWindow Toolbar «احصاليات» + StatisticsModuleView Hero + كل أزرارها |
| `\uE9F9` | Library | WorksheetModuleView كلا الزرين |
| `\uEA86` | Library | Tools «مكتبة التحاليل» |
| `\uEB9F` | Camera | Tools «مكتبة الصور» |
| `\uED1E` | Calculator | Tools «الآلة الحاسبة» |
| `\uED43` | Repair | ToolsModuleView Hero icon |
| `\uEE40` | AddFriend | Users «انشاء مستخدمين» |
| `\uE70B` | Edit | MainWindow Toolbar «ورقة عمل» + PatientModuleView «ادخال نتائج التحاليل» |
| `\uE72B` | Manage | AccountsModuleView «الجرد وحساب الدرج» |

---

## 8) الـ Effects و الـ Triggers الموحَّدة

### نمط الـ Hover القياسي

```xml
<Trigger Property="IsMouseOver" Value="True">
    <Setter TargetName="Root" Property="Opacity" Value="0.88" />
</Trigger>
```
(مُطبَّق على كل ModuleButtonStyle)

### نمط الـ Pressed القياسي

```xml
<Trigger Property="IsPressed" Value="True">
    <Setter TargetName="Root" Property="Opacity" Value="0.75" />
</Trigger>
```

### نمط الـ Focus (LoginView فقط)

```xml
<Trigger Property="IsFocused" Value="true">
    <Setter TargetName="border" Property="BorderBrush" Value="#106EBE"/>
    <Setter TargetName="border" Property="BorderThickness" Value="2"/>
</Trigger>
```

---

## 9) ResourceDictionary الموحَّدة المُقترحة

في الكود الحالي، **لا يوجد ResourceDictionary مشترك**. كل ModuleView يعيد تعريف `ModuleButtonStyle` محلياً. هذا تكرار ضخم (8 ملفات XAML تحوي نفس الـ Style تقريباً مع فروقات الحجم فقط).

### Resources المُقترح توحيدها

```xml
<!-- يجب وضعها في App.xaml أو Themes/Default.xaml -->
<ResourceDictionary>
    <!-- Brushes -->
    <SolidColorBrush x:Key="Primary.Dark" Color="#0B2239" />
    <SolidColorBrush x:Key="Primary.Mid" Color="#234B67" />
    <SolidColorBrush x:Key="Primary.Hover" Color="#244F70" />
    <SolidColorBrush x:Key="Primary.Active" Color="#4F7590" />
    <SolidColorBrush x:Key="Primary.ActiveBorder" Color="#B7D6E8" />
    <SolidColorBrush x:Key="Hub.Bg" Color="#25333E" />
    <SolidColorBrush x:Key="Hub.Card.Bg" Color="#FAFAFA" />
    <SolidColorBrush x:Key="Hub.Button" Color="#8DB600" />
    <SolidColorBrush x:Key="Main.Content.Bg" Color="#F7FAF7" />
    
    <LinearGradientBrush x:Key="TopToolbarBrush" StartPoint="0,0" EndPoint="1,1">
        <GradientStop Color="#0B2239" Offset="0" />
        <GradientStop Color="#234B67" Offset="1" />
    </LinearGradientBrush>
    
    <!-- Module Button Style (Unified) -->
    <Style x:Key="ModuleButtonStyle" TargetType="Button">
        <Setter Property="Background" Value="{StaticResource Hub.Button}" />
        <Setter Property="Foreground" Value="White" />
        <Setter Property="FontSize" Value="16" />
        <Setter Property="FontWeight" Value="SemiBold" />
        <Setter Property="Cursor" Value="Hand" />
        <Setter Property="BorderThickness" Value="0" />
        <!-- Template + Triggers -->
    </Style>
</ResourceDictionary>
```

---

## 10) الـ FlowDirection (RTL/LTR) — قواعد الإدارة

### القاعدة العامة
- **كل UserControl:** `FlowDirection=RightToLeft` على المستوى الجذر (مؤكد في 12 ملف XAML)
- **Window:** نفس القاعدة — MainWindow و LoginWindow كلاهما RTL

### الاستثناءات المؤكَّدة (LTR محلياً)
1. **Segoe MDL2 Icons:** `ToolbarIconStyle` و `EyeButtonStyle` و كل أيقونة Hero في الـ Hubs يحوي `FlowDirection=LeftToRight` على الـ TextBlock
2. **LoginView ModernInputField:** `FlowDirection=LeftToRight` (لأن Username و Password عادة بالحروف الإنجليزية)
3. **CheckBox RememberMe في LoginView:** `FlowDirection=LeftToRight` (مربع الاختيار على اليسار + النص العربي بعده باتجاه طبيعي)
4. **Hub Logo Grid (Col 0):** `<Grid Grid.Column="0" FlowDirection="LeftToRight">` لأن الأشكال الرسومية لا يجب أن تنعكس
5. **WelcomeView Canvas TextBlocks:** كل TextBlocks بداخل Canvas (Open / Lab / Sys / ☘ / MS SQL SERVER / Open Lab System) فيها `FlowDirection=LeftToRight`
6. **MainWindow StatusBar أيقونة `&#xE77B;`:** بدون FlowDirection محلي (يرث RTL — لكن لأنها رمز Unicode وحيد لا يهم)

---

## 11) ملخص الأنماط البصرية الموحَّدة

### النمط الموحَّد لكل Hub
1. خلفية داكنة `#25333E`
2. كرت معلوماتي علوي بخلفية `#FAFAFA` بحدود `#D5D8DA` و CornerRadius=3
3. شعار رسومي داخل Ellipse 126×126 + Segoe MDL2 icon
4. عنوان «من خلال هذه النافذة...» بلون برتقالي `#D56E13` خط 20pt Bold
5. 4 نقاط bullet بخط 16pt أسود `#111`
6. أزرار خضراء `#8DB600` بنص أبيض Bold بحجم 14-16pt
7. أيقونة Segoe MDL2 يسار النص داخل DockPanel

### النمط الموحَّد للـ Toolbar
1. تدرج لوني `#0B2239 → #234B67` قطري
2. 12 خانة UniformGrid
3. كل RadioButton: أيقونة 24pt في الأعلى + نص 13pt تحتها
4. Hover ⇒ `#244F70`
5. Checked ⇒ `#4F7590` + حدود `#B7D6E8` + Bold

---
