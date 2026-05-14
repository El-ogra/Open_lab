# 06 — UI Keyboard Shortcuts — Open lab system (Final Independent Audit)

> جدول كامل لاختصارات لوحة المفاتيح. مأخوذ من تحليل الكود الفعلي + المرجعين `real lab system help.pdf` و `RLS_Learn.pdf`.

---

## 0) النتيجة الحاسمة من فحص الكود

### بحث صريح بـ grep:
```
grep -rn "KeyBinding" Open_lab/ --include="*.xaml" --include="*.cs"
⇒ 0 نتائج

grep -rn "InputBindings" Open_lab/ --include="*.xaml" --include="*.cs"
⇒ 0 نتائج

grep -rn "Key=" Open_lab/ --include="*.xaml"
⇒ 0 نتائج تتعلق باختصارات لوحة المفاتيح

grep -rn "Modifiers=" Open_lab/ --include="*.xaml"
⇒ 0 نتائج
```

### الخلاصة:
**النظام الحالي (فرع Fi5ve، كوميت 011f15c) لا يحوي أي اختصار لوحة مفاتيح فعّال على الإطلاق، باستثناء الاختصارات الافتراضية لـ WPF.**

---

## 1) الاختصارات الافتراضية الموجودة فعلياً (WPF Built-in)

### في LoginView
| المفتاح | المُحفِّز | السبب التقني |
|---|---|---|
| **Enter** | في أي حقل داخل اللوغين | الزر «دخول» محدد بـ `IsDefault=True` في XAML السطر 203 |
| **Tab** | تنقّل بين الحقول | الترتيب التلقائي: Username → PasswordBox → Eye → CheckBox → Button |
| **Space** | فوق CheckBox أو Button مركَّز | يُفعّل العنصر (سلوك WPF) |

### في MainWindow Toolbar
| المفتاح | المُحفِّز | السبب التقني |
|---|---|---|
| **Tab** | تنقّل بين الـ 12 خانة العلوية | ترتيب XAML |
| **Space** أو **Enter** | على RadioButton/Button مركَّز | تنفيذ Command (سلوك WPF) |
| **Arrow Keys** | داخل GroupName="TopModules" | تنقّل بين RadioButtons داخل نفس المجموعة |

### في كل Hub Module View
| المفتاح | المُحفِّز |
|---|---|
| **Tab** | تنقّل بين أزرار الـ Hub |
| **Space/Enter** | تنفيذ Command لزر مركَّز |

### في PlaceholderView
| المفتاح | المُحفِّز |
|---|---|
| **Tab** | تركيز زر «رجوع» |
| **Space/Enter** | تنفيذ BackCommand |
| **Esc** | ❌ لا ربط (يجب إضافة `<KeyBinding Key="Escape" Command="{Binding BackCommand}" />`) |

---

## 2) الاختصارات المُتوقَّعة من المرجعين (PDFs)

### المرجع الأول: `real lab system help.pdf` (Real Lab System Version 1.0 — AIS Group)

| المفتاح | السياق | الوظيفة المطلوبة | الـ Command المقابل في الكود الحالي |
|---|---|---|---|
| **F1** | داخل PatientRegistration | إضافة سجل مريض جديد | NewCommand (موجود في PatientRegistrationViewModel) |
| **F2** | في MainWindow | فتح PatientModuleView | NavigateToPatientsCommand |
| **F3** | في MainWindow | فتح PatientSearch | NavigatePatientSearchCommand |
| **F4** | في MainWindow | فتح ResultsEntry | NavigateResultsEntryCommand |
| **F5** | في PatientSearch | RefreshSearch | (غير موجود — يجب إضافته) |
| **F6** | في MainWindow | فتح Delivery (تسليم نتائج) | NavigateDeliveryCommand |
| **F7** | في MainWindow | فتح External Samples | NavigateAccountsTreasuryCommand (مع تبويبة External) |
| **F8** | في ResultsEntry | Toggle Reviewed | VerifyResultsCommand (موجود) |
| **F9** | عام (في أي شاشة بحفظ) | Save / Toggle Complete | SaveCommand (موجود في معظم VMs) |
| **F10** | في PatientRegistration | طباعة الباركود | (غير موجود — لكن `Open_lab/Services/BarcodeService.cs` متاح) |
| **F11** | في PatientRegistration | طباعة الإيصال | (غير موجود — لكن `ReceiptService.cs` متاح) |
| **F12** | في PatientRegistration | Delete / Print Preview | DeleteCommand (موجود) أو PrintCommand |

### المرجع الثاني: `RLS_Learn.pdf` (Real Lab System Guide Book — RealLab Co.)

