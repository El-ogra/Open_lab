# وثيقة اعتماد هندسي وتحليل أمني معماري لمعالجة F9

**الثغرة المختارة:** F9 — غياب التخويل على مستوى `UserAdminService`  
**نوع الوثيقة:** تحليل معماري وتخطيط إصلاح فقط  
**حالة التنفيذ:** لا تتضمن هذه الوثيقة أي تعديل برمجي على كود الإنتاج أو الاختبارات أو الإعدادات  
**المرجع الأمني:** `Open_lab/Docs/The_final_completely_analysis_for_open_lab.md`  

---

## 1. مبررات اختيار الثغرة الفنية

### 1.1 تحديد الثغرة

الثغرة F9 كما وردت في التقرير الشامل هي غياب التخويل داخل `UserAdminService`. الحالة الحالية تعتمد على أن طبقة الواجهة `UsersPermissionsViewModel` تمنع تنفيذ أوامر الإدارة عندما لا يملك المستخدم `PermissionCodes.UsersEdit` أو `PermissionCodes.UsersView`. لكن الخدمة نفسها لا تتحقق من صلاحيات المستدعي قبل تنفيذ عمليات عالية الحساسية مثل إنشاء المستخدمين، تعديل المستخدمين، حذفهم، إنشاء الأدوار، حذف الأدوار، تعيين الأدوار، وحفظ صلاحيات الأدوار.

هذا يعني أن التخويل الحالي موجود عند حافة UI فقط، وليس عند حد الثقة الحقيقي: service boundary.

### 1.2 لماذا F9 هي الأولوية التالية بعد F8

بعد إغلاق F8 عملياً، تصبح F9 هي الثغرة التالية الأكثر منطقية للأسباب التالية:

- التقرير المرجعي صنف F9 كثاني خطر جوهري بعد F8 في قسم المخاطر الحرجة المتبقية، لأنها تسمح لأي مستهلك يتجاوز UI باستدعاء عمليات إدارية مباشرة.
- F9 ليست خللاً موضعياً في دالة واحدة، بل كسر في طبقة الثقة بين ViewModel والخدمات. إصلاحها يرفع مستوى الأمان المعماري للنظام كله.
- F9 تضخم أثر ثغرات أخرى متبقية. على سبيل المثال، `SaveRolePermissionsAsync` يستطيع منح `PermissionCodes.FullAccess = "ALL"` لأي دور، وهذا يضاعف خطر F16. كما أن `CreateUserAsync` مع كلمة مرور فارغة يرتبط بسلسلة F6.
- إصلاح F9 يوفر defense-in-depth حتى لو بقيت واجهات UI صحيحة، لأن الخدمات لن تثق بالمستدعي تلقائياً.
- هذه الثغرة تمس إدارة المستخدمين والأدوار، وهي أكثر منطقة حساسية بعد إزالة الأسرار وقبل ترقية KDF أو إعادة تصميم `FullAccess`.

الاختيار ليس تجاهلاً لـ F6. F6 لا تزال High ويجب معالجتها لاحقاً، لكنها تعتمد جزئياً على القدرة على خلق أو تعديل حالة مستخدم خطرة. F9 تغلق الطريق العام لاستدعاء هذه العمليات من خارج واجهة المستخدم، لذلك هي أولوية معمارية قبل معالجة التفاصيل داخل auth/password migration.

### 1.3 الاعتماديات المعمارية

| الثغرة | علاقتها بـ F9 |
|---|---|
| F5 | `SessionContext` أصبح مصدر صلاحيات مركزياً، لكن `UserAdminService` لا يستخدمه بعد. إصلاح F9 يستثمر هذا التجريد. |
| F6 | غياب التخويل في `UserAdminService` يجعل استدعاء `CreateUserAsync`/`UpdateUserAsync` أخطر، خصوصاً مع سلوك كلمة المرور الفارغة. |
| F12/F14 | منطق admin الحالي ما زال مرتبطاً بأسماء المستخدمين وبعض الاستدعاءات في login، لكن F9 يجب أن يعتمد على الصلاحيات لا على الاسم. |
| F16 | `SaveRolePermissionsAsync` يستطيع حفظ `"ALL"` لأي دور؛ إضافة تخويل داخل الخدمة تقلل مسار الاستغلال، مع ضرورة رفض `"ALL"` كتشديد لاحق أو ضمن نفس الإصلاح. |
| Bootstrap | `BootstrapViewModel` يستخدم `UserAdminService.CreateUserAsync` قبل وجود جلسة مستخدم، لذلك إصلاح F9 يحتاج ممر bootstrap مضبوطاً وليس bypass عاماً. |
| AdminCli | `Open_lab.AdminCli` يستدعي `UserAdminService` مباشرة لإعادة تعيين admin، لذلك سيحتاج سياقاً نظامياً أو مساراً مخولاً خاصاً. |

