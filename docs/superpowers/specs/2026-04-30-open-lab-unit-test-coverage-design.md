# تصميم تغطية اختبارات الوحدة — Open_lab (97 وظيفة)

## الهدف
إضافة/تحسين اختبارات وحدة تغطي **كل** الوظائف الـ97 الموثقة في:
[Open_lab_Modules_Documentation.md](file:///workspace/Open_lab/Docs/Open_lab_Modules_Documentation.md)

مع الالتزام الصارم بتعليمات:
[UnitTest_Skill.md](file:///workspace/Open_lab/Docs/UnitTest_Skill.md)

وتحديث ملف تتبع التغطية:
[unit_test_result.md](file:///workspace/Open_lab/Docs/unit_test_result.md)

## قيود أساسية
- لا يتم تعديل كود الإنتاج داخل مشروع [Open_lab.csproj](file:///workspace/Open_lab/Open_lab.csproj).
- يتم تعديل/إضافة الاختبارات فقط داخل مشروع [Open_lab.Tests.csproj](file:///workspace/Open_lab.Tests/Open_lab.Tests.csproj).
- اختبارات طبقة Service يجب أن تكون Mock فقط بدون استخدام DbContext/InMemory.
- الاختبارات الحالية التي تعتمد على EF InMemory سيتم نقلها وتصنيفها كاختبارات تكامل Integration.

## تعريف “مكتمل” لكل وظيفة
لكل وظيفة (x.y) يجب توفير:
- **Service layer (إلزامي):**
  - Success test واحدة على الأقل
  - Failure test واحدة على الأقل
  - Edge case test واحدة على الأقل
- **ViewModel layer (إلزامي):**
  - Success test للـCommand الرئيسي
  - Failure test للـCommand الرئيسي
  - Edge case test إذا كانت الوظيفة تتعامل مع إدخالات/قِيَم
    - إذا لم ينطبق، يضاف توضيح داخل ملف الاختبارات بحسب تعليمات UnitTest_Skill.md
- **Business Rules (BR-XXX-XXX):**
  - لكل BR موجود في التوثيق يجب وجود اختبار مخصص باسمه ويوثَّق في تتبع النتائج.

## استراتيجية الاختبارات
### 1) ViewModel Unit Tests (Moq فقط)
- يتم Mock لكل I*Service يتم حقنه في الـViewModel.
- يتم استدعاء أوامر RelayCommand باستخدام Execute(null) ثم await Task.Delay.
- يتم إثبات:
  - تغييرات خصائص الـViewModel
  - رسائل الخطأ/الحالة
  - Verify لاستدعاءات الـService في عمليات الكتابة

### 2) Service Unit Tests (Moq فقط) بدون DbContext
بما أن أغلب الخدمات تعتمد مباشرة على OpenLabDbContext، سيتم إنشاء طبقة مساعدة داخل الاختبارات لتوفير:
- Mock لـ OpenLabDbContext
- Mock لـ DbSet<T> يدعم LINQ وعمليات async الشائعة (ToListAsync/FirstOrDefaultAsync/AnyAsync)
- “مخزن بيانات” in-memory على هيئة List<T> لكل Entity لازمة للسيناريو

نطاق الدعم سيكون “حسب الحاجة”:
- نبدأ بالعمليات والـqueries التي تستخدمها الوظائف الـ97 فعلياً.
- إذا ظهرت عملية EF غير مدعومة في mock infrastructure أثناء التنفيذ:
  - نوسّع الـhelpers داخل Open_lab.Tests فقط دون تعديل كود الإنتاج.

## تنظيم الاختبارات
### الملفات/المجلدات
- Unit:
  - Open_lab.Tests/Services/*.cs
  - Open_lab.Tests/ViewModels/*.cs
- Integration:
  - Open_lab.Tests/Integration/*.cs (نقل اختبارات EF InMemory الحالية هنا)
  - إضافة [Trait("Category","Integration")] لتسهيل الفلترة عند الحاجة.

### قواعد التسمية والربط
لكل اختبار:
- اسم الاختبار: `[FunctionName]_[StateUnderTest]_[ExpectedResult]`
- أول سطر داخل جسم الاختبار: `// Function: x.y — <English Name>`
- بنية AAA مع فواصل واضحة: Arrange / Act / Assert

## خطة التنفيذ عالية المستوى (بدون تنفيذ هنا)
1. نقل اختبارات InMemory الحالية إلى Integration ووضع Trait لها.
2. إنشاء Mock EF infrastructure داخل Open_lab.Tests/Infrastructure (DbSet mock + async query provider).
3. تغطية الوظائف 97 على دفعات بحسب الموديولات:
   - لكل وظيفة: قراءة BR من التوثيق + فحص الاختبارات الحالية + كتابة الناقص.
4. بعد كل دفعة:
   - dotnet test
   - تحديث unit_test_result.md (Service/ViewModel/Status + أسماء الاختبارات)

## ناتج التسليم
- اختبارات وحدة متوافقة مع UnitTest_Skill.md وتغطي 97 وظيفة.
- تحديث [unit_test_result.md](file:///workspace/Open_lab/Docs/unit_test_result.md) ليعكس الوضع الحقيقي.
- بقاء اختبارات التكامل (إن وجدت) داخل Integration ومعلّمة كتTests تكامل.

