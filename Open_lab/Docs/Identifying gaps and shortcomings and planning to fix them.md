# Identifying gaps and shortcomings and planning to fix them

## 1. جدول جرد المكوّنات الكامل

النافذة الفعلية هي `Open_lab/Views/Patients/PatientRegistrationView.xaml`، وهي مربوطة ضمن `DataTemplate` إلى `PatientRegistrationViewModel` في `Open_lab/App.xaml:55-56`. لا يوجد `DataContext` في code-behind؛ الملف الخلفي يستدعي `InitializeComponent()` فقط في `Open_lab/Views/Patients/PatientRegistrationView.xaml.cs:9`.

| # | النوع | النص/الغرض الظاهر | Binding | مرجع الواجهة | حالة الربط |
|---|---|---|---|---|---|
| 1 | Button | start | `StartCommand` | `PatientRegistrationView.xaml:193-195` | موجود في VM: `PatientRegistrationViewModel.cs:224,1042` |
| 2 | TextBox | بحث التحاليل | `TestSearchText` | `PatientRegistrationView.xaml:198-200` | موجود في VM: `PatientRegistrationViewModel.cs:827` |
| 3 | RadioButton | TM | `TestCategoryFilter` مع `StringEqualsConverter` | `PatientRegistrationView.xaml:201-203` | موجود في VM: `PatientRegistrationViewModel.cs:809`، والـ converter مسجل في `App.xaml:33` |
| 4 | RadioButton | TG | `TestCategoryFilter` | `PatientRegistrationView.xaml:207-209` | موجود |
| 5 | RadioButton | CC | `TestCategoryFilter` | `PatientRegistrationView.xaml:213-215` | موجود |
| 6 | DataGrid | قائمة التحاليل المتاحة | `AvailableTests`, `SelectedAvailableTest`; double click إلى `AddSelectedTestCommand` | `PatientRegistrationView.xaml:233-243` | موجودة في VM: `927,931,1026` |
| 7 | DataGrid | تحاليل المريض | `SelectedTests`, `SelectedTest` | `PatientRegistrationView.xaml:272-280` | موجودة في VM: `928,944` |
| 8 | TextBlock | عدد تحاليل المريض | `SelectedTests.Count` | `PatientRegistrationView.xaml:260` | مصدره `SelectedTests` موجود في VM: `928` |
| 9 | Button | Add select test | `AddTestCommand` | `PatientRegistrationView.xaml:292-294` | موجود: `1020` |
| 10 | Button | Delete | `RemoveTestCommand` | `PatientRegistrationView.xaml:297-299` | موجود: `1022` |
| 11 | Button | All | `AddAllTestsCommand` | `PatientRegistrationView.xaml:302-304` | موجود: `1021` |
| 12 | CheckBox | Urine | `HasUrineSample` | `PatientRegistrationView.xaml:316` | موجود: `783` |
| 13 | CheckBox | Stool | `HasStoolSample` | `PatientRegistrationView.xaml:317` | موجود: `789` |
| 14 | CheckBox | Blood | `HasBloodSample` | `PatientRegistrationView.xaml:318` | موجود: `771` |
| 15 | CheckBox | Semen | `HasSemenSample` | `PatientRegistrationView.xaml:319` | موجود: `795` |
| 16 | CheckBox | CSF | `HasCsfSample` | `PatientRegistrationView.xaml:320` | موجود: `801` |
| 17 | Grid | تعطيل/تفعيل بيانات المريض | `IsEditMode` | `PatientRegistrationView.xaml:326` | موجود: `1000` |
| 18 | DatePicker | تاريخ الدخول | `EntryDate` | `PatientRegistrationView.xaml:352` | موجود: `420` |
| 19 | TextBox | وقت الدخول | `EntryTimeText` | `PatientRegistrationView.xaml:353` | موجود: `456` |
| 20 | DatePicker | تاريخ الاستلام | `ReceptionDate` | `PatientRegistrationView.xaml:357` | موجود: `432` |
| 21 | TextBox | وقت الاستلام | `ReceptionTimeText` | `PatientRegistrationView.xaml:358` | موجود: `462` |
| 22 | TextBox | كود المريض | `PatientCode` | `PatientRegistrationView.xaml:363` | موجود: `280` |
| 23 | CheckBox | VIP | `IsVip` | `PatientRegistrationView.xaml:365` | موجود: `382` |
| 24 | TextBox | Lab ID | `LabId` | `PatientRegistrationView.xaml:370` | موجود: `268` |
| 25 | ComboBox | لقب المريض | `Title` | `PatientRegistrationView.xaml:393-398` | موجود: `406` |
| 26 | TextBox | اسم المريض | `FullName` | `PatientRegistrationView.xaml:399` | موجود: `286` |
| 27 | RadioButton | Male | `Gender` مع converter | `PatientRegistrationView.xaml:401` | موجود: `292` |
| 28 | RadioButton | Female | `Gender` مع converter | `PatientRegistrationView.xaml:402` | موجود: `292` |
| 29 | TextBox | الموبايل | `MobilePhone` | `PatientRegistrationView.xaml:417` | موجود، ويحدّث `Phone`: `340-348` |
| 30 | TextBox | هاتف المنزل | `HomePhone` | `PatientRegistrationView.xaml:418` | موجود: `352` |
| 31 | TextBox | العمر | `Age` | `PatientRegistrationView.xaml:427` | موجود: `370` |
| 32 | ComboBox | وحدة العمر | `AgeUnit` | `PatientRegistrationView.xaml:428-432` | موجود: `376` |
| 33 | TextBox | الرقم القومي | `NationalId` | `PatientRegistrationView.xaml:446` | موجود: `358` |
| 34 | ComboBox | نوع الحساب | `AccountType` | `PatientRegistrationView.xaml:450-454` | موجود: `400` |
| 35 | TextBox | عنوان المريض | `Address` | `PatientRegistrationView.xaml:465` | موجود: `310` |
| 36 | ComboBox | لقب الطبيب | `DoctorTitle` | `PatientRegistrationView.xaml:485-489` | موجود: `512` |
| 37 | TextBox | الطبيب المعالج/ملاحظة الإحالة | `ReferralTitleNote` | `PatientRegistrationView.xaml:490` | موجود: `506` |
| 38 | CheckBox | Print | `PrintReferralOnReport` | `PatientRegistrationView.xaml:491-493` | موجود: `536` |
| 39 | ComboBox | جهة الإحالة | `Referrals`, `SelectedReferral`, `ReferralSource` | `PatientRegistrationView.xaml:503-509` | موجودة: `926,957,476` |
| 40 | CheckBox | هل يأخذ علاج السكر | `TakesDiabetesTreatment` | `PatientRegistrationView.xaml:535` | alias موجود: `601-605` |
| 41 | CheckBox | مريض فقر الدم | `HasAnemia` | `PatientRegistrationView.xaml:536` | موجود: `733` |
| 42 | TextBox | عدد ساعات الصيام | `FastingHours` | `PatientRegistrationView.xaml:537` | موجود: `394` |
| 43 | CheckBox | صائم | `IsFasting` | `PatientRegistrationView.xaml:538` | موجود: `388` |
| 44 | CheckBox | هل يأخذ علاج للغدة | `TakesGlandsTreatment` | `PatientRegistrationView.xaml:540` | alias موجود: `673-677` |
| 45 | CheckBox | مريض بالتهاب المفاصل | `HasJointInflammation` | `PatientRegistrationView.xaml:541` | موجود: `739` |
| 46 | CheckBox | هل يأخذ مضاد حيوي | `TakesAntibiotics` | `PatientRegistrationView.xaml:546` | alias موجود: `583-587` |
| 47 | CheckBox | مريض بالضغط الدم | `HasHypertension` | `PatientRegistrationView.xaml:547` | موجود: `745` |
| 48 | CheckBox | هل يأخذ علاج لسيولة الدم | `TakesBloodThinners` | `PatientRegistrationView.xaml:548` | alias موجود: `619-623` |
| 49 | CheckBox | هل يأخذ علاج للكبد | `TakesLiverTreatment` | `PatientRegistrationView.xaml:550` | alias موجود: `691-695` |
| 50 | CheckBox | مريض بالفشل الكلوي | `HasKidneyFailure` | `PatientRegistrationView.xaml:551` | موجود: `751` |
| 51 | CheckBox | هل يأخذ علاج للفيروسات | `TakesAntiVirals` | `PatientRegistrationView.xaml:552` | alias موجود: `637-641` |
| 52 | CheckBox | للسيدات هل يوجد حمل | `IsPregnant` | `PatientRegistrationView.xaml:554` | موجود: `697` |
| 53 | CheckBox | مريض بالإصابة بمرض مزمن | `HasChronicDisease` | `PatientRegistrationView.xaml:555` | موجود: `757` |
| 54 | CheckBox | مضاعفات الحمل | `HasPregnancyComplication` | `PatientRegistrationView.xaml:556` | موجود: `763` |
| 55 | CheckBox | أشعة صبغة/موجات خلال يومين | `HadContrastImaging` | `PatientRegistrationView.xaml:557` | alias موجود: `721-725` |
| 56 | TextBox | ملاحظات طبية | `MedicalNotes` | `PatientRegistrationView.xaml:559` | موجود: `334` |
| 57 | TextBox | الباقي للمعمل | `BalanceForLab` OneWay | `PatientRegistrationView.xaml:586` | محسوب: `913` |
| 58 | TextBox | إجمالي التحاليل | `TotalAmount` OneWay | `PatientRegistrationView.xaml:590` | محسوب: `835` |
| 59 | TextBox | الباقي للمريض | `BalanceForPatient` OneWay | `PatientRegistrationView.xaml:595` | محسوب: `914` |
| 60 | TextBox | نسبة الخصم | `DiscountPercent` | `PatientRegistrationView.xaml:602` | موجود: `837` |
| 61 | TextBox | قيمة الخصم المدخلة | `DiscountValue` | `PatientRegistrationView.xaml:603` | موجود: `849` |
| 62 | TextBox | المدفوع | `PaidAmount` | `PatientRegistrationView.xaml:608` | موجود: `875` |
| 63 | TextBox | قيمة الخصم المحسوبة | `DiscountAmount` OneWay | `PatientRegistrationView.xaml:612` | محسوب: `864` |
| 64 | TextBox | التكلفة بعد الخصم | `NetTotal` OneWay | `PatientRegistrationView.xaml:617` | محسوب: `873` |
| 65 | TextBox | المدفوع سابقاً | `PreviousPaidAmount` | `PatientRegistrationView.xaml:622` | موجود: `900` |
| 66 | Button | موافق | `SaveCommand` | `PatientRegistrationView.xaml:627` | موجود: `1014` |
| 67 | Button | تراجع | `UndoCommand` | `PatientRegistrationView.xaml:628` | موجود: `1037` |
| 68 | Button | خالص | `SettleCommand` | `PatientRegistrationView.xaml:629` | موجود: `1038` |
| 69 | Button | تراجع ✖ | `UndoCommand` | `PatientRegistrationView.xaml:641` | موجود |
| 70 | Button | حفظ ✔ | `SaveCommand` | `PatientRegistrationView.xaml:642` | موجود |
| 71 | Button | تعديل 👤 | `EditCommand` | `PatientRegistrationView.xaml:643` | موجود: `1028` |
| 72 | Button | إضافة 👤 | `NewCommand` | `PatientRegistrationView.xaml:644` | موجود: `1015` |
| 73 | Button | الكارنية | `InsuranceCommand` | `PatientRegistrationView.xaml:648` | موجود: `1041` |
| 74 | Button | ورقة العمل | `WorksheetCommand` | `PatientRegistrationView.xaml:649` | موجود: `1040` |
| 75 | Button | الباركود | `ShowBarcodeCommand` | `PatientRegistrationView.xaml:650` | موجود: `1023` |
| 76 | Button | الإيصال | `PrintReceiptCommand` | `PatientRegistrationView.xaml:651` | موجود: `1024` |
| 77 | Button | القائمة الرئيسية | `GoToHomeCommand` | `PatientRegistrationView.xaml:656` | موجود: `1032` |
| 78 | Button | نتائج التحاليل | `GoToResultsCommand` | `PatientRegistrationView.xaml:657` | موجود: `1029` |
| 79 | Button | قائمة المرضى | `ShowTodayPatientsCommand` | `PatientRegistrationView.xaml:658` | موجود: `1035` |
| 80 | Popup | قائمة مرضى اليوم السريعة | `IsQuickPatientsListVisible` | `PatientRegistrationView.xaml:662-666` | موجود: `994` |
| 81 | DataGrid | مرضى اليوم داخل Popup | `QuickPatientsList`, `QuickSelectedPatient` | `PatientRegistrationView.xaml:668-676` | موجودة: `929,981` |

