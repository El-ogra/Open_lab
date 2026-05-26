# وثيقة اعتماد هندسي وتحليل أمني معماري لمعالجة F6

**الثغرة المختارة:** F6 — مسار كلمات المرور النصية القديمة داخل `AuthService` عند غياب `Salt`  
**نوع الوثيقة:** تحليل معماري وتخطيط إصلاح فقط  
**حالة التنفيذ:** لا تتضمن هذه الوثيقة أي تعديل برمجي على كود الإنتاج أو الاختبارات أو الإعدادات  
**السياق الحالي:** تم اعتبار F8 وF9 مغلقتين، ويستند هذا التقرير إلى الوضع المحلي بعد إدخال حراسة `UserAdminService` بـ `ISessionContext`  

---

## 1. مبررات اختيار الثغرة الفنية

### 1.1 وصف الثغرة

F6 هي بقاء مسار توافق قديم داخل `AuthService.ValidateCredentialsAsync` يقبل المستخدم الذي لا يملك `Salt` باعتباره مستخدماً بكلمة مرور نصية مخزنة في `PasswordHash`. عند نجاح المقارنة النصية، يقوم المسار بترقية كلمة المرور إلى hash جديد.

هذا المسار كان ظاهرياً هدفه ترحيل بيانات قديمة، لكنه أصبح خطراً لأن `UserAdminService.ApplyPassword` ما زال يكتب:

- `PasswordHash = string.Empty`
- `Salt = string.Empty`

عندما تنشأ أو تعدل حسابات دون كلمة مرور. النتيجة أن النظام يستطيع تخزين حالة مستخدم غير صالحة أمنياً، ثم يتعامل `AuthService` معها كحالة legacy قابلة للمصادقة النصية.

### 1.2 لماذا F6 هي الأولوية التالية بعد F8 وF9

بعد إغلاق F8، لم تعد كلمة مرور قاعدة البيانات hardcoded. وبعد إغلاق F9، لم تعد عمليات إدارة المستخدمين متاحة من الخدمة دون جلسة مخولة أو عملية نظامية. لذلك تصبح F6 هي أعلى خطر متبقٍ للأسباب التالية:

- هي الثغرة المتبقية الوحيدة المصنفة High في مسار المصادقة المباشر بعد استبعاد F8 وF9.
- F9 قللت سطح الاستغلال، لكنها لم تلغ الحالة غير الآمنة نفسها. أي مستخدم يملك `UsersEdit` أو مسار system operation يستطيع أن يخلق مستخدماً بلا كلمة مرور إذا بقي `ApplyPassword` كما هو.
- F6 تقف قبل F15 منطقياً. لا يصح ترقية KDF إلى PBKDF2 بينما ما زال هناك مسار قبول كلمات مرور نصية أو حسابات بلا salt.
- F6 تمس boundary المصادقة نفسه. أي قبول لسجل user بدون salt كـ plaintext هو استثناء خطر يجب أن يكون منتهياً أو محصوراً بعملية migration منفصلة ومراقبة.
- الاختبارات الحالية تثبت السلوك القديم كأنه مطلوب، لذلك لا يمكن اعتبار الإصلاح بسيطاً؛ يجب عكس contract الاختبارات بشكل واعٍ.

### 1.3 علاقة F6 بالإصلاحات الأخيرة

| الإصلاح السابق | علاقته بـ F6 |
|---|---|
| F8 | أزال خطر الأسرار من configuration. F6 الآن خطر مصادقة داخلي مستقل عن اتصال قاعدة البيانات. |
| F9 | أضاف حراسة على `UserAdminService`، لكنه يسمح للمستخدم المخول أو `IsSystemOperation` بالوصول إلى `CreateUserAsync`. لذلك يجب أن تمنع الخدمة نفسها إنشاء حسابات بلا كلمة مرور. |
| Bootstrap | يستخدم `UserAdminService.CreateUserAsync` بعملية نظامية. يجب ألا يسمح bootstrap بكلمة مرور فارغة، وهو حالياً يملك validation قوية بطول 12 حرفاً ورمزاً. |
| AdminCli | يستخدم `UserAdminService.UpdateUserAsync` بعملية نظامية. يجب أن يبقى مسموحاً بكلمة مرور قوية فقط، وهو حالياً يتحقق من سياسة كلمة المرور قبل الاستدعاء. |

