# وثيقة اعتماد هندسي وتحليل أمني معماري لمعالجة F8

**النطاق:** F8 — SQL Server Password `sa` Hardcoded `og2026ra`  
**نوع الوثيقة:** تحليل معماري وتخطيط إصلاح فقط  
**حالة التنفيذ:** لا تتضمن هذه الوثيقة أي تعديل تطبيقي على كود الإنتاج أو الاختبارات أو الإعدادات  
**المراجع التي تم فحصها:**  
- `Open_lab/Docs/The_final_completely_analysis_for_open_lab.md`
- `Open_lab/App.config`
- `Open_lab/Data/OpenLabDbContext.cs`
- `Open_lab/Data/OpenLabDbContextFactory.cs`
- مسارات bootstrap وDI والاختبارات ذات الصلة للفهم المعماري فقط

---

## 1. ملخص الفهم الأمني والمعماري

### 1.1 فهم الثغرة

الثغرة F8 تتمثل في وجود كلمة مرور SQL Server لحساب `sa` داخل المستودع وفي مسارات runtime/design-time. القيمة المكشوفة هي `og2026ra`، وتظهر في ملف الإعدادات وفي منطق fallback داخل `OpenLabDbContext` و`OpenLabDbContextFactory`.

هذه الثغرة حرجة عملياً حتى لو صُنفت في التقرير المرجعي كـ High، لأن الحساب المستخدم هو `sa` وليس حساباً محدود الصلاحيات. حساب `sa` يمثل امتيازاً عالياً على خادم SQL Server، وأي تسريب لكلمة مروره قد يؤدي إلى السيطرة على قاعدة البيانات، تعديل بيانات المرضى والفواتير والنتائج، حذف سجلات audit، أو استخدام الخادم كنقطة حركة جانبية داخل البيئة.

نموذج التهديد المرتبط بها يشمل:

| محور التهديد | التحليل |
|---|---|
| Credential Exposure | السر موجود كنص صريح في المستودع ويمكن نسخه من التاريخ أو من ملفات النشر. |
| Privilege Escalation | استخدام `sa` يمنح صلاحيات أعلى من احتياج التطبيق، وقد يحول اختراق ملف إعدادات إلى اختراق كامل لقاعدة البيانات. |
| Database Compromise | المهاجم يستطيع قراءة أو تعديل أو حذف البيانات الطبية والمالية إذا وصل إلى SQL Server. |
| Lateral Movement | بيانات اعتماد SQL قد يعاد استخدامها أو تساعد في التحرك داخل الشبكة إذا كانت إعدادات البيئة ضعيفة. |
| Configuration Leakage | App.config وملفات build والنشر قد تنقل السر خارج بيئة التطوير. |
| Secret Reuse Risk | إذا استُخدمت كلمة المرور نفسها في أكثر من بيئة، يصبح المستودع نقطة اختراق للبيئات الأخرى. |

### 1.2 تحليل الحالة الحالية

الملفات المتأثرة مباشرة:

- `Open_lab/App.config`
- `Open_lab/Data/OpenLabDbContext.cs`
- `Open_lab/Data/OpenLabDbContextFactory.cs`

الكلاسات المتأثرة:

- `Open_lab.Data.OpenLabDbContext`
- `Open_lab.Data.OpenLabDbContextFactory`
- `Open_lab.App` بشكل غير مباشر لأنه ينشئ `OpenLabDbContextFactory` داخل تسجيل DI.

الدوال والمسارات المتأثرة:

- `OpenLabDbContext.OnConfiguring`
- `OpenLabDbContextFactory.CreateDbContext`
- `App.ConfigureServices`
- `App.ShowStartupWindow`
- `IAdminSetupService.IsBootstrapRequiredAsync` بشكل غير مباشر لأنه أول مستهلك DB في startup bootstrap.

آلية انتقال كلمة المرور الحالية:

1. يحاول التطبيق قراءة `OPENLAB_CONNECTION`.
2. إذا لم توجد، يقرأ connection string باسم `OpenLabDb` من `App.config`.
3. إذا لم توجد connection string، يبني connection string داخلياً باستخدام `OPENLAB_DB_PASSWORD`.
4. إذا لم توجد `OPENLAB_DB_PASSWORD`، يستخدم fallback صريحاً إلى `og2026ra`.
5. تمر connection string إلى `UseSqlServer`.
6. عند تشغيل التطبيق، `App.ConfigureServices` ينشئ `OpenLabDbContextFactory`، ثم تستخدم خدمات bootstrap والمصادقة هذا السياق.