نفس الاختصارات تقريباً مع تأكيد إضافي للاختصارات في:
- **PatientRegistration:** F2 لفتح، F1 إضافة، F9 حفظ، F10 باركود، F11 إيصال، F12 حذف
- **ResultsEntry:** F4 فتح، F8 تبديل المراجعة، F9 تبديل الاكتمال، F12 معاينة الطباعة، Enter للانتقال
- **PatientSearch:** F3 فتح، F5 تحديث
- **Delivery:** F6 فتح

---

## 3) القواعد المعيارية المُقترحة لتطبيق الاختصارات

### مكان التعريف (المعيار في WPF)
1. **اختصارات عامة (Global Scope):** `Window.InputBindings` في `MainWindow.xaml`
   - تعمل في كل الـ Views المستضافة في ContentControl
2. **اختصارات محلية (Local Scope):** `UserControl.InputBindings` في كل XAML خاص
   - تعمل فقط داخل الـ View النشطة
3. **التعارض:** الاختصار المحلي يتجاوز العام داخل نفس Scope (WPF يستخدم Bubbling/Tunneling)

### النمط المعياري المُقترح
```xml
<UserControl.InputBindings>
    <KeyBinding Key="F1" Command="{Binding NewCommand}" />
    <KeyBinding Key="F9" Command="{Binding SaveCommand}" />
    <KeyBinding Key="F12" Command="{Binding DeleteCommand}" />
    <KeyBinding Key="Escape" Command="{Binding BackCommand}" />
    <KeyBinding Key="S" Modifiers="Control" Command="{Binding SaveCommand}" />
</UserControl.InputBindings>
```

---

## 4) الجدول الموحَّد للاختصارات المطلوب تنفيذها

### F-Keys العامة

| المفتاح | المُحفِّز | الشرط | النتيجة المطلوبة | النطاق المُقترح |
|---|---|---|---|---|
| F1 | ضغط F1 | داخل PatientRegistration | NewCommand | محلي |
| F2 | ضغط F2 | في MainWindow بعد Login | NavigateToPatientsCommand | عام |
| F3 | ضغط F3 | في MainWindow | NavigatePatientSearchCommand | عام |
| F4 | ضغط F4 | في MainWindow | NavigateResultsEntryCommand | عام |
| F5 | ضغط F5 | داخل PatientSearch | SearchCommand (refresh) | محلي |
| F6 | ضغط F6 | في MainWindow | NavigateDeliveryCommand | عام |
| F7 | ضغط F7 | في MainWindow | NavigateAccountsTreasuryCommand | عام |
| F8 | ضغط F8 | داخل ResultsEntry | VerifyResultsCommand (Toggle Reviewed) | محلي |
| F9 | ضغط F9 | داخل شاشة فيها Save | SaveCommand (Save / Toggle Complete) | محلي |
| F10 | ضغط F10 | داخل PatientRegistration / ReceiptPrinting | PrintBarcodeCommand | محلي |
| F11 | ضغط F11 | داخل PatientRegistration / ReceiptPrinting | PrintReceiptCommand | محلي |
| F12 | ضغط F12 | داخل شاشة فيها Delete | DeleteCommand / Print Preview | محلي |

### اختصارات Ctrl المُقترحة (غير موجودة في PDFs لكن معيار صناعي)

| المفتاح | المُحفِّز | النتيجة المُقترحة |
|---|---|---|
| Ctrl+S | عام | SaveCommand للنموذج النشط |
| Ctrl+N | عام | NewCommand للنموذج النشط |
| Ctrl+F | عام | SearchCommand للنموذج النشط |
| Ctrl+P | عام | PrintCommand للنموذج النشط |
| Ctrl+L | عام | LogoutCommand |
| Ctrl+Tab | عام | تنقّل بين الـ Hubs |

### اختصارات Esc و Enter

| المفتاح | المُحفِّز | النتيجة المُقترحة |
|---|---|---|
| Esc | داخل PlaceholderView | BackCommand |
| Esc | داخل أي Hub | ❌ لا فعل (الـ Hub هو نقطة البداية) |
| Esc | داخل LoginView | تفريغ Username و Password |
| Enter | في PatientRegistration LabId | LoadByLabIdCommand |
| Enter Enter | في PatientBilling | Settle account (PDF reference) |
| Enter | في ResultsEntry قيمة نتيجة | الانتقال للحقل التالي |

### اختصارات Alt المعيارية

| المفتاح | المُحفِّز | النتيجة |
|---|---|---|
| Alt+F4 | على أي Window | إغلاق النافذة (سلوك Windows افتراضي) |
| Alt+Space | على أي Window | فتح قائمة النافذة (افتراضي) |

---

## 5) التحقق من الاختصارات الفعلية

### حالة كل Command في الكود من حيث القابلية للربط بـ KeyBinding

