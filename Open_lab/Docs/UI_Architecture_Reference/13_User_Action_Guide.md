# 13 — User Action Guide — Open lab system (Final Independent Audit)

> دليل الإجراءات التشغيلي للمستخدم النهائي للنظام في حالته الحالية + مرجع كامل للوظائف المتوقعة من PDFs.
>
> - **الفرع:** `Fi5ve`
> - **الكوميت:** `011f15c` («بعد حذف الشغل القديم»)
> - **المراجع:** `real lab system help.pdf`، `RLS_Learn.pdf`

---

## القسم الأول — ما يستطيع المستخدم فعله الآن (الحالة الفعلية)

### 1.1 تشغيل النظام (يعمل ✅)

#### الخطوة 1: تشغيل التطبيق
- شغّل ملف `Open_lab.exe` بعد البناء
- ستظهر شاشة `Login Window` بحجم 400×550 في وسط الشاشة، غير قابلة لتغيير الحجم

#### الخطوة 2: تسجيل الدخول
1. أدخل اسم المستخدم في حقل «اسم المستخدم»
2. أدخل كلمة المرور في حقل «كلمة المرور»
3. (اختياري) اضغط على أيقونة العين لإظهار كلمة المرور كنص
4. (اختياري) ضع علامة على «تذكر بياناتي» لحفظ اسم المستخدم للجلسات القادمة
5. اضغط Enter أو الزر «دخول»

**الحالات الممكنة:**
| الحالة | الرسالة | الإجراء |
|---|---|---|
| نجاح | "تم تسجيل الدخول بنجاح." | تُفتح النافذة الرئيسية تلقائياً |
| فشل المصادقة | "بيانات الدخول غير صحيحة." | راجع بيانات الدخول |
| حقول فارغة | "يرجى إدخال اسم المستخدم وكلمة المرور." | املأ الحقول |
| خطأ تقني | "خطأ: {رسالة}" | تواصل مع مسؤول النظام |

**حساب الإدارة الافتراضي (من PDFs):**
- اسم المستخدم: `admin`
- كلمة المرور: `admin` (أو `123` حسب PDF القديم)
- عند الدخول لأول مرة، يستدعي AdminSetupService.EnsureAdminAccessAsync لمنح جميع الصلاحيات

### 1.2 التنقل في النافذة الرئيسية (يعمل ✅)

بعد Login، تظهر MainWindow بأبعاد 1100×700 (CanResize). تحوي:

#### الشريط العلوي (Toolbar — 90 px)
12 خانة بترتيب يميني-يساري:

| # | الزر | الأيقونة | السلوك |
|---|---|---|---|
| 1 | المرضى | 👤 | يفتح PatientModuleView |
| 2 | أدوات | 🔧 | يفتح ToolsModuleView |
| 3 | ورقة عمل | 📝 | يفتح WorksheetModuleView |
| 4 | حسابات | 🧮 | يفتح AccountsModuleView |
| 5 | احصاليات | 📊 | يفتح StatisticsModuleView |
| 6 | المستخدمين | 👤 | يفتح UsersModuleView |
| 7 | بيانات النظام | ☁️ | يفتح SystemDataModuleView |
| 8 | اعدادات | ⚙️ | يفتح SettingsModuleView |
| 9 | الموظفين | 👤 | يفتح WelcomeView (شعار النظام فقط) |
| 10 | هل تعلم | ℹ️ | يفتح WelcomeView (شعار النظام فقط) |
| 11 | نبذة | ℹ️ | يفتح WelcomeView (شعار النظام فقط) |
| 12 | خروج | ❌ | تسجيل الخروج |

#### المحتوى المركزي (Content Area)
- في البداية: شعار النظام (WelcomeView) بألوان خضراء
- بعد الضغط على موديول: Hub Card علوي + شبكة أزرار سفلية

#### شريط الحالة السفلي (StatusBar — 30 px)
- 👤 اسم المستخدم: `<CurrentUser>`
- آخر دخول: `<LastLoginDate>`
- قاعدة البيانات: متصل (⚠️ نص ثابت، ليس حقيقياً)
- اليوم: `<CurrentDate>` بصيغة yyyy/MM/dd

### 1.3 التنقل داخل الـ Hubs (يعمل ✅)