### 1.4 مقارنة F6 بالثغرات المتبقية الأخرى

| الثغرة | سبب عدم اختيارها الآن |
|---|---|
| F5 | مهمة، لكنها تتعلق بتحسين عزل `SessionContext` وتضييق setters. F6 أكثر مباشرة في مسار login. |
| F10 | يؤثر على audit attribution، لكنه لا يسمح بذاته بالمصادقة غير الآمنة. |
| F12/F14 | مهمة لإزالة username-as-policy، لكنها متعلقة بتجديد صلاحيات admin بعد login. F6 يعالج قبول كلمة مرور غير آمنة قبل إنشاء الجلسة. |
| F13 | توسيع auth outcomes والlockout مهم، لكنه أكبر وأفضل بعد تنظيف مسارات كلمة المرور غير الصالحة. |
| F15 | ترقية KDF ضرورية، لكنها تعتمد على وجود نموذج كلمات مرور صحيح وخالٍ من plaintext legacy. |
| F16 | يعيد تعريف admin authority، لكنه لا يغلق مسار كلمة المرور النصية. |

---

## 2. الفحص والتحليل البرمجي للوضع الحالي

### 2.1 الملفات والدوال المتأثرة

| الملف | الكلاس | الدالة | الدور في الثغرة |
|---|---|---|---|
| `Open_lab/Services/AuthService.cs` | `AuthService` | `ValidateCredentialsAsync` | يقبل `Salt` الفارغ كمسار plaintext legacy. |
| `Open_lab/Services/UserAdminService.cs` | `UserAdminService` | `CreateUserAsync`, `UpdateUserAsync`, `ApplyPassword` | يكتب `PasswordHash` و`Salt` فارغين عند غياب كلمة المرور. |
| `Open_lab/Services/PasswordSecurity.cs` | `PasswordSecurity` | `ComputeSha256`, `Verify` | المسار الحديث يعتمد على salt/hash. |
| `Open_lab/ViewModels/LoginViewModel.cs` | `LoginViewModel` | `LoginAsync` | يمنع إدخال كلمة مرور فارغة عبر UI، لكنه ليس boundary كافياً وحده. |
| `Open_lab.Tests/Services/AuthServiceTests.cs` | `AuthServiceTests` | `ValidateCredentials_WithLegacyPlainText_ShouldMigrateToHash` | يثبت قبول plaintext legacy حالياً. |
| `Open_lab.Tests/Services/UserAdminServiceTests.cs` | `UserAdminServiceTests` | `CreateUser_WithNullPassword_ShouldCreateUserWithEmptyHash` | يثبت إنشاء user بحالة hash/salt فارغة حالياً. |
| `Open_lab.Tests/Services/FunctionCoverageGapTests.cs` | `FunctionCoverageGapTests` | `AuthService_ValidateCredentialsAsync_With_LegacyPassword_Should_MigrateSaltAndHash_EdgeGuard` | يثبت مسار migration القديم. |

### 2.2 موضع الضعف في `AuthService`

الموضع التقريبي: `Open_lab/Services/AuthService.cs:31-44`

```csharp
var hasSalt = !string.IsNullOrWhiteSpace(user.Salt);
if (!hasSalt)
{
    if (!string.Equals(user.PasswordHash, password, StringComparison.Ordinal))
    {
        return null;
    }

    var salt = PasswordSecurity.GenerateSalt();
    user.Salt = salt;
    user.PasswordHash = PasswordSecurity.ComputeSha256(password, salt);
    await _db.SaveChangesAsync();
    return user;
}
```

التحليل الأمني:

- غياب `Salt` لا يجب أن يكون حالة مصادقة صالحة في النظام الحالي.
- الكود يعامل `PasswordHash` كأنه كلمة مرور نصية عند غياب salt.
- إذا كان `PasswordHash` فارغاً ومرر caller كلمة مرور فارغة، تصبح المقارنة النصية صحيحة. واجهة login تمنع الفراغ، لكن `AuthService` نفسه لا يفعل.
- هذا يخلط بين runtime authentication وdata migration. ترحيل كلمات المرور القديمة يجب ألا يحدث ضمن مسار login المفتوح بلا حوكمة.