### 1.3 الأدلة البرمجية

#### `Open_lab/App.config:4`

```xml
<add name="OpenLabDb" connectionString="Server=.\SQLEXPRESS;Database=OpenLab;User ID=sa;Password=og2026ra;TrustServerCertificate=True;MultipleActiveResultSets=True;Encrypt=False;Connect Timeout=30" providerName="Microsoft.Data.SqlClient" />
```

هذا الموضع يكشف السر مباشرة داخل ملف إعدادات قابل للنشر. الخطر هنا لا يتوقف على runtime fallback؛ حتى لو تم ضبط environment variable، يبقى السر داخل المستودع.

#### `Open_lab/Data/OpenLabDbContext.cs:75-87`

```csharp
var envConnection = Environment.GetEnvironmentVariable("OPENLAB_CONNECTION");
var configConnection = ConfigurationManager.ConnectionStrings["OpenLabDb"]?.ConnectionString;
var connectionString = !string.IsNullOrWhiteSpace(envConnection)
    ? envConnection
    : configConnection;

if (string.IsNullOrWhiteSpace(connectionString))
{
    var dbPassword = Environment.GetEnvironmentVariable("OPENLAB_DB_PASSWORD") ?? "og2026ra";
    connectionString = $"Server=.\\SQLEXPRESS;Database=OpenLab;User ID=sa;Password={dbPassword};...";
}

optionsBuilder.UseSqlServer(connectionString);
```

هذا المسار يحتوي على fallback credential صريح. وجود `OPENLAB_DB_PASSWORD` لا يكفي لمعالجة الثغرة طالما أن غياب المتغير يؤدي إلى كلمة مرور افتراضية. الخطر الأكبر هنا أن failure mode الحالي ليس آمناً؛ بدلاً من أن يتوقف التطبيق عند غياب الإعداد، يحاول الاتصال بكلمة مرور منشورة.

#### `Open_lab/Data/OpenLabDbContextFactory.cs:12-25`

```csharp
var envConnection = Environment.GetEnvironmentVariable("OPENLAB_CONNECTION");
var configConnection = ConfigurationManager.ConnectionStrings["OpenLabDb"]?.ConnectionString;
var connectionString = !string.IsNullOrWhiteSpace(envConnection)
    ? envConnection
    : configConnection;

if (string.IsNullOrWhiteSpace(connectionString))
{
    var dbPassword = Environment.GetEnvironmentVariable("OPENLAB_DB_PASSWORD") ?? "og2026ra";
    connectionString = $"Server=.\\SQLEXPRESS;Database=OpenLab;User ID=sa;Password={dbPassword};...";
}
```

هذا يكرر نفس الخطر في design-time path. أي إصلاح لا يشمل factory سيترك EF migrations وأدوات التصميم تستخدم fallback غير آمن، أو تفشل بطريقة مختلفة عن runtime.

#### `Open_lab/App.xaml.cs:35-37`

```csharp
services.AddTransient<OpenLabDbContext>(_ =>
{
    var context = new OpenLabDbContextFactory().CreateDbContext(System.Array.Empty<string>());
```

تسجيل DI يعتمد على `OpenLabDbContextFactory` حتى أثناء التشغيل العادي. لذلك فإن `CreateDbContext` ليس مسار migrations فقط؛ هو مسار bootstrap runtime فعلي. هذا يرفع أهمية معالجة factory بنفس درجة معالجة `OnConfiguring`.

---

## 2. الخطة المعمارية التفصيلية للإصلاح

هذه الخطة تصميمية فقط، وليست تفويضاً للتنفيذ.

### 2.1 استراتيجية إزالة كلمة المرور

المطلوب في مرحلة التنفيذ اللاحقة:

- تنظيف `App.config` من أي `Password=...` صريح.
- منع وجود `og2026ra` أو أي كلمة مرور ثابتة في `OpenLabDbContext`.
- منع وجود `og2026ra` أو أي كلمة مرور ثابتة في `OpenLabDbContextFactory`.
- عدم استبدال السر الصريح بسر صريح آخر أو connection string كاملة hardcoded.
- إضافة حارس مراجعة ثابتة لاحقاً يمنع عودة أنماط مثل `Password=`, `User ID=sa`, أو القيمة `og2026ra` إلى ملفات الإنتاج.