#### Hub المرضى
بعد الضغط على «المرضى» في الـ Toolbar، تظهر شاشة بـ:
- كرت معلومات علوي يصف الوظائف المتاحة:
  - "إضافة مريض جديد وطباعة الباركود والإيصال الخاص به وإدخال النتائج وطباعتها"
  - "البحث عن مريض محدد أو عرض مرضى فترة معينة ومعرفة النتائج الغير منتهية"
  - "وكذلك الغير محققة والغير مطبوعة"
  - "تسليم نتائج المرضى وتصفية الحسابات الخاصة بهم وحصر النتائج الغير مسلمة"
- شبكة 2×2 من الأزرار الخضراء:
  - «اضافة وتعديل بيانات المرضى» (➕)
  - «ادخال نتائج التحاليل» (📝)
  - «تسليم نتائج المرضى» (📄)
  - «بحث عن مريض» (🔍)

⚠️ **عند الضغط على أي زر:** يفتح PlaceholderView بعنوان «نافذة مؤقتة للوظيفة، وسيتم استكمال محتواها لاحقاً.» مع زر «رجوع» للعودة للـ Hub.

#### Hub بيانات النظام
شبكة 4×4 من 14 زر:
- بيانات التحاليل، Barcode Types، Culture Antibiotics
- مجموعات التحاليل، Test Units، Test Comments
- Lab. branches، القاب وتعريفات المرضى، Custom Groups
- مجموعات العمل (Log)، طباعة قائمة اسعار التحاليل
- أجهزة ومعدات المعمل (لون أزرق سماوي)
- 2 زرين برتقاليين: «الجهات الخارجية والمعدل» و «قائمة أسعار التحاليل للجهات»

⚠️ كلها تفتح PlaceholderView.

#### Hub الحسابات (4 أزرار)
- الجرد وحساب الدرج
- العينات المرسلة للخارج
- صرف وإيداع نقدية
- حساب شركات ومندوبين

#### Hub ورقة العمل (2 أزرار)
- ورقة عمل بأسماء المرضى
- ورقة عمل بأسماء التحاليل (Log)

#### Hub الإحصائيات (6 أزرار)
- احصاليات وفقاً لعدد المرضى
- احصاليات وفقاً لعدد التحاليل
- احصاليات خاصة بفروع المعمل
- احصاليات العينات المرسلة
- احصاليات تقيم ومتابعة العمل
- متابعة ومراقبة النتائج

#### Hub الإعدادات (2 أزرار)
- اعدادات النظام
- Database Maintenance

#### Hub الأدوات (9 أزرار في شبكة 3×3)
- مكتبة التحاليل، ساعة التوقيت Stopwatch، قائمة المطلوبات والمشتريات
- مكتبة الصور، محول وحدات نتائج التحاليل، نونة المواعيد
- قاموس لاختصارات، الآلة الحاسبة، دليل الهاتف

#### Hub المستخدمين (4 أزرار)
- انشاء مستخدمين
- تغيير كلمة المرور
- الحضور والإنصراف
- Login detector

### 1.4 الرجوع من PlaceholderView (يعمل ✅)

كل PlaceholderView يحوي زر «رجوع» في الأسفل. عند الضغط:
1. يختفي PlaceholderView
2. يعود الـ Toolbar للظهور
3. يُعاد بناء Hub المناسب وفقاً لـ `ActiveModule` الحالي

### 1.5 تسجيل الخروج (يعمل ✅)

اضغط زر «خروج» في الـ Toolbar:
1. يُغلق سجل الحضور (CloseAttendanceAsync)
2. تُمسح AppSession (UserId، Username، Permissions، AttendanceLogId)
3. تُغلق MainWindow
4. تظهر LoginWindow من جديد

---

## القسم الثاني — ما لا يستطيع المستخدم فعله حالياً (الفجوات)

### 2.1 الوظائف المعطّلة (45 وظيفة)

كل وظيفة فرعية من الـ Hubs تفتح PlaceholderView. لا يمكن:
- ❌ إضافة أو تعديل بيانات مريض
- ❌ إدخال نتائج تحاليل
- ❌ تسليم نتائج للمرضى
- ❌ طباعة باركود أو إيصال
- ❌ البحث عن مريض
- ❌ إنشاء فاتورة
- ❌ معاينة تقرير
- ❌ إدارة كتالوج التحاليل أو القيم المرجعية
- ❌ إضافة مستخدمين أو تغيير صلاحيات
- ❌ عمل نسخة احتياطية
- ❌ معاينة الإحصائيات
- ❌ تسجيل الحضور
- ❌ إدارة الخزينة
- ❌ المزارع البكتيرية

### 2.2 الميزات المفقودة في تجربة المستخدم

