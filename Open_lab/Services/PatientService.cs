using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.Services
{
    public class PatientService : IPatientService
    {
        private static readonly string[] AllowedGenders = { "Male", "Female", "ذكر", "أنثى" };
        private readonly OpenLabDbContext _db;

        public PatientService(OpenLabDbContext db)
        {
            _db = db;
        }

        public Task<Patient?> GetByIdAsync(int patientId)
        {
            return _db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.PatientId == patientId);
        }

        public Task<Patient?> GetByLabIdAsync(string labId)
        {
            if (string.IsNullOrWhiteSpace(labId))
            {
                throw new ArgumentException("LabId is required.", nameof(labId));
            }

            return _db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.LabId == labId.Trim());
        }

        public Task<List<Patient>> SearchAsync(string? name, string? phone)
        {
            var query = _db.Patients.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                var term = name.Trim();
                query = query.Where(p => p.FullName.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(phone))
            {
                var phoneTerm = phone.Trim();
                query = query.Where(p => p.Phone != null && p.Phone.Contains(phoneTerm));
            }

            return query.OrderBy(p => p.FullName).ToListAsync();
        }

        public async Task<Patient> CreateAsync(Patient patient)
        {
            if (patient == null)
            {
                throw new ArgumentNullException(nameof(patient));
            }

            Normalize(patient);
            ValidateBusinessRules(patient);

            var exists = await _db.Patients.AnyAsync(p => p.LabId == patient.LabId);
            if (exists)
            {
                throw new InvalidOperationException("LabId already exists.");
            }

            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();
            return patient;
        }

        public async Task UpdateAsync(Patient patient)
        {
            if (patient == null)
            {
                throw new ArgumentNullException(nameof(patient));
            }

            Normalize(patient);
            ValidateBusinessRules(patient);

            var current = await _db.Patients.FirstOrDefaultAsync(p => p.PatientId == patient.PatientId);
            if (current == null)
            {
                throw new InvalidOperationException("Patient not found.");
            }

            var duplicateLabId = await _db.Patients.AnyAsync(p => p.PatientId != patient.PatientId && p.LabId == patient.LabId);
            if (duplicateLabId)
            {
                throw new InvalidOperationException("LabId already exists.");
            }

            current.LabId = patient.LabId;
            current.FullName = patient.FullName;
            current.Gender = patient.Gender;
            current.BirthDate = patient.BirthDate;
            current.Phone = patient.Phone;
            current.Address = patient.Address;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int patientId)
        {
            var patient = await _db.Patients.FirstOrDefaultAsync(p => p.PatientId == patientId);
            if (patient == null)
            {
                return;
            }

            var hasVisits = await _db.Visits.AnyAsync(v => v.PatientId == patientId);
            if (hasVisits)
            {
                throw new InvalidOperationException("Cannot delete patient with visits.");
            }

            _db.Patients.Remove(patient);
            await _db.SaveChangesAsync();
        }

        private static void Normalize(Patient patient)
        {
            patient.LabId = patient.LabId?.Trim() ?? string.Empty;
            patient.FullName = patient.FullName?.Trim() ?? string.Empty;
            patient.Gender = patient.Gender?.Trim() ?? string.Empty;
            patient.Phone = string.IsNullOrWhiteSpace(patient.Phone) ? null : patient.Phone.Trim();
            patient.Address = string.IsNullOrWhiteSpace(patient.Address) ? null : patient.Address.Trim();
        }

        private static void ValidateBusinessRules(Patient patient)
        {
            if (string.IsNullOrWhiteSpace(patient.FullName))
            {
                throw new ArgumentException("FullName is required.", nameof(patient));
            }

            if (string.IsNullOrWhiteSpace(patient.LabId))
            {
                throw new ArgumentException("LabId is required.", nameof(patient));
            }

            if (string.IsNullOrWhiteSpace(patient.Gender))
            {
                throw new ArgumentException("Gender is required.", nameof(patient));
            }

            if (!AllowedGenders.Contains(patient.Gender, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Gender value is invalid.", nameof(patient));
            }

            if (patient.BirthDate.HasValue && patient.BirthDate.Value.Date > DateTime.Today)
            {
                throw new ArgumentException("BirthDate cannot be in the future.", nameof(patient));
            }
        }
    }
}
