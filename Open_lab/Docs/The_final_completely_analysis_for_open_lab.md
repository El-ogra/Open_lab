
# The_final_completely_analysis_for_open_lab.md

**المشروع:** Open_lab
**المستودع:** https://github.com/El-ogra/Open_lab.git
**الفرع المُدقَّق:** `Fi5ve`
**Commit المرجعي السابق:** `a00591eb8a8a031c81ea03ae3c66981fe8154d8b` — *"الحصول علي التقريران للمرة الثانية"*
**Commit الحالي (HEAD):** `7bcda0cb054a43671d01d5157fadacb9e78392cf` — *"إصلاح الثغرة الأولى"* (تاريخ: 2026-05-26)
**Commit الذي طبَّق المعالجات المعمارية الأكبر:** `d8b043e` — *"نقطه تحقق قبل اختبار bootstrap"* (تاريخ سابق على HEAD مباشرة)
**نوع المهمة:** تدقيق أمني مستقل + تحقق من التصحيحات + تخطيط معالجة (لا تعديل في كود الإنتاج).
**النطاق:** المسار الكامل للمصادقة، إدارة الجلسة، التخويل على مستوى الخدمات، Bootstrap، أسرار مُضمَّنة (Hardcoded Secrets)، KDF، Audit Interceptor.
**التقرير المرجعي:** `Open_lab/Docs/Completely_analysis_for_open_lab.md` (1049 سطراً، تمت قراءته وتحليله بالكامل).

---

## ملاحظة جوهرية حول تسمية Commit الأخير

عند فحص الـ commit المُسمَّى **"إصلاح الثغرة الأولى"** (`7bcda0c`)، تبيَّن أن التغيير الفعلي الوحيد في هذا الـ commit هو **تنسيق المسافات البادئة** في ملف `Open_lab/App.config` (استبدال 2 spaces بـ tabs، 4 إضافات + 4 حذوفات في ملف واحد). لم تحدث أي إزالة لكلمة المرور `og2026ra`، ولم يُلمَس أي سطر من كود `AuthService` أو `SessionContext` أو `SystemSettingsService` في هذا الـ commit بالذات.

التغييرات الأمنية الجوهرية (إزالة الـ backdoor، إنشاء `ISessionContext` و `SessionContext`، حذف `AppSession`، إنشاء `BootstrapViewModel` و `Open_lab.AdminCli`، عكس الاختبار `ValidateCredentials_AdminDevelopmentFallback_…`) جميعها وقعت في الـ commit السابق له مباشرة **`d8b043e` — "نقطه تحقق قبل اختبار bootstrap"**. وبما أن HEAD الحالي للفرع `Fi5ve` يحتوي على كل تلك التغييرات (لأنها تسبقه)، فإن جميع التحقُّقات أدناه أُجريت على هيئة الكود الحالية في HEAD = `7bcda0c`. يُذكر هذا للشفافية فقط، ولا يُغيِّر شيئاً من نتائج التحقق.

---

## 1. الملخص التنفيذي

تمت معالجة عدد كبير من الثغرات الحرجة في التقرير المرجعي بشكل فعلي على مستوى الكود — وليس بشكل سطحي. تحديداً تم تأكيد الحالة التالية:

- **F1 — Backdoor الإداري:** تمت إزالته كاملاً من `AuthService.cs`. الـ branch السطور 54-64 لم تعد موجودة؛ الدالة الآن تنتهي بـ `return null` بعد فشل الـ verify الحقيقي. الاختبار `ValidateCredentials_AdminDevelopmentFallback_ShouldResetHashAndReturnUser` تم **عكسه** كاملاً (الاسم الجديد: `ShouldReturnNullAndNotMutateUser`) ويؤكد الآن أن `admin/admin123` يُرجع `null` ولا يعدّل صف المستخدم.
- **F2 — الثابت `AdminDevelopmentPassword`:** محذوف بالكامل من `AuthService.cs`. لا توجد أي مرجعية لمعرّفه في كود الإنتاج أو الاختبارات.
- **F3 — Default master password `"admin123"`:** محذوف من `SystemSettingsService.VerifyMasterPasswordAsync`. الدالة الآن تُرجع `false` عند غياب الـ hash، ولا يوجد أي fallback نصّي مُشفَّر. الاختبار المُقابل تم عكسه أيضاً ويؤكد أن `"admin123"` يُرفض.
- **F4 — استثناء `admin` من فحص `!IsActive`:** أُزيل من `AuthService.cs`. الفحص الآن `if (!user.IsActive) return null;` بدون أي استثناء بالاسم.
- **F5 — `AppSession` الـ static class القابلة للكتابة:** الملف `Open_lab/ViewModels/AppSession.cs` **تم حذفه كلياً** من الشجرة. حلَّ محله `ISessionContext` + `SessionContext` (sealed class) مع `IsAdmin` كخاصية مُحتسَبة (`=> HasPermission(PermissionCodes.FullAccess)`). جميع ViewModels (30+ ملف) تمت ترقيتها لاستخدام `SessionContext.Current`. هذا تقدُّم معماري كبير، لكنه **ليس كاملاً** (تفاصيل في §3.5).
- **F7 — خلل `SaveProfileAsync`:** أُصلح. الدالة الآن تحسب الـ hash من جانب الخادم بـ salt متطابق.
- **F11 — غياب Bootstrap:** أُنشئ مسار Bootstrap كامل: `IsBootstrapRequiredAsync`, `MarkBootstrapCompleteAsync`, `BootstrapViewModel`, `BootstrapView`, تكامل في `App.xaml.cs:58-104` يستخدم `Bootstrap.AdminCreatedAt` كعلامة + فحص وجود دور `Administrator`. اختبارات تكامل ثلاث سيناريوهات (`BootstrapIntegrationTests`) موجودة.
- **Phase 2 (CLI خارج النطاق):** مشروع `Open_lab.AdminCli` تم إنشاؤه ويحقق متطلبات `--reset-admin --new-password` مع تحقق سياسة قوة وكلمة تأكيد `RESET-ADMIN`.

في المقابل، **ثغرات حرجة ومتوسطة لم تُعالَج** ما زالت موجودة في الكود:

- **F6 (High) — مسار النص الصريح القديم:** لا يزال موجوداً في `AuthService.cs` السطور 32-45.
- **F8 (High) — كلمة مرور SQL `og2026ra`:** **لم تُلمَس على الإطلاق**. موجودة في ثلاثة مواقع: `App.config:4`، `OpenLabDbContext.cs:83`، `OpenLabDbContextFactory.cs:20`. الـ commit المُسمى "إصلاح الثغرة الأولى" غيَّر **تنسيق المسافات** في `App.config` ولم يحذف كلمة المرور.
- **F9 (Medium) — غياب التخويل على مستوى الخدمة:** `UserAdminService` لم تُضَف إليه أي فحوصات `HasPermission(...)`.
- **F10 (Medium) — `AuditInterceptor.cs:43`:** السطر `int userId = _currentUserId ?? 1;` لم يتغيَّر، والإنشاء `new AuditInterceptor()` بدون DI لم يتغيَّر.
- **F12 (Medium) — `LoginViewModel:113-117`:** ما زال يستخدم `user.Username.Equals("admin", …)` لاستدعاء `EnsureAdminAccessAsync` (سياسة بالـ string).
- **F13 (Medium) — `IAuthService` الواجهة الرفيعة:** لم تُوسَّع.
- **F14 (Medium) — `EnsureAdminAccessAsync` غير-idempotent في الاتجاه الخاطئ:** يُستدعى من Bootstrap الآن (تحسين)، **لكنه أيضاً ما زال يُستدعى من LoginViewModel** عند كل تسجيل دخول إداري، أي السلوك القديم لم يُعزَل تماماً.
- **F15 (Medium) — KDF أحادي الجولة:** `PasswordSecurity.ComputeSha256` لم يتغيَّر. (تحسين صغير: `Verify` تستخدم الآن `FixedTimeEquals`).
- **F16 (Medium) — `PermissionCodes.FullAccess = "ALL"`:** ما زال السلسلة السحرية الوحيدة. (تحسين معماري: `IsAdmin` مُحتسَبة الآن، لا setter).
- **ثغرة جانبية إضافية (لم تُذكَر في التقرير الأصلي صراحةً لكن منبثقة من F4 بعد إصلاحه):** `UserAdminService.UpdateUserAsync:100` ما زال يجبر `IsActive=true` على `admin` (`current.IsActive = user.IsActive || string.Equals(current.Username, AdminUsername, …)`). أي أن المنع المعماري لتعطيل `admin` انتقل من AuthService إلى UserAdminService بدلاً من إزالته.

**الحكم الإجمالي:** المشروع أنجز بنجاح وبشكل صحيح **Phase 1 + Phase 2 + Phase 3** من خطة المعالجة المرجعية (Bootstrap + Out-of-band reset + إزالة الـ backdoor) وقسماً معتبراً من **Phase 4** (تغليف الجلسة). المخاطر الأمنية الجوهرية المتبقية تتمحور حول **Phase 5 (تخويل على مستوى الخدمة)**، **Phase 6 (KDF)**، **Phase 7 (تهيئة قاعدة البيانات)**، و**Phase 8 (Audit + Lockout)**. أبرز خطر متبقٍ هو **F8** لأن كلمة مرور SQL `sa` ما زالت مفضوحة في المستودع العام.

**مستوى المخاطر العام الحالي:** متوسط–عالٍ (انخفض من *حرج* إلى *متوسط–عالٍ* بعد إزالة F1/F2/F3/F4/F11). تحسُّن جوهري لكن غير مكتمل.

**تقييم جودة التصحيحات:** التصحيحات المُنجزة على F1, F2, F3, F4, F5 (جزئي), F7, F11 ذات **جودة عالية** ومُدعومة باختبارات سليمة ومعكوسة. الـ commit الذي يحمل عنوان "إصلاح الثغرة الأولى" **مضلِّل تسميةً** لأنه لا يُصلح أي ثغرة في الواقع (تغيير تنسيق فقط)، بينما الإصلاحات الفعلية لـ F1 وغيرها وقعت في الـ commit السابق له.

---

## 2. جدول المقارنة وحالة الثغرات (F1 → F16)