| الميزة | الحالة | التأثير |
|---|---|---|
| اختصارات لوحة المفاتيح (F1-F12) | ❌ مفقودة | كل عمل يحتاج ماوس |
| قوائم سياقية (Right-Click) | ❌ مفقودة | لا اختصارات سريعة |
| Dialog تأكيد قبل الحذف | ❌ مفقودة | لا حماية من الحذف العرضي |
| شريط تحميل (Progress Bar) | ❌ مفقودة | لا تغذية راجعة أثناء العمليات الطويلة |
| ToolTips | ❌ مفقودة | لا شرح لكل زر |
| Toast Notifications | ❌ مفقودة | الأخطاء فقط في StatusMessage |
| Theming / Dark Mode | ❌ مفقودة | ثيم واحد فقط |
| البحث الفوري (Live Search) | ❌ مفقودة | يحتاج ضغط زر «بحث» |
| Drag & Drop | ❌ مفقودة | غير معتمد |
| تصدير لـ Excel / PDF مباشرة | ⚠️ في الخدمات | غير متاح من UI |

---

## القسم الثالث — الإجراءات المُتوقَّعة من PDFs (للمرجعية والتنفيذ المستقبلي)

### 3.1 تسجيل مريض جديد (المرجع: real lab system help.pdf، RLS_Learn.pdf)

#### الإجراء المتوقع:
1. اضغط F2 (أو الزر «المرضى» في Toolbar) — يفتح PatientModuleView
2. اضغط «اضافة وتعديل بيانات المرضى» — يفتح PatientRegistrationView (غير مُنفَّذ بعد)
3. في PatientRegistrationView (متوقع):
   - اضغط F1 أو الزر «جديد» لتفريغ النموذج
   - النظام يولّد LabId تلقائياً (يمكن إعادة توليده بـ GenerateLabIdCommand)
   - أدخل البيانات الإلزامية:
     - **الاسم الكامل** (مطلوب)
     - **النوع:** ذكر / أنثى
     - **تاريخ الميلاد** أو السن بالأيام/الشهور/السنوات
   - أدخل البيانات الاختيارية:
     - رقم الهاتف
     - العنوان
     - الرقم القومي
     - جهة الإحالة (من ComboBox Referrals)
     - نوع الحساب: Cash / Referral
   - أدخل التاريخ الطبي:
     - الأمراض المزمنة
     - الحساسيات
     - الأدوية الحالية
     - ملاحظات طبية
4. اضغط F9 أو الزر «حفظ» — SaveCommand
5. اضغط F10 لطباعة الباركود — PrintBarcode (غير منفذ)
6. اضغط F11 لطباعة الإيصال — PrintReceipt (غير منفذ)

#### تعديل مريض موجود:
1. أدخل LabId واضغط Enter — LoadByLabIdCommand
2. أو ابحث بالاسم/الهاتف واضغط Enter في الحقل — SearchCommand
3. اختر المريض من DataGrid Results — SelectedPatient يحمّل تلقائياً
4. عدّل البيانات
5. F9 للحفظ

### 3.2 اختيار التحاليل لمريض (PatientTestsSelection)

1. أدخل LabId المريض واضغط «تحميل المريض»
2. اضغط «إنشاء زيارة» — CreateVisitCommand
3. حدد نوع الحساب: Cash أو Referral
4. إذا Referral، اختر الجهة من Combo
5. من القائمة اليسرى (Available Tests):
   - ابحث بكتابة في SearchText
   - اختر تحليل
   - اضغط مزدوج لإضافته (أو AddTestCommand)
   - أو اختر مجموعة مخصصة من ComboBox CustomGroups + AddCustomGroupCommand
6. التحاليل تظهر في القائمة اليمنى (Selected Tests)
7. لإزالة تحليل: اختره من Selected Tests + RemoveTestCommand
8. الـ TotalAmount يُحسب تلقائياً وفقاً لـ AccountType + Referral PriceList

### 3.3 إدخال نتائج التحاليل (ResultsEntry)

1. اضغط F4 لفتح ResultsEntry
2. حدد نطاق التاريخ DateFrom/DateTo
3. اضغط LoadVisitTestsCommand
4. من قائمة VisitTests (يمين)، اختر فحص
5. القائمة الوسطى تعرض ResultItems للفحص المختار
6. أدخل القيم (Enter للانتقال للحقل التالي)
7. F8 للتبديل بين Reviewed/Not Reviewed
8. F9 لحفظ + Toggle Complete (Verify)
9. F12 لمعاينة الطباعة
10. إذا كان الفحص محقَّقاً وتحتاج إعادة فتح: ReopenResultsCommand

