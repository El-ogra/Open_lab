# Identifying gaps and shortcomings and planning to fix them

## نطاق التدقيق

- الواجهة المستهدفة: `Open_lab/Views/Patients/PatientRegistrationView.xaml`.
- الـ ViewModel المرتبط: `Open_lab/ViewModels/PatientRegistrationViewModel.cs`.
- الخدمات المرتبطة فعلياً من الـ ViewModel: `IPatientService`, `ITestCatalogService`, `IVisitService`, `IInvoiceService`, `IBarcodeDialogService`, `IPrintService`, `INavigationService` كما تظهر حقول الاعتماد في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:28-34`.
- ربط الواجهة بالـ ViewModel يتم عبر `DataTemplate` في `Open_lab/App.xaml:55-57`، وإنشاء الـ ViewModel يتم من `ViewModelFactory` عند `NavigationTarget.PatientRegistration` في `Open_lab/ViewModels/ViewModelFactory.cs:22`.
- لم توجد ملفات Repository ضمن `Open_lab` عند البحث عن `Repository|Repositories`; الخدمات المرتبطة تتعامل مباشرة مع `OpenLabDbContext`.

## 1. جدول جرد المكونات الكامل

| # | النوع | النص/الوصف الظاهر | Binding / Event | المرجع | حالة الربط |
|---|---|---|---|---|---|
| 1 | Button | `start` | `Command={Binding StartCommand}` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:193-195` | خلل: الأمر غير موجود في الـ ViewModel. |
| 2 | TextBox | بحث التحاليل | `Text=TestSearchText` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:198-200` | موجود. |
| 3 | RadioButton | `TM` | `IsChecked=True` فقط | `Open_lab/Views/Patients/PatientRegistrationView.xaml:201-205` | غير مربوط. |
| 4 | RadioButton | `TG` | لا يوجد Binding | `Open_lab/Views/Patients/PatientRegistrationView.xaml:207-211` | غير مربوط. |
| 5 | RadioButton | `CC` | لا يوجد Binding | `Open_lab/Views/Patients/PatientRegistrationView.xaml:212-216` | غير مربوط. |
| 6 | DataGrid | قائمة التحاليل المتاحة | `ItemsSource=AvailableTests`, `SelectedItem=SelectedAvailableTest`, عمود `NameReport`, حدث `MouseDoubleClick` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:231-239` | مربوط؛ الحدث منفذ في code-behind. |
| 7 | TextBlock | عدد تحاليل المريض | `SelectedTests.Count` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:261-266` | مربوط. |
| 8 | DataGrid | تحاليل المريض المختارة | `ItemsSource=SelectedTests`, `SelectedItem=SelectedTest`, عمود `TestName` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:268-276` | مربوط. |
| 9 | Button | `Add select test` | `AddTestCommand` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:288-290` | مربوط. |
| 10 | Button | `Delete` | `RemoveTestCommand` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:293-295` | مربوط. |
| 11 | Button | `All` | `AddAllTestsCommand` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:298-300` | مربوط. |
| 12 | CheckBox | `Urine` | `HasUrineSample` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:312` | موجود. |
| 13 | CheckBox | `Stool` | `HasStoolSample` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:313` | موجود. |
| 14 | CheckBox | `Blood` | `HasBloodSample` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:314` | موجود. |
| 15 | CheckBox | `Semen` | `HasSemenSample` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:315` | موجود. |
| 16 | CheckBox | `CSF` | `HasCsfSample` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:316` | موجود. |
| 17 | TextBox | تاريخ الدخول | `EntryDate` مع `StringFormat` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:348` | موجود، لكنه TextBox لتاريخ لا DatePicker. |
| 18 | TextBox | وقت الدخول | `EntryTimeText` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:349` | موجود. |
| 19 | TextBox | تاريخ الاستلام | `ReceptionDate` مع `StringFormat` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:353` | موجود، لكنه TextBox لتاريخ لا DatePicker. |
| 20 | TextBox | وقت الاستلام | `ReceptionTimeText` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:354` | موجود. |
| 21 | TextBox | كود المريض | `PatientCode` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:359` | موجود. |
| 22 | CheckBox | `VIP` | `IsVip` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:361` | موجود. |
| 23 | TextBox | `Lab. ID` | `LabId` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:366` | موجود. |
| 24 | ComboBox | لقب اسم المريض | `SelectedItem=Title` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:389-394` | موجود. |
| 25 | TextBox | اسم المريض | `FullName` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:395` | موجود. |
| 26 | RadioButton | `Male` | `Gender` عبر `StringEqualsConverter` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:397` | موجود؛ الـ Converter معرف في `Open_lab/App.xaml:33`. |
| 27 | RadioButton | `Female` | `Gender` عبر `StringEqualsConverter` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:398` | موجود؛ الـ Converter معرف في `Open_lab/App.xaml:33`. |
| 28 | TextBox | موبايل | `MobilePhone` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:413` | موجود. |
| 29 | TextBox | تليفون | `HomePhone` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:414` | موجود. |
| 30 | TextBox | سن المريض | `Age` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:423` | موجود. |
| 31 | ComboBox | وحدة السن | `SelectedItem=AgeUnit` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:424-428` | موجود. |
| 32 | TextBox | رقم البطاقة | `NationalId` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:442` | موجود. |
| 33 | ComboBox | نوع الحساب | `SelectedItem=AccountType` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:446-450` | موجود. |
| 34 | TextBox | عنوان المريض | `Address` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:461` | موجود. |
| 35 | ComboBox | لقب الطبيب المعالج | `SelectedItem=DoctorTitle` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:481-485` | موجود. |
| 36 | TextBox | اسم/ملاحظة الطبيب المعالج | `ReferralTitleNote` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:486` | موجود. |
| 37 | CheckBox | `Print` | `PrintReferralOnReport` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:487-494` | موجود. |
| 38 | ComboBox | جهة الإحالة | `ItemsSource=Referrals`, `SelectedItem=SelectedReferral`, `Text=ReferralSource` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:499-509` | موجود. |
| 39 | Button | `معلومات اضافية` | لا يوجد Command | `Open_lab/Views/Patients/PatientRegistrationView.xaml:510` | خلل: زر بلا وظيفة. |
| 40 | CheckBox | هل يأخذ علاج السكر | `TakesDiabetesTreatment` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:532` | موجود. |
| 41 | CheckBox | مريض فقر الدم | `HasAnemia` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:533` | خلل: الخاصية غير موجودة. |
| 42 | TextBox | عدد ساعات الصيام | `FastingHours` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:534` | موجود. |
| 43 | CheckBox | صائم | `IsFasting` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:535` | موجود. |
| 44 | CheckBox | هل يأخذ علاج للغدة | `TakesAntiVirals` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:537` | خلل منطقي: مربوطة بعلاج الفيروسات لا الغدة. |
| 45 | CheckBox | مريض بالتهاب المفاصل | `HasJointInflammation` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:538` | خلل: الخاصية غير موجودة. |
| 46 | CheckBox | هل يأخذ مضاد حيوي | `TakesAntibiotics` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:543` | موجود. |
| 47 | CheckBox | مريض بالضغط الدم | `HasHypertension` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:544` | خلل: الخاصية غير موجودة. |
| 48 | CheckBox | هل يأخذ علاج لسيولة الدم | `TakesBloodThinners` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:545` | موجود. |
| 49 | CheckBox | هل يأخذ علاج للكبد | `TakesLiverTreatment` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:547` | موجود. |
| 50 | CheckBox | مريض بالفشل الكلوي | `HasKidneyFailure` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:548` | خلل: الخاصية غير موجودة. |
| 51 | CheckBox | هل يأخذ علاج للفيروسات | `TakesAntiVirals` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:549` | موجود، لكنه يتكرر مع سطر الغدة. |
| 52 | CheckBox | للسيدات هل يوجد حمل | `IsPregnant` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:551` | موجود. |
| 53 | CheckBox | مريض بالإصابة بمرض مزمن | `HasPregnancyComplication` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:552` | خلل: الخاصية غير موجودة واسمها لا يطابق النص. |
| 54 | CheckBox | هل تم عمل اشعة بالصبغة... | `HadContrastImaging` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:553` | موجود. |
| 55 | TextBox | ملاحظات | `MedicalNotes` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:555` | موجود. |
| 56 | TextBox | الباقي للمعمل | `BalanceForLab`, `OneWay` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:582` | موجود. |
| 57 | TextBox | اجمالي التحاليل | `TotalAmount`, `OneWay` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:586` | موجود. |
| 58 | TextBox | الباقي للمريض | `BalanceForPatient`, `OneWay` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:591` | موجود. |
| 59 | TextBox | نسبة الخصم | `DiscountPercent` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:598` | موجود. |
| 60 | TextBox | قيمة خصم مدخلة | `DiscountValue` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:599` | موجود لكن لا يدخل في حساب `DiscountAmount`. |
| 61 | TextBox | المدفوع | `PaidAmount` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:604` | موجود. |
| 62 | TextBox | قيمة الخصم المحسوبة | `DiscountAmount`, `OneWay` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:608` | موجود. |
| 63 | TextBox | التكلفة بعد الخصم | `NetTotal`, `OneWay` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:613` | موجود. |
| 64 | TextBox | المدفوع سابقا | `PreviousPaidAmount` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:618` | موجود. |
| 65 | Button | `موافق` | `SaveCommand` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:623` | مربوط. |
| 66 | Button | `تراجع` | `UndoCommand` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:624` | مربوط. |
| 67 | Button | `خالص` | `SettleCommand` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:625` | مربوط. |
| 68 | Button | `تراجع ✖` | `UndoCommand` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:637` | مربوط ومكرر. |
| 69 | Button | `حفظ ✔` | `SaveCommand` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:638` | مربوط ومكرر. |
| 70 | Button | `تعديل 👤` | `EditCommand` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:639` | مربوط. |
| 71 | Button | `إضافة 👤` | `NewCommand` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:640` | مربوط. |
| 72 | Button | `الكارنية` | `InsuranceCommand` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:644` | مربوط لكن التنفيذ Placeholder. |
| 73 | Button | `ورقة العمل` | `WorksheetCommand` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:645` | مربوط. |
| 74 | Button | `الباركود` | `ShowBarcodeCommand` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:646` | مربوط. |
| 75 | Button | `الإيصال` | `PrintReceiptCommand` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:647` | مربوط. |
| 76 | Button | `القائمة الرئيسية` | `GoToHomeCommand` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:652` | مربوط. |
| 77 | Button | `نتائج التحاليل` | `GoToResultsCommand` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:653` | مربوط. |
| 78 | Button | `قائمة المرضى` | `ShowTodayPatientsCommand` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:654` | مربوط لكنه لا يفتح قائمة عرض في هذه الواجهة. |

## 2. تقرير تدقيق: الواجهة ↔ ViewModel

### الربط العام

- `PatientRegistrationViewModel` هو الـ DataContext الفعلي للواجهة عبر `DataTemplate` في `Open_lab/App.xaml:55-57`.
- إنشاء الـ ViewModel يتم عبر DI في `Open_lab/ViewModels/ViewModelFactory.cs:73-74`.
- الخدمات المطلوبة يتم تسجيلها بالاتفاقية: `RegisterServicesByConvention` يبحث عن كلاس في `Open_lab.Services` ويضيف الواجهة المطابقة `I{Name}` كـ transient في `Open_lab/App.xaml.cs:112-127`.

### أوامر مربوطة في الواجهة وموجودة في ViewModel

الأوامر التالية موجودة في الواجهة ومعلنة في الـ ViewModel: `SaveCommand`, `NewCommand`, `AddTestCommand`, `AddAllTestsCommand`, `RemoveTestCommand`, `ShowBarcodeCommand`, `PrintReceiptCommand`, `EditCommand`, `GoToResultsCommand`, `GoToHomeCommand`, `ShowTodayPatientsCommand`, `UndoCommand`, `SettleCommand`, `WorksheetCommand`, `InsuranceCommand`. إعلان الأوامر موجود في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:932-959`، وتكوينها في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:180-210`.

### أوامر أو عناصر واجهة بها خلل

| الأولوية | الخلل | الدليل من الواجهة | الدليل من ViewModel | الأثر |
|---|---|---|---|---|
| P0 | زر `start` مربوط بأمر غير موجود. | `StartCommand` في `Open_lab/Views/Patients/PatientRegistrationView.xaml:193-195` | قائمة الأوامر لا تحتوي `StartCommand` في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:932-959` | الزر لن ينفذ أي شيء. |
| P0 | خمس CheckBox طبية مربوطة بخصائص غير موجودة. | `HasAnemia`, `HasJointInflammation`, `HasHypertension`, `HasKidneyFailure`, `HasPregnancyComplication` في `Open_lab/Views/Patients/PatientRegistrationView.xaml:533-552` | البحث في الـ ViewModel لا يظهر هذه الخصائص؛ الخصائص الطبية الموجودة موثقة بين `Open_lab/ViewModels/PatientRegistrationViewModel.cs:535-715` | القيم لا تحفظ ولا تقرأ، وBinding يفشل. |
| P1 | CheckBox "هل يأخذ علاج للغدة" مربوطة بـ `TakesAntiVirals` بدلاً من خاصية الغدة. | `Open_lab/Views/Patients/PatientRegistrationView.xaml:537` | `TakesGlandsTreatment` موجود كـ alias للغدة في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:658-662`، بينما `TakesAntiVirals` للفيروسات في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:622-626` | اختيار الغدة يغير حالة الفيروسات. |
| P1 | زر `معلومات اضافية` بلا Command. | `Open_lab/Views/Patients/PatientRegistrationView.xaml:510` | لا يوجد Command مربوط؛ الأوامر المتاحة في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:932-959` | الزر ظاهر بلا وظيفة. |
| P1 | RadioButton `TM/TG/CC` غير مربوطة. | `Open_lab/Views/Patients/PatientRegistrationView.xaml:201-216` | توجد خاصية `TestCategoryFilter` في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:758-762` لكنها غير مربوطة | اختيار نوع قائمة التحاليل لا يغير النتائج. |
| P1 | `DiscountValue` قابل للإدخال لكن لا يدخل في الحساب. | الحقل مربوط في `Open_lab/Views/Patients/PatientRegistrationView.xaml:599` | الحساب يستخدم `DiscountPercent` فقط: `DiscountAmount => TotalAmount * DiscountPercent / 100m` في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:804` | المستخدم قد يكتب قيمة خصم بلا أثر مالي. |
| P2 | `ShowTodayPatientsCommand` لا يعرض ListView/DataGrid للمرضى داخل الواجهة. | الزر في `Open_lab/Views/Patients/PatientRegistrationView.xaml:654` | الأمر ينفذ `LoadTodayPatientsAsync` فقط في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:201`، والقائمة `QuickPatientsList` موجودة في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:867` | الزر يحدث بيانات غير معروضة. |
| P2 | DoubleClick في DataGrid يستخدم code-behind بدلاً من Command/Behavior. | `MouseDoubleClick="AvailableTestsList_MouseDoubleClick"` في `Open_lab/Views/Patients/PatientRegistrationView.xaml:236` | التنفيذ يستدعي `AddSelectedTestCommand` من `DataContext` في `Open_lab/Views/Patients/PatientRegistrationView.xaml.cs:13-21` | يعمل، لكنه ليس MVVM نقياً. |

### خصائص مستخدمة في الواجهة وموجودة في ViewModel

الخصائص التالية مستخدمة في الواجهة وموجودة في الـ ViewModel: `LabId`, `PatientCode`, `FullName`, `Gender`, `Address`, `MobilePhone`, `HomePhone`, `NationalId`, `Age`, `AgeUnit`, `IsVip`, `IsFasting`, `FastingHours`, `AccountType`, `Title`, `EntryDate`, `ReceptionDate`, `EntryTimeText`, `ReceptionTimeText`, `ReferralSource`, `ReferralTitleNote`, `DoctorTitle`, `PrintReferralOnReport`, `TakesDiabetesTreatment`, `TakesAntibiotics`, `TakesBloodThinners`, `TakesLiverTreatment`, `TakesAntiVirals`, `IsPregnant`, `HadContrastImaging`, `MedicalNotes`, `HasUrineSample`, `HasStoolSample`, `HasBloodSample`, `HasSemenSample`, `HasCsfSample`, `TestSearchText`, `AvailableTests`, `SelectedAvailableTest`, `SelectedTests`, `SelectedTest`, `Referrals`, `SelectedReferral`, `TotalAmount`, `BalanceForLab`, `BalanceForPatient`, `DiscountPercent`, `DiscountValue`, `PaidAmount`, `DiscountAmount`, `NetTotal`, `PreviousPaidAmount`. تعريفاتها الأساسية موثقة في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:253-905`.

