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
        var test = await SeedTestAsync("CBC", "Complete Blood Count", 120m);
        var viewModel = new PatientRegistrationViewModel(
            new PatientService(Db),
            new TestCatalogService(Db),
            new VisitService(Db),
            new InvoiceService(Db),
            barcodeDialogService: null,
            printService: null,
            initialize: false)
        {
            FullName = "Integration Patient",
            Gender = "Male",
            Age = 35,
            Phone = "01000000000",
            NationalId = "29901011234567",
            Email = "patient@example.test",
            DiscountPercent = 10m,
            PaidAmount = 50m
        };
        viewModel.SelectedTests.Add(new SelectedTestItem
        {
            TestId = test.TestId,
            TestName = test.NameReceipt,
            Price = test.PatientPrice ?? test.Price
        });

        viewModel.SaveCommand.Execute(null);
        await WaitUntilAsync(() => viewModel.CurrentVisitId > 0 || viewModel.StatusMessage.StartsWith("خطأ"), "Patient registration save did not complete.");

        viewModel.StatusMessage.Should().NotStartWith("خطأ");
        var patient = await Db.Patients.SingleAsync(p => p.FullName == "Integration Patient");
        patient.LabId.Should().NotBeNullOrWhiteSpace();
        patient.NationalId.Should().Be("29901011234567");

        var visit = await Db.Visits.Include(v => v.VisitTests).SingleAsync(v => v.PatientId == patient.PatientId);
        visit.VisitTests.Should().ContainSingle(vt => vt.TestId == test.TestId && vt.Price == 120m);

        var invoice = await Db.Invoices.SingleAsync(i => i.VisitId == visit.VisitId);
        invoice.Total.Should().Be(120m);
        invoice.Discount.Should().Be(12m);
        invoice.NetTotal.Should().Be(108m);
        invoice.Paid.Should().Be(50m);
        invoice.Balance.Should().Be(58m);
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