#### تنبيهات طبية:
- إذا كان للمريض ChronicDiseases / Allergies / Medications، يظهر panel «MedicalHistorySummary» بلون مميز
- HasMedicalAlerts = true ⇒ يجب الانتباه

### 3.4 حساب المريض (PatientBilling)

1. أدخل VisitId واضغط LoadVisitCommand
2. تظهر القيم:
   - Total (الإجمالي)
   - Discount (الخصم — قابل للتعديل)
   - Paid (المدفوع)
   - NetTotal (المبلغ الصافي)
   - Balance (الرصيد المتبقي)
3. لإضافة دفعة جديدة:
   - اختر PaymentMethod من ComboBox
   - أدخل المبلغ
   - AddPaymentCommand
4. لتعديل دفعة موجودة:
   - اختر من DataGrid Payments — SelectedPayment يُحمَّل في EditPaymentAmount
   - عدّل المبلغ
   - EditPaymentCommand
5. لحذف دفعة:
   - اختر من DataGrid
   - DeletePaymentCommand
6. لإضافة رسم إضافي:
   - أدخل NewChargeDescription
   - أدخل NewChargeAmount
   - AddChargeCommand
7. للتصفية: SettleAccountCommand (مع Reason)
8. للحفظ: SaveInvoiceCommand
9. للطباعة: PrintInvoiceCommand
10. **اختصار سريع (من PDF):** اضغط Enter Enter (مرتين) لإتمام الحساب

### 3.5 تسليم النتائج (Delivery)

1. اضغط F6 أو الزر «تسليم نتائج المرضى»
2. SearchCommand لاسترجاع Visits الجاهزة
3. اختر من قائمة Visits
4. اضغط DeliverCommand لتسليم النتائج (يسجل التاريخ + المستخدم)
5. لإعادة فتح زيارة مُسلَّمة: ReopenCommand

### 3.6 البحث عن مريض (PatientSearch)

1. اضغط F3 أو الزر «بحث عن مريض»
2. املأ واحداً أو أكثر من الحقول:
   - Name (مطابق أو عشوائي)
   - Phone
   - LabId
   - Date (تاريخ الزيارة)
3. اضغط SearchCommand (أو F5)
4. النتائج تظهر في DataGrid Patients
5. اختر مريض ⇒ يُحمَّل تلقائياً Visits الخاصة به في DataGrid Visits

### 3.7 تاريخ المريض (PatientHistory)

1. أدخل LabId
2. حدد نطاق From / To
3. LoadHistoryCommand
4. تظهر History (PatientHistoryReportData) + Visits
5. PrintHistoryCommand للطباعة

### 3.8 المزارع والمضادات الحيوية (CultureSensitivity)

أكثر شاشة تعقيداً (548 سطر VM):

#### إدارة Cultures:
1. AddCultureCommand لإضافة بكتيريا جديدة
2. DeleteCultureCommand للحذف

#### إدارة Antibiotics:
1. AddAntibioticCommand
2. DeleteAntibioticCommand

#### الربط:
1. اختر Culture
2. اختر Antibiotic
3. LinkCommand — يربطهما
4. UnlinkCommand — يفصلهما
5. النتيجة في LinkedAntibiotics

#### إدخال نتائج المزرعة:
1. LoadVisitTestsCommand لاسترجاع المزارع الجاهزة
2. اختر VisitTest
3. لكل Antibiotic، حدد الحساسية من AllowedSensitivities (S/I/R)
4. SaveResultCommand
5. PrintCultureReportCommand للطباعة

### 3.9 إدارة المستخدمين والصلاحيات (UsersPermissions)

#### إضافة مستخدم:
1. املأ Username + Password
2. اختر Role من ComboBox
3. SaveUserCommand

#### إضافة دور:
1. أدخل اسم الدور
2. SaveRoleCommand

#### تعيين صلاحيات لدور:
1. اختر Role
2. حدد Permissions من ListBox (PermissionToggle)
3. SaveRolePermissionsCommand

#### تعيين دور لمستخدم:
1. اختر User
2. اختر Role
3. AssignRoleCommand / UnassignRoleCommand

### 3.10 الإحصائيات (Statistics)

1. حدد From/To
2. (اختياري) فلتر Gender أو Referral
3. LoadCommand
4. تظهر 8 تبويبات:
   - ByGender — التوزيع حسب الجنس
   - ByReferral — التوزيع حسب جهة الإحالة
   - MonthlyAnalysis — تحليل شهري
   - TopTests — أكثر التحاليل طلباً
   - YearlySamples — العينات السنوية
   - UserProductivity — إنتاجية المستخدمين