---

## 2. الفحص والتحليل البرمجي للوضع الحالي

### 2.1 الملفات والدوال المتأثرة

| الملف | الكلاس/الواجهة | الدوال المتأثرة |
|---|---|---|
| `Open_lab/Services/UserAdminService.cs` | `UserAdminService` | جميع دوال التغيير الإداري تقريباً. |
| `Open_lab/Services/IUserAdminService.cs` | `IUserAdminService` | قد تبقى signatures كما هي، لكن الاعتماديات constructor ستتغير. |
| `Open_lab/Services/SessionContext.cs` | `SessionContext` | مصدر الصلاحيات الحالي الذي يجب حقنه في الخدمة. |
| `Open_lab/Services/ISessionContext.cs` | `ISessionContext`, `IMutableSessionContext` | قد يحتاج حالة bootstrap/system operation محكومة. |
| `Open_lab/ViewModels/UsersPermissionsViewModel.cs` | `UsersPermissionsViewModel` | يحتوي حراسة UI حالية، لكنها غير كافية وحدها. |
| `Open_lab/ViewModels/BootstrapViewModel.cs` | `BootstrapViewModel` | ينشئ أول مستخدم admin قبل وجود session. |
| `Open_lab.AdminCli/Program.cs` | CLI reset flow | يستدعي `UserAdminService` خارج DI وخارج session. |
| `Open_lab.Tests/Services/UserAdminServiceTests.cs` | اختبارات الخدمة | ستحتاج تهيئة session مخولة أو اختبارات رفض غير المخولين. |
| `Open_lab.Tests/Infrastructure/AppSessionTestHelper.cs` | test helper | مناسب لإعداد جلسة Admin في الاختبارات بعد الإصلاح. |

### 2.2 مواضع الضعف في `UserAdminService`

#### إنشاء مستخدم دون تخويل

```csharp
public async Task<User> CreateUserAsync(User user, string? plainPassword)
{
    ...
    ApplyPassword(user, plainPassword);
    _db.Users.Add(user);
    await _db.SaveChangesAsync();
    return user;
}
```

المشكلة الأمنية: الدالة تنفذ إنشاء مستخدم جديد دون فحص `UsersEdit`. أي كود يمتلك مرجعاً إلى `IUserAdminService` أو يستطيع إنشاء `UserAdminService` مباشرة يمكنه إضافة مستخدمين خارج قواعد UI.

#### تعديل مستخدم دون تخويل

```csharp
public async Task UpdateUserAsync(User user, string? plainPassword)
{
    ...
    current.Username = normalizedUsername;
    current.FullName = user.FullName;
    current.IsActive = user.IsActive || string.Equals(current.Username, AdminUsername, StringComparison.OrdinalIgnoreCase);
    ...
    await _db.SaveChangesAsync();
}
```

المشكلة الأمنية: التعديل لا يتحقق من صلاحية المستدعي. توجد حراسة اسمية خاصة بـ `admin`، لكنها لا تعوض غياب authorization. كما أن السطر الذي يجبر `admin` على البقاء نشطاً هو سياسة عمل حساسة يجب توثيقها أو فصلها عن إصلاح F9 حتى لا تختلط المعالجات.

#### تعديل صلاحيات دور دون تخويل

```csharp
public async Task SaveRolePermissionsAsync(int roleId, IEnumerable<string> permissionCodes)
{
    var existing = await _db.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync();
    _db.RolePermissions.RemoveRange(existing);

    foreach (var code in permissionCodes.Distinct())
    {
        _db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionCode = code });
    }

    await _db.SaveChangesAsync();
}
```