### 2.3 موضع الضعف في `UserAdminService.ApplyPassword`

الموضع التقريبي: `Open_lab/Services/UserAdminService.cs:270-278`

```csharp
private static void ApplyPassword(User user, string? plainPassword)
{
    if (string.IsNullOrWhiteSpace(plainPassword))
    {
        user.PasswordHash = string.Empty;
        user.Salt = string.Empty;
        return;
    }
```

التحليل الأمني:

- الدالة تنشئ حالة بيانات غير صالحة أمنياً بدلاً من رفضها.
- السلوك الحالي يعني أن "لا كلمة مرور" تتحول إلى زوج فارغ `PasswordHash/Salt`.
- هذا الزوج الفارغ هو نفسه الشرط الذي يفتح مسار plaintext legacy في `AuthService`.
- بعد F9، لا يستطيع أي caller غير مخول الوصول إلى هذه الدالة عبر `UserAdminService`، لكن كل caller مخول ما زال يستطيع إنشاء مستخدم بحالة قابلة للاستغلال أو على الأقل حالة غير قابلة للإدارة الآمنة.

### 2.4 مسار Login الحالي

الموضع التقريبي: `Open_lab/ViewModels/LoginViewModel.cs:85-90`

```csharp
if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
{
    StatusMessage = "يرجى إدخال اسم المستخدم وكلمة المرور.";
    return;
}
```

التحليل الأمني:

- هذا guard مفيد لكنه UI-level فقط.
- لا يكفي لحماية `AuthService` من tests أو CLI أو code paths داخلية أو أي استدعاء مباشر.
- إصلاح F6 يجب أن يكون داخل الخدمة، لا داخل `LoginViewModel` فقط.

### 2.5 الاختبارات الحالية التي ستتغير

#### اختبار قبول plaintext legacy

```csharp
public async Task ValidateCredentials_WithLegacyPlainText_ShouldMigrateToHash()
```

هذا الاختبار سيحتاج إلى عكسه ليؤكد أن المستخدم بلا salt لا يستطيع تسجيل الدخول، وأن السجل لا يترقى تلقائياً عبر login.

#### اختبار إنشاء مستخدم بلا كلمة مرور

```csharp
public async Task CreateUser_WithNullPassword_ShouldCreateUserWithEmptyHash()
```

هذا الاختبار يثبت السلوك غير الآمن. يجب عكسه ليصبح رفضاً صريحاً لكلمة مرور فارغة عند إنشاء مستخدم جديد.

#### اختبار gap coverage لمسار legacy

```csharp
AuthService_ValidateCredentialsAsync_With_LegacyPassword_Should_MigrateSaltAndHash_EdgeGuard
```

هذا الاختبار يجب عكسه أو نقله إلى اختبار migration tool منفصل إذا تقرر إنشاء مسار ترحيل خارج login.

---

## 3. الخطة المعمارية المقترحة للإصلاح

هذه خطة تصميم فقط وليست تنفيذ.

### 3.1 الهدف الأمني

الهدف هو جعل الحالة التالية غير صالحة:

- `Salt` فارغ.
- أو `PasswordHash` فارغ.
- أو قبول `PasswordHash` ككلمة مرور نصية.

قاعدة المصادقة الجديدة:

> لا تتم المصادقة إلا على مستخدم نشط يملك `Salt` غير فارغ و`PasswordHash` غير فارغ، ويتم التحقق عبر `PasswordSecurity.Verify` فقط.

### 3.2 تعديل `AuthService`

الخطة لاحقاً:

1. حذف مسار plaintext legacy من `ValidateCredentialsAsync`.
2. عند غياب `Salt` أو `PasswordHash`، إرجاع `null` دون تعديل قاعدة البيانات.
3. عدم إنشاء salt/hash أثناء login.
4. إبقاء inactive-user check قبل أي تحقق كلمة مرور.
5. عدم رمي استثناء في login العادي عند وجود بيانات legacy؛ الفشل يجب أن يكون آمناً وصامتاً من منظور المستخدم، برسالة "بيانات الدخول غير صحيحة".