## 2. تقرير تدقيق: الواجهة ↔ ViewModel

كل الـ bindings الظاهرة في الواجهة لها Property أو Command مطابق في `PatientRegistrationViewModel` حسب الجدول أعلاه. كما أن الـ View مرتبط بالـ ViewModel عبر `DataTemplate` في `App.xaml:55-56`، وهذا متوافق مع MVVM.

الملاحظات:

| الأولوية | الملاحظة | الدليل من الواجهة | الدليل من ViewModel | الأثر |
|---|---|---|---|---|
| P1 | زر `الكارنية` مربوط لكنه ليس تنفيذاً كاملاً؛ يعرض رسالة "قيد التطوير" فقط. | `PatientRegistrationView.xaml:648` | إنشاء الأمر في `PatientRegistrationViewModel.cs:223` وتنفيذه في `1900-1908` | المستخدم يرى زر وظيفة كبيرة لكن لا توجد شاشة/طباعة/حفظ كارنية. |
| P2 | `PatientCode` له حقل إدخال، لكن لا يوجد زر أو حدث في الواجهة يستدعي `LoadByCodeCommand`. | الحقل في `PatientRegistrationView.xaml:363` | الأمر موجود في VM: `212,1033` وتنفيذه `1171-1189` | إدخال كود المريض لا يؤدي لتحميل المريض من الواجهة الحالية. |
| P2 | أوامر بحث/تحميل/حذف موجودة في VM وغير ظاهرة في الواجهة: `GenerateLabIdCommand`, `LoadByLabIdCommand`, `SearchCommand`, `DeleteCommand`, `ResetCommand`, `DocumentsCommand`, `NewPatientCommand`, `ShowMovementCommand`, `SendCommand`, `RemoveSelectedTestCommand`. | لا توجد Binding مقابلة لهذه الأوامر في جرد أزرار XAML. | تعريفها في `PatientRegistrationViewModel.cs:195-216` و `1016-1037` | وظائف موجودة برمجياً لكنها غير قابلة للوصول من هذه النافذة. |
| P2 | عناصر بيانات موجودة في VM وغير مستخدمة مباشرة في الواجهة: `BirthDate`, `DeliveryDate`, `SystemTimeText`, `PatientTitleNote`, `ResponsibleName`, `ReferralAddress`, `ReferralPhone`, `ReferralAltPhone`, `ReferralResponsible`, `Email`, `ChronicDiseases`, `Allergies`, `Medications`, `SearchText`, `Results`, `SelectedPatient`. | لا تظهر في جرد XAML. | أمثلة: `BirthDate` في `298`, `DeliveryDate` في `426`, `SystemTimeText` في `468`, `Email` في `364`, `Results` في `925` | إما بقايا من شاشة أوسع أو نقص في عناصر الواجهة. |
| P3 | `SelectedTests.Count` مستخدم في TextBlock، لكنه لا يراقب تغيّر Count تلقائياً عبر Binding مستقل؛ الكود يستدعي `OnPropertyChanged(nameof(SelectedTests))` عند الإضافة فقط. | `PatientRegistrationView.xaml:260` | الإضافة تستدعي `OnPropertyChanged(nameof(SelectedTests))` في `1466-1467`، والحذف لا يظهر في المقطع إلا بعد `1492-1494` بدون `OnPropertyChanged(nameof(SelectedTests))` | قد لا يتحدث عداد التحاليل بعد الحذف حسب سلوك Binding إلى `Count`. |