| ID | الثغرة | الحالة الحالية | مستوى الخطورة | حالة الإصلاح | ملاحظات |
|----|--------|----------------|--------------|---------------|---------|
| **F1** | Administrator Backdoor (`admin/admin123` يعمل دائماً) | Branch السطور 54-64 محذوف بالكامل من `AuthService.cs` | Critical | ✅ **تم الإصلاح بالكامل** | الاختبار المُلازم تم عكسه ويؤكد رفض الـ backdoor الآن. |
| **F2** | الثابت `AdminDevelopmentPassword = "admin123"` | الثابت محذوف؛ لا مرجعية له في الكود | Critical | ✅ **تم الإصلاح بالكامل** | grep يؤكد عدم وجود أي مرجعية. |
| **F3** | Default master password `"admin123"` في `VerifyMasterPasswordAsync` | الـ fallback أُزيل؛ تُرجع `false` الآن | Critical | ✅ **تم الإصلاح بالكامل** | الاختبار المُلازم في `Module13ServiceTests_Additional.cs:715-727` معكوس. |
| **F4** | إعفاء `admin` من فحص `!IsActive` في AuthService | الإعفاء أُزيل من AuthService | High | ✅ **تم الإصلاح في AuthService** ⚠️ **لكن انتقل للـ UserAdminService** | `UserAdminService.UpdateUserAsync:100` ما زال يُجبر `IsActive=true` على `admin`. |
| **F5** | `AppSession` static class مع `public set` | الملف محذوف؛ `ISessionContext` و `SessionContext` يحلان محله؛ `IsAdmin` مُحتسَبة | High | 🟡 **تم الإصلاح جزئياً** | `static Current` ما زالت موجودة + setters على `UserId/Username/AttendanceLogId` ما زالت public؛ تفاصيل في §3.5. |
| **F6** | المسار plain-text القديم في AuthService عند خلوّ Salt | الكود موجود كما هو في السطور 32-45 | High | ❌ **لم يُصلَح** | الـ exploit chain عبر `UserAdminService.ApplyPassword` (كتابة `""` لـ `PasswordHash` و `Salt`) ما زال قائماً. |
| **F7** | `SaveProfileAsync` يولّد salt جديد دون hash متطابق | الدالة تحسب الـ hash من جانب الخادم الآن | High | ✅ **تم الإصلاح بالكامل** | السطور 160-166 في `SystemSettingsService.cs`. |
| **F8** | كلمة مرور SQL `og2026ra` Hardcoded | موجودة في 3 مواقع بدون أي تعديل | High | ❌ **لم يُصلَح** | الـ commit الأخير غيَّر تنسيق المسافات فقط في `App.config`. |
| **F9** | غياب تخويل على مستوى `UserAdminService` | لا توجد فحوصات `HasPermission` داخل الخدمة | Medium | ❌ **لم يُصلَح** | الحماية ما زالت في طبقة الـ ViewModel فقط. الحماية بالاسم على `admin/Administrator` ما زالت قائمة. |
| **F10** | `AuditInterceptor` يستخدم `?? 1` | السطر 43 لم يتغيَّر؛ الإنشاء `new AuditInterceptor()` لم يتغيَّر | Medium | ❌ **لم يُصلَح** | لا حقن DI، لا ربط بـ ISessionContext. |
| **F11** | غياب Bootstrap | مسار Bootstrap كامل موجود | High | ✅ **تم الإصلاح بالكامل** | App.xaml.cs:58-104 + BootstrapViewModel + AdminSetupService.IsBootstrapRequiredAsync + علامة `Bootstrap.AdminCreatedAt` + اختبارات تكامل. |
| **F12** | `LoginViewModel` يقرر "سلوك admin" من الـ username string | السطور 113-117 لم تتغيَّر | Medium | ❌ **لم يُصلَح** | ما زال `user.Username.Equals("admin", …)` يستدعي `EnsureAdminAccessAsync`. |
| **F13** | `IAuthService` واجهة رفيعة (لا outcomes، لا lockout) | الواجهة هي ذاتها (`Task<User?> ValidateCredentialsAsync(...)`) | Medium | ❌ **لم يُصلَح** | لا `AuthOutcome`، لا `ChangePassword`، لا `FailedAttempts`. |
| **F14** | `EnsureAdminAccessAsync` يُعيد منح جميع الصلاحيات عند كل دخول إداري | يُستدعى من Bootstrap الآن (إضافة) + ما زال يُستدعى من LoginViewModel | Medium | 🟡 **تم الإصلاح جزئياً** | المنطق نفسه (السطور 53-84) لم يتغيَّر؛ السلوك "Restore-on-login" ما زال قائماً عبر `LoginViewModel.cs:116`. |
| **F15** | SHA-256 جولة واحدة | `PasswordSecurity.ComputeSha256` لم يتغيَّر | Medium | ❌ **لم يُصلَح** | تحسين بسيط: `Verify` تستخدم `CryptographicOperations.FixedTimeEquals`. |
| **F16** | `PermissionCodes.FullAccess = "ALL"` سلسلة سحرية | الثابت كما هو؛ `IsAdmin` مُحتسَبة منها الآن | Medium | 🟡 **تحسُّن معماري دون إصلاح الجذر** | السلسلة "ALL" ما زالت المعرّف الوحيد للمسؤول، لكن `IsAdmin` لم تعد setter منفصلاً. |

**الإجمالي:**
- ✅ تم الإصلاح بالكامل: **6 ثغرات** (F1, F2, F3, F4 جزئياً انتقل، F7, F11)
- 🟡 تم الإصلاح جزئياً أو تحسُّن معماري: **3 ثغرات** (F5, F14, F16)
- ❌ لم يُصلَح: **7 ثغرات** (F6, F8, F9, F10, F12, F13, F15)

---

## 3. التحقق التفصيلي من الكود

كل فقرة أدناه تستند إلى قراءة الكود الحالي في HEAD = `7bcda0c`. أرقام الأسطر دقيقة على هذا الـ commit.

### 3.1 F1 و F2 — الـ Backdoor والثابت `AdminDevelopmentPassword`

**الملفات:** `Open_lab/Services/AuthService.cs`, `Open_lab.Tests/Services/AuthServiceTests.cs`.

**الكود الحالي لـ `AuthService.cs`** (الملف كاملاً 55 سطر):

```csharp
public async Task<User?> ValidateCredentialsAsync(string username, string password)
{
    var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
    if (user == null)
    {
        return null;
    }

    if (!user.IsActive)            // ← السطر 26-29: الإعفاء بالاسم أُزيل (إصلاح F4)
    {
        return null;
    }

    // Backward-compatible path: legacy plain-text passwords are migrated on successful login.
    var hasSalt = !string.IsNullOrWhiteSpace(user.Salt);
    if (!hasSalt)                  // ← السطر 32-45: المسار القديم ما زال موجوداً (F6 لم يُصلَح)
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

    if (PasswordSecurity.Verify(password, user.Salt, user.PasswordHash))
    {
        return user;
    }

    return null;                   // ← السطر 52: الـ backdoor branch محذوف بالكامل
}
```

**الأدلة المؤكِّدة لإزالة F1 و F2:**

1. لا يوجد ثابت `AdminUsername` ولا `AdminDevelopmentPassword` في الملف (السطور 11-12 السابقة محذوفة).
2. لا يوجد `string.Equals(password, "admin123", ...)` ولا أي مرجعية لـ `"admin123"` في الـ AuthService.
3. الـ branch السابق (السطور 54-64 في `a00591e`) الذي كان يُعيد كتابة `Salt + PasswordHash + IsActive=true` غير موجود.
4. `grep -rn "AdminDevelopmentPassword" Open_lab/ Open_lab.Tests/` يُرجع نتائج صفر.

**عكس الاختبار المُلازم** — `Open_lab.Tests/Services/AuthServiceTests.cs:120-145`:

```csharp
[Fact]
public async Task ValidateCredentials_AdminDevelopmentFallback_ShouldReturnNullAndNotMutateUser()
{
    // Function: 10.8 — Logout (security: hardcoded admin fallback is rejected)
    var salt = PasswordSecurity.GenerateSalt();
    var wrongHash = PasswordSecurity.ComputeSha256("wrong_pass", salt);
    _db.Users.Add(new User
    {
        Username = "admin",
        PasswordHash = wrongHash,
        Salt = salt,
        IsActive = false
    });
    await _db.SaveChangesAsync();

    var user = await _service.ValidateCredentialsAsync("admin", "admin123");

    user.Should().BeNull();                                  // ← يؤكد الرفض
    var refreshed = await _db.Users.SingleAsync(u => u.Username == "admin");
    refreshed.IsActive.Should().BeFalse();                   // ← يؤكد عدم إعادة التفعيل
    refreshed.Salt.Should().Be(salt);                        // ← يؤكد عدم إعادة كتابة Salt
    refreshed.PasswordHash.Should().Be(wrongHash);           // ← يؤكد عدم إعادة كتابة Hash
}
```

**الاستنتاج:** F1 و F2 — **تم الإصلاح بالكامل**. الاختبار المُلازم تم عكسه بشكل صحيح ومتسق مع تغيير الكود.

**مخاطر متبقية بعد الإصلاح:** عند نشر هذا الإصلاح على مثبَّت سابق كانت كلمة مرور المسؤول فيه قد كُتِبت بالـ backdoor، سيفقد المسؤول الوصول إلا عبر `Open_lab.AdminCli --reset-admin`. هذا متوافق مع تسلسل Phase 1+2 → Phase 3 الموصى به في التقرير المرجعي (§11.5 منه).

---

### 3.2 F3 — كلمة المرور الافتراضية للـ Master Password

**الملف:** `Open_lab/Services/SystemSettingsService.cs:169-185`.

**الكود الحالي:**

```csharp
public async Task<bool> VerifyMasterPasswordAsync(string password)
{
    var hash = await _db.Settings.Where(s => s.Key == MasterPasswordHashKey).Select(s => s.Value).FirstOrDefaultAsync();
    var salt = await _db.Settings.Where(s => s.Key == MasterPasswordSaltKey).Select(s => s.Value).FirstOrDefaultAsync();

    if (string.IsNullOrEmpty(hash))
    {
        return false;                       // ← السابق: if (password == "admin123") return true;
    }

    if (string.IsNullOrEmpty(salt))
    {
        return false;                       // ← السابق: salt = GenerateSecureSalt();
    }

    return PasswordSecurity.Verify(password, salt, hash);
}
```

**الأدلة:**
- السطر 177 السابق (`if (password == "admin123") return true;`) غير موجود في الكود الحالي.
- لا أي مرجعية لـ `"admin123"` في `SystemSettingsService.cs`.

**الاختبار المُلازم** — `Module13ServiceTests_Additional.cs:715-727`:

```csharp
[Fact]
public async Task SetSystemPassword_VerifyMasterPassword_WhenNoPasswordEverSet_ShouldRejectEveryPassword_EdgeGuard()
{
    var defaultOk = await _systemSettingsService.VerifyMasterPasswordAsync("admin123");
    var anythingElse = await _systemSettingsService.VerifyMasterPasswordAsync("anythingElse#2026");

    defaultOk.Should().BeFalse();           // ← الاسم القديم كان ShouldAllowDefaultAdmin123
    anythingElse.Should().BeFalse();
}
```

والاختبار الإضافي `SystemSettingsServiceTests.cs:110-122` يعكس نفس السلوك للسلامة المضاعَفة.

**الاستنتاج:** F3 — **تم الإصلاح بالكامل**.

---

### 3.3 F4 — استثناء `admin` من فحص `!IsActive`

**الإصلاح في AuthService:** تم بشكل صحيح. السطر 26-29 الحالي:

```csharp
if (!user.IsActive)
{
    return null;
}
```

لا يوجد استثناء بالاسم. حساب `admin` المعطَّل لا يستطيع تسجيل الدخول.

**⚠️ ملاحظة معمارية مهمة:** نفس فكرة "حماية admin بشكل غير قابل للتعطيل" انتقلت إلى ملف آخر — `Open_lab/Services/UserAdminService.cs:100`:

```csharp
current.IsActive = user.IsActive || string.Equals(current.Username, AdminUsername, StringComparison.OrdinalIgnoreCase);
```

