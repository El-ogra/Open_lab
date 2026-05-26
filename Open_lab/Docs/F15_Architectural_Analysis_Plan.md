# وثيقة اعتماد هندسي وتحليل أمني معماري لمعالجة F15

**الثغرة المختارة:** F15 — استخدام SHA-256 أحادي الجولة لتخزين كلمات المرور  
**نوع الوثيقة:** تحليل معماري وتخطيط إصلاح فقط  
**حالة التنفيذ:** لا تتضمن هذه الوثيقة أي تعديل برمجي على كود الإنتاج أو الاختبارات أو الإعدادات  
**السياق الحالي:** تم اعتبار F8 وF9 وF6 مغلقة، ويستند هذا التقرير إلى الوضع المحلي بعد إزالة مسار plaintext ومنع credentials الفارغة  

---

## 1. مبررات اختيار الثغرة الفنية

### 1.1 وصف الثغرة

F15 هي اعتماد النظام على `SHA-256` أحادي الجولة مع salt لتخزين كلمات مرور المستخدمين وكلمة مرور النظام الرئيسية. رغم وجود salt واستخدام `CryptographicOperations.FixedTimeEquals` في التحقق، فإن SHA-256 خوارزمية hashing عامة وسريعة جداً، وليست KDF مخصصة لكلمات المرور. في حال تسرب قاعدة البيانات أو نسخ احتياطية منها، تصبح تكلفة التخمين offline منخفضة مقارنة بخوارزميات مثل PBKDF2 أو Argon2 أو bcrypt.

المشكلة ليست أن SHA-256 "مكسور" كخوارزمية hash عامة، بل أنه غير مناسب وحده لكلمات المرور لأنه مصمم للسرعة. كلمة مرور ضعيفة أو متوسطة يمكن اختبارها بملايين أو مليارات المحاولات حسب العتاد.

### 1.2 لماذا F15 هي الأولوية التالية بعد F8 وF9 وF6

بعد إغلاق الثغرات الثلاث الأخيرة:

- F8 أزال الأسرار hardcoded من مسار قاعدة البيانات.
- F9 وضع تخويلاً داخل `UserAdminService`.
- F6 أزال قبول plaintext ومنع إنشاء hash/salt فارغين.

أصبح مسار كلمات المرور "صحيحاً وظيفياً" لكنه ما زال ضعيفاً من منظور مقاومة التسريب. لذلك يصبح F15 هو الترتيب الطبيعي التالي للأسباب التالية:

- لا يصح تنفيذ lockout أو auth outcomes قبل معالجة ضعف التخزين الأساسي إذا كان threat model يشمل database compromise.
- إصلاح F6 مهّد لـ F15 بإزالة legacy plaintext path. لو بقي F6 موجوداً، لكان دعم multi-version hash أكثر تعقيداً وخطورة.
- F9 يحمي عمليات تغيير كلمات المرور داخل `UserAdminService`، لكن لا يغير جودة hash الناتج.
- F8 يقلل خطر تسرب DB عبر credential hardcoding، لكنه لا يلغي احتمال تسرب backup أو وصول داخلي أو compromise. F15 يقلل أثر أي تسرب لاحق.
- F15 يمس مسارين مهمين: كلمات مرور المستخدمين وmaster password في `SystemSettingsService`.

### 1.3 الاعتماديات المعمارية مع الثغرات المتبقية

| الثغرة | العلاقة بـ F15 |
|---|---|
| F10 | Audit لا يؤثر على تخزين كلمة المرور، لكن بعد F15 يمكن للـ audit تسجيل password reset/migration events بشكل أفضل لاحقاً. |
| F12/F14 | منطق admin-on-login يمكن عزله لاحقاً؛ F15 يحسن تخزين كلمات مرور كل المستخدمين بغض النظر عن سياسات admin. |
| F13 | توسيع `IAuthService` إلى outcomes وlockout سيكون أسهل بعد وجود versioned KDF؛ لكنه قد يتداخل إن نُفذ قبله. |
| F16 | إعادة تعريف admin authority مستقلة عن KDF. |
| F5 | تحسين session setters لا يغير طريقة تخزين كلمة المرور. |