هذا هو الموضع الأخطر داخل F9. يستطيع المستدعي منح أي permission code لأي دور، بما في ذلك `"ALL"` حالياً. هذا يخلق مسار تصعيد امتيازات مباشر من خدمة غير محمية إلى session admin لاحقاً.

#### تعيين دور دون تخويل

```csharp
public async Task AssignSingleRoleAsync(int userId, int roleId)
{
    ...
    var existing = await _db.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();
    _db.UserRoles.RemoveRange(existing);
    _db.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
    await _db.SaveChangesAsync();
}
```

المشكلة الأمنية: يستطيع caller استبدال دور مستخدم كامل بدور آخر دون فحص صلاحية. عند الجمع مع `SaveRolePermissionsAsync` يصبح السيناريو: إنشاء دور، منحه `"ALL"`، ثم تعيينه لمستخدم.

### 2.3 الحراسة الحالية في ViewModel لا تكفي

```csharp
SaveUserCommand = new RelayCommand(async _ => await SaveUserAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.UsersEdit));
SaveRolePermissionsCommand = new RelayCommand(async _ => await SaveRolePermissionsAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.UsersEdit));
AssignRoleCommand = new RelayCommand(async _ => await AssignRoleAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.UsersEdit) && SelectedUser != null && SelectedRole != null);
```

هذه الحراسة مفيدة لتجربة UI، لكنها لا تؤمن الخدمة. أي مسار لا يمر عبر `CanExecute` يستطيع استدعاء الخدمة مباشرة: CLI، اختبارات، tooling، plug-in مستقبلي، أو كود داخلي جديد.

### 2.4 مسارات خاصة يجب عدم كسرها

#### Bootstrap

```csharp
var user = await _userAdminService.CreateUserAsync(new User
{
    Username = Username.Trim(),
    FullName = string.IsNullOrWhiteSpace(FullName) ? null : FullName.Trim(),
    IsActive = true
}, Password);

await _adminSetupService.EnsureAdminAccessAsync(user.UserId);
await _adminSetupService.MarkBootstrapCompleteAsync();
```

هذا المسار مشروع لكنه يعمل قبل وجود session، لذلك إضافة `EnsureUsersEdit` بشكل مباشر ستكسر bootstrap ما لم يوجد ممر bootstrap مضبوط ومحدود.

#### Admin CLI

```csharp
var userAdminService = new UserAdminService(db);
...
admin = await userAdminService.CreateUserAsync(...);
...
await userAdminService.UpdateUserAsync(admin, newPassword);
```

هذا المسار يعمل خارج DI وخارج `SessionContext`. إصلاح F9 يجب أن يحدد هل CLI يبدأ session نظامية بصلاحية `FullAccess` بعد تأكيد `RESET-ADMIN`، أم يستخدم service مخصصاً لإعادة تعيين admin لا يفتح كل عمليات `UserAdminService`.

---

## 3. الخطة المعمارية المقترحة للإصلاح

هذه خطة تصميم فقط وليست تنفيذ.

### 3.1 مبدأ الإصلاح

يجب نقل التخويل من UI-only إلى service-level authorization. القاعدة:

> كل عملية تغيير في المستخدمين أو الأدوار يجب أن تفشل داخل `UserAdminService` إذا لم تكن الجلسة الحالية تحمل `PermissionCodes.UsersEdit` أو ممر bootstrap/system مصرحاً ومحدوداً.

### 3.2 ما يجب إضافته لاحقاً

1. حقن `ISessionContext` في `UserAdminService` عبر constructor.
2. إضافة دالة guard داخل الخدمة مثل `EnsureCanEditUsers`.
3. تطبيق guard في بداية دوال التغيير:
   - `CreateUserAsync`
   - `UpdateUserAsync`
   - `DeleteUserAsync`
   - `CreateRoleAsync`
   - `DeleteRoleAsync`
   - `AssignSingleRoleAsync`
   - `RemoveUserRoleAsync`
   - `SaveRolePermissionsAsync`
