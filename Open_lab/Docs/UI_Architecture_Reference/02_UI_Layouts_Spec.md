# 02 — UI Layouts Spec — Open lab system (Final Independent Audit)

> مواصفات التخطيط الكاملة، مأخوذة حرفياً من XAML الفعلي في فرع `Fi5ve` كوميت `011f15c`.

---

## 0) القواعد العامة المؤكَّدة من الكود

- **اتجاه الواجهة:** كل UserControl يحوي `FlowDirection="RightToLeft"` على المستوى الجذر. هذا مؤكد في كل XAML الـ 12.
- **استثناءات RTL → LTR محلية:**
  - أيقونات Segoe MDL2 ⇒ `ToolbarIconStyle` يحوي `FlowDirection=LeftToRight`
  - حقول إدخال LoginView ⇒ `ModernInputField` يحوي `FlowDirection=LeftToRight`
  - Grid الأشكال الزخرفية في الـ Hubs (Ellipse الصورة) ⇒ `FlowDirection=LeftToRight` (سطر `<Grid Grid.Column="0" FlowDirection="LeftToRight">`)
  - شعارات WelcomeView النصية ⇒ كل TextBlock فيها `FlowDirection=LeftToRight`
- **النوافذ المنبثقة (Window):** الموجود فعلياً Window نوعان فقط:
  - LoginWindow (مبني برمجياً في App.xaml.cs، يستضيف LoginView)
  - MainWindow (XAML مستقل)
- **ShellWindow.xaml:** Window مُعرَّف لكنه Dead Code (لا أحد ينشئه)
- **شريط الأدوات العلوي في MainWindow:** `UniformGrid Columns="12"` بـ 11 RadioButton + 1 Button، خلفية متدرجة `#0B2239 → #234B67`
- **شريط الحالة:** `Border Grid.Row="2"` بنفس التدرج، ارتفاع 30 px، يحوي اسم المستخدم وآخر دخول وحالة DB والتاريخ

---

## 0.1 LoginView Layout

- **الغرض:** التحقق من الهوية ومنح صلاحيات الجلسة
- **حاوية:** Window مخصصة في `App.xaml.cs` بأبعاد 400×550، `ResizeMode=NoResize`, `WindowStartupLocation=CenterScreen`, `FlowDirection=RightToLeft`
- **الهيكل (الجذر):**
  - `Grid Background="White"`
    - `Border Width=360 Height=Auto VerticalAlignment=Center HorizontalAlignment=Center BorderBrush=#F3F5F7 BorderThickness=1 CornerRadius=8 Padding=30`
      - `Grid` بـ 4 صفوف:
        - Row 0 (Auto) — Header — Title + Subtitle
        - Row 1 (*) — Inputs
        - Row 2 (Auto) — Message (StatusMessage)
        - Row 3 (Auto) — Button «دخول»

### تفاصيل Row 0
- `StackPanel Margin=0,0,0,25`
  - TextBlock «تسجيل الدخول» — FontSize=26, FontWeight=ExtraBold, Foreground=#1B3B6F, HorizontalAlignment=Center
  - TextBlock «Open Lab System» — FontSize=12, Foreground=#106EBE, FontWeight=SemiBold, HorizontalAlignment=Center, Margin=0,4,0,0

### تفاصيل Row 1
- `StackPanel`
  - TextBlock «اسم المستخدم» — FontWeight=SemiBold, Foreground=#323130, Margin=0,0,0,8
  - TextBox `Text={Binding Username, UpdateSourceTrigger=PropertyChanged}` بأسلوب `ModernInputField`
  - TextBlock «كلمة المرور» — Margin=0,20,0,8
  - `Grid` (يحوي ثلاثة عناصر فوق بعضها):
    - PasswordBox x:Name=PasswordInput — `behaviors:PasswordBoxAssistant.BindPassword=True`, `BoundPassword={Binding Password, Mode=TwoWay}` — يختفي عندما `IsPasswordVisible=True`
    - TextBox `Text={Binding Password}` — مخفي افتراضياً، يظهر عندما `IsPasswordVisible=True`
    - Button (أيقونة عين) HorizontalAlignment=Left, VerticalAlignment=Center, `Command={Binding TogglePasswordVisibilityCommand}`, Style=EyeButtonStyle
      - أيقونة افتراضية `&#xE723;` (عين مغلقة)
      - تتحول إلى `&#xE7B3;` (عين مفتوحة) عندما `IsPasswordVisible=True`
  - `StackPanel Orientation=Horizontal HorizontalAlignment=Right Margin=0,15,0,0`
    - TextBlock «تذكر بياناتي» — Foreground=#605E5C, FontSize=13
    - CheckBox `IsChecked={Binding RememberMe}` — FlowDirection=LeftToRight