### 1.4 قرار الأولوية

القرار المقترح: اختيار **F15** كالثغرة التالية، لأنها تكمل سلسلة تأمين كلمة المرور بعد إزالة plaintext والفراغ، وتقلل أثر تسرب قاعدة البيانات، وتؤسس لعمل F13 لاحقاً على auth outcomes والlockout فوق أساس تخزين آمن.

---

## 2. الفحص والتحليل البرمجي للوضع الحالي

### 2.1 الملفات والدوال المتأثرة

| الملف | الدالة/العنصر | المشكلة |
|---|---|---|
| `Open_lab/Services/PasswordSecurity.cs` | `ComputeSha256` | يستخدم SHA-256 سريعاً أحادي الجولة. |
| `Open_lab/Services/PasswordSecurity.cs` | `Verify` | يتحقق دائماً عبر SHA-256 فقط، دون versioning. |
| `Open_lab/Services/AuthService.cs` | `ValidateCredentialsAsync` | يعتمد على `PasswordSecurity.Verify` فقط، ولا يدعم hash versions. |
| `Open_lab/Services/UserAdminService.cs` | `ApplyPassword` | يكتب hashes جديدة بصيغة SHA-256 الحالية. |
| `Open_lab/Services/SystemSettingsService.cs` | `SaveProfileAsync`, `SetMasterPasswordAsync`, `VerifyMasterPasswordAsync` | يستخدم نفس SHA-256 للـ master password. |
| `Open_lab/Models/Entities.cs` | `User` | لا يحتوي `HashVersion` أو metadata تحدد نوع KDF. |
| `Open_lab/Data/OpenLabDbContext.cs` | `OnModelCreating.User` | لا يعرّف عموداً لإصدار hash أو قيوداً مرتبطة به. |
| `Open_lab.Tests/Services/PasswordSecurityTests.cs` | اختبارات hash الحالية | تفترض SHA-256 وطول 64 hex. |

### 2.2 موضع الضعف في `PasswordSecurity`

```csharp
public static string ComputeSha256(string password, string salt)
{
    using var sha = SHA256.Create();
    var bytes = Encoding.UTF8.GetBytes(password + "::" + salt);
    var hash = sha.ComputeHash(bytes);
    return Convert.ToHexString(hash);
}
```

التحليل الأمني:

- salt موجود، وهذا يمنع rainbow table العامة، لكنه لا يجعل الخوارزمية مقاومة للتخمين عالي السرعة.
- لا توجد iterations.
- لا توجد ذاكرة عالية أو تكلفة قابلة للضبط.
- لا توجد metadata تحدد أن hash هذا legacy أو modern.

### 2.3 موضع الضعف في `Verify`

```csharp
public static bool Verify(string password, string salt, string expectedHash)
{
    var computed = ComputeSha256(password, salt);
    var left = Encoding.UTF8.GetBytes(computed);
    var right = Encoding.UTF8.GetBytes(expectedHash);
    return CryptographicOperations.FixedTimeEquals(left, right);
}
```

التحليل الأمني:

- `FixedTimeEquals` جيد من ناحية timing، لكنه لا يعالج ضعف KDF.
- الدالة لا تعرف hash version.
- أي تغيير مباشر إلى PBKDF2 دون خطة versioning سيكسر جميع المستخدمين الحاليين.

### 2.4 مسار المستخدمين في `AuthService` و`UserAdminService`

`AuthService` حالياً يعتمد على:

```csharp
if (PasswordSecurity.Verify(password, user.Salt, user.PasswordHash))
{
    return user;
}
```

`UserAdminService.ApplyPassword` حالياً ينتج:

```csharp
var salt = PasswordSecurity.GenerateSalt();
user.Salt = salt;
user.PasswordHash = PasswordSecurity.ComputeSha256(plainPassword, salt);
```

التحليل:

- إنشاء المستخدمين وتغيير كلماتهم ينتج SHA-256 legacy مباشرة.
- login لا يملك آلية rehash-on-login.
- لا يوجد fallback آمن لترقية hash بعد نجاح التحقق.

### 2.5 مسار master password في `SystemSettingsService`

```csharp
var newHash = PasswordSecurity.ComputeSha256(newPassword, newSalt);
...
return PasswordSecurity.Verify(password, salt, hash);
```

التحليل:

- كلمة مرور النظام الرئيسية تتبع نفس الضعف.
- تخزينها داخل `Settings` يختلف عن جدول `Users`، لذلك versioning قد يحتاج مفاتيح إعدادات جديدة مثل `Security.MasterPasswordHashVersion` أو encoding مضمّن داخل قيمة hash.
- تجاهل master password في خطة F15 سيترك ثغرة موازية بنفس السبب.

### 2.6 بنية `User` الحالية

```csharp
public class User
{
    public string PasswordHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
}
```

التحليل:

- لا يوجد `HashVersion`.
- لا يوجد `PasswordHashAlgorithm`.
- لا يوجد تاريخ آخر rehash أو metadata للترحيل.
- إضافة PBKDF2 دون عمود أو encoding داخلي ستجعل التمييز بين SHA-256 وPBKDF2 غير موثوق.

---

## 3. الخطة المعمارية المقترحة للإصلاح

هذه خطة تصميم فقط وليست تنفيذ.

### 3.1 الهدف الأمني

الهدف هو الانتقال من SHA-256 أحادي الجولة إلى KDF مخصص لكلمات المرور مع:

- دعم آمن للمستخدمين الحاليين.
- عدم كسر login.
- ترقية تدريجية عند نجاح المصادقة.
- عدم إرجاع plaintext أو fallback.
- بقاء F6 مغلقاً: لا plaintext ولا salt فارغ.

### 3.2 اختيار الخوارزمية

الخيار العملي داخل .NET دون إضافة dependency خارجية هو PBKDF2 عبر `Rfc2898DeriveBytes` مع SHA-256 أو SHA-512.

التوصية الأولية:

- الخوارزمية: PBKDF2-HMAC-SHA256.
- salt: 32 bytes random على الأقل.
- hash length: 32 bytes أو 64 bytes.
- iterations: قيمة قابلة للضبط، تبدأ مبدئياً من 210,000 إلى 600,000 حسب benchmark على أجهزة التشغيل الفعلية.

يجب ألا تُختار قيمة iterations نهائياً بلا benchmark محلي، لأن WPF desktop على أجهزة مختبرات قد يعمل على عتاد ضعيف. الهدف الواقعي أن تكون عملية login ضمن زمن مقبول، مثلاً 200-500ms، مع حماية كافية.

### 3.3 تصميم versioning

هناك خياران:

| الخيار | المزايا | العيوب |
|---|---|---|
| عمود `HashVersion` في جدول `Users` | واضح، سهل الاستعلام والترحيل. | يحتاج migration وتحديث snapshot والاختبارات. |
| Hash string بصيغة self-describing مثل `pbkdf2$sha256$iterations$salt$hash` | لا يحتاج أعمدة كثيرة، أسهل مستقبلاً. | يغير معنى `Salt` الحالي وقد يحتاج تنظيفاً معمارياً. |

التوصية للمشروع الحالي:

- إضافة `HashVersion` للمستخدمين كمرحلة أولى محافظة.
- جعل `HashVersion = 1` للـ SHA-256 الحالي.
- جعل `HashVersion = 2` للـ PBKDF2.
- إبقاء `Salt` منفصلاً لتقليل churn.
- كتابة hashes الجديدة دائماً بـ version 2.

بالنسبة للـ master password:

- إما إضافة setting جديد `Security.MasterPasswordHashVersion`.
- أو ترميز version داخل قيمة hash.

التوصية: استخدام setting منفصل لتقليل تغيير contract الحالي:

- `Security.MasterPasswordHashVersion = 1 | 2`
- القيمة غير الموجودة تعني `1` للبيانات القديمة.

### 3.4 تعديل `PasswordSecurity`

الخطة لاحقاً:

- إبقاء دالة SHA-256 للاستخدام legacy الداخلي فقط، لا كمسار إنشاء جديد.
- إضافة دالة توليد PBKDF2.
- إضافة verify يقبل version.
- إضافة helper يحدد هل hash يحتاج upgrade.
- توحيد salt generation على 32 bytes، أو إبقاء 16 bytes legacy و32 bytes للجديد.

لا ينبغي حذف SHA-256 فوراً لأن المستخدمين الحاليين يحتاجون تحققاً مرة واحدة للترقية. الحذف الكامل يمكن أن يكون مرحلة لاحقة بعد انتهاء فترة ترحيل.

### 3.5 تعديل `AuthService`

الخطة:

1. تحميل المستخدم كالمعتاد.
2. رفض inactive أو missing hash/salt كما يفعل F6.
3. تحديد `HashVersion`.
4. التحقق بالخوارزمية المناسبة.
5. إذا نجح التحقق وكان version legacy:
   - توليد salt جديد.
   - حساب PBKDF2.
   - تحديث `PasswordHash`, `Salt`, `HashVersion`.
   - حفظ التغيير.
6. إرجاع المستخدم بعد الترقية.

هذا يحقق rehash-on-login دون مطالبة كل المستخدمين بتغيير كلمة المرور فوراً.

### 3.6 تعديل `UserAdminService`

الخطة:

- `CreateUserAsync` يكتب PBKDF2 version 2 دائماً.
- `UpdateUserAsync` عند تغيير كلمة المرور يكتب PBKDF2 version 2.
- عند عدم تغيير كلمة المرور، لا يغير hash ولا version.
- يحافظ على حراسة F9 كما هي.
- لا يعيد إدخال أي مسار فارغ أو plaintext، حفاظاً على F6.

### 3.7 تعديل `SystemSettingsService`

الخطة:

- `SetMasterPasswordAsync` و`SaveProfileAsync` يكتبان PBKDF2 version 2.
- `VerifyMasterPasswordAsync` يدعم version 1 و2.
- عند نجاح verify على version 1، يمكن ترقية master password إلى version 2 إذا كانت كلمة المرور متاحة في الذاكرة أثناء التحقق.
- يجب توخي الحذر في `SaveProfileAsync` لأن الحقل `MasterPasswordHash` اسمه مضلل وظيفياً، إذ يبدو أنه يحمل كلمة مرور جديدة من UI وليس hashاً جاهزاً. يجب توثيق هذا قبل التنفيذ أو إعادة تسميته لاحقاً ضمن refactor منفصل.

### 3.8 EF Migration

التنفيذ اللاحق سيحتاج migration:

- إضافة `HashVersion` إلى `Users` بقيمة default `1`.
- تحديث `OpenLabDbContextModelSnapshot`.
- عدم تعديل `PasswordHash` الحالي داخل migration، لأن كلمة المرور الأصلية غير معروفة.
- لا توجد حاجة لتعديل جميع hashes أثناء migration؛ الترقية تتم عند login أو reset.

### 3.9 ما يجب عدم فعله

- عدم إجبار جميع المستخدمين على reset فوري دون خطة تشغيلية.
- عدم كسر SHA-256 verification مباشرة قبل وجود migration وترقية.
- عدم إعادة فتح مسار plaintext الذي أغلقه F6.
- عدم تجاوز حراسة F9 لتحديث كلمات المرور.
- عدم إدخال dependency cryptography خارجية دون حاجة.
- عدم رفع iterations لقيمة تسبب تجميد UI أو timeout في الاختبارات.