من المهم عدم كتابة كود migration ضمن login. إذا احتاجت قواعد إنتاجية قديمة إلى ترحيل، يجب أن يكون ذلك عبر مسار إداري صريح، مثل command أو maintenance task، بعد حصر السجلات القديمة، وليس أثناء المصادقة.

### 3.3 تعديل `UserAdminService`

الخطة لاحقاً:

1. في `CreateUserAsync`: يجب رفض `plainPassword` إذا كان null أو whitespace.
2. في `UpdateUserAsync`: التفريق بين حالتين:
   - null أو whitespace يعني "لا تغير كلمة المرور" عند تحديث مستخدم قائم.
   - إذا كان المستخدم الحالي لا يملك hash/salt صالحين، يجب رفض تركه بلا كلمة مرور أو إجبار caller على تمرير كلمة مرور جديدة.
3. في `ApplyPassword`: يجب ألا تكتب `string.Empty` أبداً في `PasswordHash` أو `Salt`.
4. إنشاء helper validation لكلمة المرور أو استخدام سياسة موجودة إن كانت متاحة، مع تجنب نقل كل سياسة Bootstrap إلى service إذا كان ذلك سيغير UX بشكل واسع.

قرار مهم:

- `CreateUserAsync(user, null)` يجب أن يفشل.
- `UpdateUserAsync(existingUser, null)` يمكن أن يستمر كـ "لا تغير كلمة المرور" بشرط أن السجل الحالي لديه `Salt` و`PasswordHash` صالحان.
- `UpdateUserAsync(legacyOrBrokenUser, null)` يجب أن يفشل لأن ترك حساب broken مستمر غير آمن.

### 3.4 التعامل مع بيانات legacy

هناك خياران:

| الخيار | التحليل |
|---|---|
| رفض كل legacy فوراً | أبسط وأأمن. أي مستخدم قديم بلا salt يحتاج reset password عبر Admin UI/CLI. |
| أداة ترحيل منفصلة | أكثر لطفاً للبيانات القديمة، لكنها تحتاج حوكمة لأن plaintext password غير متاح غالباً إلا إذا كان مخزناً في `PasswordHash`. |

التوصية: اعتماد الرفض الفوري داخل login، وتوفير مسار reset password إداري للحسابات المتأثرة. هذا متسق مع وجود `Open_lab.AdminCli` ومع F9 الذي يحمي عمليات الإدارة.

### 3.5 ما يجب عدم فعله

- لا ينبغي إبقاء fallback plaintext ولو بشرط جديد.
- لا ينبغي قبول empty password حتى لأغراض الاختبار.
- لا ينبغي تحويل `PasswordHash=""` إلى hash لكلمة مرور فارغة.
- لا ينبغي خلط إصلاح F6 بترقية PBKDF2 الكاملة الخاصة بـ F15. تنظيف الحالة أولاً، ثم ترقية KDF لاحقاً.
- لا ينبغي كسر F8 بإضافة secrets أو configuration جديدة.
- لا ينبغي تخطي F9 بإضافة system operation حول إصلاح كلمات المرور إلا في مسارات محددة مثل CLI reset.

### 3.6 رسائل الاستثناء المقترحة

لإنشاء مستخدم بلا كلمة مرور:

```text
Password is required when creating a user.
```

لتحديث مستخدم broken بلا كلمة مرور جديدة:

```text
A valid password is required because the existing user credentials are incomplete.
```

يجب أن تبقى رسائل login العامة غير كاشفة:

```text
بيانات الدخول غير صحيحة.
```

---

## 4. ضمان عدم الانهيار البرمجي والاستقرار

### 4.1 حماية إصلاح F8

إصلاح F6 لا يجب أن يلمس:

- `App.config`
- `OpenLabDbContext`
- `OpenLabDbContextFactory`
- متغيرات `OPENLAB_DB_PASSWORD` أو `OPENLAB_CONNECTION`