5. PrintCommand لطباعة التقرير

### 3.11 إعدادات النظام (SystemSettings)

#### إعدادات الـ Profile:
- نطاق التسليم
- الطابعة الافتراضية للباركود
- رقم الفرع
- بيانات الـ Server (Host, Database name, Auth)
- SaveProfileCommand

#### إعدادات Raw:
- DataGrid Settings (key-value)
- SaveRawSettingCommand
- DeleteRawSettingCommand
- ReloadCommand

#### تغيير كلمة مرور المشرف:
- ChangeMasterPasswordCommand

### 3.12 النسخ الاحتياطي (BackupRestore)

#### نسخ احتياطي يدوي:
1. BackupCommand — ينشئ ملف backup فوراً

#### استرجاع:
1. LoadBackupsCommand — يعرض الملفات في BackupFiles
2. اختر ملف من ListBox
3. RestoreCommand — يستعيد النظام

#### جدولة:
1. ConfigureScheduleCommand
2. DisableScheduleCommand
3. RefreshScheduleCommand

### 3.13 الحضور والانصراف (AttendanceLog)

1. LoadLogsCommand لتحميل السجلات
2. ClockInCommand لتسجيل الدخول
3. ClockOutCommand لتسجيل الخروج
4. StartBreakCommand لبداية الاستراحة
5. EndBreakCommand لنهاية الاستراحة
6. LoadDailySummaryCommand لملخص اليوم

### 3.14 الخزينة (AccountsTreasury)

1. LoadCommand بمعاملات (DateFrom, DateTo)
2. أزرار سريعة:
   - DailyCommand
   - WeeklyCommand
   - MonthlyCommand
3. التبويبات: Payments / ByUser / ByReferral / ByBranch / ByDoctor
4. PrintCommand للطباعة

### 3.15 جمع العينات (SampleCollection)

1. LoadCommand لعرض العينات
2. لكل عينة:
   - MarkCollectedCommand — تم سحب العينة من المريض
   - MarkExternalCollectedCommand — تم سحب عينة خارجية
   - MarkSeparatedCommand — تم فصل العينة
   - MarkNotCollectedCommand — لم تُسحب
3. RefreshSampleStatusCommand لتحديث الحالة

### 3.16 إدارة المعامل الخارجية (ExternalLabManagement)

#### قائمة الانتظار:
1. LoadReferralsCommand
2. LoadQueueCommand
3. اختر من PendingQueue
4. CreateManifestCommand لإنشاء بيان شحن

#### بيانات الشحن (Manifests):
1. LoadManifestsCommand
2. UpdateStatusCommand لتحديث حالة الشحن (مرسلة / مستلمة / النتائج)

#### التسوية المالية:
1. LoadSettlementCommand
2. CreateSettlementCommand لإنشاء تسوية

#### إدخال النتائج الخارجية:
1. EnterExternalResultCommand
2. PrintExternalReportCommand

### 3.17 طباعة الإيصال والباركود (ReceiptPrinting)

1. LoadCommand بـ VisitId
2. PrintReceiptCommand — F11
3. PrintBarcodeCommand — F10

### 3.18 التقرير المجمع (CombinedReport)

1. LoadCommand بـ VisitId
2. Tests يظهر في DataGrid
3. MoveUpCommand / MoveDownCommand لإعادة الترتيب
4. SaveReportOrderCommand لحفظ الترتيب الجديد

### 3.19 التقرير الفارغ (BlankReport)

1. LoadCommand
2. PrintBlankCommand — يطبع رأس التقرير ببيانات المريض فقط لإدخال يدوي

### 3.20 المقارنة بالتاريخ (CompareWithHistory)

1. أدخل LabId
2. LoadPatientCommand
3. LoadHistoryCommand
4. تظهر المقارنة بين Tests الحالية و HistoryResults السابقة
5. ClearCommand للتفريغ

### 3.21 ورقة عمل المجموعة (GroupWorksheet)

1. LoadGroupsCommand لتحميل Groups + CustomGroups
2. اختر مجموعة
3. LoadWorksheetCommand لتحميل Rows
4. PrintCommand للطباعة

### 3.22 سجل تصنيف التحاليل (TestClassificationLog)

1. LoadCommand لاسترجاع ReagentConsumptionReport
2. PrintCommand

### 3.23 سجل نشاط المستخدمين (UserActivityLog)

