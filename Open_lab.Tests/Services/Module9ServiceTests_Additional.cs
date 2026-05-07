using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests.Services
{
    /// <summary>
    /// اختبارات الموديول 9 — الإحصائيات والتحليلات
    /// يغطي الوظائف: 9.1, 9.2, 9.3, 9.4, 9.5, 9.6
    /// </summary>
    public class Module9ServiceTests_Additional : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly StatisticsService _statsService;
        private readonly UserProductivityService _productivityService;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public Module9ServiceTests_Additional()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _statsService = new StatisticsService(_db);
            _productivityService = new UserProductivityService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        // ===================================================================
        // 9.1 توزيع المرضى حسب الجنس — Patient Count by Gender
        // ===================================================================

        [Fact]
        public async Task PatientCountByGender_WithMaleAndFemalePatients_ShouldReturnCorrectGenderDistribution()
        {
            // Function: 9.1 — Patient Count by Gender
            // Arrange
            var male1 = new Patient { LabId = "M001", FullName = "أحمد علي", Gender = "ذكر" };
            var male2 = new Patient { LabId = "M002", FullName = "محمد حسن", Gender = "ذكر" };
            var female1 = new Patient { LabId = "F001", FullName = "فاطمة محمد", Gender = "أنثى" };
            _db.Patients.AddRange(male1, male2, female1);
            await _db.SaveChangesAsync();

            var visit1 = new Visit { PatientId = male1.PatientId, VisitDate = DateTime.Today };
            var visit2 = new Visit { PatientId = male2.PatientId, VisitDate = DateTime.Today };
            var visit3 = new Visit { PatientId = female1.PatientId, VisitDate = DateTime.Today };
            _db.Visits.AddRange(visit1, visit2, visit3);
            await _db.SaveChangesAsync();

            // Act
            var snapshot = await _statsService.GetSnapshotAsync(
                DateTime.Today.AddDays(-1),
                DateTime.Today.AddDays(1),
                "الكل",
                null);

            // Assert
            snapshot.ByGender.Should().NotBeNull();
            var maleRow = snapshot.ByGender.FirstOrDefault(g => g.Gender == "ذكر");
            var femaleRow = snapshot.ByGender.FirstOrDefault(g => g.Gender == "أنثى");
            maleRow.Should().NotBeNull("يجب أن يظهر صف الذكور في التقرير");
            femaleRow.Should().NotBeNull("يجب أن يظهر صف الإناث في التقرير");
            maleRow!.VisitsCount.Should().Be(2);
            femaleRow!.VisitsCount.Should().Be(1);
        }

        [Fact]
        public async Task PatientCountByGender_WithGenderFilter_ShouldExcludeOtherGenders()
        {
            // Function: 9.1 — Patient Count by Gender
            // Arrange
            var male = new Patient { LabId = "M010", FullName = "خالد", Gender = "ذكر" };
            var female = new Patient { LabId = "F010", FullName = "نورة", Gender = "أنثى" };
            _db.Patients.AddRange(male, female);
            await _db.SaveChangesAsync();

            _db.Visits.AddRange(
                new Visit { PatientId = male.PatientId, VisitDate = DateTime.Today },
                new Visit { PatientId = female.PatientId, VisitDate = DateTime.Today });
            await _db.SaveChangesAsync();

            // Act
            var snapshot = await _statsService.GetSnapshotAsync(
                DateTime.Today.AddDays(-1),
                DateTime.Today.AddDays(1),
                "ذكر",
                null);

            // Assert
            snapshot.Summary.VisitCount.Should().Be(1, "الفلتر بالجنس يجب أن يُظهر زيارة واحدة فقط");
            snapshot.ByGender.Should().AllSatisfy(g => g.Gender.Should().Be("ذكر"),
                "عند التصفية بالجنس يجب أن تظهر بيانات الجنس المطلوب فقط");
        }

        [Fact]
        public async Task PatientCountByGender_WithNoPatientsInPeriod_ShouldReturnEmptyGenderList()
        {
            // Function: 9.1 — Patient Count by Gender
            // Arrange
            var patient = new Patient { LabId = "G001", FullName = "سعد", Gender = "ذكر" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();
            _db.Visits.Add(new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today.AddDays(-60) });
            await _db.SaveChangesAsync();

            // Act — نطاق مستقبلي لا يحتوي بيانات
            var snapshot = await _statsService.GetSnapshotAsync(
                DateTime.Today.AddDays(10),
                DateTime.Today.AddDays(20),
                "الكل",
                null);

            // Assert
            snapshot.Summary.VisitCount.Should().Be(0);
            snapshot.ByGender.Should().BeEmpty("لا توجد زيارات في النطاق الزمني المحدد");
        }

        [Fact]
        public async Task PatientCountByGender_WithUnspecifiedGender_ShouldGroupAsUnspecified()
        {
            // Function: 9.1 — Patient Count by Gender
            // Arrange
            var patient = new Patient { LabId = "U001", FullName = "مجهول", Gender = "" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();
            _db.Visits.Add(new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today });
            await _db.SaveChangesAsync();

            // Act
            var snapshot = await _statsService.GetSnapshotAsync(
                DateTime.Today.AddDays(-1),
                DateTime.Today.AddDays(1),
                "الكل",
                null);

            // Assert
            var unspecifiedRow = snapshot.ByGender.FirstOrDefault(g => g.Gender == "غير محدد");
            unspecifiedRow.Should().NotBeNull("المرضى بجنس فارغ يجب تجميعهم تحت 'غير محدد'");
            unspecifiedRow!.VisitsCount.Should().Be(1);
        }

        [Fact]
        public async Task PatientCountByGender_WithInvalidDateRange_ShouldReturnZeroSnapshot_FailureGuard()
        {
            // Function: 9.1 — Patient Count by Gender
            // Arrange
            // Act
            var male = new Patient { LabId = "INV01", FullName = "مريض", Gender = "ذكر" };
            _db.Patients.Add(male);
            await _db.SaveChangesAsync();
            _db.Visits.Add(new Visit { PatientId = male.PatientId, VisitDate = DateTime.Today });
            await _db.SaveChangesAsync();

            var snapshot = await _statsService.GetSnapshotAsync(
                DateTime.Today.AddDays(1),
                DateTime.Today.AddDays(-1),
                "الكل",
                null);

            // Assert
            snapshot.Summary.VisitCount.Should().Be(0);
            snapshot.Summary.PatientCount.Should().Be(0);
            snapshot.ByGender.Should().BeEmpty();
            snapshot.ByReferral.Should().BeEmpty();
        }

        // ===================================================================
        // 9.2 توزيع المرضى حسب الشهر — Patient Count by Month
        // ===================================================================

        [Fact]
        public async Task PatientCountByMonth_WithVisitsInSpecificMonth_ShouldReturnCorrectMonthlyCount()
        {
            // Function: 9.2 — Patient Count by Month
            // Arrange
            var patient = new Patient { LabId = "MON01", FullName = "ياسر", Gender = "ذكر" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit1 = new Visit { PatientId = patient.PatientId, VisitDate = new DateTime(DateTime.Today.Year, 3, 10) };
            var visit2 = new Visit { PatientId = patient.PatientId, VisitDate = new DateTime(DateTime.Today.Year, 3, 20) };
            var visit3 = new Visit { PatientId = patient.PatientId, VisitDate = new DateTime(DateTime.Today.Year, 7, 5) };
            _db.Visits.AddRange(visit1, visit2, visit3);
            await _db.SaveChangesAsync();

            // Act
            var result = await _statsService.GetMonthlyAnalysisAsync(DateTime.Today.Year);

            // Assert
            result.Should().HaveCount(12, "يجب إرجاع 12 شهراً دائماً");
            var march = result.First(r => r.Month == 3);
            march.VisitCount.Should().Be(2, "شهر مارس يحتوي على زيارتين");
            var july = result.First(r => r.Month == 7);
            july.VisitCount.Should().Be(1, "شهر يوليو يحتوي على زيارة واحدة");
        }

        [Fact]
        public async Task PatientCountByMonth_WithNoVisitsInYear_ShouldReturn12MonthsWithZeroCounts()
        {
            // Function: 9.2 — Patient Count by Month
            // Arrange — لا توجد زيارات

            // Act
            var result = await _statsService.GetMonthlyAnalysisAsync(DateTime.Today.Year);

            // Assert
            result.Should().HaveCount(12);
            result.Should().AllSatisfy(r =>
            {
                r.VisitCount.Should().Be(0);
                r.TestCount.Should().Be(0);
                r.Revenue.Should().Be(0m);
            }, "جميع الأشهر يجب أن تكون صفراً عند غياب البيانات");
        }

        [Fact]
        public async Task PatientCountByMonth_WhenLoaded_ShouldIncludeArabicMonthNames()
        {
            // Function: 9.2 — Patient Count by Month
            // Arrange

            // Act
            var result = await _statsService.GetMonthlyAnalysisAsync(DateTime.Today.Year);

            // Assert
            result.Should().HaveCount(12);
            result.Should().AllSatisfy(r =>
                r.MonthName.Should().NotBeNullOrWhiteSpace("كل شهر يجب أن يحمل اسماً"));
            // التحقق من وجود أسماء الأشهر العربية
            result.First(r => r.Month == 1).MonthName.Should().NotBeEmpty();
            result.First(r => r.Month == 12).MonthName.Should().NotBeEmpty();
        }

        [Fact]
        public async Task PatientCountByMonth_WithVisitsAndTests_ShouldAggregateTestCountAndRevenue()
        {
            // Function: 9.2 — Patient Count by Month
            // Arrange
            var patient = new Patient { LabId = "MON02", FullName = "هاني", Gender = "ذكر" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = new DateTime(DateTime.Today.Year, 5, 15) };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test1 = new Test { Code = "T9A", NameReport = "T9A" };
            var test2 = new Test { Code = "T9B", NameReport = "T9B" };
            _db.Tests.AddRange(test1, test2);
            await _db.SaveChangesAsync();

            _db.VisitTests.AddRange(
                new VisitTest { VisitId = visit.VisitId, TestId = test1.TestId, Price = 50m },
                new VisitTest { VisitId = visit.VisitId, TestId = test2.TestId, Price = 75m });
            await _db.SaveChangesAsync();

            var invoice = new Invoice { VisitId = visit.VisitId, NetTotal = 125m, Paid = 125m };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            // Act
            var result = await _statsService.GetMonthlyAnalysisAsync(DateTime.Today.Year);

            // Assert
            var may = result.First(r => r.Month == 5);
            may.VisitCount.Should().Be(1);
            may.TestCount.Should().Be(2);
            may.Revenue.Should().Be(125m);
        }

        [Fact]
        public async Task PatientCountByMonth_WithInvalidYear_ShouldThrowArgumentOutOfRangeException_FailureGuard()
        {
            // Function: 9.2 — Patient Count by Month
            // Arrange
            // Act
            Func<Task> act = async () => await _statsService.GetMonthlyAnalysisAsync(0);
            // Assert
            await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
        }

        // ===================================================================
        // 9.3 تحليل الطلب على التحاليل — Test Demand Analysis
        // ===================================================================

        [Fact]
        public async Task TestDemandAnalysis_WithMultipleTests_ShouldReturnTop10OrderedByDemand()
        {
            // Function: 9.3 — Test Demand Analysis
            // Arrange
            var patient = new Patient { LabId = "DEM01", FullName = "سلمى", Gender = "أنثى" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var cbc = new Test { Code = "CBC", NameReport = "صورة دم كاملة" };
            var glu = new Test { Code = "GLU", NameReport = "سكر الدم" };
            var urea = new Test { Code = "UREA", NameReport = "يوريا" };
            _db.Tests.AddRange(cbc, glu, urea);
            await _db.SaveChangesAsync();

            // CBC مطلوب 3 مرات، GLU مرتين، UREA مرة
            _db.VisitTests.AddRange(
                new VisitTest { VisitId = visit.VisitId, TestId = cbc.TestId, Price = 30m },
                new VisitTest { VisitId = visit.VisitId, TestId = cbc.TestId, Price = 30m },
                new VisitTest { VisitId = visit.VisitId, TestId = cbc.TestId, Price = 30m },
                new VisitTest { VisitId = visit.VisitId, TestId = glu.TestId, Price = 20m },
                new VisitTest { VisitId = visit.VisitId, TestId = glu.TestId, Price = 20m },
                new VisitTest { VisitId = visit.VisitId, TestId = urea.TestId, Price = 25m });
            await _db.SaveChangesAsync();

            // Act
            var top = await _statsService.GetTop10TestsAsync(
                DateTime.Today.AddDays(-1),
                DateTime.Today.AddDays(1));

            // Assert
            top.Should().HaveCount(3);
            top[0].TestName.Should().Be("صورة دم كاملة");
            top[0].DemandCount.Should().Be(3);
            top[0].TotalRevenue.Should().Be(90m);
            top[1].TestName.Should().Be("سكر الدم");
            top[1].DemandCount.Should().Be(2);
        }

        [Fact]
        public async Task TestDemandAnalysis_WithMoreThan10Tests_ShouldReturnOnly10()
        {
            // Function: 9.3 — Test Demand Analysis
            // Arrange
            var patient = new Patient { LabId = "DEM02", FullName = "عمر", Gender = "ذكر" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            for (var i = 1; i <= 12; i++)
            {
                var test = new Test { Code = $"T9_{i:D2}", NameReport = $"تحليل-{i}" };
                _db.Tests.Add(test);
                await _db.SaveChangesAsync();
                _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = i * 10m });
                await _db.SaveChangesAsync();
            }

            // Act
            var top = await _statsService.GetTop10TestsAsync(
                DateTime.Today.AddDays(-1),
                DateTime.Today.AddDays(1));

            // Assert
            top.Should().HaveCount(10, "يجب إرجاع أعلى 10 تحاليل طلباً فقط");
        }

        [Fact]
        public async Task TestDemandAnalysis_WithNoTestsInPeriod_ShouldReturnEmptyList()
        {
            // Function: 9.3 — Test Demand Analysis
            // Arrange
            var patient = new Patient { LabId = "DEM03", FullName = "ليلى", Gender = "أنثى" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today.AddDays(-30) };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            // Act — نطاق لا يشمل التحاليل الموجودة
            var top = await _statsService.GetTop10TestsAsync(
                DateTime.Today.AddDays(5),
                DateTime.Today.AddDays(10));

            // Assert
            top.Should().BeEmpty("لا توجد تحاليل في النطاق الزمني المطلوب");
        }

        [Fact]
        public async Task TestDemandAnalysis_WhenLoaded_ShouldCalculateTotalRevenuePerTest()
        {
            // Function: 9.3 — Test Demand Analysis
            // Arrange
            var patient = new Patient { LabId = "DEM04", FullName = "جمال", Gender = "ذكر" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "CHEM", NameReport = "كيمياء الدم" };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            _db.VisitTests.AddRange(
                new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 100m },
                new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 120m });
            await _db.SaveChangesAsync();

            // Act
            var top = await _statsService.GetTop10TestsAsync(
                DateTime.Today.AddDays(-1),
                DateTime.Today.AddDays(1));

            // Assert
            top.Should().ContainSingle();
            top[0].TotalRevenue.Should().Be(220m, "مجموع الإيراد = 100 + 120");
            top[0].DemandCount.Should().Be(2);
        }

        [Fact]
        public async Task TestDemandAnalysis_WithInvalidDateRange_ShouldReturnEmptyList_FailureGuard()
        {
            // Function: 9.3 — Test Demand Analysis
            // Arrange
            // Act
            var list = await _statsService.GetTop10TestsAsync(DateTime.Today.AddDays(1), DateTime.Today.AddDays(-1));
            // Assert
            list.Should().BeEmpty();
        }

        // ===================================================================
        // 9.4 عدد العينات سنوياً — Sample Count per Year
        // ===================================================================

        [Fact]
        public async Task SampleCountPerYear_WithDataForCurrentYear_ShouldReturnCorrectYearlyCount()
        {
            // Function: 9.4 — Sample Count per Year
            // Arrange
            var patient = new Patient { LabId = "YR001", FullName = "راشد", Gender = "ذكر" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test1 = new Test { Code = "YT1", NameReport = "T1" };
            var test2 = new Test { Code = "YT2", NameReport = "T2" };
            _db.Tests.AddRange(test1, test2);
            await _db.SaveChangesAsync();

            _db.VisitTests.AddRange(
                new VisitTest { VisitId = visit.VisitId, TestId = test1.TestId, Price = 50m },
                new VisitTest { VisitId = visit.VisitId, TestId = test2.TestId, Price = 60m });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _statsService.GetSampleCountPerYearAsync(3);

            // Assert
            rows.Should().NotBeNull();
            var currentYearRow = rows.FirstOrDefault(r => r.Year == DateTime.Today.Year);
            currentYearRow.Should().NotBeNull("يجب أن يظهر السنة الحالية في التقرير");
            currentYearRow!.SamplesCount.Should().Be(2, "تم تسجيل عينتين في هذا العام");
        }

        [Fact]
        public async Task SampleCountPerYear_WithDefaultYearsBack_ShouldReturnFiveYears()
        {
            // Function: 9.4 — Sample Count per Year
            // Arrange — لا توجد بيانات، نختبر عدد السنوات

            // Act
            var rows = await _statsService.GetSampleCountPerYearAsync(5);

            // Assert
            rows.Should().HaveCount(5, "يجب إرجاع 5 سنوات عند القيمة الافتراضية");
        }

        [Fact]
        public async Task SampleCountPerYear_WithZeroYearsBack_ShouldDefaultToFiveYears_FailureGuard()
        {
            // Function: 9.4 — Sample Count per Year
            // Arrange — قيمة صفر يجب أن تُعالَج وترجع 5 سنوات

            // Act
            var rows = await _statsService.GetSampleCountPerYearAsync(0);

            // Assert
            rows.Should().HaveCount(5, "عند إدخال صفر يجب الرجوع للقيمة الافتراضية 5 سنوات");
        }

        [Fact]
        public async Task SampleCountPerYear_WithNegativeYearsBack_ShouldDefaultToFiveYears()
        {
            // Function: 9.4 — Sample Count per Year
            // Arrange — قيمة سالبة يجب معالجتها

            // Act
            var rows = await _statsService.GetSampleCountPerYearAsync(-3);

            // Assert
            rows.Should().HaveCount(5, "عند إدخال قيمة سالبة يجب الرجوع للقيمة الافتراضية 5 سنوات");
        }

        [Fact]
        public async Task SampleCountPerYear_WithDataAcrossMultipleYears_ShouldReturnCorrectCountPerYear()
        {
            // Function: 9.4 — Sample Count per Year
            // Arrange
            var patient = new Patient { LabId = "YR002", FullName = "بدر", Gender = "ذكر" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var currentYear = DateTime.Today.Year;
            var visit1 = new Visit { PatientId = patient.PatientId, VisitDate = new DateTime(currentYear, 6, 1) };
            var visit2 = new Visit { PatientId = patient.PatientId, VisitDate = new DateTime(currentYear - 1, 3, 15) };
            _db.Visits.AddRange(visit1, visit2);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "MULT", NameReport = "MULT" };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            _db.VisitTests.AddRange(
                new VisitTest { VisitId = visit1.VisitId, TestId = test.TestId, Price = 30m },
                new VisitTest { VisitId = visit1.VisitId, TestId = test.TestId, Price = 30m },
                new VisitTest { VisitId = visit2.VisitId, TestId = test.TestId, Price = 30m });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _statsService.GetSampleCountPerYearAsync(3);

            // Assert
            var thisYearRow = rows.FirstOrDefault(r => r.Year == currentYear);
            var lastYearRow = rows.FirstOrDefault(r => r.Year == currentYear - 1);
            thisYearRow.Should().NotBeNull();
            lastYearRow.Should().NotBeNull();
            thisYearRow!.SamplesCount.Should().Be(2, "السنة الحالية تحتوي على عينتين");
            lastYearRow!.SamplesCount.Should().Be(1, "السنة السابقة تحتوي على عينة واحدة");
        }

        [Fact]
        public async Task SampleCountPerYear_WhenLoaded_ShouldReturnRowsOrderedByYearAscending()
        {
            // Function: 9.4 — Sample Count per Year
            // Arrange

            // Act
            var rows = await _statsService.GetSampleCountPerYearAsync(4);

            // Assert
            rows.Should().BeInAscendingOrder(r => r.Year, "النتائج يجب ترتيبها تصاعدياً حسب السنة");
        }

        [Fact]
        public async Task ReferralSourceAnalysis_WithInvalidDateRange_ShouldReturnZeroSnapshot_FailureGuard()
        {
            // Function: 9.5 — Referral Source Analysis
            // Arrange
            // Act
            var referral = new Referral { Name = "R-INV" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var patient = new Patient { LabId = "RINV01", FullName = "مريض", Gender = "ذكر" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            _db.Visits.Add(new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today, ReferralId = referral.ReferralId });
            await _db.SaveChangesAsync();

            var snapshot = await _statsService.GetSnapshotAsync(
                DateTime.Today.AddDays(1),
                DateTime.Today.AddDays(-1),
                "الكل",
                referral.ReferralId);

            // Assert
            snapshot.Summary.VisitCount.Should().Be(0);
            snapshot.ByReferral.Should().BeEmpty();
        }

        // ===================================================================
        // 9.5 تحليل مصادر الإحالة — Referral Source Analysis
        // ===================================================================

        [Fact]
        public async Task ReferralSourceAnalysis_WithMultipleReferrals_ShouldReturnCorrectDistribution()
        {
            // Function: 9.5 — Referral Source Analysis
            // Arrange
            var ref1 = new Referral { Name = "د. أحمد" };
            var ref2 = new Referral { Name = "عيادة النور" };
            _db.Referrals.AddRange(ref1, ref2);
            await _db.SaveChangesAsync();

            var pat1 = new Patient { LabId = "REF01", FullName = "مريض1", Gender = "ذكر" };
            var pat2 = new Patient { LabId = "REF02", FullName = "مريض2", Gender = "أنثى" };
            var pat3 = new Patient { LabId = "REF03", FullName = "مريض3", Gender = "ذكر" };
            _db.Patients.AddRange(pat1, pat2, pat3);
            await _db.SaveChangesAsync();

            var visit1 = new Visit { PatientId = pat1.PatientId, VisitDate = DateTime.Today, ReferralId = ref1.ReferralId };
            var visit2 = new Visit { PatientId = pat2.PatientId, VisitDate = DateTime.Today, ReferralId = ref1.ReferralId };
            var visit3 = new Visit { PatientId = pat3.PatientId, VisitDate = DateTime.Today, ReferralId = ref2.ReferralId };
            _db.Visits.AddRange(visit1, visit2, visit3);
            await _db.SaveChangesAsync();

            var inv1 = new Invoice { VisitId = visit1.VisitId, NetTotal = 100m, Paid = 100m };
            var inv2 = new Invoice { VisitId = visit2.VisitId, NetTotal = 150m, Paid = 100m };
            var inv3 = new Invoice { VisitId = visit3.VisitId, NetTotal = 80m, Paid = 80m };
            _db.Invoices.AddRange(inv1, inv2, inv3);
            await _db.SaveChangesAsync();

            // Act
            var snapshot = await _statsService.GetSnapshotAsync(
                DateTime.Today.AddDays(-1),
                DateTime.Today.AddDays(1),
                "الكل",
                null);

            // Assert
            snapshot.ByReferral.Should().NotBeNull();
            var docRow = snapshot.ByReferral.FirstOrDefault(r => r.ReferralName == "د. أحمد");
            var clinicRow = snapshot.ByReferral.FirstOrDefault(r => r.ReferralName == "عيادة النور");
            docRow.Should().NotBeNull();
            clinicRow.Should().NotBeNull();
            docRow!.VisitsCount.Should().Be(2, "الدكتور أحمد له زيارتان");
            docRow.Revenue.Should().Be(250m, "إيراد د. أحمد = 100 + 150");
            clinicRow!.VisitsCount.Should().Be(1);
            clinicRow.Revenue.Should().Be(80m);
        }

        [Fact]
        public async Task ReferralSourceAnalysis_WithReferralFilter_ShouldReturnOnlySelectedReferral()
        {
            // Function: 9.5 — Referral Source Analysis
            // Arrange
            var ref1 = new Referral { Name = "مصدر-A" };
            var ref2 = new Referral { Name = "مصدر-B" };
            _db.Referrals.AddRange(ref1, ref2);
            await _db.SaveChangesAsync();

            var pat1 = new Patient { LabId = "REF10", FullName = "م1", Gender = "ذكر" };
            var pat2 = new Patient { LabId = "REF11", FullName = "م2", Gender = "ذكر" };
            _db.Patients.AddRange(pat1, pat2);
            await _db.SaveChangesAsync();

            _db.Visits.AddRange(
                new Visit { PatientId = pat1.PatientId, VisitDate = DateTime.Today, ReferralId = ref1.ReferralId },
                new Visit { PatientId = pat2.PatientId, VisitDate = DateTime.Today, ReferralId = ref2.ReferralId });
            await _db.SaveChangesAsync();

            // Act — تصفية بجهة إحالة محددة
            var snapshot = await _statsService.GetSnapshotAsync(
                DateTime.Today.AddDays(-1),
                DateTime.Today.AddDays(1),
                "الكل",
                ref1.ReferralId);

            // Assert
            snapshot.Summary.VisitCount.Should().Be(1, "التصفية بجهة الإحالة تُظهر زيارة واحدة فقط");
        }

        [Fact]
        public async Task ReferralSourceAnalysis_WithNoReferral_ShouldGroupAsBadoonJiha()
        {
            // Function: 9.5 — Referral Source Analysis
            // Arrange
            var patient = new Patient { LabId = "NOREF", FullName = "مريض بلا إحالة", Gender = "أنثى" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            _db.Visits.Add(new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today, ReferralId = null });
            await _db.SaveChangesAsync();

            // Act
            var snapshot = await _statsService.GetSnapshotAsync(
                DateTime.Today.AddDays(-1),
                DateTime.Today.AddDays(1),
                "الكل",
                null);

            // Assert
            var noRefRow = snapshot.ByReferral.FirstOrDefault(r => r.ReferralName == "بدون جهة");
            noRefRow.Should().NotBeNull("المرضى بلا إحالة يجب تجميعهم تحت 'بدون جهة'");
            noRefRow!.VisitsCount.Should().Be(1);
        }

        [Fact]
        public async Task GetReferralsAsync_WhenCalled_ShouldReturnAllReferralsOrderedByName()
        {
            // Function: 9.5 — Referral Source Analysis
            // Arrange
            _db.Referrals.AddRange(
                new Referral { Name = "يونيفرسال" },
                new Referral { Name = "أحمد" },
                new Referral { Name = "مستشفى النور" });
            await _db.SaveChangesAsync();

            // Act
            var list = await _statsService.GetReferralsAsync();

            // Assert
            list.Should().HaveCount(3);
            list.Should().BeInAscendingOrder(r => r.Name, "القائمة يجب أن تكون مرتبة أبجدياً");
        }

        [Fact]
        public async Task GetReferralsAsync_WithNoReferrals_ShouldReturnEmptyList()
        {
            // Function: 9.5 — Referral Source Analysis
            // Arrange — لا توجد جهات إحالة

            // Act
            var list = await _statsService.GetReferralsAsync();

            // Assert
            list.Should().BeEmpty("عند غياب البيانات يجب إرجاع قائمة فارغة");
        }

        // ===================================================================
        // 9.6 تقرير إنتاجية المستخدمين — User Productivity Report
        // ===================================================================

        [Fact]
        public async Task UserProductivityReport_WithVerifiedTests_ShouldCountDistinctTestsPerUser()
        {
            // Function: 9.6 — User Productivity Report
            // Arrange
            var user1 = new User { Username = "tech.sara" };
            var user2 = new User { Username = "tech.omar" };
            _db.Users.AddRange(user1, user2);
            await _db.SaveChangesAsync();

            var from = DateTime.Today.AddDays(-1);
            var to = DateTime.Today.AddDays(1);

            // tech.sara أنجزت VisitTest 10 (معاملتان) + VisitTest 11 = تحليلان مميزان
            _db.ResultValues.AddRange(
                new ResultValue { VisitTestId = 10, ParameterId = 1, VerifiedAt = DateTime.Today, VerifiedBy = user1.UserId },
                new ResultValue { VisitTestId = 10, ParameterId = 2, VerifiedAt = DateTime.Today, VerifiedBy = user1.UserId },
                new ResultValue { VisitTestId = 11, ParameterId = 1, VerifiedAt = DateTime.Today, VerifiedBy = user1.UserId },
                new ResultValue { VisitTestId = 20, ParameterId = 1, VerifiedAt = DateTime.Today, VerifiedBy = user2.UserId });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _productivityService.GetUserPerformanceAsync(from, to);

            // Assert
            rows.Should().HaveCount(2);
            var saraRow = rows.FirstOrDefault(r => r.Username == "tech.sara");
            var omarRow = rows.FirstOrDefault(r => r.Username == "tech.omar");
            saraRow.Should().NotBeNull();
            omarRow.Should().NotBeNull();
            saraRow!.CompletedTestsCount.Should().Be(2, "sara أنجزت VisitTest 10 و 11 (نكرر المعاملتين لكن VisitTestId مختلفان)");
            omarRow!.CompletedTestsCount.Should().Be(1);
        }

        [Fact]
        public async Task UserProductivityReport_WithSameVisitTestMultipleParameters_ShouldCountOnce()
        {
            // Function: 9.6 — User Productivity Report
            // Arrange — نفس VisitTestId بمعاملات متعددة يجب احتسابه مرة واحدة
            var user = new User { Username = "tech.ali" };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _db.ResultValues.AddRange(
                new ResultValue { VisitTestId = 50, ParameterId = 1, VerifiedAt = DateTime.Today, VerifiedBy = user.UserId },
                new ResultValue { VisitTestId = 50, ParameterId = 2, VerifiedAt = DateTime.Today, VerifiedBy = user.UserId },
                new ResultValue { VisitTestId = 50, ParameterId = 3, VerifiedAt = DateTime.Today, VerifiedBy = user.UserId });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _productivityService.GetUserPerformanceAsync(
                DateTime.Today.AddDays(-1),
                DateTime.Today.AddDays(1));

            // Assert
            rows.Should().ContainSingle();
            rows[0].CompletedTestsCount.Should().Be(1, "VisitTest 50 يُحسب مرة واحدة رغم وجود 3 معاملات");
        }

        [Fact]
        public async Task UserProductivityReport_WithVerificationsOutsidePeriod_ShouldNotIncludeThem()
        {
            // Function: 9.6 — User Productivity Report
            // Arrange
            var user = new User { Username = "tech.nour" };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // تحقق قديم خارج النطاق
            _db.ResultValues.Add(new ResultValue
            {
                VisitTestId = 99,
                ParameterId = 1,
                VerifiedAt = DateTime.Today.AddDays(-30),
                VerifiedBy = user.UserId
            });
            await _db.SaveChangesAsync();

            // Act — البحث في نطاق زمني لا يشمل التحقق القديم
            var rows = await _productivityService.GetUserPerformanceAsync(
                DateTime.Today.AddDays(-5),
                DateTime.Today.AddDays(-1));

            // Assert
            rows.Should().BeEmpty("لا توجد تحققات في النطاق الزمني المطلوب");
        }

        [Fact]
        public async Task UserProductivityReport_WhenLoaded_ShouldReturnRowsOrderedByCompletedTestsDescending()
        {
            // Function: 9.6 — User Productivity Report
            // Arrange
            var userA = new User { Username = "tech.a" };
            var userB = new User { Username = "tech.b" };
            var userC = new User { Username = "tech.c" };
            _db.Users.AddRange(userA, userB, userC);
            await _db.SaveChangesAsync();

            var now = DateTime.Today;
            // userB لديه أعلى عدد
            _db.ResultValues.AddRange(
                new ResultValue { VisitTestId = 301, ParameterId = 1, VerifiedAt = now, VerifiedBy = userA.UserId },
                new ResultValue { VisitTestId = 302, ParameterId = 1, VerifiedAt = now, VerifiedBy = userB.UserId },
                new ResultValue { VisitTestId = 303, ParameterId = 1, VerifiedAt = now, VerifiedBy = userB.UserId },
                new ResultValue { VisitTestId = 304, ParameterId = 1, VerifiedAt = now, VerifiedBy = userB.UserId },
                new ResultValue { VisitTestId = 305, ParameterId = 1, VerifiedAt = now, VerifiedBy = userC.UserId },
                new ResultValue { VisitTestId = 306, ParameterId = 1, VerifiedAt = now, VerifiedBy = userC.UserId });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _productivityService.GetUserPerformanceAsync(
                now.AddDays(-1),
                now.AddDays(1));

            // Assert
            rows.Should().BeInDescendingOrder(r => r.CompletedTestsCount,
                "يجب ترتيب التقرير تنازلياً حسب عدد التحاليل المنجزة");
            rows[0].Username.Should().Be("tech.b");
        }

        [Fact]
        public async Task UserProductivityReport_WithUnknownVerifier_ShouldLabelAsUnknown()
        {
            // Function: 9.6 — User Productivity Report
            // Arrange — معرّف موظف غير موجود في جدول المستخدمين
            _db.ResultValues.Add(new ResultValue
            {
                VisitTestId = 999,
                ParameterId = 1,
                VerifiedAt = DateTime.Today,
                VerifiedBy = 88888
            });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _productivityService.GetUserPerformanceAsync(
                DateTime.Today.AddDays(-1),
                DateTime.Today.AddDays(1));

            // Assert
            rows.Should().ContainSingle();
            rows[0].Username.Should().Be("Unknown",
                "عندما لا يُوجد المستخدم في قاعدة البيانات يجب استخدام 'Unknown'");
        }

        [Fact]
        public async Task UserProductivityReport_WithNoVerifications_ShouldReturnEmptyList()
        {
            // Function: 9.6 — User Productivity Report
            // Arrange — لا توجد تحققات على الإطلاق

            // Act
            var rows = await _productivityService.GetUserPerformanceAsync(
                DateTime.Today.AddDays(-1),
                DateTime.Today.AddDays(1));

            // Assert
            rows.Should().BeEmpty();
        }

        // ===================================================================
        // اختبارات إضافية — حالات الحدود والتكامل
        // ===================================================================

        [Fact]
        public async Task GetSnapshotAsync_WithBothGenderAndReferralFilters_ShouldApplyBothFilters()
        {
            // Function: 9.5 — Referral Source Analysis (تصفية مركبة)
            // Arrange
            var ref1 = new Referral { Name = "عيادة A" };
            _db.Referrals.Add(ref1);
            await _db.SaveChangesAsync();

            var malePatient = new Patient { LabId = "COMBO1", FullName = "ذكر", Gender = "ذكر" };
            var femalePatient = new Patient { LabId = "COMBO2", FullName = "أنثى", Gender = "أنثى" };
            _db.Patients.AddRange(malePatient, femalePatient);
            await _db.SaveChangesAsync();

            _db.Visits.AddRange(
                new Visit { PatientId = malePatient.PatientId, VisitDate = DateTime.Today, ReferralId = ref1.ReferralId },
                new Visit { PatientId = femalePatient.PatientId, VisitDate = DateTime.Today, ReferralId = ref1.ReferralId });
            await _db.SaveChangesAsync();

            // Act — تصفية بالجنس والجهة معاً
            var snapshot = await _statsService.GetSnapshotAsync(
                DateTime.Today.AddDays(-1),
                DateTime.Today.AddDays(1),
                "ذكر",
                ref1.ReferralId);

            // Assert
            snapshot.Summary.VisitCount.Should().Be(1, "تصفية مركبة: جنس ذكر + جهة A = زيارة واحدة");
        }

        [Fact]
        public async Task GetSnapshotAsync_WhenCalled_ShouldCalculateTotalPatientsAndVisitsSeparately()
        {
            // Function: 9.1 — Patient Count by Gender (عدد المرضى الفريدين)
            // Arrange — نفس المريض بزيارتين
            var patient = new Patient { LabId = "UNIQ01", FullName = "مريض متكرر", Gender = "ذكر" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            _db.Visits.AddRange(
                new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today },
                new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today.AddDays(-1) });
            await _db.SaveChangesAsync();

            // Act
            var snapshot = await _statsService.GetSnapshotAsync(
                DateTime.Today.AddDays(-2),
                DateTime.Today.AddDays(1),
                "الكل",
                null);

            // Assert
            snapshot.Summary.VisitCount.Should().Be(2, "إجمالي الزيارات = 2");
            snapshot.Summary.PatientCount.Should().Be(1, "عدد المرضى المميزين = 1 (نفس المريض مرتين)");
        }
    }
}