لذلك لا يوجد سبب معماري لأن يؤثر F6 على اتصال قاعدة البيانات. يجب التحقق بالبناء والاختبارات فقط، ولا حاجة لتغيير configuration.

### 4.2 حماية إصلاح F9

بما أن `UserAdminService` أصبح محمياً بـ `UsersEdit` و`IsSystemOperation`:

- يجب إبقاء هذه الحراسة في بداية دوال الكتابة.
- يجب ألا يضاف bypass جديد لإصلاح F6.
- اختبارات `UserAdminServiceTests` التي تستخدم `AppSessionTestHelper.ResetToAdmin()` يجب أن تبقى كذلك.
- مسارات `BootstrapViewModel` و`AdminCli` يجب أن تبقى system operation مؤقتة فقط، وأن تمرر كلمة مرور قوية لا فارغة.

### 4.3 Bootstrap

Bootstrap حالياً يتحقق من:

- عدم فراغ username.
- تطابق كلمة المرور والتأكيد.
- طول كلمة المرور لا يقل عن 12.
- رفض `"admin123"`.
- احتواء uppercase/lowercase/digit/symbol.

لذلك رفض `CreateUserAsync` لكلمة المرور الفارغة لن يكسر bootstrap الصحيح. سيناريو bootstrap الفاشل بسبب كلمة مرور فارغة سيبقى يفشل في ViewModel قبل الوصول إلى الخدمة.

### 4.4 AdminCli

AdminCli يتحقق من:

- وجود `--new-password`.
- طول كلمة المرور لا يقل عن 12.
- رفض `"admin123"`.
- احتواء uppercase/lowercase/digit/symbol.
- تأكيد `RESET-ADMIN`.

لذلك إصلاح F6 لا يجب أن يكسر reset الصحيح. بل سيضيف طبقة أمان لو حاول caller مستقبلاً تمرير قيمة فارغة من مسار غير CLI validation.

### 4.5 الاختبارات الآلية

الاختبارات التي ستحتاج تغييراً مقصوداً:

- `AuthServiceTests.ValidateCredentials_WithLegacyPlainText_ShouldMigrateToHash`
- `FunctionCoverageGapTests.AuthService_ValidateCredentialsAsync_With_LegacyPassword_Should_MigrateSaltAndHash_EdgeGuard`
- `UserAdminServiceTests.CreateUser_WithNullPassword_ShouldCreateUserWithEmptyHash`

الاختبارات التي يجب ألا تتأثر:

- اختبارات F8 الخاصة بالبناء والاتصال عبر options configured.
- اختبارات F9 التي تستخدم session admin.
- اختبارات LoginViewModel العادية.
- اختبارات BootstrapIntegrationTests.
- اختبارات AdminCli إن وجدت أو help command.

استراتيجية Zero-Regression:

1. عكس الاختبارات التي تثبت السلوك القديم بدلاً من حذفها.
2. إضافة اختبار أن `ValidateCredentialsAsync` يرجع `null` عند `Salt=""`.
3. إضافة اختبار أن `CreateUserAsync` يرفض null/whitespace password.
4. إضافة اختبار أن `UpdateUserAsync(existing, null)` لا يغير hash صالحاً.
5. إضافة اختبار أن `UpdateUserAsync(brokenUser, null)` يفشل.
6. تشغيل كامل الاختبارات: 1584 unit + 10 integration.

---

## 5. سيناريوهات التحقق المستقبلية

### 5.1 سيناريو النجاح

بعد تنفيذ الإصلاح لاحقاً، يجب تحقق ما يلي:

- مستخدم لديه `Salt` و`PasswordHash` صالحان يستطيع تسجيل الدخول بكلمة مروره الصحيحة.
- نفس المستخدم يفشل بكلمة مرور خاطئة.
- إنشاء مستخدم جديد بكلمة مرور غير فارغة ينتج `Salt` غير فارغ و`PasswordHash` غير فارغ.
- تحديث مستخدم قائم دون كلمة مرور جديدة لا يغير كلمة مروره إذا كانت بياناته الحالية صحيحة.
- Bootstrap ينشئ admin بكلمة مرور قوية كما كان.
- AdminCli يعيد تعيين كلمة مرور admin بكلمة مرور قوية كما كان.

