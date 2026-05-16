using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Open_lab.IntegrationTests.Infrastructure;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.ViewModels;

namespace Open_lab.IntegrationTests;

public class PatientScreensSqliteIntegrationTests : SqliteIntegrationTestBase
{
    [Fact]
    public async Task PatientRegistration_Save_ShouldPersistPatientVisitSelectedTestsAndInvoice()
    {
        // Arrange — catalog
        var test = await SeedTestAsync("CBC", "Complete Blood Count", 120m);

        // Build the four aggregates the registration screen produces.
        var patient = new Patient
        {
            FullName = "Integration Patient",
            Gender = "Male",
            Age = 35,
            Phone = "01000000000",
            NationalId = "29901011234567",
            Email = "patient@example.test"
        };

        var visit = new Visit
        {
            VisitDate = DateTime.Today.AddHours(9),
            AccountType = "Cash",
            Status = "Open"
        };

        var visitTests = new[]
        {
            new VisitTest
            {
                TestId = test.TestId,
                Price = test.PatientPrice ?? test.Price,
                Status = "Pending"
            }
        };

        // Invoice math matching the original assertions (10% discount, 50 paid).
        const decimal total = 120m;
        const decimal discount = 12m;        // 10% of 120
        const decimal netTotal = 108m;       // 120 - 12
        const decimal paid = 50m;
        const decimal balance = 58m;         // 108 - 50

        var invoice = new Invoice
        {
            Total = total,
            Discount = discount,
            NetTotal = netTotal,
            Paid = paid,
            Balance = balance,
            Status = "Open"
        };

        // Act — single atomic transaction (Fix #2).
        var service = new PatientService(Db);
        var visitId = await service.SaveRegistrationAsync(patient, visit, visitTests, invoice);

        // Assert — persistence + relationships.
        visitId.Should().BeGreaterThan(0);

        Db.ChangeTracker.Clear();

        var savedPatient = await Db.Patients.AsNoTracking()
            .SingleAsync(p => p.FullName == "Integration Patient");
        savedPatient.LabId.Should().NotBeNullOrWhiteSpace();
        savedPatient.NationalId.Should().Be("29901011234567");

        var savedVisit = await Db.Visits.AsNoTracking()
            .Include(v => v.VisitTests)
            .SingleAsync(v => v.PatientId == savedPatient.PatientId);
        savedVisit.VisitTests.Should().ContainSingle(vt => vt.TestId == test.TestId && vt.Price == 120m);

        var savedInvoice = await Db.Invoices.AsNoTracking()
            .SingleAsync(i => i.VisitId == savedVisit.VisitId);
        savedInvoice.Total.Should().Be(120m);
        savedInvoice.Discount.Should().Be(12m);
        savedInvoice.NetTotal.Should().Be(108m);
        savedInvoice.Paid.Should().Be(50m);
        savedInvoice.Balance.Should().Be(58m);
    }

    [Fact]
    public async Task PatientUpdate_ShouldPersistAllSixPreviouslyDroppedFields()
    {
        // Arrange — create a baseline patient.
        var original = new Patient
        {
            LabId = "LAB-U-001",
            FullName = "Update Target",
            Gender = "Female",
            Age = 20,
            IsVip = false,
            IsPregnant = false,
            HomePhone = "0220000000",
            NationalId = "20001011234567",
            Email = "old@example.test"
        };
        Db.Patients.Add(original);
        await Db.SaveChangesAsync();
        var patientId = original.PatientId;
        Db.ChangeTracker.Clear();

        // Act — update all six historically dropped fields plus a few base ones.
        var edited = new Patient
        {
            PatientId = patientId,
            LabId = "LAB-U-001",
            FullName = "Update Target",
            Gender = "Female",
            Age = 31,
            IsVip = true,
            IsPregnant = true,
            HomePhone = "0233333333",
            NationalId = "29501011234567",
            Email = "new@example.test"
        };

        var service = new PatientService(Db);
        await service.UpdateAsync(edited, userId: 1);

        Db.ChangeTracker.Clear();

        // Assert — every one of the six fields was persisted (C-02 regression guard).
        var reloaded = await Db.Patients.AsNoTracking().SingleAsync(p => p.PatientId == patientId);
        reloaded.Age.Should().Be(31);
        reloaded.IsVip.Should().BeTrue();
        reloaded.IsPregnant.Should().BeTrue();
        reloaded.HomePhone.Should().Be("0233333333");
        reloaded.NationalId.Should().Be("29501011234567");
        reloaded.Email.Should().Be("new@example.test");
    }

