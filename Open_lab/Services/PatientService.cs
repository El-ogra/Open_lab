using System;
using System.Collections.Generic;
using System.Globalization;
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
            return _db.Patients.AsNoTracking().Include(p => p.Referral).FirstOrDefaultAsync(p => p.PatientId == patientId);
        }

        public Task<Patient?> GetByLabIdAsync(string labId)
        {
            if (string.IsNullOrWhiteSpace(labId))
            {
                throw new ArgumentException("LabId is required.", nameof(labId));
            }

            return _db.Patients.AsNoTracking().Include(p => p.Referral).FirstOrDefaultAsync(p => p.LabId == labId.Trim());
        }

        public Task<MedicalHistory?> GetMedicalHistoryAsync(int patientId)
        {
            if (patientId <= 0)
            {
                throw new ArgumentException("PatientId is required.", nameof(patientId));
            }

            return _db.MedicalHistories
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.PatientId == patientId);
        }

        public async Task SaveMedicalHistoryAsync(int patientId, MedicalHistory history)
        {
            if (patientId <= 0)
            {
                throw new ArgumentException("PatientId is required.", nameof(patientId));
            }

            if (history == null)
            {
                throw new ArgumentNullException(nameof(history));
            }

            var patientExists = await _db.Patients.AnyAsync(p => p.PatientId == patientId);
            if (!patientExists)
            {
                throw new InvalidOperationException("Patient not found.");
            }

            var current = await _db.MedicalHistories.FirstOrDefaultAsync(m => m.PatientId == patientId);
            if (current == null)
            {
                current = new MedicalHistory
                {
                    PatientId = patientId
                };
                _db.MedicalHistories.Add(current);
            }

            current.ChronicDiseases = string.IsNullOrWhiteSpace(history.ChronicDiseases) ? null : history.ChronicDiseases.Trim();
            current.Allergies = string.IsNullOrWhiteSpace(history.Allergies) ? null : history.Allergies.Trim();
            current.Medications = string.IsNullOrWhiteSpace(history.Medications) ? null : history.Medications.Trim();
            current.Notes = string.IsNullOrWhiteSpace(history.Notes) ? null : history.Notes.Trim();

            await _db.SaveChangesAsync();
        }

        public Task<List<Patient>> SearchAsync(string? name, string? phone, DateTime? date = null, string? labId = null)
        {
            var query = _db.Patients.AsNoTracking().Include(p => p.Referral).AsQueryable();

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

            if (!string.IsNullOrWhiteSpace(labId))
            {
                var idTerm = labId.Trim();
                query = query.Where(p => p.LabId.Contains(idTerm));
            }

            if (date.HasValue)
            {
                var searchDate = date.Value.Date;
                query = query.Where(p => p.Visits.Any(v => v.VisitDate.Date == searchDate));
            }

            return query.OrderBy(p => p.FullName).ToListAsync();
        }

        public async Task<string> GenerateNextLabIdAsync(DateTime? forDate = null)
        {
            var date = (forDate ?? DateTime.Today).Date;
            var prefix = date.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            var latestForDay = await _db.Patients
                .AsNoTracking()
                .Where(p => p.LabId.StartsWith(prefix))
                .Select(p => p.LabId)
                .ToListAsync();

            var maxSequence = 0;
            foreach (var labId in latestForDay)
            {
                if (labId.Length <= prefix.Length)
                {
                    continue;
                }

                var suffix = labId[prefix.Length..];
                if (int.TryParse(suffix, out var value) && value > maxSequence)
                {
                    maxSequence = value;
                }
            }

            var next = maxSequence + 1;
            var candidate = $"{prefix}{next:D3}";
            while (await _db.Patients.AnyAsync(p => p.LabId == candidate))
            {
                next++;
                candidate = $"{prefix}{next:D3}";
            }

            return candidate;
        }

        public async Task<Patient> CreateAsync(Patient patient)
        {
            if (patient == null)
            {
                throw new ArgumentNullException(nameof(patient));
            }

            Normalize(patient);
            await ValidateReferralAsync(patient.ReferralId);
            if (string.IsNullOrWhiteSpace(patient.LabId))
            {
                patient.LabId = await GenerateNextLabIdAsync(DateTime.Today);
            }

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

        public Task UpdateAsync(Patient patient)
        {
            return UpdateCoreAsync(patient, _db.CurrentUserId);
        }

        public Task UpdateAsync(Patient patient, int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("UserId is required for patient audit trail.", nameof(userId));
            }

            return UpdateCoreAsync(patient, userId);
        }

        public async Task AssignReferralAsync(int patientId, int? referralId, int userId)
        {
            if (patientId <= 0)
            {
                throw new ArgumentException("PatientId is required.", nameof(patientId));
            }

            if (userId <= 0)
            {
                throw new ArgumentException("UserId is required for contract assignment audit trail.", nameof(userId));
            }

            await ValidateReferralAsync(referralId);

            var current = await _db.Patients.FirstOrDefaultAsync(p => p.PatientId == patientId);
            if (current == null)
            {
                throw new InvalidOperationException("Patient not found.");
            }

            if (current.ReferralId == referralId)
            {
                return;
            }

            var oldValues = $"ReferralId={current.ReferralId?.ToString() ?? "null"}";
            current.ReferralId = referralId;
            AddAuditLog(userId, "ASSIGN_PATIENT_CONTRACT", current.PatientId.ToString(), oldValues, $"ReferralId={referralId?.ToString() ?? "null"}");
            await _db.SaveChangesAsync();
        }

        private async Task UpdateCoreAsync(Patient patient, int? userId)
        {
            if (patient == null)
            {
                throw new ArgumentNullException(nameof(patient));
            }

            Normalize(patient);
            await ValidateReferralAsync(patient.ReferralId);
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

            var oldValues = BuildPatientSnapshot(current);

            current.LabId = patient.LabId;
            current.FullName = patient.FullName;
            current.Gender = patient.Gender;
            current.BirthDate = patient.BirthDate;
            current.Phone = patient.Phone;
            current.Address = patient.Address;
            current.ReferralId = patient.ReferralId;

            var newValues = BuildPatientSnapshot(current);
            if (userId.HasValue && !string.Equals(oldValues, newValues, StringComparison.Ordinal))
            {
                AddAuditLog(userId.Value, "EDIT_PATIENT", current.PatientId.ToString(), oldValues, newValues);
            }

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
            if (patient.ReferralId <= 0)
            {
                patient.ReferralId = null;
            }
        }

        private async Task ValidateReferralAsync(int? referralId)
        {
            if (!referralId.HasValue)
            {
                return;
            }

            var exists = await _db.Referrals.AnyAsync(r => r.ReferralId == referralId.Value);
            if (!exists)
            {
                throw new InvalidOperationException("Referral contract not found.");
            }
        }

        private void AddAuditLog(int userId, string action, string recordId, string oldValues, string newValues)
        {
            _db.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                Action = action,
                TableName = "Patients",
                RecordId = recordId,
                OldValues = oldValues,
                NewValues = newValues,
                Timestamp = DateTime.UtcNow
            });
        }

        private static string BuildPatientSnapshot(Patient patient)
        {
            return string.Join(" | ", new[]
            {
                $"LabId={patient.LabId}",
                $"FullName={patient.FullName}",
                $"Gender={patient.Gender}",
                $"BirthDate={patient.BirthDate:yyyy-MM-dd}",
                $"Phone={patient.Phone}",
                $"Address={patient.Address}",
                $"ReferralId={patient.ReferralId?.ToString() ?? "null"}"
            });
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