## 3. تقرير تدقيق: ViewModel ↔ Service

الـ ViewModel يستقبل الخدمات عبر constructor في `PatientRegistrationViewModel.cs:169-177` ويحفظها في حقول خاصة في `179-185`. كما ينشئ الأوامر في `193-224` ويبدأ التحميل بـ `_ = InitializeAsync()` في `226-228`.

| استدعاء VM | السطر في VM | الخدمة/الدالة المطلوبة | وجودها | الحالة |
|---|---:|---|---|---|
| إنشاء مريض | `1058` | `IPatientService.CreateAsync` | `IPatientService.cs:16`, `PatientService.cs:159-183` | موجود ومكتمل |
| تحديث مريض | `1084-1101` | `IPatientService.UpdateAsync(patient, userId)` | `IPatientService.cs:18`, `PatientService.cs:191-198` | موجود ومكتمل |
| توليد LabId | `1137` | `GenerateNextLabIdAsync` | `IPatientService.cs:15`, `PatientService.cs:123-157` | موجود ومكتمل |
| تحميل بـ LabId | `1155`, `1181` | `GetByLabIdAsync` | `IPatientService.cs:11`, `PatientService.cs:27-35` | موجود ومكتمل |
| البحث | `1201`, `1420` | `SearchAsync` | `IPatientService.cs:14`, `PatientService.cs:92-120` | موجود ومكتمل |
| حذف مريض | `1224` | `DeleteAsync` | `IPatientService.cs:20`, `PatientService.cs:375-390` | موجود ومكتمل |
| تحميل الإحالات | `1347` | `ITestCatalogService.GetReferralsAsync` | `ITestCatalogService.cs:52`, `TestCatalogService.cs:560-562` | موجود ومكتمل |
| تحميل التحاليل | `1365`, `1381` | `ITestCatalogService.GetAllTestsAsync` | `ITestCatalogService.cs:10`, `TestCatalogService.cs:30-32` | موجود ومكتمل |
| إنشاء/تحميل زيارة | `1521`, `1526` | `IVisitService.GetByIdAsync/CreateAsync` | `IVisitService.cs:9,12`, `VisitService.cs:22-24,51-91` | موجود ومكتمل |
| تحاليل الزيارة | `1537`, `1545`, `1483` | `GetVisitTestsAsync`, `AddTestToVisitAsync`, `RemoveVisitTestAsync` | `IVisitService.cs:11,15,17`, `VisitService.cs:41-43,129-163,223-253` | موجود ومكتمل |
| الفاتورة | `1550`, `1771` | `IInvoiceService.CreateOrUpdateInvoiceAsync` | `IInvoiceService.cs:9`, `InvoiceService.cs:20-52` | موجود ومكتمل |
| تحميل فاتورة الزيارة | `1667` | `IInvoiceService.GetByVisitIdAsync` | `IInvoiceService.cs:16`, `InvoiceService.cs:234-236` | موجود ومكتمل |
| التاريخ المرضي | `1812`, `1833` | `GetMedicalHistoryAsync`, `SaveMedicalHistoryAsync` | `IPatientService.cs:12-13`, `PatientService.cs:37-89` | موجود ومكتمل |
| الطباعة | `1743`, `1894` | `IPrintService.PrintReceiptAsync`, `PrintWorksheetByPatientAsync` | `IPrintService.cs:11,14`, `PrintService.cs:23,126` | موجود ومكتمل من ناحية النداء |