1. LoadCommand — يعرض كل العمليات (إضافة، تعديل، حذف) مع المستخدم والتوقيت

### 3.24 مراقبة استخدام النظام (SystemUsageMonitor)

1. RefreshCommand — يعرض الـ Sessions النشطة

### 3.25 فاتورة التعاقد (ContractInvoice)

1. LoadReferralsCommand
2. اختر جهة
3. LoadPendingCommand لعرض الفواتير المعلقة
4. CreateInvoiceCommand لإنشاء فاتورة جديدة
5. LoadHistoryCommand لعرض الفواتير السابقة
6. SettleSelectedCommand لتسوية فاتورة

### 3.26 لوحة المعلومات (Dashboard)

1. تتحمل تلقائياً عند فتحها
2. تعرض:
   - PatientCount — عدد المرضى
   - VisitCount — عدد الزيارات
   - TestCount — عدد التحاليل

### 3.27 إدارة كتالوج التحاليل (TestCatalog)

#### إضافة تحليل:
1. NewCommand لتفريغ النموذج
2. املأ TestName, ReportName, Group, SampleType, Unit, Price
3. GenerateBarcodeCommand لتوليد barcode فريد
4. SaveCommand للحفظ

#### تعديل/حذف:
- اختر من DataGrid Tests
- DeleteCommand للحذف
- ReloadCommand لإعادة التحميل

### 3.28 القيم المرجعية (ReferenceRanges)

1. اختر تحليل من DataGrid Tests
2. LoadCommand لتحميل Ranges
3. أضف range جديد في DataGrid Ranges:
   - MinValue, MaxValue
   - AgeFrom, AgeTo
   - Gender
   - LowComment, HighComment
   - IsCritical
4. SaveCommand للحفظ
5. DeleteCommand للحذف

### 3.29 تعليقات التحاليل (TestComments)

نمط مشابه لـ ReferenceRanges مع DataGrid Comments بدلاً من Ranges.

### 3.30 قوائم الأسعار (PriceLists)

1. LoadCommand
2. اختر قائمة أسعار من ListBox PriceLists
3. SaveListCommand / UpdateListCommand
4. لكل قائمة، إدارة Items:
   - AddItemCommand (Test + Price)
   - UpdateItemCommand
   - DeleteItemCommand
5. PrintListCommand للطباعة

### 3.31 المجموعات المخصصة (CustomGroups)

1. أنشئ مجموعة جديدة
2. SaveGroupCommand
3. AddItemCommand لإضافة Test للمجموعة
4. DeleteItemCommand للحذف

### 3.32 الجهات (Referrals)

1. DataGrid Referrals قابل للتحرير
2. الأعمدة: Name, ReferralType (None/Doctor/Lab/Insurance), Phone, Address, Commission, PriceList
3. SaveCommand
4. DeleteCommand

### 3.33 الثوابت والمعادلات (Constants)

1. SeedDefaultsCommand لتهيئة القيم الافتراضية أول مرة
2. ReloadCommand
3. SaveCommand
4. DeleteCommand

### 3.34 تقرير الحضور (AttendanceReport)

1. LoadUsersCommand
2. اختر مستخدم + نطاق تاريخ
3. GenerateReportCommand — تقرير تفصيلي
4. GeneratePayrollSummaryCommand — ملخص الرواتب
5. ClearFilterCommand للتفريغ

### 3.35 معاينة التقرير (ReportViewer)

1. أدخل VisitId
2. LoadReportCommand — يبني التقرير
3. PreviewPdfPath / PreviewPdfUri يُعرض في WebBrowser
4. PrintCommand للطباعة
5. ReprintCommand لإعادة الطباعة (يسجل reprint count)

### 3.36 ورقة عمل بأسماء المرضى (WorkSheetByPatient)
LoadCommand → DataGrid Rows → PrintCommand

### 3.37 ورقة عمل بأسماء التحاليل (WorkSheetByTest)
نفس النمط

### 3.38 حساب المريض حسب التاريخ (PatientBillingByDate)

1. أدخل PatientId
2. حدد FromDate / ToDate
3. SearchCommand
4. تظهر TotalInvoiced, TotalPaid, Balance
5. التبويبات: Invoices DataGrid / Payments DataGrid

---

## القسم الرابع — السيناريوهات الكاملة (End-to-End)

### Scenario S.01 — يوم عمل كامل لاستقبال مريض

