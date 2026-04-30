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
- اختبارات طبقة Service يمكن أن تستخدم EF Core InMemory كبديل آمن عن SQL Server (بدون أي اتصال بقاعدة بيانات حقيقية).
- استخدام EF Core InMemory في هذا المشروع يُعامل كاختبار Unit عمليًا لأنه لا يعتمد على موارد خارجية (SQL Server/شبكة/ملفات) ويُسهل اختبار منطق EF والـqueries بشكل واقعي.

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

### 2) Service Unit Tests (EF Core InMemory)
- نستخدم OpenLabDbContext عبر EF InMemory داخل الاختبار بدل SQL Server.
- هذا يتيح اختبار منطق الخدمات والـqueries/relationships بشكل أقرب للواقع وبمجهود أقل مقارنة بمحاولة Mock لـ DbContext/DbSet.
- لا يتم استخدام أي اتصال خارجي: لا SQL Server ولا ملفات ولا شبكة.

## تنظيم الاختبارات
### الملفات/المجلدات
- Unit:
  - Open_lab.Tests/Services/*.cs
  - Open_lab.Tests/ViewModels/*.cs
- اختبارات تعتمد على “تدفق شامل جدًا” يمكن تمييزها لاحقًا بـTrait (اختياري) إذا احتجنا فصلها عند التشغيل، لكن لن نقوم بأي نقل افتراضي.

### قواعد التسمية والربط
لكل اختبار:
- اسم الاختبار: `[FunctionName]_[StateUnderTest]_[ExpectedResult]`
- أول سطر داخل جسم الاختبار: `// Function: x.y — <English Name>`
- بنية AAA مع فواصل واضحة: Arrange / Act / Assert

## خطة التنفيذ عالية المستوى (بدون تنفيذ هنا)
1. اعتماد EF InMemory كآلية اختبار للخدمات (Service) بدل SQL Server.
2. تغطية الوظائف 97 على دفعات بحسب الموديولات:
   - لكل وظيفة: قراءة BR من التوثيق + فحص الاختبارات الحالية + كتابة الناقص.
3. بعد كل دفعة:
   - dotnet test
   - تحديث unit_test_result.md (Service/ViewModel/Status + أسماء الاختبارات)

## ناتج التسليم
- اختبارات وحدة متوافقة مع UnitTest_Skill.md وتغطي 97 وظيفة.
- تحديث [unit_test_result.md](file:///workspace/Open_lab/Docs/unit_test_result.md) ليعكس الوضع الحقيقي.
- إبقاء أي اختبارات “تكامل/سيناريو شامل” ضمن نفس المشروع، مع إمكانية وسمها لاحقًا عند الحاجة للفصل أثناء التشغيل.