---

## 4. ضمان عدم الانهيار البرمجي والاستقرار

### 4.1 حماية F8

F15 لا يجب أن يلمس:

- `App.config`
- `OpenLabDbContextFactory`
- منطق `OPENLAB_DB_PASSWORD`
- منطق `OPENLAB_CONNECTION`

التحقق: build واختبارات EF/InMemory/SQLite يجب أن تستمر دون متغيرات DB للأغلب، كما هو الحال بعد F8.

### 4.2 حماية F9

F15 يلمس `UserAdminService.ApplyPassword` لكنه يجب أن يحافظ على:

- `EnsureUsersEditPermission()` في بداية عمليات الكتابة.
- `IsSystemOperation` المؤقت في Bootstrap وCLI.
- عدم إضافة bypass جديد لتغيير كلمات المرور.

أي اختبار جديد لـ `UserAdminService` يجب أن يستخدم جلسة admin كما في الوضع الحالي بعد F9.

### 4.3 حماية F6

يجب الحفاظ على:

- رفض hash/salt الفارغين في `AuthService`.
- رفض إنشاء مستخدم بلا كلمة مرور.
- عدم إعادة أي test يقبل legacy plaintext.
- عدم استخدام SHA-256 كـ fallback عندما يكون salt فارغاً.

F15 يجب أن يدعم legacy SHA-256 فقط عندما يكون hash/salt مكتملين و`HashVersion=1`.

### 4.4 Bootstrap

Bootstrap ينشئ admin عبر `UserAdminService.CreateUserAsync` بكلمة مرور قوية. بعد F15:

- يجب أن ينتج admin الجديد hash version 2.
- يجب ألا يحتاج bootstrap إلى migration مسبق إذا كانت قاعدة البيانات جديدة.
- يجب أن يبقى `EnsureAdminAccessAsync` كما هو من ناحية الأدوار، ولا يختلط بإصلاح KDF.

### 4.5 AdminCli

AdminCli reset يجب أن:

- يستمر في validation الحالي لكلمة المرور.
- يكتب PBKDF2 version 2 عند reset.
- يحافظ على `IsSystemOperation` المؤقت من F9.
- لا يحتاج إلى معرفة تفاصيل KDF إذا بقي `UserAdminService` هو نقطة كتابة كلمة المرور.

### 4.6 الاختبارات الآلية

الاختبارات المتأثرة غالباً:

- `PasswordSecurityTests`: يجب تحويلها من افتراض SHA-256 فقط إلى اختبار PBKDF2 + legacy verify.
- `AuthServiceTests`: إضافة/تعديل اختبارات rehash-on-login.
- `UserAdminServiceTests`: توقع `HashVersion=2` للمستخدمين الجدد بعد إضافة العمود.
- `SystemSettingsServiceTests` و`Module13ServiceTests_Additional`: تحديث توقعات master password حسب version.
- أي اختبارات تنشئ `User { PasswordHash = "hash", Salt = "salt" }` قد لا تتطلب login، ويمكن تركها إن لم تستخدم auth. أما اختبارات login يجب أن تستخدم hash صالحاً متوافقاً مع version.

استراتيجية الحفاظ على 1594 اختباراً:

1. إضافة tests جديدة قبل/مع التنفيذ.
2. إبقاء compatibility لـ SHA-256 version 1 حتى تمر اختبارات المستخدمين الحاليين.
3. عدم تشغيل migration على InMemory tests إلا عند الحاجة.
4. تنفيذ migration بطريقة لا تكسر SQLite integration.
5. قياس زمن اختبارات auth إذا كانت PBKDF2 iterations عالية، واستخدام إعداد test iterations منخفض فقط إن كان التصميم يسمح بذلك بأمان دون تسرب للإنتاج.

### 4.7 الأداء وتجربة المستخدم

PBKDF2 سيرفع زمن login وreset password. يجب:

- قياس زمن `Verify` و`HashPassword`.
- عدم تشغيل hash ثقيل على UI thread إذا ظهر freeze ملحوظ.
- إبقاء الزمن ضمن حد مقبول.
- التفكير في spinner الحالي عبر `IsBusy` في LoginViewModel، وهو موجود بالفعل.

---

## 5. سيناريوهات التحقق المستقبلية

### 5.1 سيناريو النجاح

بعد التنفيذ لاحقاً:

- إنشاء مستخدم جديد ينتج `HashVersion=2`.
- login لمستخدم version 2 ينجح بكلمة صحيحة ويفشل بكلمة خاطئة.
- reset كلمة مرور عبر AdminCli ينتج version 2.
- bootstrap admin في قاعدة جديدة ينتج version 2.
- master password الجديد يستخدم version 2.

### 5.2 سيناريو ترقية legacy

المدخل:

- مستخدم قديم لديه SHA-256 hash صالح و`HashVersion=1`.

المتوقع:

- أول login صحيح ينجح.
- يتم تحديث `PasswordHash`, `Salt`, `HashVersion=2`.
- login التالي يستخدم PBKDF2.
- كلمة المرور الخاطئة لا ترقي ولا تعدل السجل.

### 5.3 سيناريو الفشل الآمن

محاولات يجب أن تفشل:

- hash/salt فارغان: يرجع login `null` كما في F6.
- `HashVersion` غير معروف: يفشل login بأمان دون استثناء UI عشوائي.
- hash malformed: يفشل verify بأمان.
- master password بلا version وبلا salt: يرجع false.

### 5.4 سيناريو الأداء

- قياس hash/verify عند iterations المقترحة على جهاز تطوير متوسط.
- قياس login end-to-end.
- التأكد من أن suite لا تصبح بطيئة جداً.
- إن استُخدم test configuration للiterations، يجب أن يكون واضحاً ومحصوراً بالاختبارات.

### 5.5 سيناريو عدم كسر F8/F9/F6

- F8: تشغيل build/tests دون إدخال secrets جديدة في config.
- F9: محاولة `UserAdminService.CreateUserAsync` دون `UsersEdit` ما زالت تفشل.
- F6: محاولة مستخدم legacy plaintext بلا salt ما زالت تفشل ولا تُرقى.

---

## 6. قرار معماري مقترح

القرار المقترح لمرحلة التنفيذ اللاحقة:

1. اختيار F15 كالإصلاح التالي.
2. اعتماد PBKDF2-HMAC-SHA256 كخوارزمية KDF مبدئية.
3. إضافة versioning لكلمات مرور المستخدمين عبر `HashVersion`.
4. إضافة versioning للـ master password عبر setting منفصل.
5. جعل كل كلمات المرور الجديدة version 2.
6. دعم SHA-256 version 1 للتحقق فقط، مع rehash-on-success.
7. عدم إعادة أي plaintext أو empty credential path.
8. تأجيل lockout/auth outcomes إلى F13 بعد اكتمال KDF.

---

## 7. خلاصة الاعتماد

F15 هي الخطوة التالية الأكثر منطقية بعد F8 وF9 وF6 لأن مسار كلمة المرور أصبح الآن نظيفاً من الأسرار hardcoded ومن تجاوزات الخدمة ومن plaintext/empty credentials، لكنه ما زال يعتمد على hash سريع غير مخصص لكلمات المرور. معالجة F15 ستنقل النظام من "صحة منطقية" إلى "مقاومة أفضل لتسرب قاعدة البيانات".

الخطة يجب أن تكون versioned وتدريجية، لا تغييراً قاطعاً يكسر المستخدمين. كما يجب أن تشمل master password، لا كلمات مرور المستخدمين فقط، لأن الاثنين يستخدمان `PasswordSecurity` نفسه حالياً.

هذه الوثيقة تخطيطية فقط. لم يتم تعديل أي كود إنتاجي أو اختبار أو إعدادات ضمن هذه المرحلة.