هذا السطر يجبر `admin` على البقاء `IsActive=true` حتى لو حدَّد المسؤول قيمة `false` صراحةً في `UpdateUserAsync`. أي أن "soft-disable" على `admin` ما زال غير ممكن عبر مسار التحديث (UI أو خدمات أخرى)، لكن إن جرى تعديل قاعدة البيانات يدوياً، فالـ AuthService الآن سيرفض المستخدم. هذا انتقال للسلوك من طبقة Authentication إلى طبقة Administration، يجعل F4 **مُحلولاً جوهرياً** (لا يستطيع الـ attacker تجاوز AuthService) لكن منع تعطيل `admin` من واجهة الإدارة ما زال قائماً (هذا قد يكون مقصوداً كـ defensive measure لمنع قفل الذات، لكن يجب توثيقه).

**الاستنتاج:** F4 — **تم الإصلاح في AuthService بشكل صحيح**. سلوك "حماية admin من التعطيل" انتقل إلى UserAdminService كقاعدة عمل بدلاً من ثغرة مصادقة، وهذا تصنيف مقبول مع شرط التوثيق.

---

### 3.4 F5 — `AppSession` العالمية القابلة للكتابة

**حذف الملف:** `Open_lab/ViewModels/AppSession.cs` لم يعد موجوداً في الشجرة. `git log --diff-filter=D --name-only -- Open_lab/ViewModels/AppSession.cs` يؤكد الحذف في commit `d8b043e`.

**الإحلال المعماري:** ملفان جديدان:

```csharp
// Open_lab/Services/ISessionContext.cs
public interface ISessionContext
{
    int UserId { get; }
    string Username { get; }
    bool IsAdmin { get; }
    int AttendanceLogId { get; }
    bool HasPermission(string permissionCode);
}

public interface IMutableSessionContext : ISessionContext
{
    void BeginSession(int userId, string username, IReadOnlyCollection<string> permissionCodes, int attendanceLogId = 0);
    void UpdateAttendanceLog(int attendanceLogId);
    void EndSession();
}
```

```csharp
// Open_lab/Services/SessionContext.cs (49 سطر)
public sealed class SessionContext : IMutableSessionContext
{
    private readonly HashSet<string> _grantedPermissions = new(StringComparer.OrdinalIgnoreCase);

    public static SessionContext Current { get; } = new();   // ⚠️ singleton ثابت

    public int UserId { get; set; }                          // ⚠️ public set
    public string Username { get; set; } = string.Empty;     // ⚠️ public set
    public bool IsAdmin => HasPermission(PermissionCodes.FullAccess);  // ✅ computed
    public int AttendanceLogId { get; set; }                 // ⚠️ public set
    ...
}
```

**التحسينات المؤكَّدة:**
- ✅ `IsAdmin` صار خاصية محسوبة من حالة الصلاحيات، لا setter منفصل. أي أن `SessionContext.Current.IsAdmin = true;` صار **compile error** — تحسين حقيقي.
- ✅ `HashSet<string>` للصلاحيات `private readonly` ولا يمكن التلاعب به مباشرة من الخارج.
- ✅ DI registration في `App.xaml.cs:47-48`: `services.AddSingleton<ISessionContext>(SessionContext.Current);` — الكود الجديد يستهلك `ISessionContext` (للقراءة فقط).
- ✅ `Open_lab.Tests/Infrastructure/AppSessionTestHelper.cs` أُعيد كتابته: `ResetToAdmin()` الآن يستخدم `SessionContext.Current.BeginSession(1, "admin", new[] { PermissionCodes.FullAccess });` — الإسناد المباشر `AppSession.IsAdmin = true;` لم يعد موجوداً.

**التحفظات المعمارية المتبقية:**
- ⚠️ `static Current { get; }` موجودة. أي كود في الـ assembly يمكنه استدعاء `SessionContext.Current.BeginSession(99, "fake", new[] { PermissionCodes.FullAccess });` — هذا يحقق نفس التصعيد بصياغة جديدة. الفرق هو أن الكود الذي يفعل ذلك يجب أن يستدعي `BeginSession` (4 معاملات) بدلاً من سطر واحد `IsAdmin = true;`، لكن النتيجة النهائية واحدة.
- ⚠️ `UserId`, `Username`, `AttendanceLogId` ما زالت `public set;`. أي كود يحصل على مرجع لـ `SessionContext` (وليس `ISessionContext`) يمكنه تعديلها مباشرة. حقن DI الحالي يسلِّم `ISessionContext` (للقراءة فقط) مما يحمي الـ ViewModels، لكن `LoginViewModel.cs:121` يستخدم `SessionContext.Current` (الـ static) مباشرةً وليس `IMutableSessionContext` المحقون.
- ⚠️ `IsAdmin` المحسوبة تعتمد على `_grantedPermissions.Contains(PermissionCodes.FullAccess)` — وهذا ينقل المشكلة إلى F16 (الـ "ALL" magic string). إذا تمكَّن كود ما من إضافة `"ALL"` إلى الصلاحيات، يحصل على IsAdmin=true.

**الاستنتاج:** F5 — **تم الإصلاح جزئياً (تحسين معماري كبير لكن غير مكتمل)**. القفل الصارم على التصعيد (compile-time enforcement) يتطلب جعل `SessionContext` غير قابلة للوصول إلا عبر `IMutableSessionContext` المحقون داخلياً في `LoginViewModel` و`MainViewModel` فقط، وتحويل setters على `UserId/Username/AttendanceLogId` إلى `private set;` ، وإزالة `static Current` أو جعلها `internal`.

---

### 3.5 F6 — مسار النص الصريح القديم

**الكود الحالي** — `AuthService.cs:32-45`:

```csharp
// Backward-compatible path: legacy plain-text passwords are migrated on successful login.
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

**سلسلة الاستغلال ما زالت قائمة:**

1. مسؤول ينشئ مستخدماً جديداً بدون كلمة مرور عبر UI أو خدمة. الـ `UserAdminService.cs:239-251` لم يتغيَّر:

   ```csharp
   private static void ApplyPassword(User user, string? plainPassword)
   {
       if (string.IsNullOrWhiteSpace(plainPassword))
       {
           user.PasswordHash = string.Empty;
           user.Salt = string.Empty;
           return;
       }
       ...
   }
   ```

2. القاعدة تحتوي صفّ مستخدم بـ `PasswordHash=""` و `Salt=""`.
3. أي كود يستدعي `_authService.ValidateCredentialsAsync(username, "")` (سلسلة فارغة) — الـ `hasSalt` = false، الـ `string.Equals("", "", Ordinal)` = true، الـ user يُرجع وتُحدَّث إلى hashed.
4. الـ `LoginViewModel.LoginAsync:97` يرفض السلاسل الفارغة في UI، لكن أي مستهلك يتجاوز هذا الـ guard (اختبارات، CLI مستقبلي، API) يمر.

**اختبار يثبت السلوك** — `AuthServiceTests.cs:70-95` (موجود ويمر):

```csharp
[Fact]
public async Task ValidateCredentials_WithLegacyPlainText_ShouldMigrateToHash()
{
    var legacy = new User { Username = "legacy_user", PasswordHash = "plain_password", Salt = "", IsActive = true };
    _db.Users.Add(legacy);
    await _db.SaveChangesAsync();

    var user = await _service.ValidateCredentialsAsync("legacy_user", "plain_password");

    user.Should().NotBeNull();
    ...
}
```

الاختبار يستخدم `Salt=""` ويتحقق من التحويل بنجاح، أي أن سلوك F6 مقفول كـ "ميزة" بدلاً من ثغرة.

**الاستنتاج:** F6 — **لم يُصلَح**. يجب: (1) إزالة الـ branch من AuthService كلياً، أو (2) إعادة كتابة `UserAdminService.ApplyPassword` لرفض كلمات المرور الفارغة بدلاً من كتابة `""`، و(3) رفض الاختبار الحالي أو تحويله ليتأكد من رفض المسار. التفاصيل في §5.

---

### 3.6 F7 — خلل `SaveProfileAsync` (الـ Salt يولد منفصلاً عن الـ Hash)

**الكود الحالي** — `SystemSettingsService.cs:136-167`:

```csharp
public async Task SaveProfileAsync(SystemSettingsProfile profile)
{
    ...
    if (!string.IsNullOrEmpty(profile.MasterPasswordHash))
    {
        var newSalt = GenerateSecureSalt();
        var newHash = PasswordSecurity.ComputeSha256(profile.MasterPasswordHash, newSalt);
        await SaveSettingAsync(MasterPasswordSaltKey, newSalt);
        await SaveSettingAsync(MasterPasswordHashKey, newHash);
    }
}
```

**التحليل:** الدالة الآن تأخذ `profile.MasterPasswordHash` كأنه **plain-text password** (وهذا هو السلوك المتوقع من الـ UI الحالي الذي يمرر كلمة مرور خام عبر هذا الحقل)، وتحسب الـ hash من جانب الخادم بـ `salt` متطابق. هذا يُصلح الـ logical defect الموثَّق في التقرير المرجعي §2.4.

**ملاحظة على الاسم:** الحقل في الـ DTO ما زال يسمى `MasterPasswordHash` بينما يحمل الآن plain-text. هذا dissonance في التسمية ويستحق إعادة تسمية إلى `MasterPasswordPlain` أو فصل DTOs المُدخَلة عن المُخرَجة، لكنه لا يُؤثر على الأمان الفعلي.

**الاستنتاج:** F7 — **تم الإصلاح بالكامل من حيث المنطق**. توصية ثانوية بإعادة تسمية الحقل لتقليل الالتباس.

---

### 3.7 F8 — كلمة مرور SQL `og2026ra`

**ثلاثة مواقع لم تتغير على الإطلاق:**

1. **`Open_lab/App.config:4`:**
   ```xml
   <add name="OpenLabDb" connectionString="Server=.\SQLEXPRESS;Database=OpenLab;User ID=sa;Password=og2026ra;TrustServerCertificate=True;..." providerName="Microsoft.Data.SqlClient" />
   ```
   الـ commit "إصلاح الثغرة الأولى" يغيّر فقط بادئة المسافات (tabs بدل 2 spaces) — `git show HEAD -- Open_lab/App.config` يثبت ذلك بصرياً.

2. **`Open_lab/Data/OpenLabDbContext.cs:81-85`:**
   ```csharp
   if (string.IsNullOrWhiteSpace(connectionString))
   {
       var dbPassword = Environment.GetEnvironmentVariable("OPENLAB_DB_PASSWORD") ?? "og2026ra";
       connectionString = $"Server=.\\SQLEXPRESS;Database=OpenLab;User ID=sa;Password={dbPassword};...";
   }
   ```

3. **`Open_lab/Data/OpenLabDbContextFactory.cs:17-22`** (موقع لم يُذكَر في التقرير المرجعي الأصلي):
   ```csharp
   if (string.IsNullOrWhiteSpace(connectionString))
   {
       var dbPassword = Environment.GetEnvironmentVariable("OPENLAB_DB_PASSWORD") ?? "og2026ra";
       connectionString = $"Server=.\\SQLEXPRESS;Database=OpenLab;User ID=sa;Password={dbPassword};...";
   }
   ```

**الاستنتاج:** F8 — **لم يُصلَح**. ثلاثة مواقع لكلمة المرور (App.config + DbContext + DbContextFactory)، اثنان منها (`DbContext` و `DbContextFactory`) يُكرِّران نفس fallback. الـ `App.config` ما زال يُسلَّم إلى GitHub العام. هذا الخطر **حرج** لأي نسخة منشورة لم تُغيِّر كلمة مرور SQL، خصوصاً أن المستخدم هو `sa` (super-user).

**تحفظ تسموي:** الـ commit المسمى "إصلاح الثغرة الأولى" مضلِّل: يوحي بأنه يُصلح ثغرة، بينما هو في الواقع لا يصلح أي شيء — تنسيق بصري فقط. هذا قد يربك مراجعات الـ PR ويُعطي انطباعاً زائفاً بالتقدم.

---

### 3.8 F9 — غياب التخويل على مستوى الخدمة في `UserAdminService`

**فحص `Open_lab/Services/UserAdminService.cs` بالكامل** (253 سطر):

- `CreateUserAsync` (السطور 40-64): لا يوجد فحص `HasPermission`.
- `UpdateUserAsync` (السطور 66-108): لا يوجد فحص `HasPermission`. الحماية الوحيدة هي بالاسم على `admin`.
- `DeleteUserAsync` (السطور 110-142): لا يوجد فحص `HasPermission`. الحماية بالاسم على `admin` + فحص `hasOperationalData` (تحسين منذ التقرير المرجعي).
- `CreateRoleAsync` (السطور 144-162): لا يوجد فحص.
- `DeleteRoleAsync` (السطور 164-191): لا يوجد فحص. الحماية بالاسم على `Administrator` + فحص `isAssigned`.
- `AssignSingleRoleAsync` (السطور 193-206): لا يوجد فحص.
- `RemoveUserRoleAsync` (السطور 208-224): لا يوجد فحص. الحماية بالاسم على `admin`.
- `SaveRolePermissionsAsync` (السطور 226-237): **لا يوجد فحص على الإطلاق** — هذه أخطر دالة لأنها تستطيع منح `PermissionCodes.FullAccess` لأي دور.

**التخويل ما زال في الـ ViewModel فقط** — `UsersPermissionsViewModel.cs:30-37`:

```csharp
SaveUserCommand = new RelayCommand(async _ => await SaveUserAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.UsersEdit));
...
SaveRolePermissionsCommand = new RelayCommand(async _ => await SaveRolePermissionsAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.UsersEdit));
```

**سلسلة الاستغلال:** أي كود يتجاوز الـ UI (اختبارات، CLI، إضافات DLL، شيفرة scripted) يمكنه استدعاء `_userAdminService.SaveRolePermissionsAsync(roleId, new[] { "ALL" })` ويحوِّل أي دور إلى Administrator. الحماية الاسمية على `admin/Administrator` لا تمنع هذا لأن الـ attacker يستطيع منح "ALL" لدور آخر.

**الاستنتاج:** F9 — **لم يُصلَح**. هذا أحد أهم الثغرات المعمارية المتبقية وذو **أثر تصاعد امتيازات حقيقي**. التفاصيل في §5.

---

### 3.9 F10 — `AuditInterceptor` يستخدم `?? 1` ويُنشأ بدون DI

**الكود الحالي:**

`Open_lab/Data/AuditInterceptor.cs:42-47`:
```csharp
int userId = _currentUserId ?? 1;
if (context is OpenLabDbContext openLabContext && openLabContext.CurrentUserId.HasValue)
{
    userId = openLabContext.CurrentUserId.Value;
}
```

`Open_lab/Data/OpenLabDbContext.cs:90`:
```csharp
optionsBuilder.AddInterceptors(new AuditInterceptor());
```

السطر 17-22 من `AuditInterceptor.cs` ما زال يأخذ معامل `int? currentUserId = null` لكن لا أحد يمرّره. تحسين جانبي في `App.xaml.cs:36-43`:

```csharp
services.AddTransient<OpenLabDbContext>(_ => 
{
    var context = new OpenLabDbContextFactory().CreateDbContext(System.Array.Empty<string>());
    if (SessionContext.Current.UserId > 0)
    {
        context.CurrentUserId = SessionContext.Current.UserId;
    }
    return context;
});
```

أي أن `CurrentUserId` على الـ context يُملأ من `SessionContext` عند الإنشاء، لكن:

- إذا تم إنشاء الـ context قبل تسجيل الدخول (مثلاً في Bootstrap)، `SessionContext.Current.UserId = 0` فلا يُسنَد.
- الـ Interceptor نفسه ما زال يقرأ من الـ context (السطر 44) بدلاً من `ISessionContext` المحقون.
- إذا تغيَّر `SessionContext.Current.UserId` بعد إنشاء الـ context (logout مثلاً)، الـ `context.CurrentUserId` لا يُحدَّث.

**الاستنتاج:** F10 — **لم يُصلَح**. الـ `?? 1` ما زال هناك. التحسين في `App.xaml.cs` يقلِّل الاحتمال لكن لا يُزيل الـ root cause.

---

### 3.10 F11 — Bootstrap Flow

**كل المكوّنات المطلوبة موجودة:**

1. **`AdminSetupService.cs:20-32`** — `IsBootstrapRequiredAsync`:
   ```csharp
   public async Task<bool> IsBootstrapRequiredAsync()
   {
       var markerExists = await _db.Settings.AnyAsync(s => s.Key == BootstrapAdminCreatedAtKey);
       if (markerExists) return false;
       var hasAdministrator = await _db.UserRoles.AnyAsync(ur => ur.Role.RoleName == AdministratorRoleName);
       return !hasAdministrator;
   }
   ```

2. **`AdminSetupService.cs:34-51`** — `MarkBootstrapCompleteAsync` يكتب علامة `Bootstrap.AdminCreatedAt`.

3. **`App.xaml.cs:58-104`** — تكامل بدء التشغيل:
   ```csharp
   private async void ShowStartupWindow()
   {
       ...
       var adminSetupService = _serviceProvider.GetRequiredService<IAdminSetupService>();
       if (await adminSetupService.IsBootstrapRequiredAsync())
       {
           ShowBootstrapWindow();
           return;
       }
       ShowLoginWindow();
   }
   ```

4. **`BootstrapViewModel.cs`** (166 سطر) — يحتوي على:
   - سياسة كلمة مرور قوية (`ValidatePassword`): طول ≥12، يجب أن تحتوي كبير وصغير ورقم ورمز.
   - **Blocklist صريحة** للكلمة `"admin123"` (السطور 150-153) — هذا استخدام صحيح للسلسلة (للرفض).
   - استدعاء `_userAdminService.CreateUserAsync` ثم `_adminSetupService.EnsureAdminAccessAsync` ثم `MarkBootstrapCompleteAsync`.

5. **`Open_lab/Views/Bootstrap/BootstrapView.xaml`** — UI كامل (192 سطر) منفصلة عن نافذة تسجيل الدخول.

6. **اختبارات تكامل** — `BootstrapIntegrationTests.cs` يغطّي السيناريوهات الثلاثة المطلوبة في التقرير المرجعي §11.3:
   - فارغ → `IsBootstrapRequired = true`
   - يحوي Administrator → `false`
   - مارَكَر مستعاد → `false`

**الاستنتاج:** F11 — **تم الإصلاح بالكامل**. هذا أحد أنجح أجزاء المعالجة. التطبيق يتبع نموذج Phase 1 الموصى به في التقرير المرجعي حرفياً.

**ملاحظة على Race Condition:** لم أرَ كائن `Transaction` صريحاً يلفّ مسار Bootstrap في `BootstrapViewModel.CreateAdminAsync`. السيناريو المُحتمَل (محطّتان تستخدمان نفس DB) لا يحدث على WPF محلي عادة، لكنه يستحق `IExecutionStrategy` + `BeginTransactionAsync` للأمان. هذا تحسين، لا ثغرة.

---

### 3.11 F12 — `LoginViewModel` يقرر سلوك admin من الـ username

**الكود الحالي** — `LoginViewModel.cs:113-117`:

```csharp
var isAdminAccount = user.Username.Equals("admin", StringComparison.OrdinalIgnoreCase);
if (isAdminAccount)
{
    await _adminSetupService.EnsureAdminAccessAsync(user.UserId);
}
```

السطر لم يتغيَّر عن الـ commit المرجعي.

**التأثير:** كل ما يحتاجه الـ attacker لإطلاق `EnsureAdminAccessAsync` (التي تمنح كل الصلاحيات للدور `Administrator`) هو أن يكون اسم المستخدم في DB حرفياً `admin`. بعد Bootstrap الأول، هذا الاسم محجوز ومحمي بالاسم في `UserAdminService` (لا يُحذَف، لا يُعاد تسميته)، لذا الأثر العملي محدود ضمن منع التهيئة الخاطئة. **لكن** المنطق ما زال username-as-policy وليس permission-as-policy.

**الاستنتاج:** F12 — **لم يُصلَح**. الإصلاح المنطقي: استبدال الشرط بـ `permissionCodes.Contains(PermissionCodes.FullAccess, StringComparer.OrdinalIgnoreCase)` (وهذا يتطلب تحريك `EnsureAdminAccessAsync` إلى Bootstrap حصراً كما يوصي التقرير المرجعي).

---

### 3.12 F13 — واجهة `IAuthService` رفيعة

**الكود الحالي:**

```csharp
public interface IAuthService
{
    Task<User?> ValidateCredentialsAsync(string username, string password);
}
```

لا تغيير. لا `AuthOutcome`، لا `ChangePasswordAsync`، لا `RecordFailedAttemptAsync`، لا `LockAccountAsync`.

**الأثر العملي:**
- `LoginViewModel` لا يميِّز بين "wrong password" و "inactive user" و "user not found" — كلها تعيد `null`. لا يمكن لـ Audit log أن يفرّق.
- لا توجد طبقة لمنع brute-force.

**الاستنتاج:** F13 — **لم يُصلَح**. تحسين Phase 8 الموصى به.

---

### 3.13 F14 — `EnsureAdminAccessAsync` غير-idempotent

**الكود الحالي** — `AdminSetupService.cs:53-84`:

```csharp
public async Task EnsureAdminAccessAsync(int userId)
{
    var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == AdministratorRoleName);
    if (role == null) { ...إنشاء... }
    var hasUserRole = await _db.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == role.RoleId);
    if (!hasUserRole) { ...ربط المستخدم... }

    var existing = await _db.RolePermissions.Where(rp => rp.RoleId == role.RoleId).Select(rp => rp.PermissionCode).ToListAsync();
    foreach (var code in PermissionCodes.All)
    {
        if (!existing.Contains(code))
        {
            _db.RolePermissions.Add(new RolePermission { RoleId = role.RoleId, PermissionCode = code });
        }
    }
    await _db.SaveChangesAsync();
}
```

المنطق لم يتغيَّر. الدالة:
- تُنشئ دور `Administrator` إن لم يوجد.
- تربط المستخدم بالدور إن لم يكن مرتبطاً.
- **تُضيف كل permission code مفقود من `PermissionCodes.All` إلى الدور**.

الاستدعاءان:
1. من `BootstrapViewModel.cs:112` — استدعاء سليم (مرة واحدة عند التهيئة).
2. **من `LoginViewModel.cs:116` — كل تسجيل دخول للـ admin يعيد منح كل صلاحية مفقودة**.

**الأثر:** أي صلاحية أزالها المسؤول من دور `Administrator` عن قصد (في `UsersPermissionsViewModel` مثلاً) ستُستعاد تلقائياً عند أول تسجيل دخول لـ `admin`. ملاحظة: هذا في الواقع قد لا يكون استغلالاً مباشراً لـ attacker، لكنه يكسر الـ least-privilege للمسؤولين.

**الاستنتاج:** F14 — **تم تحسينه (Bootstrap call أُضيف) لكن السلوك في login لم يُزَل**. لإصلاح كامل: إزالة الاستدعاء من `LoginViewModel.cs:116`.

---

### 3.14 F15 — KDF أحادي الجولة

**الكود الحالي** — `PasswordSecurity.cs`:

```csharp
public static string ComputeSha256(string password, string salt)
{
    using var sha = SHA256.Create();
    var bytes = Encoding.UTF8.GetBytes(password + "::" + salt);
    var hash = sha.ComputeHash(bytes);
    return Convert.ToHexString(hash);
}