### تفاصيل Row 2
- TextBlock `Text={Binding StatusMessage}` — Foreground=#D13438, FontSize=12, TextWrapping=Wrap, Margin=0,10, TextAlignment=Center, MinHeight=18

### تفاصيل Row 3
- Button Content=«دخول» — IsDefault=True, `Command={Binding LoginCommand}`, Style=LoginButtonStyle (FontSize=16, Bold, Background=#106EBE, Height=45)

### قابلية تغيير الحجم
- غير قابلة (Window حاوية بـ `ResizeMode=NoResize`)

---

## 0.2 MainWindow Layout

- **الغرض:** Shell الرئيسي — يستضيف باقي الشاشات
- **حاوية:** Window مستقلة، Title=«Open Lab System >> القائمة الرئيسية», Height=720, Width=1180, MinHeight=640, MinWidth=980, WindowStartupLocation=CenterScreen, FlowDirection=RightToLeft
- **تعارض الأبعاد:** XAML يحدد 1180×720 لكن MainWindowLayoutService.ApplyAppLayout يضبطه لاحقاً إلى 1100×700 — السلوك الفعلي = 1100×700
- **الهيكل:** `Grid` بـ 3 صفوف:
  - Row 0 (Auto, Height=90) — شريط الأدوات العلوي
  - Row 1 (*) — منطقة المحتوى (ContentControl)
  - Row 2 (Height=30) — شريط الحالة

### Row 0 — شريط الأدوات العلوي
- `Border Height=90 Background={StaticResource TopToolbarBrush}` (متدرج `#0B2239 → #234B67`)
- `Visibility={Binding IsToolbarVisible, Converter={StaticResource BoolToVisibility}}`
- `UniformGrid Columns=12 FlowDirection=RightToLeft`
- ترتيب الأزرار (من اليمين لليسار):
  1. RadioButton «المرضى» (icon `&#xE716;`) ⇒ NavigateToPatientsCommand
  2. RadioButton «أدوات» (icon `&#xE90F;`) ⇒ NavigateToToolsCommand
  3. RadioButton «ورقة عمل» (icon `&#xE70B;`) ⇒ NavigateToWorksheetCommand
  4. RadioButton «حسابات» (icon `&#xE8C7;`) ⇒ NavigateToAccountsCommand
  5. RadioButton «احصاليات» (icon `&#xE9D2;`) ⇒ NavigateToStatisticsCommand
  6. RadioButton «المستخدمين» (icon `&#xE716;`) ⇒ NavigateToUsersCommand
  7. RadioButton «بيانات النظام» (icon `&#xE7F0;`) ⇒ NavigateToSystemDataCommand
  8. RadioButton «اعدادات» (icon `&#xE713;`) ⇒ NavigateToSettingsCommand
  9. RadioButton «الموظفين» (icon `&#xE77B;`) ⇒ NavigateToEmployeesCommand
  10. RadioButton «هل تعلم» (icon `&#xE946;`) ⇒ NavigateToDidYouKnowCommand
  11. RadioButton «نبذة» (icon `&#xE946;`) ⇒ NavigateToAboutCommand
  12. Button «خروج» (icon `&#xE8BB;`) ⇒ LogoutCommand
- كل RadioButton لها GroupName=`TopModules` (مجموعة واحدة فقط — واحد منها مختار في أي وقت)

### Style — ToolbarRadioButtonStyle
- Foreground=White
- Margin=3,8
- Cursor=Hand
- ControlTemplate:
  - Border CornerRadius=6, Padding=4
  - `Trigger IsMouseOver=True` ⇒ Background=#244F70
  - `Trigger IsChecked=True` ⇒ Background=#4F7590, BorderBrush=#B7D6E8, FontWeight=Bold

### Style — ToolbarExitButtonStyle
- مماثل لـ RadioButtonStyle لكن `Trigger IsPressed=True` ⇒ Background=#4F7590

### Style — ToolbarIconStyle (TextBlock)
- FontFamily=`Segoe MDL2 Assets`
- FontSize=24
- HorizontalAlignment=Center
- Margin=0,2,0,5
- FlowDirection=LeftToRight (الأيقونات لا تنعكس)

### Style — ToolbarTextStyle (TextBlock)
- FontSize=13
- HorizontalAlignment=Center
- TextAlignment=Center

### Row 1 — منطقة المحتوى
- `Border Grid.Row=1 Background=#F7FAF7`
- `ContentControl Content={Binding CurrentView}` — يستضيف WelcomeView / ModuleView / PlaceholderView

### Row 2 — شريط الحالة
- `Border Background={StaticResource TopToolbarBrush} Padding=14,0`
- `DockPanel LastChildFill=False VerticalAlignment=Center`
- `StackPanel DockPanel.Dock=Right Orientation=Horizontal VerticalAlignment=Center`
  - TextBlock أيقونة شخص `&#xE77B;` Foreground=#DCECF6
  - TextBlock «اسم المستخدم: » FontWeight=SemiBold Foreground=#DCECF6
  - TextBlock `Text={Binding CurrentUser}` Foreground=White Margin=0,0,18,0
  - TextBlock «آخر دخول: » FontWeight=SemiBold Foreground=#DCECF6
  - TextBlock `Text={Binding LastLoginDate}` Foreground=White Margin=0,0,18,0
  - TextBlock «قاعدة البيانات: » FontWeight=SemiBold Foreground=#DCECF6
  - TextBlock «متصل» Foreground=#BDE5C8 Margin=0,0,18,0 (نص ثابت)
  - TextBlock «اليوم: » FontWeight=SemiBold Foreground=#DCECF6
  - TextBlock `Text={Binding CurrentDate}` Foreground=White

---

## 0.3 WelcomeView Layout

- **الغرض:** شعار النظام / حالة فارغة
- **حاوية:** UserControl FlowDirection=RightToLeft
- **الهيكل:** `Grid Background=#F7FAF7` يحوي 3 عناصر:
  - TextBlock «MS SQL SERVER» HorizontalAlignment=Left VerticalAlignment=Top Margin=24 Foreground=#4F5F5C FontSize=16 FontWeight=SemiBold FlowDirection=LeftToRight
  - `Canvas Width=520 Height=360 HorizontalAlignment=Center VerticalAlignment=Center` يحوي:
    - Ellipse كبيرة 260×260 Canvas.Left=130 Canvas.Top=25 Fill=#1F4D3A Opacity=0.96 (الخلفية الكبرى)
    - TextBlock «☘» FontSize=78 Foreground=#DDEFE4 — Canvas.Left=214 Canvas.Top=91
    - Ellipse متوسطة 170×170 Canvas.Left=64 Canvas.Top=145 Fill=#2C6B4F
    - TextBlock «Open» FontSize=30 FontWeight=Bold Foreground=White — Canvas.Left=106 Canvas.Top=203
    - Ellipse متوسطة 150×150 Canvas.Left=255 Canvas.Top=145 Fill=#3E7E58
    - TextBlock «Lab» — Canvas.Left=310 Canvas.Top=197
    - Ellipse صغيرة 120×120 Canvas.Left=205 Canvas.Top=220 Fill=#5C9A68
    - TextBlock «Sys» FontSize=25 — Canvas.Left=247 Canvas.Top=260
  - TextBlock «Open Lab System» HorizontalAlignment=Right VerticalAlignment=Bottom Margin=24 Foreground=#41624C FontSize=16 FontWeight=SemiBold FlowDirection=LeftToRight

---

## 0.4 PlaceholderView Layout

- **حاوية:** UserControl FlowDirection=RightToLeft
- **الهيكل:**
  - `Grid Background=#F7FAF7`
    - `Border Width=520 MinHeight=210 Padding=32 HorizontalAlignment=Center VerticalAlignment=Center Background=White BorderBrush=#D7DFD8 BorderThickness=1 CornerRadius=5`
      - `StackPanel`
        - TextBlock `Text={Binding FunctionTitle}` FontSize=30 FontWeight=Bold Foreground=#203B2B TextAlignment=Center Margin=0,0,0,18
        - TextBlock «نافذة مؤقتة للوظيفة، وسيتم استكمال محتواها لاحقاً.» FontSize=16 Foreground=#3F4F46 TextAlignment=Center Margin=0,0,0,28
        - Button Content=«رجوع» `Command={Binding BackCommand}` Width=150 Height=42 HorizontalAlignment=Center Background=#234B67 Foreground=White FontSize=16 FontWeight=SemiBold BorderThickness=0
- **خصوصية MVVM:** `DataContext=this` في constructor + `FunctionTitle` و`BackCommand` Properties محلية في code-behind

---

## النمط الموحد لكل ModuleView (الـ Hubs الـ 8)

كل الـ 8 ModuleView يستخدم نفس الـ Skeleton التالي:

```
UserControl FlowDirection=RightToLeft
  Resources:
    Style x:Key="ModuleButtonStyle" TargetType=Button
      Background=#8DB600
      Foreground=White
      FontWeight=SemiBold
      Cursor=Hand
      BorderThickness=0
      ControlTemplate:
        Border CornerRadius=5
        Trigger IsMouseOver=True ⇒ Opacity=0.88
        Trigger IsPressed=True ⇒ Opacity=0.75
  Grid Background=#25333E
    RowDefinitions:
      Row 0 Height=215  (Hero Info Card)
      Row 1 Height=*    (Action Buttons Grid)
    Row 0:
      Border Margin=20 Padding=22 Background=#FAFAFA BorderBrush=#D5D8DA BorderThickness=1 CornerRadius=3
        Grid 2 cols (260, *)
          Col 0: شعار رسومي بـ Ellipse/Path/Rectangle + Segoe MDL2 icon (FlowDirection=LeftToRight)
          Col 1: StackPanel متعدد TextBlocks (header + 4 bullets)
    Row 1:
      Grid يحوي شبكة من Button (Style=ModuleButtonStyle)
```

### الاختلافات لكل ModuleView

#### 1.1 PatientModuleView
- Hero icon: قطرة دم — `Ellipse 126×126 Fill=#E7F2F4 Stroke=#B5CDD2` + `TextBlock "AB-" Foreground=#A71921 FontSize=27` + Segoe MDL2 `&#xE91D;` FontSize=54 Foreground=#376C78 + `Path M55,150 L205,75` (سهم سُحب الدم بلون `#2C6B4F` StrokeThickness=8) + `Path M198,70 L224,60 L213,85 Z` (رأس سهم Fill=#C0392B)
- Button size: Width=220, Height=70, Margin=10
- Grid 2×2 = 4 أزرار

#### 2.1 SystemDataModuleView
- Hero icon: «أرشيف» — Rectangle 150×120 RadiusX=5 RadiusY=5 Fill=#C6B79A Stroke=#7D6E55 + 3 رفوف داخلية Rectangle 132×26 Fill=#E7D8B8 + Segoe MDL2 `&#xE8B7;` FontSize=42 Foreground=#5B4E3B
- Button size: Height=58, Margin=8 (الأصغر بين الـ Hubs)
- Grid 4×4 + StackPanel جانبي مع زرين برتقاليين (FF8C00)
- 14 أزرار إجمالاً
- زر «أجهزة ومعدات المعمل» له `Background=#87CEEB` Foreground=#143347 (لون أزرق سماوي مميَّز)

#### 3.1 AccountsModuleView
- Hero icon: خزينة — Ellipse 126×126 + Segoe MDL2 `&#xE825;` FontSize=64 Foreground=#376C78
- Button size: 220×70 Margin=10
- Grid 2×2 = 4 أزرار

#### 4.1 WorksheetModuleView
- Hero icon: جدول — Ellipse 126×126 + Segoe MDL2 `&#xE81C;` FontSize=64
- Button size: 250×70 Margin=15
- صف واحد من زرين

#### 5.1 StatisticsModuleView
- Hero icon: رسم بياني — Ellipse 126×126 + Segoe MDL2 `&#xE9D2;` FontSize=64
- Button size: 250×70 Margin=10
- Grid 2×3 = 6 أزرار

#### 6.1 SettingsModuleView
- Hero icon: ترس — Segoe MDL2 `&#xE713;` FontSize=64
- Button size: 220×70 Margin=15
- صف واحد من زرين

#### 7.1 ToolsModuleView
- Hero icon: مفتاح ربط — Segoe MDL2 `&#xED43;` FontSize=64
- Button size: 200×65 Margin=8 (الأصغر بعد SystemData)
- Grid 3×3 = 9 أزرار

#### 8.1 UsersModuleView
- Hero icon: شخص — Segoe MDL2 `&#xE77B;` FontSize=64
- Button size: 220×70 Margin=10
- Grid 2×2 = 4 أزرار

---

## التخطيطات المفقودة في Repo (Specifications مستخلصة من PDFs)

### PatientRegistration (المرجع PDF: «إضافة وتعديل بيانات المرضى»)
- **التخطيط المتوقع:** نموذج إدخال يسار-يمين على نمط Form:
  - Header: Lab ID + زر «Generate Barcode» + تاريخ التسجيل
  - Form Fields (Grid 2 أعمدة): Name, Gender (RadioButton: ذكر/أنثى), Age (3 حقول Days/Months/Years), Phone, Address, National ID, ReferralCombo, AccountTypeCombo, Discount, Notes
  - MedicalHistory Section: ChronicDiseases, Allergies, Medications, Notes (TextBoxes متعددة الأسطر)
  - DataGrid أسفل النموذج لعرض Results من البحث
  - Action Bar: New (F1), Save (F9), Delete (F12), Print Barcode (F10), Print Receipt (F11), Search
- **اللون:** خلفية بيضاء، Headers برتقالية `#D56E13`، Action buttons بلون `#234B67` أو `#8DB600`

### PatientTestsSelection (المرجع PDF: «اختيار التحاليل»)
- **التخطيط المتوقع:** 3 أعمدة:
  - Col 1 (يسار): Available Tests — ListBox/DataGrid (مصدر AvailableTests) + SearchBox + RefreshButton
  - Col 2 (وسط): أزرار إضافة/حذف بين القائمتين
  - Col 3 (يمين): Selected Tests — DataGrid (مصدر SelectedTests) + Total
  - Header: LabId, PatientName, VisitId, AccountTypeCombo, ReferralCombo, CustomGroupCombo
  - Footer: TotalAmount + CreateVisit + Save buttons

### ResultsEntry (المرجع PDF: «إدخال نتائج التحاليل»)
- **التخطيط المتوقع:** 3 أعمدة:
  - Col 1: VisitTests List (DataGrid) + DateFrom/DateTo فلتر
  - Col 2: Result Items (DataGrid قابل للتحرير) + Save/Verify/Reopen buttons
  - Col 3: MedicalHistory Panel + Status notifications

### PatientBilling (المرجع PDF: «حساب المريض»)
- **التخطيط المتوقع:** Grid 3×3:
  - Header: VisitId + LoadVisitCommand
  - Middle Left: Totals (Total, Discount, Paid, NetTotal, Balance)
  - Middle Right: Payments DataGrid + Add/Edit/Delete buttons
  - Bottom: AdditionalCharges DataGrid + AddCharge button + Settle + PrintInvoice

### ReportViewer (المرجع PDF: «التقارير»)
- **التخطيط المتوقع:**
  - Header: VisitId + Load + Print + Reprint
  - Body: PDF Preview (WebBrowser أو IFrame عبر `PreviewPdfUri`)
  - Sidebar: Tests List

### Cultures (المرجع PDF: «المزارع»)
- 6 collections في الـ ViewModel ⇒ تخطيط متعدد التبويبات (Tab):
  - Tab 1: Cultures + AddCulture/DeleteCulture
  - Tab 2: Antibiotics + AddAntibiotic/DeleteAntibiotic
  - Tab 3: Link/Unlink Cultures with Antibiotics
  - Tab 4: VisitTests + SaveResult + PrintCultureReport (Sensitivity Matrix)

---

## القياسات الحرجة المؤكدة من XAML

| الـ View | الحاوية | حدود الـ Border العلوية | حجم زر القياسي |
|---|---|---|---|
| LoginView | Window 400×550 | Border 360×Auto Padding=30 | Button 45 high |
| MainWindow | Window 1180×720 (Service ⇒ 1100×700) | Toolbar 90 high, Status 30 high | RadioButton (in 12 col UniformGrid) |
| Hubs الـ 8 | UserControl | Row 0 = 215 px | متفاوت 200-250 × 58-70 |
| PlaceholderView | UserControl | Border 520×210 Padding=32 | Button «رجوع» 150×42 |
| WelcomeView | UserControl | Canvas 520×360 | لا أزرار |

---