### 5.2 سيناريو الفشل الآمن

محاولة الاستغلال:

1. إنشاء مستخدم بحالة `PasswordHash=""` و`Salt=""`.
2. استدعاء `AuthService.ValidateCredentialsAsync(username, "")` أو أي كلمة مرور.

المتوقع بعد الإصلاح:

- يرجع `null`.
- لا يتم إنشاء salt جديد.
- لا يتم تعديل `PasswordHash`.
- لا يتم إنشاء session.

محاولة أخرى:

1. استدعاء `UserAdminService.CreateUserAsync(user, null)` من session مخولة.

المتوقع:

- ترمى `ArgumentException` أو `InvalidOperationException` واضحة.
- لا يتم إضافة المستخدم.
- لا يتم حفظ `PasswordHash=""` أو `Salt=""`.

محاولة ثالثة:

1. تحديث مستخدم broken لديه `Salt=""` و`PasswordHash=""` دون كلمة مرور جديدة.

المتوقع:

- يفشل التحديث أو يطلب كلمة مرور جديدة.
- لا يستمر الحساب في حالة broken.

### 5.3 سيناريو التحقق الثابت

فحوصات static مقترحة بعد التنفيذ:

- البحث عن `PasswordHash = string.Empty`.
- البحث عن `Salt = string.Empty`.
- البحث عن `if (!hasSalt)` داخل `AuthService`.
- البحث عن `legacy plain-text password migration`.
- التأكد من أن أي ذكر لـ legacy موجود فقط في اختبارات رفض أو وثائق.

### 5.4 سيناريو التحقق التشغيلي

- تسجيل الدخول بحساب موجود صالح.
- إنشاء مستخدم جديد من UI بكلمة مرور قوية.
- محاولة إنشاء مستخدم بلا كلمة مرور من UI إن كان المسار يسمح بذلك؛ يجب أن يرفض قبل أو داخل الخدمة.
- تشغيل AdminCli help وربما reset في بيئة اختبارية فقط.
- تشغيل اختبارات التكامل الخاصة بالـ bootstrap.

---

## 6. قرار معماري مقترح

القرار المقترح لمرحلة التنفيذ اللاحقة:

1. اختيار F6 كالإصلاح التالي.
2. إزالة مسار plaintext legacy من `AuthService`.
3. منع `UserAdminService.ApplyPassword` من كتابة hash/salt فارغين.
4. رفض إنشاء مستخدم بلا كلمة مرور.
5. السماح بتحديث مستخدم قائم دون تغيير كلمة المرور فقط إذا كانت بياناته الحالية سليمة.
6. عكس الاختبارات التي تثبت السلوك القديم إلى اختبارات رفض آمن.
7. عدم إدخال PBKDF2 أو hash version ضمن نفس التغيير؛ يترك ذلك لـ F15 بعد تنظيف الحالة.

هذا القرار يزيل آخر مسار مصادقة High-risk واضح قبل الدخول في تحسينات أوسع مثل lockout وPBKDF2 وإعادة تصميم `FullAccess`.

---

## 7. خلاصة الاعتماد

F6 هي الخطوة التالية الأكثر أولوية لأنها تمس مسار المصادقة نفسه وتسمح للنظام بالتعامل مع غياب `Salt` كحالة legacy قابلة للدخول والترقية. إصلاح F9 جعل الوصول إلى إنشاء المستخدمين محمياً، لكنه لم يلغ إنتاج حالة `PasswordHash/Salt` الفارغة ولم يلغ قبول plaintext داخل `AuthService`.

الخطة الآمنة هي حذف قبول plaintext من login، منع إنشاء credentials فارغة، وتحويل الاختبارات الحالية من إثبات السلوك القديم إلى إثبات الفشل الآمن. بهذا يصبح النظام جاهزاً لاحقاً لمعالجة F15 بترقية KDF دون حمل legacy plaintext path إلى التصميم الجديد.

هذه الوثيقة تخطيطية فقط. لم يتم تعديل أي كود إنتاجي أو اختبار أو إعدادات ضمن هذه المرحلة.