public static bool Verify(string password, string salt, string expectedHash)
{
    var computed = ComputeSha256(password, salt);
    var left = Encoding.UTF8.GetBytes(computed);
    var right = Encoding.UTF8.GetBytes(expectedHash);
    return CryptographicOperations.FixedTimeEquals(left, right);
}
```

**تحسين جانبي:** `FixedTimeEquals` تحمي من **timing attacks** على المقارنة. هذا تحسين دفاعي حقيقي.

**الجوهر:** ما زال جولة واحدة من SHA-256 مع salt 16 بايت. لا PBKDF2، لا Argon2.

**الاستنتاج:** F15 — **لم يُصلَح**. تحسين دفاعي ضد timing فقط.

---

### 3.15 F16 — `PermissionCodes.FullAccess = "ALL"` السلسلة السحرية

**الكود الحالي** — `PermissionCodes.cs:7`:
```csharp
public const string FullAccess = "ALL";
```

لم يتغيَّر. لكن استهلاكها تحسَّن:

- `SessionContext.HasPermission` (السطور 35-39):
   ```csharp
   public bool HasPermission(string permissionCode)
   {
       return _grantedPermissions.Contains(PermissionCodes.FullAccess) ||
              _grantedPermissions.Contains(permissionCode);
   }
   ```
- `IsAdmin => HasPermission(PermissionCodes.FullAccess)` — مُحتسَب، لا setter.

**التحفظ:** أي كود يستطيع إقناع `SessionContext` بقبول `"ALL"` (عبر `BeginSession` أو عبر تسجيل دخول مستخدم منح له `SaveRolePermissionsAsync` صلاحية `"ALL"` — لا يوجد ما يمنع ذلك) يصبح Admin. لا توجد defense-in-depth إضافية (مثل "يجب أيضاً أن يكون عضواً في دور `Administrator`").

**الاستنتاج:** F16 — **تحسُّن معماري دون إصلاح الجذر**. الإصلاح الكامل يتطلَّب: (1) إضافة فحص "في دور Administrator" بجانب "يملك FullAccess"؛ أو (2) تحويل `FullAccess` إلى مفهوم محسوب من العضوية لا codeword قابل للمنح.

---

## 4. خطة الإصلاح المرتبة منطقياً (المتبقي فقط)

### 4.1 الـ Dependency Graph للمتبقي

```
                  ┌─────────────────────────────────────────────────┐
                  │  Phase R0: تحقق نظيف من نطاق الـ commit الحالي  │
                  │  + توثيق إعادة تسمية مضلِّلة لـ "إصلاح الثغرة"   │
                  └────────────────────────┬────────────────────────┘
                                           │
        ┌──────────────────────────────────┼──────────────────────────────────┐
        ▼                                  ▼                                  ▼
┌────────────────────┐         ┌──────────────────────────┐      ┌─────────────────────────┐
│ Phase R1: F8 — SQL │         │ Phase R2: F5 — تشديد     │      │ Phase R3: F6 — إزالة    │
│ credentials hardening│        │ تغليف SessionContext     │      │ مسار plain-text + إصلاح │
│ (مستقل)            │         │ + جعل static Current     │      │ UserAdminService.       │
│                    │         │ internal                 │      │ ApplyPassword           │
└─────────┬──────────┘         └────────────┬─────────────┘      └────────────┬────────────┘
          │                                 │                                 │
          │                                 ▼                                 ▼
          │            ┌──────────────────────────────────────────┐  ┌────────────────────┐
          │            │ Phase R4: F9 — تخويل على مستوى الخدمة     │  │ Phase R5: F12, F14│
          │            │ في UserAdminService (يعتمد على R2)        │  │ — إزالة username- │
          │            │                                          │  │ as-policy        │
          │            └────────────────────┬─────────────────────┘  └─────────┬──────────┘
          │                                 │                                 │
          │                                 └──────────────┬──────────────────┘
          │                                                ▼
          │                          ┌───────────────────────────────────────────┐
          │                          │ Phase R6: F15 — ترقية KDF                │
          │                          │ (PBKDF2/Argon2 + HashVersion)             │
          │                          └────────────────────┬──────────────────────┘
          │                                               │
          │                          ┌───────────────────────────────────────────┐
          └─────────────────────────►│ Phase R7: F10, F13 — Audit + Outcomes    │
                                     │ + Lockout (يعتمد على R2 + R6)            │
                                     └────────────────────┬──────────────────────┘
                                                          │
                                                          ▼
                                     ┌───────────────────────────────────────────┐
                                     │ Phase R8: F16 — إعادة تصميم FullAccess   │
                                     │ (يعتمد على R2 + R4 + R7)                  │
                                     └───────────────────────────────────────────┘
