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

            return _db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.LabId == labId);
        }

        public Task<List<Patient>> SearchAsync(string? name, string? phone)
        {
            var query = _db.Patients.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(p => p.FullName.Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(phone))
            {
                query = query.Where(p => p.Phone != null && p.Phone.Contains(phone));
            }

            return query.OrderBy(p => p.FullName).ToListAsync();
        }

        public async Task<Patient> CreateAsync(Patient patient)
        {
            if (patient == null)
            {
                throw new ArgumentNullException(nameof(patient));
            }

            if (string.IsNullOrWhiteSpace(patient.FullName))
            {
                throw new ArgumentException("FullName is required.", nameof(patient));
            }

            if (string.IsNullOrWhiteSpace(patient.LabId))
            {
                throw new ArgumentException("LabId is required.", nameof(patient));
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

            _db.Patients.Update(patient);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int patientId)
        {
            var patient = await _db.Patients.FirstOrDefaultAsync(p => p.PatientId == patientId);
            if (patient == null)
            {
                return;
            }

            _db.Patients.Remove(patient);
            await _db.SaveChangesAsync();
        }
    }
}
