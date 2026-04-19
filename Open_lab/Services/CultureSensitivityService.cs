using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;
using Open_lab.ViewModels;

namespace Open_lab.Services
{
    public class CultureSensitivityService : ICultureSensitivityService
    {
        private readonly OpenLabDbContext _db;

        public CultureSensitivityService(OpenLabDbContext db)
        {
            _db = db;
        }

        public Task<List<Culture>> GetCulturesAsync()
        {
            return _db.Cultures.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<Culture> CreateCultureAsync(Culture culture)
        {
            if (culture == null || string.IsNullOrWhiteSpace(culture.Name))
            {
                throw new ArgumentException("اسم المزرعة مطلوب.", nameof(culture));
            }

            culture.Name = culture.Name.Trim();
            var duplicate = await _db.Cultures.AnyAsync(c => c.Name == culture.Name);
            if (duplicate)
            {
                throw new InvalidOperationException("اسم المزرعة موجود مسبقًا.");
            }

            _db.Cultures.Add(culture);
            await _db.SaveChangesAsync();
            return culture;
        }

        public async Task DeleteCultureAsync(int cultureId)
        {
            var culture = await _db.Cultures.FirstOrDefaultAsync(c => c.CultureId == cultureId);
            if (culture == null)
            {
                return;
            }

            _db.Cultures.Remove(culture);
            await _db.SaveChangesAsync();
        }

        public Task<List<Antibiotic>> GetAntibioticsAsync()
        {
            return _db.Antibiotics.AsNoTracking().OrderBy(a => a.Name).ToListAsync();
        }

        public async Task<Antibiotic> CreateAntibioticAsync(Antibiotic antibiotic)
        {
            if (antibiotic == null || string.IsNullOrWhiteSpace(antibiotic.Name))
            {
                throw new ArgumentException("اسم المضاد الحيوي مطلوب.", nameof(antibiotic));
            }

            antibiotic.Name = antibiotic.Name.Trim();
            var duplicate = await _db.Antibiotics.AnyAsync(a => a.Name == antibiotic.Name);
            if (duplicate)
            {
                throw new InvalidOperationException("اسم المضاد الحيوي موجود مسبقًا.");
            }

            _db.Antibiotics.Add(antibiotic);
            await _db.SaveChangesAsync();
            return antibiotic;
        }

        public async Task DeleteAntibioticAsync(int antibioticId)
        {
            var antibiotic = await _db.Antibiotics.FirstOrDefaultAsync(a => a.AntibioticId == antibioticId);
            if (antibiotic == null)
            {
                return;
            }

            _db.Antibiotics.Remove(antibiotic);
            await _db.SaveChangesAsync();
        }

        public Task<List<CultureAntibiotic>> GetCultureAntibioticsAsync(int cultureId)
        {
            return _db.CultureAntibiotics
                .AsNoTracking()
                .Include(ca => ca.Antibiotic)
                .Where(ca => ca.CultureId == cultureId)
                .OrderBy(ca => ca.Antibiotic.Name)
                .ToListAsync();
        }

        public async Task LinkAntibioticAsync(int cultureId, int antibioticId)
        {
            var exists = await _db.CultureAntibiotics
                .AnyAsync(ca => ca.CultureId == cultureId && ca.AntibioticId == antibioticId);

            if (exists)
            {
                return;
            }

            _db.CultureAntibiotics.Add(new CultureAntibiotic
            {
                CultureId = cultureId,
                AntibioticId = antibioticId
            });
            await _db.SaveChangesAsync();
        }

        public async Task UnlinkAntibioticAsync(int cultureId, int antibioticId)
        {
            var link = await _db.CultureAntibiotics
                .FirstOrDefaultAsync(ca => ca.CultureId == cultureId && ca.AntibioticId == antibioticId);

            if (link == null)
            {
                return;
            }

            _db.CultureAntibiotics.Remove(link);
            await _db.SaveChangesAsync();
        }

        public async Task<List<CultureVisitTestRow>> SearchCultureVisitTestsAsync(string? labId, DateTime from, DateTime to)
        {
            var query = _db.VisitTests
                .AsNoTracking()
                .Include(vt => vt.Visit)
                .ThenInclude(v => v.Patient)
                .Include(vt => vt.Test)
                .Where(vt => vt.Visit.VisitDate >= from && vt.Visit.VisitDate <= to)
                .Where(vt => vt.Test.NameReport.Contains("Culture") || vt.Test.NameReport.Contains("مزرعة"));

            if (!string.IsNullOrWhiteSpace(labId))
            {
                var term = labId.Trim();
                query = query.Where(vt => vt.Visit.Patient.LabId.Contains(term));
            }

            return await query
                .OrderByDescending(vt => vt.Visit.VisitDate)
                .Select(vt => new CultureVisitTestRow
                {
                    VisitTestId = vt.VisitTestId,
                    VisitId = vt.VisitId,
                    LabId = vt.Visit.Patient.LabId,
                    PatientName = vt.Visit.Patient.FullName,
                    TestName = vt.Test.NameReport,
                    VisitDate = vt.Visit.VisitDate,
                    Status = vt.Status
                })
                .ToListAsync();
        }

        public async Task<List<Antibiotic>> GetFilteredAntibioticsAsync(int visitTestId)
        {
            var visitTest = await _db.VisitTests
                .AsNoTracking()
                .Include(vt => vt.Visit)
                .ThenInclude(v => v.Patient)
                .FirstOrDefaultAsync(vt => vt.VisitTestId == visitTestId);

            if (visitTest == null) return await GetAntibioticsAsync();

            var patient = visitTest.Visit.Patient;
            var isChild = patient.Age.HasValue && patient.Age < 12;
            var isPregnant = patient.IsPregnant;

            var query = _db.Antibiotics.AsNoTracking();

            if (isPregnant)
            {
                query = query.Where(a => a.IsSafeForPregnancy);
            }

            if (isChild)
            {
                query = query.Where(a => a.IsSafeForChildren);
            }

            return await query.OrderBy(a => a.Name).ToListAsync();
        }

        public async Task SaveCultureResultAsync(int visitTestId, int cultureId, IReadOnlyCollection<CultureSensitivityValue> sensitivities)
        {
            var visitTest = await _db.VisitTests
                .Include(vt => vt.Test)
                .FirstOrDefaultAsync(vt => vt.VisitTestId == visitTestId);
            if (visitTest == null)
            {
                throw new InvalidOperationException("Visit test not found.");
            }

            if (string.Equals(visitTest.Status, "Verified", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("لا يمكن تعديل نتيجة معتمدة.");
            }

            var culture = await _db.Cultures.FirstOrDefaultAsync(c => c.CultureId == cultureId);
            if (culture == null)
            {
                throw new InvalidOperationException("المزرعة غير موجودة.");
            }

            var cultureParameter = await EnsureParameterAsync(visitTest.TestId, "Culture");
            await UpsertResultValueAsync(visitTestId, cultureParameter.ParameterId, culture.Name, null);

            foreach (var item in sensitivities.Where(s => !string.IsNullOrWhiteSpace(s.Sensitivity)))
            {
                var antibiotic = await _db.Antibiotics.FirstOrDefaultAsync(a => a.AntibioticId == item.AntibioticId);
                if (antibiotic == null)
                {
                    continue;
                }

                var parameter = await EnsureParameterAsync(visitTest.TestId, antibiotic.Name);
                await UpsertResultValueAsync(visitTestId, parameter.ParameterId, item.Sensitivity.Trim(), item.Comment);
            }

            visitTest.Status = "InProgress";
            await _db.SaveChangesAsync();
        }

        private async Task<TestParameter> EnsureParameterAsync(int testId, string parameterName)
        {
            var existing = await _db.TestParameters.FirstOrDefaultAsync(p => p.TestId == testId && p.Name == parameterName);
            if (existing != null)
            {
                return existing;
            }

            var maxOrder = await _db.TestParameters
                .Where(p => p.TestId == testId)
                .Select(p => (int?)p.OrderNo)
                .MaxAsync() ?? 0;

            var parameter = new TestParameter
            {
                TestId = testId,
                Name = parameterName,
                OrderNo = maxOrder + 1
            };
            _db.TestParameters.Add(parameter);
            await _db.SaveChangesAsync();
            return parameter;
        }

        private async Task UpsertResultValueAsync(int visitTestId, int parameterId, string value, string? comment)
        {
            var existing = await _db.ResultValues.FirstOrDefaultAsync(r => r.VisitTestId == visitTestId && r.ParameterId == parameterId);
            if (existing == null)
            {
                _db.ResultValues.Add(new ResultValue
                {
                    VisitTestId = visitTestId,
                    ParameterId = parameterId,
                    Value = value,
                    Comment = comment,
                    Flag = null
                });
                return;
            }

            existing.Value = value;
            existing.Comment = comment;
            existing.Flag = null;
            existing.VerifiedAt = null;
            existing.VerifiedBy = null;
        }
    }
}