مشاكل ViewModel غير مكتملة أو غير مستخدمة:

| الأولوية | العنصر | الدليل | الحالة |
|---|---|---|---|
| P1 | `InsuranceCommand` | `PatientRegistrationViewModel.cs:223`, `1900-1908` | موجود لكنه placeholder فقط. |
| P2 | `DocumentsCommand` | `PatientRegistrationViewModel.cs:210`, `1922-1931` | غير مستخدم في الواجهة، والتنفيذ يعرض StatusMessage فقط. |
| P2 | `SearchCommand`, `LoadByCodeCommand`, `LoadByLabIdCommand`, `DeleteCommand` | `PatientRegistrationViewModel.cs:196-198,212` | أوامر موجودة وغير مربوطة بأزرار في هذه الواجهة. |
| P2 | `SaveVisitAndInvoiceAsync` يستخدم `TransactionScope` | `PatientRegistrationViewModel.cs:1512-1516` | يوجد تعليق في `PatientService.SaveRegistrationAsync` بأن SQLite لا يدعم ambient transactions واختار `BeginTransactionAsync` بدلاً منها في `PatientService.cs:284-293`. هذا تعارض تصميمي داخل الكود بين مسار VM ومسار Service. |
| P3 | خصائص alias طبية | `TakesAntibiotics` في `583-587`, `TakesDiabetesTreatment` في `601-605`, `HadContrastImaging` في `721-725` | موجودة لتطابق XAML؛ ليست خللاً. |