تنظيف `App.config` يجب أن يكون مصمماً بحيث لا يكسر parser الخاص بـ `ConfigurationManager`. الخيار الآمن هو أن يحتوي الملف على connection string غير سرية أو أن يزال منها جزء كلمة المرور ويصبح مصدر كلمة المرور خارج المستودع. لكن إذا بقيت connection string بلا كلمة مرور، يجب ألا يحاول SQL client استخدامها بصمت ثم ينتج خطأ غامضاً؛ يجب أن يتم validation قبل `UseSqlServer`.

### 2.2 استراتيجية الاعتماد على Environment Variables

الاستراتيجية الأنسب للحالة الحالية هي فصل البنية غير السرية عن السر:

- معلومات غير سرية: server, database, user id, trust/encrypt flags.
- السر: `OPENLAB_DB_PASSWORD`.

يجب قراءة `OPENLAB_DB_PASSWORD` في نقطة مركزية واحدة لمسار runtime ومسار design-time، أو على الأقل بمنطق مشترك متطابق. نقطة التحقق يجب أن تكون قبل بناء connection string وقبل `UseSqlServer`.

قواعد التحميل المقترحة معمارياً:

1. إذا وُجد `OPENLAB_CONNECTION` كـ override كامل، يجب التحقق أنه لا يحتوي على secret افتراضي معروف.
2. إذا لم يوجد override كامل، يتم بناء connection string من إعدادات غير سرية وكلمة المرور من `OPENLAB_DB_PASSWORD`.
3. إذا كان `OPENLAB_DB_PASSWORD` غائباً أو فارغاً أو whitespace، يجب fail fast.
4. لا يوجد أي fallback إلى كلمة مرور افتراضية.
5. لا يتم تسجيل connection string كاملة في logs أو exception messages.

يجب منع mixing غير المنضبط بين App.config وenvironment variables. الوضع الحالي يسمح بثلاثة مصادر بترتيب ضمني، وهذا يجعل التشخيص صعباً. التصميم الأفضل هو توثيق precedence بوضوح:

| الأولوية | المصدر | الاستخدام |
|---|---|---|
| 1 | `OPENLAB_CONNECTION` | override كامل للبيئات الآلية أو CI، مع فحص عدم احتواء أسرار افتراضية. |
| 2 | App.config غير السري + `OPENLAB_DB_PASSWORD` | المسار المحلي الافتراضي للتطبيق. |
| 3 | لا شيء | فشل آمن برسالة واضحة. |

### 2.3 تصميم Fail-Fast Architecture

يجب أن يحدث fail-fast في أقرب نقطة قبل الاتصال بقاعدة البيانات. عملياً هذا يعني داخل منطق resolve الخاص بالـ connection قبل `UseSqlServer`.

المواضع التي يجب أن تفشل لاحقاً:

- `OpenLabDbContextFactory.CreateDbContext` عند غياب السر المطلوب.
- أي helper مشترك لحل connection string إن تم استخراجه.
- `OpenLabDbContext.OnConfiguring` إذا استُخدم constructor الافتراضي خارج DI.

نوع الاستثناء المناسب:

- `InvalidOperationException`

رسالة الاستثناء المقترحة:

```text
Database password is not configured. Set the OPENLAB_DB_PASSWORD environment variable or provide a secure OPENLAB_CONNECTION value. Hardcoded fallback credentials are not supported.
```

خصائص الرسالة:

- لا تكشف كلمة المرور.
- لا تطبع connection string.
- توضح متغير البيئة المطلوب.
- مناسبة للإنتاج لأنها تشخص المشكلة بدون تسريب.
- تمنع الالتباس مع أخطاء SQL Server الشبكية.

يجب أن يتوقف التطبيق قبل عرض نافذة bootstrap أو login إذا لم يتم حل الإعدادات. عرض bootstrap دون DB غير ممكن لأن `IsBootstrapRequiredAsync` يعتمد على قاعدة البيانات، لذلك الفشل المبكر هنا صحيح معمارياً.

### 2.4 تحليل بدائل التصميم