    [Fact]
    public async Task ResultsEntry_SaveAndVerify_ShouldPersistResultValuesAndVerifiedStatus()
    {
        var test = await SeedTestAsync("GLU", "Glucose", 80m);
        var parameter = new TestParameter { TestId = test.TestId, Name = "Glucose", OrderNo = 1 };
        Db.TestParameters.Add(parameter);
        var patient = new Patient { LabId = "LAB-R-001", FullName = "Results Patient", Gender = "Female", Age = 29 };
        Db.Patients.Add(patient);
        await Db.SaveChangesAsync();
        var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today.AddHours(9), AccountType = "Cash", Status = "Open" };
        Db.Visits.Add(visit);
        await Db.SaveChangesAsync();
        var visitTest = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 80m, Status = "Pending" };
        Db.VisitTests.Add(visitTest);
        await Db.SaveChangesAsync();

        var viewModel = new ResultsEntryViewModel(new ResultsService(Db), new PatientService(Db))
        {
            DateFrom = DateTime.Today,
            DateTo = DateTime.Today
        };
        viewModel.LoadVisitTestsCommand.Execute(null);
        await WaitUntilAsync(() => viewModel.VisitTests.Count == 1, "Results entry did not load today's visit tests.");
        viewModel.SelectedVisitTest = viewModel.VisitTests.Single();
        await WaitUntilAsync(() => viewModel.ResultItems.Count == 1, "Results entry did not load test parameters.");

        viewModel.ResultItems[0].Value = "115";
        viewModel.SaveResultsCommand.Execute(null);
        await WaitUntilAsync(() => Db.ResultValues.Any(rv => rv.VisitTestId == visitTest.VisitTestId), "Result value was not saved.");
        viewModel.VerifyResultsCommand.Execute(null);
        await WaitUntilAsync(() => Db.VisitTests.AsNoTracking().Any(vt => vt.VisitTestId == visitTest.VisitTestId && vt.Status == "Verified"), "Visit test was not verified.");

        var saved = await Db.ResultValues.AsNoTracking().SingleAsync(rv => rv.VisitTestId == visitTest.VisitTestId);
        saved.Value.Should().Be("115");
        saved.ParameterId.Should().Be(parameter.ParameterId);
        saved.VerifiedBy.Should().Be(1);
    }

    [Fact]
    public async Task PatientSearch_AdvancedFilters_ShouldFindByDateAgeGroupAndNationalIdThenLoadVisits()
    {
        var patient = new Patient
        {
            LabId = "LAB-S-001",
            FullName = "Searchable Patient",
            Gender = "Male",
            Age = 42,
            Phone = "01111111111",
            NationalId = "28102021234567"
        };
        Db.Patients.Add(patient);
        await Db.SaveChangesAsync();
        Db.Visits.Add(new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today.AddHours(10), AccountType = "Cash", Status = "Open" });
        await Db.SaveChangesAsync();

        var viewModel = new PatientSearchViewModel(new PatientSearchService(Db))
        {
            NationalId = "28102021234567",
            DateFrom = DateTime.Today,
            DateTo = DateTime.Today,
            AgeGroup = "بالغين"
        };
        viewModel.SearchCommand.Execute(null);
        await WaitUntilAsync(() => viewModel.Patients.Count == 1, "Patient search did not return the expected advanced-filter result.");

        viewModel.SelectedPatient = viewModel.Patients.Single();
        await WaitUntilAsync(() => viewModel.Visits.Count == 1, "Patient search did not load visits for the selected patient.");
        viewModel.SelectedPatient!.FullName.Should().Be("Searchable Patient");
        viewModel.Visits.Single().PatientId.Should().Be(patient.PatientId);
    }

    [Fact]
    public async Task Delivery_PayThenDeliver_ShouldPersistPaymentAndMarkVisitDelivered()
    {
        var test = await SeedTestAsync("DEL", "Delivery Test", 150m);
        var patient = new Patient { LabId = "LAB-D-001", FullName = "Delivery Patient", Gender = "Male", Age = 50 };
        Db.Patients.Add(patient);
        await Db.SaveChangesAsync();
        var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today.AddHours(11), AccountType = "Cash", Status = "Open" };
        Db.Visits.Add(visit);
        await Db.SaveChangesAsync();
        Db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 150m, Status = "Verified" });
        Db.Invoices.Add(new Invoice { VisitId = visit.VisitId, Total = 150m, NetTotal = 150m, Paid = 0m, Balance = 150m, Status = "Open" });
        await Db.SaveChangesAsync();

        var viewModel = new DeliveryViewModel(new DeliveryService(Db), new InvoiceService(Db))
        {
            DateFrom = DateTime.Today,
            DateTo = DateTime.Today
        };
        viewModel.SearchCommand.Execute(null);
        await WaitUntilAsync(() => viewModel.Visits.Count == 1, "Delivery screen did not load today's visit.");
        viewModel.SelectedVisit = viewModel.Visits.Single();
        viewModel.PaymentAmount = 150m;

        viewModel.PayCommand.Execute(null);
        await WaitUntilAsync(() => Db.Payments.Any(p => p.Invoice!.VisitId == visit.VisitId), "Delivery payment was not persisted.");
        Db.ChangeTracker.Clear();
        var invoice = await Db.Invoices.AsNoTracking().SingleAsync(i => i.VisitId == visit.VisitId);
        invoice.Balance.Should().Be(0m);
        invoice.Paid.Should().Be(150m);

        viewModel.SelectedVisit = viewModel.Visits.Single(v => v.VisitId == visit.VisitId);
        viewModel.DeliverCommand.Execute(null);
        await WaitUntilAsync(() => Db.Visits.AsNoTracking().Any(v => v.VisitId == visit.VisitId && v.Status == "Completed"), "Delivery did not mark the visit as completed.");

        var deliveredTest = await Db.VisitTests.AsNoTracking().SingleAsync(vt => vt.VisitId == visit.VisitId);
        deliveredTest.Status.Should().Be("Delivered");
    }
}