## 4. تقرير تدقيق: Service ↔ قاعدة البيانات

لا يوجد مجلد `Open_lab/Repositories` في المشروع أثناء البحث، لذلك الطبقة الأدنى للـ Services هنا هي `OpenLabDbContext`. الـ DbContext يعرّف الجداول المطلوبة مباشرة: `Patients` في `OpenLabDbContext.cs:29`, `Visits` في `30`, `Referrals` في `31`, `Tests` في `32`, `VisitTests` في `38`, `Invoices` في `41`, `Payments` في `42`, و`MedicalHistories` في `53`.

| الخدمة | الدوال المرتبطة بنافذة التسجيل | دعم DbContext | الحالة |
|---|---|---|---|
| `PatientService` | `GetByIdAsync`, `GetByLabIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `SearchAsync`, `GenerateNextLabIdAsync`, `Get/SaveMedicalHistoryAsync` | تستخدم `_db.Patients` في `24,34,94,127,175,181,243,377`، و`_db.MedicalHistories` في `44,67`، و`_db.Referrals` في `416` | مكتملة؛ لا توجد `NotImplementedException` أو دوال فارغة ضمن الدوال المستخدمة. |
| `VisitService` | `GetByIdAsync`, `CreateAsync`, `GetVisitTestsAsync`, `AddTestToVisitAsync`, `RemoveVisitTestAsync` | تستخدم `_db.Visits`, `_db.VisitTests`, `_db.Tests` في `VisitService.cs:24,43,91,131,148,163,225,253` | مكتملة حسب الدوال المستخدمة. |
| `InvoiceService` | `CreateOrUpdateInvoiceAsync`, `GetByVisitIdAsync` | تستخدم `_db.Visits`, `_db.Invoices`, `_db.VisitTests`, `_db.Payments` في `InvoiceService.cs:32,45,52,234-247` | مكتملة حسب الدوال المستخدمة. |
| `TestCatalogService` | `GetAllTestsAsync`, `GetReferralsAsync` | تستخدم `_db.Tests` في `TestCatalogService.cs:30-32` و`_db.Referrals` في `560-562` | مكتملة حسب الدوال المستخدمة. |
| `PrintService` | `PrintReceiptAsync`, `PrintWorksheetByPatientAsync` | خدمة طباعة لا تعتمد مباشرة على DbContext في النداءات أعلاه | الدوال موجودة في `PrintService.cs:23,126`. |

ملاحظة تصميمية: `PatientService` يحتوي دالة شاملة `SaveRegistrationAsync` تحفظ `Patient + Visit + VisitTests + Invoice` داخل transaction في `PatientService.cs:295-366`، لكن `PatientRegistrationViewModel.SaveAsync` لا يستخدمها؛ بدلاً من ذلك يستدعي `CreateAsync/UpdateAsync` ثم `SaveVisitAndInvoiceAsync` في `PatientRegistrationViewModel.cs:1058-1079` و`1084-1104`.

## 5. جدول الفجوات والمشاكل المكتشفة مرتّبة حسب الأولوية

| الأولوية | الفجوة | الدليل | حجم الفجوة |
|---|---|---|---|
| P1 | زر `الكارنية` وظيفي كبير لكنه placeholder. | XAML: `PatientRegistrationView.xaml:648`; VM: `PatientRegistrationViewModel.cs:1900-1908` | مربوط لكن الأمر غير مكتمل. |
| P1 | مسار حفظ التسجيل في VM لا يستخدم دالة `PatientService.SaveRegistrationAsync` الشاملة رغم وجودها. | VM: `1058-1079`, `1084-1104`, `1497-1556`; Service: `PatientService.cs:295-366` | مكتمل جزئياً مع ازدواجية منطق وخطر اختلاف قواعد الحفظ. |
| P1 | `SaveVisitAndInvoiceAsync` يستخدم `TransactionScope` بينما تعليق الخدمة يذكر تجنب ambient transactions مع SQLite. | VM: `1512-1516`; Service: `PatientService.cs:284-293` | خطر runtime حسب provider؛ فجوة تصميمية موثقة من الكود نفسه. |
| P2 | أوامر مهمة غير مربوطة في الواجهة الحالية. | VM: `GenerateLabIdCommand` `195`, `LoadByLabIdCommand` `196`, `SearchCommand` `197`, `DeleteCommand` `198`, `LoadByCodeCommand` `212`; لا تظهر في أزرار XAML | غير مربوطة كلياً. |
| P2 | حقل `PatientCode` بلا إجراء تحميل مرتبط في XAML. | XAML: `363`; VM: `LoadByCodeAsync` في `1171-1189` | إدخال بلا trigger واضح. |
| P2 | `DocumentsCommand` و`ShowDocuments` غير مستخدمين ويعرضان رسالة فقط. | VM: `210`, `1922-1931` | مربوط داخلياً لكنه غير مكشوف وغير مكتمل كشاشة مستندات. |
| P2 | خصائص كثيرة في VM ليست ممثلة في هذه الواجهة. | أمثلة VM: `BirthDate` `298`, `DeliveryDate` `426`, `Email` `364`, `ChronicDiseases` `316`, `Allergies` `322`, `Medications` `328`, `Results` `925` | إما نقص واجهة أو بقايا تصميم سابق. |
| P3 | عداد `SelectedTests.Count` قد يحتاج إشعاراً أوضح عند الحذف. | XAML: `260`; VM إضافة: `1466-1467`; حذف: `1492-1494` | احتمال خلل تحديث واجهة، ليس مؤكداً من الكود وحده. |

## 6. تصنيف الأزرار حسب نوع وظيفتها

| الزر | التصنيف | الوضع الحالي | الوضع المفترض | حجم الفجوة |
|---|---|---|---|---|
| start | أ | يطبق فلتر التحاليل عبر `ApplyTestFilterAsync` في `1374-1412` | فلترة القائمة | مكتمل |
| Add select test | أ | يضيف التحليل المحدد في الذاكرة عبر `AddSelectedTest/AddTest` في `1434-1468` | إضافة تحليل للقائمة | مكتمل |
| Delete | أ/ب متوسط | يحذف من DB إذا كان `VisitTestId > 0` ثم يزيل من القائمة في `1470-1494` | حذف تحليل مختار | مكتمل جزئياً بسبب ملاحظة العداد |
| All | أ | يضيف كل التحاليل الظاهرة في `1444-1449` | إضافة كل الظاهر | مكتمل |
| موافق / حفظ ✔ | ب | ينشئ/يحدث المريض، التاريخ المرضي، الزيارة والفاتورة في `1046-1110`, `1497-1556` | حفظ تسجيل كامل بشكل ذري ومتسق | مكتمل جزئياً بسبب ازدواجية وعدم استخدام `SaveRegistrationAsync` |
| تراجع / تراجع ✖ | أ | يصفر المدفوع والخصم فقط في `1752-1757` | إلغاء آخر تعديل مالي أو إلغاء التعديل حسب النص | مكتمل جزئياً؛ نطاقه مالي فقط |
| خالص | ب | يحدث الفاتورة إذا وجدت زيارة في `1759-1779` | تسوية الباقي وحفظ الدفع | مكتمل عند وجود زيارة |
| تعديل 👤 | أ | يفعل `IsEditMode` في `1118-1125` | فتح وضع التعديل | مكتمل |
| إضافة 👤 | أ | يستدعي `ClearFormAsync` في `1234-1308` | بدء تسجيل جديد | مكتمل |
| الكارنية | ب | يعرض StatusMessage فقط في `1900-1908` | عرض/طباعة/حفظ كارنية تأمين | مربوط لكن غير مكتمل |
| ورقة العمل | ب | يطبع عبر `PrintWorksheetByPatientAsync` في `1848-1895` | طباعة ورقة عمل للمريض | مكتمل بشرط توفر `IPrintService` |
| الباركود | ب | يفتح حوار الباركود عبر `ShowBarcode` في `1565-1597` | إنشاء/طباعة باركود عينات | مكتمل بشرط توفر الخدمة |
| الإيصال | ب | يحمّل بيانات الزيارة والفاتورة ويطبع في `1643-1744` | طباعة إيصال مطابق للحفظ | مكتمل بشرط وجود زيارة وخدمة طباعة |
| القائمة الرئيسية | أ | ينقل إلى Home في `1934-1942` | تنقل | مكتمل |
| نتائج التحاليل | أ | ينقل إلى ResultsEntry في `1911-1919` | تنقل | مكتمل |
| قائمة المرضى | أ | يحمل مرضى اليوم ويفتح Popup في `1415-1426` | عرض قائمة سريعة | مكتمل |

## 7. خطة العمل المقترحة مرحلة مرحلة

1. مرحلة تثبيت العقود: تحديد هل الحفظ الرسمي يجب أن يمر عبر `PatientService.SaveRegistrationAsync` أو يبقى موزعاً بين `PatientService`, `VisitService`, و`InvoiceService`. القرار يجب أن يعالج التعارض بين `TransactionScope` في `PatientRegistrationViewModel.cs:1512-1516` وتعليق SQLite في `PatientService.cs:284-293`.

2. مرحلة إكمال الوظائف الكبيرة: تحويل `InsuranceCommand` من placeholder في `PatientRegistrationViewModel.cs:1900-1908` إلى خدمة/نافذة/طباعة فعلية، أو إزالة/تعطيل الزر حتى يكتمل.

3. مرحلة كشف الأوامر المخفية: إما ربط `LoadByCodeCommand`, `LoadByLabIdCommand`, `SearchCommand`, `DeleteCommand`, `DocumentsCommand`, `ShowMovementCommand` بعناصر واجهة واضحة، أو حذفها من هذا ViewModel إذا كانت تخص شاشة أخرى.

4. مرحلة مراجعة نموذج بيانات المريض: مقارنة خصائص VM غير الظاهرة مثل `BirthDate`, `Email`, `ChronicDiseases`, `Allergies`, `Medications`, `DeliveryDate`, `ReferralAddress/Phone/Responsible` مع المتطلبات الوظيفية، ثم إضافة عناصر واجهة لها أو نقلها إلى ViewModel آخر.

5. مرحلة تحسين الإشعارات: مراجعة تحديث `SelectedTests.Count` بعد الحذف، وإضافة إشعار واضح لـ `SelectedTests` أو `SelectedTests.Count` إذا أثبت الاختبار أن العداد لا يتحدث.

6. مرحلة الاختبارات: إضافة اختبارات ViewModel لمسارات الحفظ، التسوية، حذف تحليل محفوظ، طباعة الإيصال، وطباعة ورقة العمل؛ وإضافة اختبار يثبت سلوك transaction المختار بعد توحيد مسار الحفظ.