4. التمييز بين read وwrite:
   - `GetUsersAsync`, `GetRolesAsync`, `GetRolePermissionCodesAsync` يمكن أن تتطلب `UsersView` أو `UsersEdit`.
   - إن كان الهدف الأدنى هو تقليل الانكسار، يمكن بدء الإصلاح بحراسة عمليات write فقط، ثم توسيع read في مرحلة لاحقة.
5. إضافة تمثيل صريح لمسار bootstrap:
   - إما `IMutableSessionContext` يحتوي حالة محدودة مثل `IsBootstrapping`.
   - أو إنشاء خدمة bootstrap داخلية لا تعتمد على `UserAdminService` العام.
   - أو تمرير authorization context داخلي لا يمكن استدعاؤه من UI عادي.
6. تحديث `AdminCli` ليستخدم سياقاً نظامياً محكوماً بعد تحقق `RESET-ADMIN`، أو نقله إلى service مخصص لعمليات reset لا يمنح صلاحيات عامة لباقي الخدمة.

### 3.3 ما يجب حذفه أو منعه

- منع الاعتماد على `UsersPermissionsViewModel` باعتباره guard الوحيد.
- منع قبول استدعاءات write عندما تكون session فارغة (`UserId = 0` ولا صلاحيات).
- منع استخدام username مثل `admin` كبديل للتخويل.
- منع `SaveRolePermissionsAsync` من قبول `"ALL"` كتشديد إضافي إذا تقرر دمجه مع F9، أو توثيقه كجزء تابع لـ F16 إذا أُجل.

### 3.4 الاستثناءات ورسائل الفشل

نوع الاستثناء الأنسب:

- `UnauthorizedAccessException`

رسالة مقترحة:

```text
UsersEdit permission is required to manage users and roles.
```

للقراءة فقط:

```text
UsersView permission is required to view users and roles.
```

هذه الرسائل لا تكشف أسراراً ولا تفاصيل قاعدة البيانات، لكنها واضحة للمطورين والاختبارات.

### 3.5 حدود نطاق الإصلاح

يفضل ألا يخلط تنفيذ F9 القادم بين:

- إصلاح F9: service-level authorization.
- إصلاح F6: رفض كلمات المرور الفارغة وإزالة plain-text migration path.
- إصلاح F16: إعادة تصميم `FullAccess = "ALL"`.

يمكن إضافة حماية صغيرة داخل `SaveRolePermissionsAsync` ضد منح `"ALL"` إذا اعتمدها الفريق كجزء من تقليل التصعيد، لكن يجب إدراك أنها تقترب من F16. الخيار الأكثر محافظة هو توثيقها كتحسين تابع ثم تنفيذ F9 أولاً.

---

## 4. ضمان عدم الانهيار البرمجي والاستقرار

### 4.1 سلامة شاشات البرنامج الأساسية

شاشة إدارة المستخدمين حالياً تضبط `CanExecute` بناءً على `SessionContext.Current.HasPermission`. بعد إضافة guard في الخدمة، المستخدم المخول لن يلاحظ فرقاً وظيفياً إذا كانت session صحيحة. المستخدم غير المخول كان ممنوعاً من UI أصلاً، وسيصبح ممنوعاً أيضاً من service.

خطة الاستقرار:

- إبقاء رسائل ViewModel الحالية التي تعرض `خطأ: ...` عند exception.
- عدم تغيير signatures في `IUserAdminService` إن لم يكن ضرورياً.
- عدم تغيير أسماء الأوامر أو خصائص binding.
- اختبار أن مستخدم لديه `UsersEdit` يستطيع تنفيذ المسارات الحالية نفسها.

### 4.2 سلامة Bootstrap

الخطر الأكبر هو أن bootstrap لا يملك session عند إنشاء أول admin. لذلك يجب اعتماد أحد الخيارين قبل التنفيذ:

| الخيار | التحليل |
|---|---|
| Bootstrap flag داخل `IMutableSessionContext` | سريع ومتسق مع session الحالية، لكنه يضيف حالة مؤقتة يجب ضبطها وإزالتها بعناية. |
| Bootstrap-specific service | أنظف أمنياً لأنه يعزل إنشاء أول admin عن service الإدارة العامة، لكنه أوسع أثراً. |