| الـ Command | موجود في VM؟ | يمكن ربطه بـ KeyBinding بعد إضافة XAML؟ |
|---|:---:|:---:|
| LoginViewModel.LoginCommand | ✅ | ✅ (Enter يعمل عبر IsDefault — لا حاجة لـ KeyBinding) |
| NewCommand (PatientRegistration) | ✅ | ✅ بعد إضافة الـ View |
| SaveCommand (متعدد) | ✅ | ✅ بعد إضافة الـ View |
| DeleteCommand (متعدد) | ✅ | ✅ بعد إضافة الـ View |
| GenerateBarcodeCommand (TestCatalog) | ✅ | ✅ |
| PrintBarcodeCommand (ReceiptPrinting) | ✅ | ✅ |
| PrintReceiptCommand (ReceiptPrinting) | ✅ | ✅ |
| LogoutCommand | ✅ (في MainViewModel) | ✅ |
| BackCommand (PlaceholderView) | ✅ (Property في code-behind) | ⚠️ يحتاج تعديل لجعله Property في ViewModel |

---

## 6) حالات خاصة وحساسية اللغة

### اختصارات لا تتأثر بـ FlowDirection
- **F-Keys (F1-F12):** ثابتة فيزيائياً بغض النظر عن RTL/LTR
- **Ctrl+*:** ثابتة
- **Alt+*:** ثابتة

### اختصارات قد تتأثر بـ Tab Order في RTL
- في `FlowDirection=RightToLeft`، Tab يتنقل من اليمين لليسار في UniformGrid
- في MainWindow Toolbar `<UniformGrid Columns="12" FlowDirection="RightToLeft">`، الترتيب: من «المرضى» (يمين) إلى «خروج» (يسار)
- لكن Tab order الفعلي في WPF لا يتأثر بـ FlowDirection (يعتمد على ترتيب التعريف في XAML)

### تعارض محتمل مع نظام التشغيل
- F1 يفتح Windows Help في بعض السياقات — يجب اعتراضه بـ `e.Handled=true`
- F10 يُفعّل قائمة النافذة في Windows — يجب اعتراضه

---

## 7) ملخص الفجوة بين الواقع والمطلوب

| الحالة | العدد |
|---|---:|
| اختصارات WPF افتراضية متاحة | ~10 (Tab, Enter (IsDefault), Space, Arrows في GroupName) |
| اختصارات F-Keys المطلوبة من PDFs | 12 |
| اختصارات F-Keys مُطبَّقة فعلياً | 0 |
| اختصارات Ctrl المُقترحة كمعيار صناعي | 6 |
| اختصارات Ctrl مُطبَّقة فعلياً | 0 |
| اختصارات Esc/Custom | 5 مطلوبة |
| اختصارات Esc/Custom مُطبَّقة | 0 |

### إجمالي فجوة الاختصارات: 23 اختصاراً يجب إضافتها

---

## 8) سيناريوهات الاستخدام السريع (مع الاختصارات المطلوبة)

### سيناريو 1: تسجيل مريض جديد سريع
```
[في MainWindow] ⇒ F2 (افتح المرضى)
    [في PatientModuleView Hub] ⇒ Click «اضافة وتعديل بيانات المرضى»
        [في PatientRegistration] ⇒ F1 (سجل جديد)
            تعبئة Form
            F10 (طباعة باركود)
            F9 (حفظ)
            F11 (طباعة إيصال)
```

### سيناريو 2: إدخال نتائج
```
[في MainWindow] ⇒ F4 (افتح إدخال النتائج)
    [في ResultsEntry] ⇒ تحديد VisitTest من القائمة
        إدخال قيم نتائج (Enter للتنقل)
        F8 (تبديل المراجعة)
        F9 (حفظ + Complete)
        F12 (معاينة الطباعة)
```

### سيناريو 3: البحث وطباعة تاريخ مريض
```
[في MainWindow] ⇒ F3 (افتح البحث)
    [في PatientSearch] ⇒ تعبئة Name أو LabId
        F5 (تحديث/بحث)
        Click على المريض من النتائج
        تظهر Visits
```

---

## 9) ملاحظات تنفيذية حرجة

1. **PlaceholderView** يفتقد لربط Esc — يجب أن يدعم Esc كاختصار للـ BackCommand
2. **LoginView** Enter موجود تلقائياً عبر IsDefault، لكن يجب إضافة Esc لتفريغ الحقول
3. **InputBindings** يجب أن تُضاف في كل XAML على مستوى `UserControl.InputBindings` أو `Window.InputBindings`
4. **التعارض بين Tab وأرقام Arabic:** لا يوجد — F-Keys مستقلة عن اللغة
5. **CanExecute** سيمنع تنفيذ الـ Command لو لم تكن الشروط متوفرة (مثلاً F9=SaveCommand لن يعمل لو لم تكن الصلاحية موجودة)
6. **اختصار IsDefault** لا يتعارض مع KeyBinding — Enter ينفِّذ الزر الـ Default، بينما KeyBinding يحدد سلوك مفتاح آخر

---