## 3. تقرير تدقيق: ViewModel ↔ Service

### الدوال التي يستدعيها الـ ViewModel من الخدمات

| الخدمة | الاستدعاء من ViewModel | وجوده في الواجهة | التنفيذ/الدعم |
|---|---|---|---|
| `IPatientService` | `CreateAsync` في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:975` | عبر `SaveCommand` | موجود في الواجهة `Open_lab/Services/IPatientService.cs:16` ومنفذ بإضافة `Patients` وحفظ في `Open_lab/Services/PatientService.cs:152-176`. |
| `IPatientService` | `UpdateAsync` في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1000-1016` | عبر `SaveCommand` و`EditCommand` | موجود في `Open_lab/Services/IPatientService.cs:17-18` ومنفذ عبر `UpdateCoreAsync` في `Open_lab/Services/PatientService.cs:179-273`. |
| `IPatientService` | `GenerateNextLabIdAsync` في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1038` | غير مربوط بزر مباشر | موجود في `Open_lab/Services/IPatientService.cs:15` ومنفذ في `Open_lab/Services/PatientService.cs:116-143`. |
| `IPatientService` | `GetByLabIdAsync` في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1056` و`1082` | لا يوجد زر تحميل مباشر في الواجهة الحالية | موجود في `Open_lab/Services/IPatientService.cs:11` ومنفذ في `Open_lab/Services/PatientService.cs:27-34`. |
| `IPatientService` | `SearchAsync` في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1102` و`1275` | زر `قائمة المرضى` فقط يستدعي نسخة اليوم | موجود في `Open_lab/Services/IPatientService.cs:14` ومنفذ في `Open_lab/Services/PatientService.cs:85-113`. |
| `IPatientService` | `DeleteAsync` في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1125` | غير مستخدم في الواجهة الحالية | موجود في `Open_lab/Services/IPatientService.cs:20` ومنفذ في `Open_lab/Services/PatientService.cs:367-382`. |
| `IPatientService` | `GetMedicalHistoryAsync` و`SaveMedicalHistoryAsync` في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1596` و`1610` | جزء من الحفظ/التحميل | موجودان في `Open_lab/Services/IPatientService.cs:12-13` ومنفذان في `Open_lab/Services/PatientService.cs:37-82`. |
| `ITestCatalogService` | `GetReferralsAsync` في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1243` | ComboBox جهة الإحالة | موجود في `Open_lab/Services/ITestCatalogService.cs:52` ومنفذ في `Open_lab/Services/TestCatalogService.cs:560-562`. |
| `ITestCatalogService` | `GetAllTestsAsync` في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1261` | DataGrid التحاليل المتاحة | موجود في `Open_lab/Services/ITestCatalogService.cs:10` ومنفذ في `Open_lab/Services/TestCatalogService.cs:30-32`. |
| `IVisitService` | `GetByIdAsync`, `CreateAsync`, `GetVisitTestsAsync`, `AddTestToVisitAsync` في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1352-1376` | عبر الحفظ عند وجود تحاليل | موجودة في `Open_lab/Services/IVisitService.cs:9-15` ومنفذة في `Open_lab/Services/VisitService.cs:22-164`. |
| `IInvoiceService` | `CreateOrUpdateInvoiceAsync` في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1381` و`1555` | عبر الحفظ و`خالص` | موجودة في `Open_lab/Services/IInvoiceService.cs:9` ومنفذة في `Open_lab/Services/InvoiceService.cs:20-59`. |
| `IBarcodeDialogService` | `ShowBarcodeDialog` في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1413-1425` | زر الباركود | موجود في `Open_lab/Services/IBarcodeDialogService.cs:3-6` ومنفذ بفتح Dialog في `Open_lab/Services/BarcodeDialogService.cs:18-28`. |
| `IPrintService` | `PrintReceiptAsync` في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1527` | زر الإيصال | موجود في `Open_lab/Services/IPrintService.cs:11` ومنفذ في `Open_lab/Services/PrintService.cs:23-52`. |
| `IPrintService` | `PrintWorksheetByPatientAsync` في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1644` | زر ورقة العمل | موجود في `Open_lab/Services/IPrintService.cs:14` ومنفذ في `Open_lab/Services/PrintService.cs:126-145`. |
| `INavigationService` | `Navigate(ResultsEntry)` و`Navigate(Home)` في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1669` و`1692` | أزرار نتائج التحاليل والقائمة الرئيسية | موجود في `Open_lab/ViewModels/NavigationService.cs:22-25`. |

### عناصر موجودة في ViewModel وغير مستخدمة في الواجهة الحالية

| النوع | الاسم | المرجع | الملاحظة |
|---|---|---|---|
| Command | `GenerateLabIdCommand`, `LoadByLabIdCommand`, `SearchCommand`, `DeleteCommand`, `SendCommand`, `RemoveSelectedTestCommand`, `ResetCommand`, `DocumentsCommand`, `LoadByCodeCommand`, `NewPatientCommand`, `ShowMovementCommand` | `Open_lab/ViewModels/PatientRegistrationViewModel.cs:934-954` | لا توجد Bindings مقابلة لها في `PatientRegistrationView.xaml`. |
| Command | `AddSelectedTestCommand` | `Open_lab/ViewModels/PatientRegistrationViewModel.cs:944` | غير مربوط في XAML، لكنه مستخدم من code-behind عند DoubleClick في `Open_lab/Views/Patients/PatientRegistrationView.xaml.cs:17-19`. |
| Collections | `Results`, `QuickPatientsList`, `SelectedPatient`, `QuickSelectedPatient` | `Open_lab/ViewModels/PatientRegistrationViewModel.cs:863`, `867`, `907-929` | لا توجد قائمة عرض مقابلة لها في الواجهة الحالية. |
| Patient fields | `BirthDate`, `Phone`, `Email`, `PatientTitleNote` | `Open_lab/ViewModels/PatientRegistrationViewModel.cs:283-293`, `349-353`, `397-401` | غير معروضة مباشرة في الواجهة الحالية. |
| Medical fields | `ChronicDiseases`, `Allergies`, `Medications`, `TestedBefore`, `HadPreviousAnalysisHere`, `HadBloodTransfusion`, `TakesGlandsTreatment`, `IsPregnantFemale`, `IsSmoker` | `Open_lab/ViewModels/PatientRegistrationViewModel.cs:301-317`, `535-715` | بعضها محفوظ في التاريخ المرضي، لكن لا يوجد عنصر واجهة مقابل لها. |
| Time/date fields | `DeliveryDate`, `EntryTime`, `DeliveryTime`, `ReceptionTime`, `SystemTimeText` | `Open_lab/ViewModels/PatientRegistrationViewModel.cs:411-457` | غير معروضة مباشرة أو لا تستخدم في XAML الحالي. |
| Referral fields | `ReferralAddress`, `ReferralPhone`, `ReferralAltPhone`, `ReferralResponsible`, `ResponsibleName`, `ReferralTitle`, `PrintReferral` | `Open_lab/ViewModels/PatientRegistrationViewModel.cs:467-519` | غير مستخدمة في الواجهة الحالية أو تستخدم داخلياً فقط كـ alias. |
| Finance aliases | `NetAmount`, `PreviousPaid`, `LabRemainder`, `PatientRemainder` | `Open_lab/ViewModels/PatientRegistrationViewModel.cs:806-854` | غير مربوطة مباشرة؛ تستخدم أو تعرض عبر aliases أخرى. |

### فجوات تنفيذ داخل ViewModel

| الأولوية | الفجوة | الدليل | الأثر |
|---|---|---|---|
| P1 | `InsuranceCommand` لا ينفذ وظيفة تأمين فعلية؛ يعرض رسالة "قيد التطوير". | `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1648-1659` | زر `الكارنية` مربوط لكنه غير مكتمل وظيفياً. |
| P1 | `ShowDocuments` مجرد رسالة ولا يفتح عارض مستندات. | `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1672-1681` | الأمر غير مستخدم في الواجهة، وإن استُخدم لاحقاً سيكون غير مكتمل. |
| P1 | `ShowMovement` يعرض ملخصاً نصياً فقط. | `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1531-1534` | الأمر غير مستخدم في الواجهة، ولا يفتح حركة مالية/سجل. |
| P1 | `PrintReceiptAsync` يبني بيانات الإيصال من `SelectedTests` الحالية بدلاً من تحميل الزيارة المحفوظة من قاعدة البيانات. | بناء `ReceiptData` يتم محلياً في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1482-1525` | إذا تغيرت الزيارة أو لم تكن كل العناصر محملة في الشاشة، الإيصال قد لا يعكس قاعدة البيانات. |
| P2 | `SaveVisitAndInvoiceAsync` يرجع دون حفظ زيارة/فاتورة إذا لم توجد تحاليل مختارة. | شرط الرجوع في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1338-1341` | حفظ بيانات المريض وحده ممكن، لكن زر الحفظ لا يوضح أن الزيارة/الفاتورة لم تنشأ. |

## 4. تقرير تدقيق: Service ↔ قاعدة البيانات

### بنية الوصول للبيانات

- لا توجد طبقة Repository مخصصة لهذا المسار داخل `Open_lab`؛ الخدمات تستخدم `OpenLabDbContext` مباشرة.
- `OpenLabDbContext` يحتوي DbSets اللازمة للمسار: `Patients`, `Visits`, `Referrals`, `Tests`, `VisitTests`, `ResultValues`, `Invoices`, `Payments`, `PriceLists`, `PriceListItems`, `CustomGroups`, `CustomGroupItems`, `MedicalHistories`, `AdditionalCharges`, `ExternalLabQueues` في `Open_lab/Data/OpenLabDbContext.cs:29-59`.

### حالة الخدمات المرتبطة

| الخدمة | الدوال المستخدمة من الشاشة | حالة التنفيذ | دعم قاعدة البيانات |
|---|---|---|---|
| `PatientService` | `CreateAsync`, `UpdateAsync`, `GetByLabIdAsync`, `SearchAsync`, `DeleteAsync`, `GetMedicalHistoryAsync`, `SaveMedicalHistoryAsync`, `GenerateNextLabIdAsync` | مكتملة وليست فارغة؛ تستخدم EF و`SaveChangesAsync` في مسارات الإنشاء/التعديل/الحذف/الحفظ الطبي. | مدعومة بـ `Patients`, `MedicalHistories`, `Visits`, `Referrals` في `Open_lab/Data/OpenLabDbContext.cs:29-31`, `53`. |
| `TestCatalogService` | `GetAllTestsAsync`, `GetReferralsAsync` | مكتملة: `GetAllTestsAsync` يرجع `_db.Tests` في `Open_lab/Services/TestCatalogService.cs:30-32`، و`GetReferralsAsync` يرجع `_db.Referrals` في `Open_lab/Services/TestCatalogService.cs:560-562`. | مدعومة بـ `Tests` و`Referrals` في `Open_lab/Data/OpenLabDbContext.cs:31-32`. |
| `VisitService` | `GetByIdAsync`, `CreateAsync`, `GetVisitTestsAsync`, `AddTestToVisitAsync` | مكتملة وتستخدم `Visits`, `VisitTests`, `Tests`, `PriceLists`, `PriceListItems` حسب الأسطر `Open_lab/Services/VisitService.cs:22-164`, وتسعير داخلي في `Open_lab/Services/VisitService.cs:257-285`. | مدعومة بـ `Visits`, `VisitTests`, `Tests`, `PriceLists`, `PriceListItems` في `Open_lab/Data/OpenLabDbContext.cs:30`, `32`, `38`, `44-45`. |
| `InvoiceService` | `CreateOrUpdateInvoiceAsync` | مكتملة: تتحقق من الزيارة، تحسب الإجمالي، تنشئ/تحدث الفاتورة، وتحفظ في `Open_lab/Services/InvoiceService.cs:20-59`. | مدعومة بـ `Invoices`, `Payments`, `AdditionalCharges`, `VisitTests` في `Open_lab/Data/OpenLabDbContext.cs:38`, `41-42`, `55`. |
| `BarcodeDialogService` | `ShowBarcodeDialog` | مكتملة كخدمة UI: تنشئ `BarcodeDialogViewModel` وتفتح `BarcodeDialog` في `Open_lab/Services/BarcodeDialogService.cs:18-28`. | لا تعتمد مباشرة على DbContext. |
| `PrintService` | `PrintReceiptAsync`, `PrintWorksheetByPatientAsync` | ليست فارغة؛ تنشئ `FlowDocument` وتستدعي `PrintDocument` في `Open_lab/Services/PrintService.cs:23-52`, `126-145`, و`PrintDocument` يكتب إلى `PrintQueue` في `Open_lab/Services/PrintService.cs:320-325`. | لا تعتمد مباشرة على DbContext؛ تعتمد على بيانات ممررة من ViewModel. |

### دوال فارغة أو غير مكتملة

| الدالة | الحكم | الدليل |
|---|---|---|
| `PrintService.PrintReceiptAsync` | ليست فارغة، لكنها async شكلياً وتعيد `Task.CompletedTask` بعد طباعة متزامنة. | `Open_lab/Services/PrintService.cs:23-52`. |
| `PrintService.PrintWorksheetByPatientAsync` | ليست فارغة، لكنها async شكلياً وتعيد `Task.CompletedTask` بعد طباعة متزامنة. | `Open_lab/Services/PrintService.cs:126-145`. |
| `InsuranceCommand` handler | غير مكتمل وظيفياً، لكنه في ViewModel وليس Service. | `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1648-1659`. |

لم يظهر في الخدمات المرتبطة أي `throw new NotImplementedException` أو `TODO` أو `return null;`. البحث أظهر فقط `return Task.CompletedTask` في `PrintService` بعد تنفيذ طباعة فعلية.

## 5. تصنيف الأزرار حسب نوع وظيفتها

| الزر | المرجع | التصنيف | الوضع الحالي | الوضع المفترض | حجم الفجوة |
|---|---|---|---|---|---|
| `start` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:193-195` | ب | مربوط بأمر غير موجود. | بدء/تحميل/تطبيق بحث أو فلتر قائمة التحاليل حسب تصميم الشاشة. | غير مربوط فعلياً. |
| `Add select test` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:288-290` | ب | مربوط ومكتمل محلياً: يضيف الاختبار المختار في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1288-1322`. | إضافة الاختبار المختار لقائمة المريض. | لا فجوة جوهرية. |
| `Delete` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:293-295` | ب | مربوط ومكتمل محلياً: يحذف من القائمة فقط في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1324-1334`. | حذف التحليل المختار من قائمة المريض وربما من الزيارة المحفوظة إذا كان محفوظاً. | مكتمل جزئياً؛ لا يستدعي `IVisitService.RemoveVisitTestAsync` للحذف من قاعدة البيانات. |
| `All` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:298-300` | ب | مربوط ويضيف كل التحاليل الظاهرة في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1298-1304`. | إضافة كل التحاليل الظاهرة/المفلترة. | جزئي بسبب عدم وجود فلترة `TM/TG/CC`. |
| `معلومات اضافية` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:510` | أ | بلا Command. | فتح/تعديل بيانات إضافية للطبيب أو جهة الإحالة. | غير مربوط كلياً. |
| `موافق` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:623` | ب | مربوط بـ `SaveCommand` وينشئ/يحدث مريضاً ويحفظ زيارة/فاتورة عند وجود تحاليل في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:963-1027`, `1336-1394`. | حفظ كل بيانات التسجيل. | مكتمل جزئياً؛ بعض حقول الواجهة الطبية غير محفوظة لأنها غير موجودة في VM. |
| `تراجع` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:624` | أ | مربوط بـ `UndoCommand` ويصفر `PaidAmount` و`DiscountPercent` فقط في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1536-1541`. | تراجع واضح عن آخر تعديل أو إلغاء النموذج. | جزئي؛ الاسم أوسع من التنفيذ. |
| `خالص` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:625` | ب | مربوط ويحدث الفاتورة إذا كان `CurrentVisitId > 0` في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1543-1564`. | تسوية الحساب بالكامل وتسجيل الدفع. | مكتمل جزئياً؛ يعتمد على وجود زيارة محفوظة. |
| `تراجع ✖` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:637` | أ | نفس `UndoCommand`. | إلغاء/تراجع. | مكرر وجزئي. |
| `حفظ ✔` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:638` | ب | نفس `SaveCommand`. | حفظ التسجيل. | نفس فجوات الحفظ. |
| `تعديل 👤` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:639` | ب | مربوط بـ `EditCommand` لكنه يستدعي `SaveAsync` فقط في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:194`. | الدخول لوضع تعديل أو حفظ تعديل قائم. | جزئي؛ لا يوجد وضع تعديل منفصل. |
| `إضافة 👤` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:640` | أ | مربوط بـ `NewCommand` ويفرغ النموذج في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:181`, `1135-1207`. | فتح نموذج مريض جديد. | مقبول. |
| `الكارنية` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:644` | ب | مربوط لكن يعرض رسالة "قيد التطوير" في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1648-1659`. | عرض/طباعة بيانات كارنية التأمين أو العقد. | مربوط لكن الأمر Placeholder. |
| `ورقة العمل` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:645` | ب | مربوط ويطبع صفاً مبنياً من الحالة الحالية في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1619-1645`. | طباعة ورقة عمل مفصلة لاختبارات الزيارة. | مكتمل جزئياً؛ لا يحمل تفاصيل الاختبارات من قاعدة البيانات. |
| `الباركود` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:646` | ب | مربوط ويفتح Dialog عبر `BarcodeDialogService` في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1396-1428` و`Open_lab/Services/BarcodeDialogService.cs:18-28`. | طباعة/عرض باركود العينات. | مقبول مع اعتماد على بيانات الشاشة. |
| `الإيصال` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:647` | ب | مربوط ويطبع من بيانات الشاشة في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1474-1528`. | طباعة إيصال الفاتورة المحفوظة. | مكتمل جزئياً؛ لا يعيد تحميل الفاتورة من DB. |
| `القائمة الرئيسية` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:652` | أ | مربوط وينتقل إلى `Home` في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1684-1692`. | تنقل مباشر. | مقبول. |
| `نتائج التحاليل` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:653` | أ | مربوط وينتقل إلى `ResultsEntry` في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1661-1669`. | تنقل مباشر. | مقبول. |
| `قائمة المرضى` | `Open_lab/Views/Patients/PatientRegistrationView.xaml:654` | ب | مربوط بتحميل مرضى اليوم في `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1270-1285` فقط. | عرض قائمة مرضى اليوم أو الانتقال لشاشة البحث. | مربوط لكن غير مكتمل بصرياً. |

## 6. جدول الفجوات والمشاكل المكتشفة مرتبة حسب الأولوية

| الأولوية | المشكلة | المرجع | السبب المباشر | الأثر |
|---|---|---|---|---|
| P0 | `StartCommand` غير موجود. | `Open_lab/Views/Patients/PatientRegistrationView.xaml:193-195`, `Open_lab/ViewModels/PatientRegistrationViewModel.cs:932-959` | Binding لأمر غير معرف. | زر معطل وظيفياً. |
| P0 | خصائص طبية مفقودة من الـ ViewModel. | `Open_lab/Views/Patients/PatientRegistrationView.xaml:533-552`, `Open_lab/ViewModels/PatientRegistrationViewModel.cs:535-715` | الواجهة تشير إلى خصائص غير معرفة. | بيانات طبية لا تحفظ ولا تقرأ. |
| P1 | ربط علاج الغدة بخاصية الفيروسات. | `Open_lab/Views/Patients/PatientRegistrationView.xaml:537`, `Open_lab/ViewModels/PatientRegistrationViewModel.cs:622-626`, `658-662` | اختيار Binding خاطئ. | فساد معنى البيانات الطبية. |
| P1 | زر `معلومات اضافية` بلا Command. | `Open_lab/Views/Patients/PatientRegistrationView.xaml:510` | لا يوجد Binding. | عنصر UI ظاهر بلا وظيفة. |
| P1 | `InsuranceCommand` Placeholder. | `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1648-1659` | التنفيذ يكتب StatusMessage فقط. | زر وظيفي كبير غير مكتمل. |
| P1 | `DiscountValue` لا يؤثر في الحساب. | `Open_lab/Views/Patients/PatientRegistrationView.xaml:599`, `Open_lab/ViewModels/PatientRegistrationViewModel.cs:792-804` | الحساب يعتمد على `DiscountPercent` فقط. | نتائج مالية مضللة. |
| P1 | حذف تحليل من القائمة لا يحذف من الزيارة المحفوظة. | `Open_lab/ViewModels/PatientRegistrationViewModel.cs:1324-1334`, `Open_lab/Services/IVisitService.cs:17` | الـ VM لا يستدعي `RemoveVisitTestAsync`. | اختلاف بين الشاشة وقاعدة البيانات بعد حفظ سابق. |
| P1 | RadioButton `TM/TG/CC` لا تغير فلتر التحاليل. | `Open_lab/Views/Patients/PatientRegistrationView.xaml:201-216`, `Open_lab/ViewModels/PatientRegistrationViewModel.cs:758-762` | لا Binding إلى `TestCategoryFilter`. | قائمة التحاليل لا تتأثر بالاختيار. |
| P2 | `ShowTodayPatientsCommand` يملأ قائمة غير معروضة. | `Open_lab/Views/Patients/PatientRegistrationView.xaml:654`, `Open_lab/ViewModels/PatientRegistrationViewModel.cs:867`, `1270-1285` | لا يوجد ListView/DataGrid لـ `QuickPatientsList`. | زر لا ينتج تغييراً مرئياً كافياً. |
| P2 | استخدام code-behind لـ DoubleClick. | `Open_lab/Views/Patients/PatientRegistrationView.xaml:236`, `Open_lab/Views/Patients/PatientRegistrationView.xaml.cs:13-21` | حدث UI مباشر بدلاً من Command behavior. | خروج محدود عن MVVM. |
| P2 | تاريخ الدخول والاستلام في TextBox. | `Open_lab/Views/Patients/PatientRegistrationView.xaml:348`, `353` | لا DatePicker/validation. | قابلية إدخال تاريخ غير صالح. |

## 7. خطة العمل المقترحة مرحلة مرحلة لسد الفجوات

### المرحلة 1: إصلاح أخطاء الربط الحرجة

1. إضافة أو استبدال `StartCommand` حسب المقصود من زر `start`: إذا المقصود بدء البحث، يربط بـ `SearchCommand` أو أمر فلترة تحاليل؛ وإذا المقصود تحميل كامل القائمة، يربط بأمر واضح في الـ ViewModel.
2. إضافة الخصائص الطبية المفقودة إلى `PatientRegistrationViewModel` أو تعديل XAML لاستخدام خصائص موجودة إذا كانت مرادفات صحيحة.
3. تصحيح Binding "هل يأخذ علاج للغدة" من `TakesAntiVirals` إلى `TakesGlandsTreatment`.
4. تحديد مصير `HasPregnancyComplication`: النص الظاهر "مريض بالإصابة بمرض مزمن" لا يطابق الاسم؛ إما تغيير الاسم إلى `HasChronicDisease` أو ربطه بحقل تاريخ مرضي مناسب.

### المرحلة 2: إكمال وظائف الأزرار الظاهرة

1. إضافة Command لزر `معلومات اضافية` يفتح أو يعرض تفاصيل جهة الإحالة/الطبيب باستخدام خصائص الـ Referral الموجودة.
2. تحويل `InsuranceCommand` من Placeholder إلى وظيفة فعلية: تحميل بيانات جهة التأمين/العقد ثم عرض/طباعة الكارنية.
3. جعل زر `قائمة المرضى` يفتح شاشة البحث أو يعرض `QuickPatientsList` داخل DataGrid/ListView في نفس الواجهة.
4. توضيح وظيفة `UndoCommand`: إما يظل تراجعاً مالياً بنص زر مناسب، أو يصبح إلغاء/إعادة تحميل حالة النموذج.

### المرحلة 3: ضبط المنطق المالي والحفظ

1. تحديد قاعدة واحدة للخصم: نسبة فقط، قيمة فقط، أو كلاهما مع أولوية واضحة.
2. إذا بقي `DiscountValue` في الواجهة، يجب أن يدخل في `DiscountAmount` أو يزال/يعرض OneWay فقط.
3. عند حذف تحليل محفوظ، استخدام `IVisitService.RemoveVisitTestAsync` عندما يكون `VisitTestId > 0`.
4. إظهار رسالة صريحة عند حفظ مريض بدون تحاليل بأن الزيارة والفاتورة لم يتم إنشاؤهما، لأن `SaveVisitAndInvoiceAsync` يرجع عند `SelectedTests.Count == 0`.

### المرحلة 4: تحسين MVVM والفلترة

1. ربط `TM/TG/CC` بخاصية فلترة مثل `TestCategoryFilter`.
2. تطبيق الفلتر على `AvailableTests` أو إضافة CollectionView بدلاً من تحميل كل الاختبارات دائماً.
3. استبدال `MouseDoubleClick` في code-behind بـ `InputBinding`/Behavior/Command إن كان ذلك ضمن نمط المشروع.
4. إضافة عناصر عرض مفقودة للـ Collections الموجودة فعلياً: `QuickPatientsList`, `Results`.

### المرحلة 5: تقوية التكامل مع قاعدة البيانات والطباعة

1. في الإيصال وورقة العمل، تحميل بيانات الزيارة/الفاتورة من الخدمات عند توفر `CurrentVisitId` بدلاً من الاعتماد فقط على حالة الشاشة.
2. إضافة/توسيع اختبارات ViewModel للـ Bindings الحرجة: الأوامر المفقودة، الخصائص الطبية، الخصم، حذف التحاليل المحفوظة.
3. إضافة اختبارات Service أو Integration للتأكد من حفظ التاريخ المرضي والخصائص الطبية الجديدة إذا أضيفت إلى نموذج البيانات.
4. إن كانت بنية المشروع تتطلب Repository لاحقاً، إدخالها كمرحلة مستقلة؛ الوضع الحالي المثبت من الكود أن الخدمات تتعامل مباشرة مع `OpenLabDbContext`.