التوصية: إن كان الهدف Zero-Regression قريب، فالخيار الأول قابل للتنفيذ بأقل تغيير بشرط:

- تفعيل flag فقط داخل `BootstrapViewModel.CreateAdminAsync`.
- استخدام `try/finally` لإلغائه.
- السماح فقط بالعمليات المطلوبة للـ bootstrap.
- عدم السماح لـ UI العادي بتفعيل flag.

### 4.3 سلامة AdminCli

`Open_lab.AdminCli` حالياً ينشئ `UserAdminService(db)` مباشرة. بعد حقن `ISessionContext` سيحدث كسر compile إن لم يعدل. التخطيط يجب أن يقرر:

- إما إنشاء `SessionContext` محلي في CLI بصلاحية `FullAccess` بعد confirmation.
- أو نقل منطق reset إلى `AdminResetService` مخصص لا يعتمد على permission UI.

للاستقرار، الخيار الأول أقل تغييراً، لكنه يجب أن يكون محصوراً داخل CLI وبعد تحقق كلمة التأكيد وسياسة قوة كلمة المرور.

### 4.4 سلامة الاختبارات الآلية

العدد الحالي للاختبارات بعد آخر تشغيل معروف هو 1594 اختباراً. أكثر الاختبارات تعرضاً للكسر:

- `UserAdminServiceTests` لأنها تنشئ الخدمة مباشرة بدون session.
- `FunctionCoverageGapTests` لأنها تستدعي `UserAdminService` مباشرة.
- اختبارات ViewModel التي تستخدم mock لن تتأثر غالباً.
- Integration tests التي تبدأ session admin في `SqliteIntegrationTestBase` غالباً مستقرة.

خطة عدم الكسر:

- تحديث setup في اختبارات `UserAdminServiceTests` لاستخدام `AppSessionTestHelper.ResetToAdmin()` أو session مكافئة قبل عمليات النجاح.
- إضافة اختبارات جديدة تثبت أن العمليات تفشل دون `UsersEdit`.
- التأكد من `Dispose` أو cleanup يعيد `SessionContext.Current.EndSession()` لمنع تلوث الاختبارات.
- إبقاء InMemory وSQLite كما هي؛ هذا الإصلاح لا يجب أن يتطلب SQL Server أو environment variables.

### 4.5 Regression Matrix

| المسار | يجب أن يبقى ناجحاً بعد الإصلاح |
|---|---|
| إنشاء مستخدم من UI بصلاحية `UsersEdit` | نعم |
| تعديل مستخدم من UI بصلاحية `UsersEdit` | نعم |
| تحميل المستخدمين بصلاحية `UsersView` | نعم |
| محاولة تعديل من UI بلا صلاحية | تبقى ممنوعة |
| استدعاء service write بلا session | يجب أن يفشل بأمان |
| Bootstrap أول تشغيل | يجب أن ينجح عبر ممر محدود |
| AdminCli reset | يجب أن ينجح بعد confirmation |
| اختبارات InMemory/SQLite | يجب أن تبقى مستقلة عن DB حقيقي |

---

## 5. سيناريوهات التحقق المستقبلية

### 5.1 سيناريو النجاح

بعد تنفيذ F9 لاحقاً، يجب التحقق من الآتي:

- مستخدم لديه `PermissionCodes.UsersEdit` يستطيع إنشاء مستخدم وتعديل مستخدم وحفظ صلاحيات دور.
- مستخدم لديه `PermissionCodes.UsersView` فقط يستطيع القراءة إن تم تفعيل read guard، ولا يستطيع التعديل.
- bootstrap يستطيع إنشاء أول admin عند كون `IsBootstrapRequiredAsync` true.
- AdminCli يستطيع إعادة تعيين كلمة مرور admin بعد `RESET-ADMIN`.
- جميع الاختبارات الحالية تمر بعد تحديث setup المخول حيث يلزم.

اختبارات مقترحة:

- `CreateUserAsync_WithoutUsersEdit_ShouldThrowUnauthorizedAccessException`
- `UpdateUserAsync_WithoutUsersEdit_ShouldThrowUnauthorizedAccessException`
- `SaveRolePermissionsAsync_WithoutUsersEdit_ShouldThrowUnauthorizedAccessException`
- `CreateUserAsync_WithUsersEdit_ShouldCreateUser`
- `BootstrapCreateAdmin_WithBootstrapContext_ShouldSucceed`
- `AdminCliReset_WithConfirmedSystemContext_ShouldSucceed` إن كان قابلاً للاختبار آلياً.

### 5.2 سيناريو الفشل الآمن

محاولة الاستغلال:

1. إنشاء `UserAdminService` أو الحصول عليه من DI.
2. عدم بدء session أو بدء session بلا `UsersEdit`.
3. استدعاء `SaveRolePermissionsAsync(roleId, new[] { PermissionCodes.FullAccess })`.

المتوقع بعد الإصلاح:

- ترمى `UnauthorizedAccessException`.
- لا يتم حذف الصلاحيات القديمة.
- لا يتم إضافة `"ALL"`.
- لا يتم استدعاء `SaveChangesAsync` لتعديل permissions.
- لا تتحول session لاحقة إلى admin بسبب الدور.

محاولة أخرى:

1. session بلا صلاحيات.
2. استدعاء `CreateUserAsync`.

المتوقع:

- لا ينشأ المستخدم.
- لا تكتب password hash أو salt.
- لا يوجد أثر جزئي في قاعدة البيانات.

### 5.3 سيناريو التحقق اليدوي

- تسجيل الدخول بحساب لا يملك `UsersEdit`.
- التأكد أن UI يمنع أوامر الإدارة كما هو حالياً.
- محاولة استدعاء الخدمة من اختبار/manual harness بلا session مخولة.
- التأكد أن الخدمة نفسها ترفض العملية حتى لو لم يمر الاستدعاء عبر ViewModel.

### 5.4 سيناريو التحقق الأمني الثابت

فحوصات static مقترحة بعد التنفيذ:

- البحث عن `new UserAdminService(db)` للتأكد من تحديث CLI والاختبارات.
- البحث عن كل دالة write داخل `UserAdminService` والتأكد أن أول خطوة فيها guard.
- البحث عن `SaveRolePermissionsAsync` والتأكد من وجود guard قبل حذف الصلاحيات القديمة.
- التأكد من عدم وجود bypass عام باسم `admin` أو `Administrator` داخل authorization guard.

---

## 6. قرار معماري مقترح

قرار التخطيط المقترح:

1. اختيار F9 كالإصلاح التالي بعد F8.
2. تنفيذ service-level authorization في `UserAdminService`.
3. استخدام `ISessionContext` كمصدر الصلاحيات.
4. حراسة كل عمليات write بـ `UsersEdit`.
5. حماية read operations بـ `UsersView` أو تأجيلها بقرار واعٍ لتقليل الانكسار.
6. تصميم ممر bootstrap محدود لا يفتح bypass عاماً.
7. تحديث AdminCli ليعمل ضمن سياق نظامي واضح أو service مخصص.
8. تحديث اختبارات الخدمة لتفصل بين مسارات النجاح المخولة ومسارات الرفض غير المخولة.

هذه الخطة تغلق فجوة الثقة بين UI والخدمات، وتبني قاعدة لازمة لمعالجة F6 وF16 لاحقاً دون بقاء service boundary مفتوحاً.

---

## 7. خلاصة الاعتماد

F9 هي الثغرة التالية الأكثر أولوية لأنها تمس حد التخويل المركزي لإدارة المستخدمين والأدوار. تركها مفتوحة يعني أن أي معالجة لاحقة في UI أو login تبقى قابلة للتجاوز عبر استدعاء الخدمات مباشرة. الإصلاح يجب أن يكون محدوداً ومنضبطاً: إضافة authorization guards داخل `UserAdminService`، الحفاظ على bootstrap وAdminCli بممرات نظامية واضحة، وتحديث الاختبارات بحيث تثبت الرفض الآمن والنجاح المخول.

هذه الوثيقة تخطيطية فقط. لم يتم تعديل أي كود إنتاجي أو اختبار أو إعدادات ضمن هذه المرحلة.