```
1. Login (admin/admin)
2. Toolbar → المرضى
3. Hub → اضافة وتعديل بيانات المرضى ⚠️ Placeholder حالياً
   [مستقبلاً] PatientRegistrationView:
   - F1 (جديد)
   - LabId يُولَّد تلقائياً
   - تعبئة Form: Name, Gender, BirthDate, Phone, ReferralCombo
   - تعبئة Medical History
   - F9 (حفظ)
   - F10 (طباعة باركود)
4. Hub → ادخال نتائج التحاليل ⚠️ Placeholder
   [مستقبلاً] ResultsEntry:
   - F4
   - LoadVisitTestsCommand
   - تحديد فحص → إدخال قيم → F9 (حفظ)
5. Hub → تسليم نتائج المرضى ⚠️ Placeholder
   [مستقبلاً] Delivery:
   - F6 → SearchCommand → اختر visit → DeliverCommand
6. Toolbar → خروج
```

### Scenario S.02 — تقرير مالي شهري

```
1. Login (مدير حسابات)
2. Toolbar → حسابات
3. Hub → الجرد وحساب الدرج ⚠️ Placeholder
   [مستقبلاً] AccountsTreasury:
   - MonthlyCommand
   - مراجعة Payments + ByUser + ByReferral
   - PrintCommand
4. Toolbar → احصاليات
5. Hub → احصاليات تقيم ومتابعة العمل ⚠️ Placeholder
   [مستقبلاً] Statistics:
   - LoadCommand (DateFrom/DateTo شهر)
   - فحص UserProductivity, MonthlyAnalysis
6. Toolbar → خروج
```

### Scenario S.03 — إضافة مستخدم جديد

```
1. Login (admin)
2. Toolbar → المستخدمين
3. Hub → انشاء مستخدمين ⚠️ Placeholder
   [مستقبلاً] UsersPermissions:
   - تعبئة Username/Password
   - اختيار Role
   - SaveUserCommand
4. Toolbar → خروج
```

---

## القسم الخامس — الأسئلة الشائعة (FAQ)

### Q1. لماذا كل زر في الـ Hubs يفتح "نافذة مؤقتة"؟
**الإجابة:** النظام في مرحلة بناء الـ UI Skeleton. الـ Hubs والشريط العلوي منفذان بالكامل، لكن الـ Sub-Views (التفصيلية) لـ 45 وظيفة لم تُنفَّذ بعد. الـ ViewModels جاهزة، تنتظر فقط XAML.

### Q2. هل يمكنني تسجيل مريض جديد الآن؟
**الإجابة:** لا — الواجهة الرسومية للوظيفة غير موجودة. لكن الـ Backend (PatientRegistrationViewModel + PatientService) جاهز للعمل.

### Q3. لماذا «متصل» تظهر دائماً حتى لو لم يكن DB متصلاً؟
**الإجابة:** هذا نص ثابت في XAML، ليس Binding حقيقي. تحتاج إلى إضافة Property `DatabaseStatus` في MainViewModel وربطه بـ HealthCheck service.

### Q4. لماذا لا أستطيع الضغط على F1 أو F9؟
**الإجابة:** لا توجد KeyBindings مخصصة في النظام الحالي. الاختصارات الافتراضية فقط (Tab, Enter via IsDefault, Space) تعمل.

### Q5. كيف أعود من PlaceholderView؟
**الإجابة:** اضغط زر «رجوع» الأزرق في أسفل الشاشة. سيُعيد بناء Hub المناسب.

### Q6. لماذا تختفي الشريط العلوي عند فتح PlaceholderView؟
**الإجابة:** هذا مقصود — `IsToolbarVisible=false` عند فتح Placeholder لتركيز المستخدم على «العودة» فقط. لاحقاً، عند تنفيذ Views حقيقية، سيظل الـ Toolbar ظاهراً.

### Q7. ما الفرق بين «الموظفين» و «المستخدمين» في الـ Toolbar؟
**الإجابة:**
- «المستخدمين» — يفتح UsersModuleView (Hub فعلي بـ 4 أزرار)
- «الموظفين» — يفتح WelcomeView فقط (شعار النظام، لا وظائف) — مخصَّص للتنفيذ المستقبلي

### Q8. لماذا في القائمة أيقونة المرضى والمستخدمين متطابقة؟
**الإجابة:** كلاهما يستخدم `\uE716` (Segoe MDL2 People icon). هذا قد يُربك بصرياً — يُنصح بتخصيص أيقونات فريدة.

### Q9. هل البيانات تُحفظ فعلاً بعد Login؟
**الإجابة:** نعم — AppSession يحتفظ بـ UserId, Username, Permissions, AttendanceLogId طوال الجلسة. عند Logout، تُمسح كلها.