| البديل | المزايا | العيوب | الملاءمة الحالية |
|---|---|---|---|
| Environment Variables | بسيطة، مدعومة محلياً وCI، لا تحتاج بنية إضافية. | تحتاج توثيقاً جيداً وقد يسهل نسيان ضبطها. | الأنسب للإصلاح الأول منخفض المخاطر. |
| Secure Secret Store | أقوى إنتاجياً، يدعم rotation وحوكمة وصول. | يحتاج بنية تشغيل وإعدادات نشر. | مناسب كمرحلة لاحقة للإنتاج. |
| .NET User Secrets | ممتاز للتطوير المحلي في مشاريع SDK-style. | ليس مناسباً للإنتاج، وWPF/.NET Framework أو mixed setup قد يحتاج ضبطاً إضافياً. | مفيد للمطورين فقط إن كان متوافقاً مع بنية المشروع. |
| Windows Credential Manager | مناسب لتطبيقات Windows desktop، يقلل ظهور الأسرار في env. | يحتاج كود تكامل وتجربة إعداد أوضح. | خيار جيد لاحقاً لتثبيتات Windows. |
| CI/CD Secret Injection | آمن للاختبارات والنشر الآلي. | لا يحل تجربة المطور المحلي وحده. | ضروري لأي pipeline لاحق. |

التوصية المعمارية:

- للإصلاح القريب: `OPENLAB_DB_PASSWORD` و/أو `OPENLAB_CONNECTION` مع fail-fast.
- للاختبارات: لا تعتمد unit/integration tests على SQL Server الحقيقي إلا في test category معزولة؛ استخدم SQLite/InMemory كما هو قائم.
- للإنتاج: الانتقال لاحقاً إلى secret store أو Windows Credential Manager مع rotation policy.

---

## 3. تحليل Zero-Regression وضمان الاستقرار

### 3.1 Bootstrap Flow Safety

مسار startup الحالي:

1. `App.OnStartup`
2. `ConfigureServices`
3. تسجيل `OpenLabDbContext` عبر `OpenLabDbContextFactory.CreateDbContext`
4. `ShowStartupWindow`
5. `IAdminSetupService.IsBootstrapRequiredAsync`
6. إما `BootstrapView` أو `LoginView`

إزالة fallback ستغير نقطة الفشل. حالياً قد يعمل التطبيق محلياً حتى بدون إعداد لأن fallback يوفر كلمة مرور. بعد الإصلاح، غياب المتغير سيؤدي إلى توقف مبكر. هذا مقصود أمنياً، لكنه يحتاج توثيقاً وتجهيزاً حتى لا يفسر المطورون الفشل كتعطل عشوائي.

يجب التأكد قبل التنفيذ من:

- أن `ConfigureServices` لا ينشئ connection فعلياً إلا عند طلب الخدمة. التسجيل الحالي transient لكن factory يستدعى عند إنشاء context.
- أن `ShowStartupWindow` هو أول مسار يطلب DB عبر `adminSetupService`.
- أن رسالة الخطأ ستظهر أو تسجل بطريقة مفهومة في WPF startup، لا تضيع داخل `async void`.
- أن bootstrap الأول لا يعتمد على كلمة المرور القديمة ضمنياً.

لا يوجد Bootstrap آمن دون DB؛ لذلك التصميم الصحيح ليس تجاوز الفشل، بل فشل واضح قبل أي محاولة إنشاء مشرف.

### 3.2 SessionContext Stability

`SessionContext` لا يحمل كلمة مرور DB مباشرة، لكنه يرتبط بالمسار عبر:

- `App.ConfigureServices` يسجل `ISessionContext`.
- إنشاء `OpenLabDbContext` ينسخ `SessionContext.Current.UserId` إلى `context.CurrentUserId`.
- `AuditInterceptor` يستخدم `CurrentUserId` لاحقاً.

إزالة fallback لا يفترض أن تكسر `SessionContext` مباشرة، لكنها قد تمنع الوصول إلى أول DB context، وبالتالي لن تبدأ session أو bootstrap. الضمان المطلوب هو فصل خطأ configuration عن خطأ session:

- غياب `OPENLAB_DB_PASSWORD` يجب أن ينتج `InvalidOperationException` واضحة عن configuration.
- لا يجب أن يظهر الخطأ كـ NullReference أو DI resolution failure.
- لا يجب بدء session جزئية عند فشل DB configuration.

