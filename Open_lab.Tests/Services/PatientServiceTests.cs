using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class PatientServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly PatientService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public PatientServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new PatientService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task CreateAsync_Should_Create_Patient_And_Generate_LabId()
        {
            // Arrange
            var patient = new Patient
            {
                FullName = "John Doe",
                Gender = "Male"
            };

            // Act
            var created = await _service.CreateAsync(patient);

            // Assert
            created.PatientId.Should().BeGreaterThan(0);
            created.LabId.Should().NotBeNullOrWhiteSpace();
            (await _db.Patients.FindAsync(created.PatientId)).Should().NotBeNull();
        }

        [Fact]
        public async Task CreateAsync_Invalid_Gender_Should_Throw()
        {
            // Arrange
            var patient = new Patient
            {
                FullName = "Jane",
                Gender = "Unknown"
            };

            // Act
            Func<Task> act = async () => await _service.CreateAsync(patient);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GenerateNextLabIdAsync_Should_Increment_Sequence()
        {
            // Arrange - seed a patient with today's prefix
            var today = DateTime.Today;
            var prefix = today.ToString("yyyyMMdd");
            var existing = new Patient { LabId = prefix + "001", FullName = "A", Gender = "Male" };
            _db.Patients.Add(existing);
            await _db.SaveChangesAsync();

            // Act
            var next = await _service.GenerateNextLabIdAsync(today);

            // Assert
            next.Should().StartWith(prefix);
            next.Should().EndWith("002");
        }

        [Fact]
        public async Task DeleteAsync_With_Visits_Should_Throw()
        {
            // Arrange
            var patient = new Patient { LabId = "L1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            _db.Visits.Add(new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now });
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.DeleteAsync(patient.PatientId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        // 1.2 - Patient Update - CRUD Logic Validation Tests
        [Fact]
        public async Task UpdateAsync_Should_Update_All_Fields()
        {
            // Arrange
            var patient = new Patient { LabId = "LUP1", FullName = "Old Name", Gender = "Male", Phone = "123456", Address = "Old Address" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var updatedPatient = new Patient
            {
                PatientId = patient.PatientId,
                LabId = "LUP1",
                FullName = "New Name",
                Gender = "Female",
                Phone = "789012",
                Address = "New Address",
                BirthDate = new DateTime(1990, 1, 1)
            };

            // Act
            await _service.UpdateAsync(updatedPatient);

            // Assert - Logic Guard: Verify all fields are updated
            var saved = await _db.Patients.FindAsync(patient.PatientId);
            saved.Should().NotBeNull();
            saved!.FullName.Should().Be("New Name");
            saved.Gender.Should().Be("Female");
            saved.Phone.Should().Be("789012");
            saved.Address.Should().Be("New Address");
            saved.BirthDate.Should().Be(new DateTime(1990, 1, 1));
        }

        [Fact]
        public async Task UpdateAsync_DuplicateLabId_Should_Throw()
        {
            // Arrange
            var patient1 = new Patient { LabId = "LUP2", FullName = "P1", Gender = "Male" };
            var patient2 = new Patient { LabId = "LUP3", FullName = "P2", Gender = "Male" };
            _db.Patients.AddRange(patient1, patient2);
            await _db.SaveChangesAsync();

            var updatedPatient = new Patient
            {
                PatientId = patient2.PatientId,
                LabId = "LUP2", // Duplicate
                FullName = "P2",
                Gender = "Male"
            };

            // Act
            Func<Task> act = async () => await _service.UpdateAsync(updatedPatient);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*LabId already exists*");
        }

        [Fact]
        public async Task UpdateAsync_InvalidGender_Should_Throw()
        {
            // Arrange
            var patient = new Patient { LabId = "LUP4", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var updatedPatient = new Patient
            {
                PatientId = patient.PatientId,
                LabId = "LUP4",
                FullName = "P",
                Gender = "InvalidGender"
            };

            // Act
            Func<Task> act = async () => await _service.UpdateAsync(updatedPatient);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Gender value is invalid*");
        }

        [Fact]
        public async Task UpdateAsync_FutureBirthDate_Should_Throw()
        {
            // Arrange
            var patient = new Patient { LabId = "LUP5", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var updatedPatient = new Patient
            {
                PatientId = patient.PatientId,
                LabId = "LUP5",
                FullName = "P",
                Gender = "Male",
                BirthDate = DateTime.Today.AddDays(1)
            };

            // Act
            Func<Task> act = async () => await _service.UpdateAsync(updatedPatient);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*BirthDate cannot be in the future*");
        }

        // 1.6 - Medical History Retrieval - Clinical Data Integrity Tests
        [Fact]
        public async Task GetMedicalHistoryAsync_Should_Return_History_For_Patient()
        {
            // Arrange
            var patient = new Patient { LabId = "LMH1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var history = new MedicalHistory
            {
                PatientId = patient.PatientId,
                ChronicDiseases = "Diabetes",
                Allergies = "Penicillin",
                Medications = "Insulin",
                Notes = "Regular checkup"
            };
            _db.MedicalHistories.Add(history);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetMedicalHistoryAsync(patient.PatientId);

            // Assert - Logic Guard: Verify all fields are returned correctly
            result.Should().NotBeNull();
            result!.ChronicDiseases.Should().Be("Diabetes");
            result.Allergies.Should().Be("Penicillin");
            result.Medications.Should().Be("Insulin");
            result.Notes.Should().Be("Regular checkup");
        }

        [Fact]
        public async Task GetMedicalHistoryAsync_InvalidPatientId_Should_Throw()
        {
            // Act
            Func<Task> act = async () => await _service.GetMedicalHistoryAsync(-1);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("*PatientId is required*");
        }

        [Fact]
        public async Task SaveMedicalHistoryAsync_Should_Create_New_History()
        {
            // Arrange
            var patient = new Patient { LabId = "LMH2", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var history = new MedicalHistory
            {
                ChronicDiseases = "Hypertension",
                Allergies = "None",
                Medications = "Aspirin",
                Notes = "New patient"
            };

            // Act
            await _service.SaveMedicalHistoryAsync(patient.PatientId, history);

            // Assert - Logic Guard: Verify history is created with correct data
            var saved = await _db.MedicalHistories.FirstOrDefaultAsync(m => m.PatientId == patient.PatientId);
            saved.Should().NotBeNull();
            saved!.ChronicDiseases.Should().Be("Hypertension");
            saved.Allergies.Should().Be("None");
            saved.Medications.Should().Be("Aspirin");
            saved.Notes.Should().Be("New patient");
        }

        [Fact]
        public async Task SaveMedicalHistoryAsync_Should_Update_Existing_History()
        {
            // Arrange
            var patient = new Patient { LabId = "LMH3", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var existingHistory = new MedicalHistory
            {
                PatientId = patient.PatientId,
                ChronicDiseases = "Old Disease",
                Allergies = "Old Allergy",
                Medications = "Old Medication",
                Notes = "Old Notes"
            };
            _db.MedicalHistories.Add(existingHistory);
            await _db.SaveChangesAsync();

            var updatedHistory = new MedicalHistory
            {
                ChronicDiseases = "New Disease",
                Allergies = "New Allergy",
                Medications = "New Medication",
                Notes = "New Notes"
            };

            // Act
            await _service.SaveMedicalHistoryAsync(patient.PatientId, updatedHistory);

            // Assert - Logic Guard: Verify history is updated
            var saved = await _db.MedicalHistories.FirstOrDefaultAsync(m => m.PatientId == patient.PatientId);
            saved.Should().NotBeNull();
            saved!.ChronicDiseases.Should().Be("New Disease");
            saved.Allergies.Should().Be("New Allergy");
            saved.Medications.Should().Be("New Medication");
            saved.Notes.Should().Be("New Notes");
        }

        [Fact]
        public async Task SaveMedicalHistoryAsync_NonExistentPatient_Should_Throw()
        {
            // Arrange
            var history = new MedicalHistory { ChronicDiseases = "Diabetes" };

            // Act
            Func<Task> act = async () => await _service.SaveMedicalHistoryAsync(99999, history);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Patient not found*");
        }
    }
}