### Q10. هل أستطيع تعديل أبعاد النافذة؟
**الإجابة:** نعم بعد Login (CanResize) مع حد أدنى 980×640. أما LoginWindow فثابتة 400×550 (NoResize).

---

## القسم السادس — التحذيرات والقيود

### تحذير W.01: لا حماية من الإغلاق العرضي
لا يوجد Dialog يسألك "هل أنت متأكد؟" قبل تسجيل الخروج. أي تغيير غير محفوظ سيُفقد.

### تحذير W.02: AttendanceLog قد يبقى مفتوحاً
لو أُغلق النظام بقوة (Task Manager / Power off)، CloseAttendanceAsync لن يُنفَّذ، وسيبقى السجل مفتوحاً في DB.

### تحذير W.03: الصلاحيات لا تُحدَّث في وسط الجلسة
AppSession.SetPermissions يحدث مرة عند Login فقط. لو غيّر المدير صلاحياتك، تحتاج Logout/Login لرؤية التغيير.

### تحذير W.04: RememberMe يحفظ اسم المستخدم فقط
لا تُحفظ كلمة المرور (وهذا أمان جيد). لكن لو تشاركت الجهاز، الاسم سيظهر لكل من يفتح النظام.

### تحذير W.05: لا اختبار اتصال DB قبل العمليات
كل عملية ViewModel تستدعي Service يصل لـ DB. لو DB غير متصل، ستحصل على Exception ⇒ StatusMessage = «خطأ: …».

---

## القسم السابع — الإجراءات الإدارية

### Admin A.01: تشغيل النظام للمرة الأولى
1. تأكد من تثبيت .NET 8 Desktop Runtime
2. تأكد من توفر SQL Server وإنشاء قاعدة بيانات
3. تعديل App.config بـ ConnectionString الصحيح
4. تشغيل Migrations: `dotnet ef database update`
5. Login بـ admin/admin (سيتم إعداد الصلاحيات تلقائياً)

### Admin A.02: نسخة احتياطية يدوية
حالياً عبر SQL Server Management Studio فقط، لأن BackupRestoreView غير مُنفَّذ.

### Admin A.03: إضافة مستخدم جديد (workaround)
1. عبر SQL Server Management Studio مباشرة في جدول Users
2. أو عبر تنفيذ AdminSetupService.EnsureAdminAccessAsync لمستخدم محدد (يتطلب تعديل كود)

---

## القسم الثامن — خلاصة الحالة التشغيلية

| المجال | الحالة | النسبة |
|---|---|---|
| تسجيل دخول / خروج | ✅ كامل | 100% |
| التنقل بين Hubs | ✅ كامل | 100% |
| العرض البصري | ✅ كامل | 100% |
| الوظائف الأساسية للمرضى | ❌ غير منفَّذ | 0% (Backend جاهز) |
| الوظائف المالية | ❌ غير منفَّذ | 0% (Backend جاهز) |
| النتائج والتقارير | ❌ غير منفَّذ | 0% (Backend جاهز) |
| إدارة المستخدمين | ❌ غير منفَّذ | 0% (Backend جاهز) |
| الإحصائيات | ❌ غير منفَّذ | 0% (Backend جاهز) |
| الإعدادات | ❌ غير منفَّذ | 0% (Backend جاهز) |
| الاختصارات | ❌ مفقودة | 0% |
| الـ UX المتقدم | ❌ مفقود | 0% |

**التقييم الإجمالي:** النظام في حالة **Shell ready, Functions pending** — البنية التحتية كاملة (Services، ViewModels، Navigation infrastructure)، لكن طبقة العرض للوظائف الفعلية تحتاج إنتاج 37 View XAML جديدة وفقاً للخطة في الملف 11.

---

## ختاماً

هذا الدليل يصف:
1. **ما يمكنك فعله الآن** (Login + التنقل في 8 Hubs + الرجوع من Placeholders)
2. **ما لا يمكنك فعله** (45 وظيفة فرعية كلها معطّلة)
3. **ما يجب أن تكون قادراً على فعله** (وفقاً لـ PDFs المرجعية)
4. **كيفية الوصول إلى الحالة المستهدفة** (راجع الملف 11 — خطة الهجرة)

عند اكتمال خطة الهجرة (~23 يوم عمل تقديري)، سيصبح النظام كاملاً قابلاً للاستخدام في معامل التحاليل الطبية الحقيقية.

---