### 3.3 Integration Tests Survival Plan

الوضع الحالي للاختبارات:

- `Open_lab.Tests` يستخدم غالباً `InMemoryDbContextFactory` الذي يبني `DbContextOptions` مسبقاً، وبالتالي `OnConfiguring` يرجع مبكراً بسبب `optionsBuilder.IsConfigured`.
- `Open_lab.IntegrationTests` يستخدم SQLite in-memory داخل `SqliteIntegrationTestBase`، أيضاً مع options configured.
- هذه الاختبارات لا تحتاج SQL password إذا بقيت تستخدم constructors التي تمرر `DbContextOptions`.

الخطر الحقيقي في الاختبارات سيكون في أي اختبار ينشئ `new OpenLabDbContext()` أو يستدعي `OpenLabDbContextFactory.CreateDbContext` دون متغيرات. لذلك يجب تصميم فحوصات regression كالآتي:

| نوع الاختبار | الاستراتيجية |
|---|---|
| Unit tests | الاستمرار في InMemory وعدم طلب أسرار. |
| Integration tests | الاستمرار في SQLite in-memory وعدم طلب أسرار. |
| SQL Server smoke tests | category منفصلة تتطلب `OPENLAB_DB_PASSWORD` أو `OPENLAB_CONNECTION`. |
| Design-time factory tests | اختبار صريح أن غياب السر ينتج fail-fast آمن. |
| Secret scanning tests | فحص static يمنع عودة `og2026ra` أو fallback password. |

للتشغيل المحلي:

- يجب توثيق متغير `OPENLAB_DB_PASSWORD` في دليل المطور.
- يجب توفير مثال إعداد لا يحتوي على السر الحقيقي.
- يجب أن تعمل أغلب الاختبارات دون SQL Server ودون متغيرات أسرار.

لـ CI/CD:

- inject secret من مخزن CI وليس من ملف.
- تشغيل unit/integration الافتراضية دون secret حيثما أمكن.
- تشغيل SQL smoke tests فقط عندما تكون secret متاحة.
- فشل pipeline إذا ظهرت قيمة `og2026ra` في أي ملف غير الوثائق التاريخية المصرح بها أو إذا ظهر fallback credential في كود الإنتاج.

### 3.4 Dependency Injection Resolution

بما أن `App.xaml.cs` يسجل `OpenLabDbContext` باستخدام `OpenLabDbContextFactory`، فإن أي تغيير في factory يؤثر على كل services المسجلة بالـ convention التي تعتمد على `OpenLabDbContext`.

نقاط الحذر:

- إذا رمى factory الاستثناء أثناء إنشاء service، يجب أن تكون الرسالة واضحة.
- لا ينبغي أن يتحول الخطأ إلى failure غامض في `GetRequiredService<IAdminSetupService>`.
- إذا تم استخراج resolver مشترك، يجب تسجيله بطريقة لا تسبب circular dependency.

### 3.5 Operational Impact

إزالة السر من المستودع لا تغير schema ولا تحتاج migration. التأثير تشغيلي:

- كل بيئة تشغيل يجب أن تضبط السر خارج المستودع.
- أي مطور اعتمد على كلمة المرور الافتراضية سيواجه فشل startup.
- أي build أو migration command يعتمد على factory سيحتاج نفس الإعداد.
- يجب تدوير كلمة مرور SQL الفعلية؛ حذفها من الكود وحده لا يكفي إذا كانت انكشفت سابقاً.

---

## 4. تحليل المخاطر