```

### 4.2 شرح كل Phase

#### **Phase R0 — توحيد قاعدة الحالة الراهنة**
- **الهدف:** قبل أي تغيير، توحيد فهم الفريق أن الـ commit الأخير (HEAD) **لا يُصلح أي ثغرة**؛ الإصلاح الفعلي وقع في `d8b043e`. إعادة تسمية الـ commit أو إضافة ملاحظة في الـ CHANGELOG.
- **الاعتماديات:** لا يوجد.
- **المخاطر:** لا توجد. إدارية بحتة.

#### **Phase R1 — F8 (SQL Credentials)**
- **الهدف:**
  - حذف `Password=og2026ra` من `App.config:4`.
  - استبدال `?? "og2026ra"` بـ `throw new InvalidOperationException(...)` في `OpenLabDbContext.cs:83` و `OpenLabDbContextFactory.cs:20`.
  - تدوير كلمة مرور SQL `sa` على جميع البيئات قبل النشر، و(يُفضَّل) إنشاء حساب least-privilege باسم `openlab_app`.
  - **لا تنسَ تدوير الـ secret في تاريخ Git أيضاً** — `og2026ra` ما زالت مكشوفة في الـ history.
- **الاعتماديات:** R0.
- **المخاطر:**
  - أي مثبَّت لم يضبط `OPENLAB_CONNECTION` أو `OPENLAB_DB_PASSWORD` سيتعطل عند الترقية. مطلوب توثيق نشر واضح.
  - الـ secret المكشوف في تاريخ Git لا يُحَل بـ commit واحد؛ يحتاج `git filter-repo` أو القبول بأن السر محروق ويُغيَّر فقط في الخادم.
- **الاختبارات المطلوبة:**
  - Unit test لـ `OpenLabDbContextFactory.CreateDbContext` يُرمي `InvalidOperationException` عندما لا يوجد env var.
  - Integration test يستخدم env var صحيحاً.
- **الأولوية:** **Critical** (سر مكشوف علناً).

#### **Phase R2 — F5 (تشديد تغليف الجلسة)**
- **الهدف:**
  - تحويل `SessionContext.UserId`, `Username`, `AttendanceLogId` إلى `internal set;`.
  - تحويل `static Current` إلى `internal static` أو إزالتها كلياً (الـ DI يحقن `IMutableSessionContext` للحاجة الداخلية في `LoginViewModel` و`MainViewModel` فقط).
  - إنشاء `LoginViewModel` و `MainViewModel` على `IMutableSessionContext` المحقون، بدلاً من `SessionContext.Current` المباشر.
  - فحص جميع ViewModels (30+ ملف) لإزالة أي `SessionContext.Current.UserId = ...` (لا توجد حالياً بعد بحث grep، لكن يجب التأكيد بعد التحويل).
- **الاعتماديات:** R0.
- **المخاطر:**
  - Test helpers (`AppSessionTestHelper`, `SqliteIntegrationTestBase`) ما زالوا يستخدمون `SessionContext.Current.BeginSession` — لا بد من تحويلهم لاستخدام test-only `IMutableSessionContext` instance.
  - WPF designer-time قد يعتمد على `static Current`.
- **الاختبارات:**
  - Unit test يُؤكد أن `SessionContext.Current` ليس مرئياً من خارج assembly.
  - Compile-time check: أي محاولة لـ `SessionContext.Current.UserId = X` خارج `LoginViewModel/MainViewModel` تفشل.

#### **Phase R3 — F6 (إزالة مسار plain-text)**
- **الهدف:**
  - إزالة الـ branch السطور 32-45 من `AuthService.cs`. سلوك جديد: `if (string.IsNullOrWhiteSpace(user.Salt)) return null;` (رفض مسار plain-text).
  - تحديث `UserAdminService.ApplyPassword`: عند `plainPassword` فارغ، رمي `ArgumentException` بدلاً من كتابة `""`. السلسلة `""` على `PasswordHash/Salt` يجب ألا تكون قيمة صالحة أبداً.
  - **عكس الاختبار `ValidateCredentials_WithLegacyPlainText_ShouldMigrateToHash`** ليؤكد أن الـ migration لم يعد ممكناً (أو حذفه إذا كان legacy support غير مطلوب).
- **الاعتماديات:** R0 (مستقل عن R2 لكن أسهل بعده).
- **المخاطر:**
  - أي مستخدمين قدامى في DB إنتاجية بـ `Salt=""` سيخسرون القدرة على تسجيل الدخول. مطلوب data migration script يبحث عن `Salt=""` ويضع `IsActive=false` + يُرسل تنبيهاً للمسؤول.
  - الـ `Open_lab.AdminCli` يجب أن يبقى الطريق الوحيد لإعادة التعيين.
- **الاختبارات:**
  - Unit test جديد: `ValidateCredentials_WithEmptySalt_ShouldReturnNull`.
  - Unit test جديد: `CreateUserAsync_WithBlankPassword_ShouldThrow`.

#### **Phase R4 — F9 (تخويل على مستوى الخدمة)**
- **الهدف:**
  - حقن `ISessionContext` في `UserAdminService` عبر الـ constructor.
  - إضافة `EnsurePermission(...)` private method:
    ```csharp
    private void EnsureUsersEdit() => 
        _sessionContext.HasPermission(PermissionCodes.UsersEdit).Should().BeTrue() // مثل
        ?? throw new UnauthorizedAccessException(...);
    ```
  - استدعاء `EnsureUsersEdit()` في بداية: `CreateUserAsync`, `UpdateUserAsync`, `DeleteUserAsync`, `AssignSingleRoleAsync`, `RemoveUserRoleAsync`, `SaveRolePermissionsAsync`, `CreateRoleAsync`, `DeleteRoleAsync`.
  - الـ ViewModels تبقي `canExecute` كـ defense-in-depth UI.
- **الاعتماديات:** R2 (السياق المعتمَد عليه).
- **المخاطر:**
  - مستخدمون يعتمدون على الـ caller-trust الحالي (Tests, CLI) سيُكسرون. مطلوب رفع امتيازات صريح في `AppSessionTestHelper` و `BootstrapViewModel` (الأخير يستدعي قبل وجود session).
  - **مشكلة خاصة بـ Bootstrap:** `BootstrapViewModel.CreateAdminAsync` يستدعي `_userAdminService.CreateUserAsync` قبل أن تكون هناك جلسة. حل: ممرّ `IBootstrapBypass` أو وضع علم `IsBootstrapping` في `IMutableSessionContext` يُحَكَّم عليه في `EnsureUsersEdit`.
- **الاختبارات:**
  - Unit test: استدعاء `UserAdminService.CreateUserAsync` بدون session نشطة → `UnauthorizedAccessException`.
  - Integration test: bootstrap flow ينجح برغم الـ gating.

#### **Phase R5 — F12 + F14 (إزالة username-as-policy)**
- **الهدف:**
  - في `LoginViewModel.cs:113-117`: استبدال الـ string compare بـ permission check:
    ```csharp
    var permissionCodes = await _authorizationService.GetPermissionCodesAsync(user.UserId);
    // EnsureAdminAccessAsync يُحذف من هنا تماماً.
    SessionContext.Current.BeginSession(user.UserId, user.Username, permissionCodes);
    ```
  - الـ `EnsureAdminAccessAsync` يصبح bootstrap-only (يُستدعى فقط من `BootstrapViewModel`).
  - **توصية إضافية:** فصل "إنشاء الدور والصلاحيات أول مرة" عن "ضمان وجود الصلاحيات" — الثاني يجب ألا يحدث في تدفق المصادقة أبداً.
- **الاعتماديات:** R4 (لمنع وصول مكالمات غير-مخوَّلة لـ `EnsureAdminAccessAsync` إن استُدعِيت من جديد).
- **المخاطر:** المسؤول الذي اعتاد على "تسجيل دخول يُصلح صلاحياتي" سيحتاج طريقاً جديداً — مثل زر "إعادة تطبيق صلاحيات Administrator" في `UsersPermissionsView`، مُخوَّل بـ `UsersEdit`.

#### **Phase R6 — F15 (ترقية KDF)**
- **الهدف:**
  - استبدال `ComputeSha256` بـ `Rfc2898DeriveBytes` (PBKDF2-HMAC-SHA256) بـ 600,000 جولة (OWASP 2023 guidance) كحدّ أدنى افتراضي.
  - إضافة عمود `HashVersion` إلى `Users` (default = 1 للـ legacy).
  - `AuthService` يقبل version 1 لجولة واحدة فقط، ويُعيد التشفير إلى version 2 بعد التحقق الناجح.
  - بعد فترة (90 يوم مثلاً قابلة للتكوين)، رفض version 1 مع توجيه للـ `AdminCli --reset-admin`.
- **الاعتماديات:** R3 (إزالة المسار القديم) + R2 (الـ context النظيف).
- **المخاطر:**
  - الأداء: PBKDF2 600k = ~200-500ms على workstations عادية. يجب benchmark على أضعف هاردوير منتشر.
  - تتطلب EF migration جديد.

#### **Phase R7 — F10 + F13 (Audit + Outcomes + Lockout)**
- **الهدف:**
  - `AuditInterceptor.cs:43`: استبدال `?? 1` بـ `null` ودعم nullable في `AuditLog.UserId`. أو ضخ principle synthetic مع `Username="system"`.
  - حقن `IMutableSessionContext` في `AuditInterceptor` عبر factory أو DbContext extension.
  - تعريف `AuthOutcome` (enum + DTO):
    ```csharp
    public enum AuthOutcome { Success, UserNotFound, BadPassword, Inactive, Locked, LegacyUpgraded }
    public record AuthResult(AuthOutcome Outcome, User? User);
    ```
  - توسيع `IAuthService` ليُرجع `Task<AuthResult>` بدلاً من `Task<User?>`.
  - عمودان جديدان على `Users`: `FailedLoginAttempts`, `LockedUntilUtc`. منطق exponential backoff.
- **الاعتماديات:** R2 + R6.
- **المخاطر:**
  - downstream consumers لـ `AuditLog.UserId` (UserActivityService, StatisticsService, DashboardService) تحتاج null-handling. هذه نقطة فحص حرجة.
  - تحويل return type لـ `ValidateCredentialsAsync` يكسر كل المستخدمين — مطلوب overload أو deprecation period.

#### **Phase R8 — F16 (إعادة تصميم FullAccess)**
- **الهدف:**
  - حذف ثابت `PermissionCodes.FullAccess = "ALL"`.
  - استبدال `IsAdmin` بـ خاصية محسوبة من **العضوية في دور `Administrator`** (يُحضَر من DB أو يُمرَّر صراحةً).
  - في `SaveRolePermissionsAsync`: رفض إضافة `"ALL"` كقيمة (تحقُّق إضافي حتى لو ظلَّت موجودة كقيمة legacy).
- **الاعتماديات:** R4 + R7.
- **المخاطر:**
  - كسر مفاجئ لأي بيانات إنتاجية تحتوي `"ALL"` في `RolePermissions`. مطلوب data migration: استبدال `"ALL"` بـ عضوية دور `Administrator` + ربط المستخدمين الحاملين له.

### 4.3 جدول مُلخَّص

| Phase | العنوان | الاعتماديات | الأولوية | جهد تقديري | مخاطر |
|------|---------|-------------|----------|------------|--------|
| R0   | توحيد الحالة | — | Critical | منخفض | منخفضة |
| R1   | F8 SQL credentials | R0 | Critical | متوسط | متوسطة (نشر) |
| R2   | F5 تشديد الجلسة | R0 | High | متوسط–عالٍ | متوسطة (test bleed) |
| R3   | F6 إزالة plain-text | R0 | High | متوسط | متوسطة (data migration) |
| R4   | F9 تخويل خدمة | R2 | High | متوسط–عالٍ | عالية (Bootstrap pass-through) |
| R5   | F12+F14 إزالة username-as-policy | R4 | Medium | منخفض | منخفضة |
| R6   | F15 KDF | R3 + R2 | Medium | متوسط–عالٍ | متوسطة (أداء + migration) |
| R7   | F10+F13 Audit + Outcomes | R2 + R6 | Medium | عالٍ | عالية (breaking change) |
| R8   | F16 إعادة تصميم FullAccess | R4 + R7 | Medium | متوسط | عالية (data migration) |

---

## 5. دليل المعالجة الاحترافي لكل ثغرة متبقية

### 5.1 F6 — مسار النص الصريح القديم

**الهدف الأمني:** ضمان أن أي مستخدم بـ `Salt=""` و/أو `PasswordHash=""` غير قابل للمصادقة، وعدم إنشاء مستخدمين في هذه الحالة أصلاً.

**السبب الجذري:** `AuthService` يقبل المسار plain-text كـ migration path، و`UserAdminService.ApplyPassword` يكتب `""` عند كلمة مرور فارغة. السلوكان معاً يخلقان exploit chain.

**الملفات المتأثرة:**
- `Open_lab/Services/AuthService.cs` (السطور 31-45)
- `Open_lab/Services/UserAdminService.cs` (السطور 239-251)
- `Open_lab.Tests/Services/AuthServiceTests.cs` (الاختبار `ValidateCredentials_WithLegacyPlainText_ShouldMigrateToHash` السطور 70-95)
- `Open_lab.Tests/Services/UserAdminServiceTests.cs` (إن وُجد فحص لـ blank password)

**التعديلات المطلوبة:**
- في `AuthService`: حذف الـ if-block السطور 32-45 كاملاً. استبدالها بـ:
  ```csharp
  if (string.IsNullOrWhiteSpace(user.Salt) || string.IsNullOrWhiteSpace(user.PasswordHash))
  {
      return null;   // accounts in inconsistent state cannot authenticate
  }
  ```
- في `UserAdminService.ApplyPassword`: عند `IsNullOrWhiteSpace(plainPassword)`، رمي `ArgumentException("Password is required for user creation/update.")` بدلاً من كتابة `""`.
- عكس / حذف الاختبار `ValidateCredentials_WithLegacyPlainText_ShouldMigrateToHash`. الاختبار البديل: `ValidateCredentials_WithEmptySalt_ShouldReturnNull`.

**التأثيرات الجانبية المتوقعة:**
- سيناريوهات تنشئ مستخدماً بلا كلمة مرور (إن وُجدت في UI أو wizards) ستحتاج تعديل.
- بيانات إنتاجية بها صفوف `Salt=""` — مطلوب data migration: إيقاف هؤلاء المستخدمين (`IsActive=false`) وإلزام مسؤول بإعادة تعيين كلمة المرور عبر `AdminCli` أو `UsersPermissionsView`.

**متطلبات الاختبارات:**
- Unit: `ValidateCredentials_WithEmptySalt_ShouldReturnNull` (إضافة جديدة).
- Unit: `ValidateCredentials_WithEmptyPasswordHash_ShouldReturnNull` (إضافة جديدة).
- Unit: `CreateUserAsync_WithBlankPassword_ShouldThrowArgumentException` (إضافة جديدة).
- Integration: تجربة تسجيل دخول لمستخدم migrated → فشل.

**أولوية التنفيذ:** **High**.

---

### 5.2 F8 — كلمات مرور SQL مُضمَّنة

**الهدف الأمني:** عدم وجود أي سر إنتاجي في المستودع، وفشل صريح عند غياب التهيئة.

**السبب الجذري:** `App.config` يحوي كلمة المرور كاملةً + كلا `OpenLabDbContext.cs:83` و `OpenLabDbContextFactory.cs:20` يستخدمان `?? "og2026ra"` كـ fallback.

**الملفات المتأثرة:**
- `Open_lab/App.config` (السطر 4)
- `Open_lab/Data/OpenLabDbContext.cs` (السطور 75-91)
- `Open_lab/Data/OpenLabDbContextFactory.cs` (السطور 11-30)
- `Open_lab/Docs/` (توثيق نشر)

**التعديلات المطلوبة:**
- `App.config:4`: استبدال السلسلة بـ placeholder صريح أو حذف العنصر بالكامل وتوثيق أن `OPENLAB_CONNECTION` env var مطلوب. مثال:
  ```xml
  <add name="OpenLabDb" connectionString="" providerName="Microsoft.Data.SqlClient" />
  ```
- في `OpenLabDbContext.cs:81-85` و `OpenLabDbContextFactory.cs:17-22`: استبدال الـ fallback بـ:
  ```csharp
  if (string.IsNullOrWhiteSpace(connectionString))
  {
      throw new InvalidOperationException(
          "Database connection string is not configured. " +
          "Set the OPENLAB_CONNECTION environment variable or provide a connection string in App.config.");
  }
  ```
- توثيق نشر جديد يشرح كيفية ضبط env var على Windows.
- **تدوير سرّ `sa` على كل بيئة نشر قبل النشر**، وتغيير المستخدم من `sa` إلى `openlab_app` بصلاحيات `db_datareader + db_datawriter` فقط.

**التأثيرات الجانبية:**
- أي مثبَّت لم يحدِّث env var سيفشل عند بدء التشغيل برسالة واضحة.
- البيئات التطويرية تحتاج توجيهاً واضحاً.
- تاريخ Git ما زال يحوي السر — الحل الكامل يتطلب `git filter-repo` (يعيد كتابة التاريخ) أو القبول بأن السر محروق ولن يُستعمل بعد الآن.

**متطلبات الاختبارات:**
- Unit: `OpenLabDbContextFactory_WithoutEnvVar_ShouldThrow`.
- Integration: تشغيل ناجح بـ `OPENLAB_CONNECTION` صحيح.

**أولوية التنفيذ:** **Critical** (السر مكشوف علناً على GitHub).

---

### 5.3 F9 — تخويل على مستوى الخدمة

**الهدف الأمني:** كل عملية تغيير امتيازات في `UserAdminService` تتطلب صلاحية صريحة في الجلسة الحالية، بغض النظر عن مصدر الاستدعاء (UI / CLI / Test).

**السبب الجذري:** `UserAdminService` يثق في caller للتخويل. الـ ViewModel فقط هو الـ guard.

**الملفات المتأثرة:**
- `Open_lab/Services/UserAdminService.cs` (8 دوال)
- `Open_lab/Services/IUserAdminService.cs` (إن غيَّرت signatures)
- `Open_lab/ViewModels/BootstrapViewModel.cs` (تمرير bypass للـ bootstrap)
- `Open_lab.Tests/Services/UserAdminServiceTests.cs`
- `Open_lab.Tests/Infrastructure/AppSessionTestHelper.cs`

**التعديلات المطلوبة:**
- إضافة `ISessionContext` معامل constructor:
  ```csharp
  public UserAdminService(OpenLabDbContext db, ISessionContext sessionContext) { ... }
  ```
- إضافة private guard:
  ```csharp
  private void EnsureCanEditUsers()
  {
      if (!_sessionContext.HasPermission(PermissionCodes.UsersEdit))
          throw new UnauthorizedAccessException("UsersEdit permission required.");
  }
  ```
- تطبيق `EnsureCanEditUsers()` في بداية: `CreateUserAsync`, `UpdateUserAsync`, `DeleteUserAsync`, `AssignSingleRoleAsync`, `RemoveUserRoleAsync`, `SaveRolePermissionsAsync`, `CreateRoleAsync`, `DeleteRoleAsync`.
- **حل مشكلة Bootstrap:** إضافة flag `IsBootstrapping` في `IMutableSessionContext` ينتقل إلى true داخل `BootstrapViewModel.CreateAdminAsync` ويُعاد إلى false بعد `MarkBootstrapCompleteAsync`. `EnsureCanEditUsers` تتجاوز عندما `IsBootstrapping=true`.
- **تشديد إضافي خاص بـ `SaveRolePermissionsAsync`:** بالإضافة لـ `UsersEdit`، رفض القيمة `"ALL"` في الـ permission codes (يصبح خيار R8 لاحقاً).

**التأثيرات الجانبية:**
- اختبارات تستخدم `UserAdminService` مباشرة بدون session نشطة ستفشل. حل: استدعاء `AppSessionTestHelper.ResetToAdmin()` في `[Setup]`.
- CLI الأمن (`Open_lab.AdminCli`) يجب أن يبدأ جلسة وهمية بصلاحيات كاملة قبل استدعاء `UserAdminService`. حالياً يستخدم الخدمات مباشرة دون SessionContext.

**متطلبات الاختبارات:**
- Unit لكل دالة: استدعاء بدون صلاحيات → `UnauthorizedAccessException`.
- Integration: Bootstrap ينجح بـ flag `IsBootstrapping`.
- Integration: مستخدم بـ `UsersView` فقط (بدون `UsersEdit`) يفشل في تعديل.

**أولوية التنفيذ:** **High**.

---

### 5.4 F10 — `AuditInterceptor` يستخدم `?? 1` ولا يُحقن

**الهدف الأمني:** عدم إسناد أي حدث audit لمستخدم غير محدَّد. إذا لم يكن هناك سياق، يجب توضيح ذلك صراحةً (null، أو principal synthetic).

**السبب الجذري:** الـ Interceptor يُنشَأ في `OnConfiguring` بـ `new AuditInterceptor()` (لا DI، لا session).

**الملفات المتأثرة:**
- `Open_lab/Data/AuditInterceptor.cs`
- `Open_lab/Data/OpenLabDbContext.cs` (السطور 88-91)
- `Open_lab/Models/AuditLog.cs` (إن تطلَّب nullable)
- `Open_lab/App.xaml.cs` (DI registration للـ Interceptor إن نُقل لـ DI)
- جميع مستهلكي `AuditLog.UserId`: `UserActivityService.cs`, `StatisticsService.cs`, `DashboardService.cs` — يجب فحصها لـ null-handling.

**التعديلات المطلوبة:**
- `AuditInterceptor`: حذف الـ `?? 1` بالكامل. إن كان `_currentUserId` و `openLabContext.CurrentUserId` كلاهما `null`، إما (1) رمي `InvalidOperationException("Audit context not configured")`، أو (2) جعل `AuditLog.UserId` nullable وتخزين `null`.
- نقل بناء `AuditInterceptor` من `OnConfiguring` إلى `App.xaml.cs` عبر:
  ```csharp
  services.AddTransient<OpenLabDbContext>(sp =>
  {
      var session = sp.GetRequiredService<ISessionContext>();
      var interceptor = new AuditInterceptor(session.UserId > 0 ? session.UserId : (int?)null);
      ...
  });
  ```
- EF migration: تغيير `AuditLog.UserId` ليكون nullable + تغيير الـ navigation property.

**التأثيرات الجانبية:**
- Reports/Dashboards تعرض user="غير معروف" بدلاً من user=1. تحديثات نصية للـ UI.
- محتمل breakage في tests تعتمد على `UserId=1`.

**متطلبات الاختبارات:**
- Unit: حفظ DB بدون session → `AuditLog.UserId=null`.
- Unit: `UserActivityService.GetActivitiesByUser` يتخطى الصفوف null.
- Integration: تسلسل Bootstrap → كل المنشآت الأولية مرفوعة بـ `Username="system"` لا UserId=1.

**أولوية التنفيذ:** Medium (لا أثر مصادقة مباشر لكن يُلوّث الـ audit trail).

---

### 5.5 F12 + F14 — Username-as-policy + Re-grant on every login

**الهدف الأمني:** "صلاحيات Admin" تُحَدَّد من المُلكية الفعلية للصلاحيات، لا من الـ username string. `EnsureAdminAccessAsync` يُستدعى مرة واحدة عند Bootstrap.

**الملفات المتأثرة:**
- `Open_lab/ViewModels/LoginViewModel.cs` (السطور 113-117)
- `Open_lab/Services/IAdminSetupService.cs` (نقاش حول إبقاء أو إخفاء `EnsureAdminAccessAsync`)
- `Open_lab/ViewModels/UsersPermissionsViewModel.cs` (إضافة زر "إعادة تطبيق صلاحيات Administrator")
- `Open_lab.Tests/ViewModels/LoginViewModelTests.cs`

**التعديلات المطلوبة:**
- استبدال السطور 113-117 في `LoginViewModel.cs` بـ:
  ```csharp
  var permissionCodes = await _authorizationService.GetPermissionCodesAsync(user.UserId);
  // لا استدعاء لـ EnsureAdminAccessAsync هنا
  SessionContext.Current.BeginSession(user.UserId, user.Username, permissionCodes);
  ```
- إبقاء `EnsureAdminAccessAsync` متاحة عبر `IAdminSetupService` لكن استدعاؤها من `BootstrapViewModel` فقط (موجود حالياً) + من زر يدوي في `UsersPermissionsViewModel` (جديد، مخوَّل بـ `UsersEdit`).

**التأثيرات الجانبية:**
- المسؤول الذي اعتاد على "تسجيل دخول يُصلح صلاحياتي" يحتاج توجيهاً لاستخدام الزر الجديد.

**متطلبات الاختبارات:**
- Unit: `LoginViewModel.LoginAsync` لا يستدعي `_adminSetupService.EnsureAdminAccessAsync`.
- Integration: مستخدم بدون `Administrator` يدخل بصلاحياته الموجودة فقط دون تجديد.

**أولوية التنفيذ:** Medium.

---

### 5.6 F15 — ترقية KDF إلى PBKDF2

**الهدف الأمني:** كلمات مرور الـ DB المسرَّبة تكلِّف أيام/أسابيع cracking بدلاً من ثوانٍ.

**الملفات المتأثرة:**
- `Open_lab/Services/PasswordSecurity.cs`
- `Open_lab/Services/AuthService.cs` (multi-version verify)
- `Open_lab/Services/UserAdminService.cs` (`ApplyPassword` يكتب version 2 افتراضياً)
- `Open_lab/Models/User.cs` (عمود `HashVersion`)
- `Open_lab/Migrations/` (migration جديد)
- `Open_lab.Tests/Services/PasswordSecurityTests.cs` (إن وُجد)

**التعديلات المطلوبة:**
- إضافة `ComputePbkdf2(password, salt, iterations)` يستخدم `Rfc2898DeriveBytes` بـ 600,000 iteration افتراضياً (قابل للتكوين عبر `SystemSetting Security.Pbkdf2Iterations`).
- إضافة `Verify(password, salt, expectedHash, version)`:
  - version 1 → SHA-256 (الـ legacy).
  - version 2 → PBKDF2.
- عمود `User.HashVersion` (default = 2 للجديد، 1 للموجود).
- `AuthService` يفحص الـ version، يَتحقَّق وفقاً لها. إذا version 1 ونجح: يُعيد التشفير بـ version 2 ويُحدِّث.
- بعد فترة سماح (90 يوماً) يُرفض version 1.

**التأثيرات الجانبية:**
- زمن login يرتفع 200-500ms. يجب benchmark.
- EF migration كبير.

**متطلبات الاختبارات:**
- Unit: `Verify_WithVersion1_AcceptsSha256`.
- Unit: `Verify_WithVersion2_AcceptsPbkdf2`.
- Integration: تسجيل دخول legacy → التحقق ينجح + HashVersion يصبح 2.
- Benchmark: قياس latency على ضعف هاردوير.

**أولوية التنفيذ:** Medium.

---

### 5.7 F13 — توسيع `IAuthService` (Outcomes + Lockout)

**الهدف الأمني:** تمكين Audit trail منسوب الدقة، وحماية أساسية ضد brute-force.

**الملفات المتأثرة:**
- `Open_lab/Services/IAuthService.cs`
- `Open_lab/Services/AuthService.cs`
- `Open_lab/Models/User.cs` (أعمدة `FailedLoginAttempts`, `LockedUntilUtc`)
- `Open_lab/Migrations/` (migration جديد)
- `Open_lab/ViewModels/LoginViewModel.cs` (التمييز بين outcomes في رسائل الخطأ — لكن دون تسريب معلومات: لا تفصح أن المستخدم موجود لكن كلمة المرور خطأ)
- `Open_lab.Tests/Services/AuthServiceTests.cs`

**التعديلات المطلوبة:**
- إضافة `enum AuthOutcome` و `record AuthResult(AuthOutcome, User?)`.
- توسيع `IAuthService` بـ `Task<AuthResult> AuthenticateAsync(string, string)` (overload يحتفظ بـ legacy method temporarily).
- إضافة منطق lockout: 5 محاولات فاشلة → قفل لمدة 5 دقائق + exponential.

**التأثيرات الجانبية:**
- `LoginViewModel` يحتاج تحديثاً للتعامل مع outcomes.
- اختبارات تعتمد على `User?` تحتاج تعديل.

**متطلبات الاختبارات:**
- Unit لكل outcome.
- Unit: 5 محاولات فاشلة → 6th محاولة Locked outcome.
- Unit: lockout ينتهي بعد المدة.

**أولوية التنفيذ:** Medium.

---

### 5.8 F16 — إعادة تصميم `FullAccess`

**الهدف الأمني:** "الإدارة" تعتمد على عضوية دور `Administrator`، لا على سلسلة `"ALL"` قابلة للمنح لأي دور.

**الملفات المتأثرة:**
- `Open_lab/Services/PermissionCodes.cs`
- `Open_lab/Services/SessionContext.cs`
- `Open_lab/Services/AuthorizationService.cs`
- `Open_lab/Services/AdminSetupService.cs`
- `Open_lab/Services/UserAdminService.cs.SaveRolePermissionsAsync`
- بيانات إنتاجية: data migration لاستبدال صفوف `RolePermissions.PermissionCode="ALL"`.

**التعديلات المطلوبة:**
- حذف `FullAccess = "ALL"` من `PermissionCodes`، أو إبقاؤه `[Obsolete]` مع رفض الإضافة عبر `SaveRolePermissionsAsync`.
- جعل `IsAdmin` يأخذ `bool isMemberOfAdministratorRole` كقيمة مُمَرَّرة من `LoginViewModel` (بعد فحص العضوية في DB) ويُخزَّن في `SessionContext`.
- `HasPermission` تتحقق من العضوية أيضاً قبل short-circuit.
- Data migration: لكل صف `PermissionCode="ALL"`، إنشاء `UserRole` رابط للـ `Administrator` بدلاً من ذلك، ثم حذف الصف.

**التأثيرات الجانبية:**
- Breaking change لأي بيانات إنتاجية موجودة.
- إعادة هيكلة AuthorizationService.

**متطلبات الاختبارات:**
- Unit: مستخدم بصلاحية `"ALL"` فقط (دون عضوية Administrator) → `IsAdmin=false`.
- Unit: عضو Administrator → `IsAdmin=true`.
- Migration test: قاعدة بـ `"ALL"` legacy تتحول بشكل صحيح.

**أولوية التنفيذ:** Medium.

---

## 6. الخلاصة والتوصيات الفنية

### 6.1 تقييم الوضع الأمني الحالي

مقارنةً بحالة الكود عند الـ commit المرجعي `a00591e`، حدث **تحسُّن أمني جوهري** على مستوى المعمارية والمصادقة الرئيسية:

- **الـ Backdoor الإداري (F1, F2, F3)** المُصنَّف الأشد خطورة في التقرير المرجعي **أُزيل بالكامل** وبشكل صحيح ومتسق، مع عكس الاختبارات المُلازِمة في نفس الـ change-set — وهو ما طالب به التقرير المرجعي حرفياً في §11.5.
- **مسار Bootstrap (F11)** الذي كان منعدماً تماماً، أصبح كاملاً مع UI مخصصة، علامة state، اختبارات تكامل، ومسار CLI مكافئ للحالات الخارجة عن UI.
- **مشكلة الـ Master Password (F7)** التي كانت كسراً منطقياً تَعَطَّل، أُصلحت بإعادة تصميم flow.
- **`AppSession` الـ static** (F5) أُحيلت إلى تجريد `ISessionContext` مع `IsAdmin` كخاصية محسوبة — تحسُّن معماري كبير.

في المقابل، الـ commit الأخير ذي العنوان المثير "إصلاح الثغرة الأولى" **لا يُصلح أي ثغرة**؛ هو تنسيق بصري بحت في `App.config`. هذا يستدعي مراجعة ممارسات تسمية الـ commits والـ PR لمنع التضليل.

**الـ profile الأمني الحالي:**

- الـ critical-severity findings: **تراجَعَت من 3 إلى 0**.
- الـ high-severity findings: **تراجَعَت من 5 إلى 3** (F6, F8, F11→صفر). متبقي: F6, F8, جزء من F5.
- الـ medium-severity findings: **تراجَعَت من 7 إلى 6** (F14 تحسَّن جزئياً). متبقي: F9, F10, F12, F13, F14, F15, F16.

### 6.2 المخاطر الحرجة المتبقية

1. **F8 — كلمة مرور SQL `og2026ra`** هي أعلى خطر متبقٍ لأن السر **مفضوح علنياً** على GitHub. أي قارئ غير-موثوق يمكنه أن يحاول استخدامها على أي تثبيت إنتاجي لم يدوّرها. **هذا التصحيح يجب أن يكون أول أولوية فورية**.
2. **F9 — غياب تخويل على مستوى الخدمة** يفتح مسار تصاعد امتيازات من أي مستهلك يتجاوز UI (CLI مستقبلي، DLL plug-in، اختبار يُنفَّذ ضمن assembly نفسه).
3. **F6 — مسار plain-text** يفتح ثغرة كلمة مرور فارغة في وجه أي caller غير-UI.

### 6.3 تقييم قابلية المشروع للتوسع الآمن

المعمارية الحالية بعد التصحيحات أصبحت **أكثر قابلية للتوسع الآمن** مما كانت عليه:

- وجود `ISessionContext` المحقون يسمح بإضافة طبقات middleware (logging, throttling, multi-tenancy) دون تعديل ViewModels.
- وجود مسار Bootstrap منفصل يفصل التهيئة عن المصادقة.
- وجود `Open_lab.AdminCli` يفتح باب لـ deployment automation آمن.

**لكن** عدد من القيود ما زالت تُعيق التوسع:

- غياب طبقة `IAuthorizationFilter` أو attribute-based authorization يجبر الفحوصات على المستهلك (مزيد من boilerplate).
- `IAuthService` الرفيع لا يدعم أي scenarios متقدمة (SSO، Multi-Factor، API tokens).
- `AuditInterceptor` الـ statically constructed يصعِّب تمييز sources مختلفة (UI vs. CLI vs. Background).

### 6.4 توصيات الحوكمة الأمنية المستقبلية

1. **سياسة تسمية الـ commits:** كل commit يدّعي إصلاح ثغرة يجب أن يحوي مرجعاً صريحاً لمعرّف الثغرة (F1, F8, …) ووصفاً لما تم وما تأكَّد. الـ commit `7bcda0c` "إصلاح الثغرة الأولى" يخالف هذه السياسة.
2. **سياسة "تصحيح + اختبار في PR واحد":** يحظَّر دمج تغيير production-code يعالج ثغرة دون عكس/إضافة اختبار يثبت السلوك الجديد. هذا تطبَّق بنجاح على F1 و F3 — يجب جعله قاعدة.
3. **قائمة فحص PR أمنية:** أي PR يلمس `AuthService`, `SessionContext`, `UserAdminService`, `AdminSetupService`, `PasswordSecurity`, `AuditInterceptor`, أو `App.config` يتطلب مراجعة من security-aware reviewer ثانٍ.
4. **منع الـ secrets في git:** تفعيل `gitleaks` أو `trufflehog` كـ pre-commit hook لمنع تكرار أخطاء مثل `og2026ra`.

### 6.5 توصيات Secure SDLC

- **Threat Modeling مرة كل ربع:** مراجعة جدول `PermissionCodes` وعلاقاته بـ Roles + UserRoles لاكتشاف drift.
- **SAST في CI:** إضافة `dotnet-security-scan` أو `Roslyn analyzers` (Microsoft.CodeAnalysis.NetAnalyzers مع CA2100-CA5400 series) على كل push.
- **Dependency scanning:** `dotnet list package --vulnerable` كـ CI step.
- **Penetration test سنوي:** خصوصاً على Bootstrap flow و AdminCli (أي مسار يستطيع إنشاء/إعادة تعيين admin هو هدف أساسي).

### 6.6 توصيات مراجعات الكود الأمنية المستقبلية

1. **عند أول PR يلمس `AuthService`:** التأكد من عدم إعادة إدخال أي `string.Equals(password, hardcoded, …)`. اعتبار هذا "لا-عودة" في القاعدة.
2. **عند أول PR يلمس `SessionContext`:** التأكد من بقاء `IsAdmin` محسوبة (لا setter)، وأن أي setter جديد على هوية الجلسة يكون `internal` على الأقل.
3. **عند أول PR يضيف Permission جديد:** التأكد من تسجيله في `PermissionCodes.All` (الذي يستهلكه `EnsureAdminAccessAsync`) — وإلا الـ Administrator role لن يحصل عليه تلقائياً.
4. **مراجعة دورية لـ `AdminCli`:** أي بدائل لـ `--reset-admin` تُضاف يجب أن تتطلب `RESET-ADMIN` confirmation prompt على الأقل، وتنشئ سجل audit صريح.

---

## 7. ملاحظات ختامية على الـ Engagement

- لا تعديلات تم إجراؤها على كود الإنتاج، الاختبارات، الـ migrations، أو الـ workflows. الملف الوحيد المُنتَج عبر هذه المهمة هو هذا التقرير (`The_final_completely_analysis_for_open_lab.md`).
- جميع الاستنتاجات مستندة إلى قراءة مباشرة للكود في HEAD `7bcda0c` على فرع `Fi5ve`. كل سطر مذكور بإحداثيات (file:line) دقيقة على هذا الـ commit.
- في حالات عدم اليقين (مثل: هل سلوك UserAdminService.UpdateUserAsync:100 مقصود كـ defense-in-depth أم باقٍ من الـ backdoor القديم؟) تم تسجيل عدم اليقين صراحةً بدلاً من إصدار حكم قاطع.
- التقرير اعتمد التصنيف الرباعي المطلوب: ✅ تم الإصلاح بالكامل / 🟡 تم الإصلاح جزئياً / ❌ لم يُصلَح / ⚠️ تم تغيير التنفيذ لكن الخطر ما زال قائماً.
- التوصية الفنية النهائية: **البدء فوراً بـ Phase R1 (F8 SQL credentials)** قبل أي عمل آخر، لأن السر `og2026ra` مكشوف علنياً ويمثِّل المخاطرة الأعلى احتمالاً للاستغلال الخارجي. باقي الـ phases يمكن جدولتها وفق سعة الفريق.

---

*نهاية التقرير — تدقيق أمني مستقل + تحقق من التصحيحات + تخطيط معالجة فقط. لم يُعدَّل أي كود مصدر أو تكوين أو اختبار أو migration أو dependency. الأثر الوحيد لهذه المهمة هو هذا الملف.*