| الخطر | السبب | التأثير | الاحتمالية | آلية التخفيف |
|---|---|---|---|---|
| Startup Failure | غياب `OPENLAB_DB_PASSWORD` بعد إزالة fallback. | التطبيق لا يبدأ. | عالية في أول يوم بعد الإصلاح. | fail-fast واضح، توثيق إعداد محلي، checklist للنشر. |
| Broken Tests | اختبارات تنشئ factory أو constructor افتراضي دون options. | فشل test suite. | متوسطة. | حصر هذه الاختبارات، توفير env في SQL tests، إبقاء InMemory/SQLite مستقلة. |
| Missing Environment Variables | عدم ضبط المتغير في جهاز مطور أو CI. | فشل migrations أو startup. | عالية. | رسالة استثناء واضحة وملف إعدادات مثال. |
| Invalid Configuration | قيمة فارغة أو whitespace أو connection string ناقصة. | SQL exception متأخر وغامض. | متوسطة. | validation قبل `UseSqlServer`. |
| Runtime Connection Failures | كلمة مرور خاطئة أو SQL غير متاح. | فشل bootstrap/login. | متوسطة. | فصل رسائل configuration عن connectivity، smoke test. |
| Developer Misconfiguration | استخدام `OPENLAB_CONNECTION` يحتوي السر القديم. | استمرار الخطر رغم تنظيف الكود. | متوسطة. | secret scanning وفحص denylist للقيم المعروفة. |
| Bootstrap Deadlock | bootstrap يحتاج DB والـ DB configuration تفشل. | لا يمكن إنشاء أول admin. | منخفضة إذا صمم fail-fast بوضوح. | فشل مبكر قبل UI مع دليل إعداد. |
| Secret Leakage Recurrence | إعادة إدخال password في App.config أو factory. | عودة الثغرة. | متوسطة. | SAST/secret scanning، review checklist، منع `Password=og2026ra`. |
| Production Drift | اختلاف إعداد الإنتاج عن التطوير. | فشل عند النشر فقط. | متوسطة. | CI smoke test مع نفس نمط الحقن. |
| Privileged Account Overuse | استمرار استخدام `sa`. | تضخم أثر أي تسريب لاحق. | عالية إن لم يتغير الحساب. | خطة لاحقة لإنشاء SQL login محدود الصلاحيات. |

---

## 5. سيناريوهات التحقق المستقبلية

هذه سيناريوهات تحقق مستقبلية وليست اختبارات منفذة حالياً.

### 5.1 سيناريو النجاح

الشروط:

- لا توجد كلمة `og2026ra` في ملفات الإنتاج.
- لا يوجد fallback password في `OpenLabDbContext` أو factory.
- `OPENLAB_DB_PASSWORD` مضبوط بقيمة صحيحة.
- App.config لا يحتوي password صريح.

المتوقع:

- التطبيق يبدأ.
- `ShowStartupWindow` يستطيع استدعاء `IsBootstrapRequiredAsync`.
- إذا كانت القاعدة فارغة أو تحتاج bootstrap، تظهر نافذة bootstrap.
- إذا كان bootstrap مكتمل، تظهر login.
- لا تظهر connection string أو password في الرسائل أو logs.

ما يجب مراقبته:

- عدم وجود SQL login failure بسبب بناء connection string خاطئ.
- عدم وجود DI resolution failure.
- عدم وجود استثناء داخل `async void ShowStartupWindow` دون عرض واضح.

### 5.2 سيناريو الفشل الآمن

الشروط:

- `OPENLAB_CONNECTION` غير مضبوط.
- `OPENLAB_DB_PASSWORD` غير مضبوط أو فارغ.
- App.config لا يحتوي password.

المتوقع:

- يفشل التطبيق قبل الاتصال بقاعدة البيانات.
- نوع الفشل `InvalidOperationException`.
- الرسالة تشير إلى `OPENLAB_DB_PASSWORD` أو secure `OPENLAB_CONNECTION`.
- لا يتم استخدام `og2026ra`.
- لا يتم إنشاء session جزئية.
- لا يتم عرض bootstrap كأنه يمكن المتابعة.

معيار النجاح الأمني:

- الفشل واضح ومقصود، وليس SQL exception متأخر أو NullReference.
- لا تظهر أسرار في الرسالة.

### 5.3 سيناريو الاختبارات الآلية

المتوقع:

- `Open_lab.Tests` يعمل دون `OPENLAB_DB_PASSWORD` لأن سياقات InMemory تمرر `DbContextOptions`.
- `Open_lab.IntegrationTests` يعمل دون `OPENLAB_DB_PASSWORD` لأن SQLite in-memory يمرر `DbContextOptions`.
- اختبار design-time factory بدون secret يفشل fail-fast.
- اختبار design-time factory مع secret dummy في بيئة test يبني connection string دون fallback.
- secret scanner يفشل إذا عادت `og2026ra` أو أي fallback credential إلى ملفات الإنتاج.

### 5.4 سيناريو CI/CD

المتوقع:

- unit tests لا تحتاج secrets.
- integration tests الافتراضية لا تحتاج secrets.
- SQL Server smoke test يعمل فقط في job يملك secret injection.
- CI لا يطبع قيمة `OPENLAB_DB_PASSWORD`.
- أي migration command يستخدم factory يحصل على secret من CI secret store.

---

## 6. إطار منع Regression

### 6.1 قواعد ثابتة للمراجعة

- أي تعديل في `App.config`, `OpenLabDbContext`, `OpenLabDbContextFactory`, أو `App.xaml.cs` يتطلب مراجعة أمنية.
- يمنع hardcoding لأي قيمة password.
- يمنع fallback credentials حتى لو كانت "للتطوير".
- يمنع استخدام `sa` كخيار إنتاجي طويل الأجل دون مبرر وحوكمة.
- يجب تدوير السر الحقيقي بعد إزالة ظهوره من الكود.

### 6.2 Static Analysis Focus

أنماط يجب فحصها مستقبلاً:

- `Password=`
- `User ID=sa`
- `OPENLAB_DB_PASSWORD") ??`
- `GetEnvironmentVariable(...) ?? "..."`
- `og2026ra`
- connection strings كاملة داخل ملفات `.cs`

يجب التفريق بين الوثائق التي تذكر الثغرة لأغراض audit وبين كود الإنتاج. لكن من منظور secure repository، يفضل لاحقاً نقل السر التاريخي إلى صيغة masked مثل `og****ra` في الوثائق الجديدة بعد اعتماد التقرير، مع إبقاء traceability في سجلات التدقيق الداخلية فقط.

### 6.3 Secure Runtime Initialization

التصميم الآمن للتهيئة يجب أن يحقق:

- validation قبل الاتصال.
- نقطة resolution واحدة أو منطق مشترك بين runtime وdesign-time.
- عدم ربط bootstrap ببدائل سرية.
- عدم تسريب connection string.
- رسائل أخطاء موجهة للمطورين والمشغلين.

---

## 7. قرار معماري مقترح

القرار المقترح لمرحلة التنفيذ اللاحقة:

1. إزالة كلمة المرور الصريحة من `App.config`.
2. إزالة fallback `?? "og2026ra"` من `OpenLabDbContext`.
3. إزالة fallback `?? "og2026ra"` من `OpenLabDbContextFactory`.
4. جعل غياب `OPENLAB_DB_PASSWORD` أو `OPENLAB_CONNECTION` الآمن يؤدي إلى `InvalidOperationException`.
5. إبقاء unit/integration tests الافتراضية معزولة عن SQL Server الحقيقي.
6. إضافة فحوصات static تمنع عودة السر.
7. تدوير كلمة مرور SQL Server الفعلية وإنشاء حساب أقل صلاحية من `sa` كتحسين إنتاجي لاحق.

هذا القرار يوازن بين إزالة الخطر المباشر وتقليل regression. الاعتماد الأولي على environment variables مناسب للمشروع الحالي لأنه يتطلب أقل تغيير معماري، ويحافظ على قابلية تشغيل الاختبارات المعزولة، ويمهد لاحقاً للانتقال إلى secret store أكثر صرامة.

---

## 8. خلاصة الاعتماد

الثغرة F8 ليست مجرد قيمة منسية في `App.config`، بل نمط configuration غير آمن يمتد إلى runtime وdesign-time factory. الخطر ناتج عن ثلاثة عوامل مجتمعة: وجود السر في المستودع، استخدام حساب `sa` عالي الامتياز، ووجود fallback يعيد إدخال السر عند غياب الإعداد.

الإصلاح الآمن يجب أن يكون fail-fast وليس fallback-based. يجب أن يفشل التطبيق بوضوح عند غياب الأسرار، وأن تبقى الاختبارات الافتراضية مستقلة عن الأسرار عبر InMemory/SQLite. كما يجب أن تتضمن مرحلة التنفيذ اللاحقة آلية منع regression، لأن حذف السر مرة واحدة لا يمنع عودته.

هذه الوثيقة تعتمد فقط خطة التحليل والإصلاح المستقبلي. لا تتضمن تنفيذ كود، ولا migration، ولا تغييراً تشغيلياً، ولا تفترض نجاح الإصلاح قبل المرور بسيناريوهات التحقق المذكورة.
