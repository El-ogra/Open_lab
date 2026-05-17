using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.Services
{
    public class ResultsService : IResultsService
    {
        private readonly OpenLabDbContext _db;

        public ResultsService(OpenLabDbContext db)
        {
            _db = db;
        }

        public Task<List<VisitTest>> GetVisitTestsByDateAsync(DateTime from, DateTime to)
        {
            return _db.VisitTests
                .AsNoTracking()
                .Include(vt => vt.Visit)
                .ThenInclude(v => v.Patient)
                .Include(vt => vt.Test)
                .Include(vt => vt.ResultValues)
                .Where(vt => vt.Visit.VisitDate >= from && vt.Visit.VisitDate <= to)
                .OrderBy(vt => vt.Visit.VisitDate)
                .ToListAsync();
        }

        public Task<VisitTest?> GetVisitTestByIdAsync(int visitTestId)
        {
            return _db.VisitTests.AsNoTracking().FirstOrDefaultAsync(vt => vt.VisitTestId == visitTestId);
        }

        public Task<List<ResultValue>> GetResultsForVisitTestAsync(int visitTestId)
        {
            return _db.ResultValues
                .AsNoTracking()
                .Include(rv => rv.Parameter)
                .Where(rv => rv.VisitTestId == visitTestId)
                .ToListAsync();
        }

        public Task<List<TestParameter>> GetParametersForTestAsync(int testId)
        {
            return _db.TestParameters
                .AsNoTracking()
                .Where(p => p.TestId == testId)
                .OrderBy(p => p.OrderNo)
                .ToListAsync();
        }

        public async Task SaveResultAsync(int visitTestId, int parameterId, string? value, string? flag, string? comment)
        {
            await SaveResultCoreAsync(visitTestId, parameterId, value, flag, comment, _db.CurrentUserId);
        }

        public async Task SaveResultAsync(int visitTestId, int parameterId, string? value, string? flag, string? comment, int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("UserId is required for result audit trail.", nameof(userId));
            }

            await SaveResultCoreAsync(visitTestId, parameterId, value, flag, comment, userId);
        }

        private async Task SaveResultCoreAsync(int visitTestId, int parameterId, string? value, string? flag, string? comment, int? userId)
        {
            var visitTest = await _db.VisitTests.FirstOrDefaultAsync(vt => vt.VisitTestId == visitTestId);
            if (visitTest == null)
            {
                throw new InvalidOperationException("Visit test not found.");
            }

            if (string.Equals(visitTest.Status, "Verified", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Verified results are locked. Reopen first.");
            }

            var existing = await _db.ResultValues
                .FirstOrDefaultAsync(rv => rv.VisitTestId == visitTestId && rv.ParameterId == parameterId);

            if (existing == null)
            {
                existing = new ResultValue
                {
                    VisitTestId = visitTestId,
                    ParameterId = parameterId,
                    Value = value,
                    Flag = flag,
                    Comment = comment
                };
                _db.ResultValues.Add(existing);
            }
            else
            {
                var oldValues = BuildResultSnapshot(existing);
                var newValues = BuildResultSnapshot(value, flag, comment);
                if (userId.HasValue && userId.Value > 0 && !string.Equals(oldValues, newValues, StringComparison.Ordinal))
                {
                    _db.AuditLogs.Add(new AuditLog
                    {
                        UserId = userId.Value,
                        Action = "EDIT_RESULT",
                        TableName = "ResultValues",
                        RecordId = $"{visitTestId}-{parameterId}",
                        OldValues = oldValues,
                        NewValues = newValues,
                        Timestamp = DateTime.UtcNow,
                    });
                }

                existing.Value = value;
                existing.Flag = flag;
                existing.Comment = comment;
                existing.VerifiedBy = null;
                existing.VerifiedAt = null;
            }

            await _db.SaveChangesAsync();

            var parameterIds = await _db.TestParameters
                .Where(p => p.TestId == visitTest.TestId)
                .Select(p => p.ParameterId)
                .ToListAsync();

            var savedResults = await _db.ResultValues
                .Where(rv => rv.VisitTestId == visitTestId)
                .ToListAsync();

            var isCompleted = parameterIds.Count == 0
                ? savedResults.Any(r => !string.IsNullOrWhiteSpace(r.Value))
                : parameterIds.All(pid =>
                    savedResults.Any(r => r.ParameterId == pid && !string.IsNullOrWhiteSpace(r.Value)));

            var newStatus = isCompleted ? "Completed" : "InProgress";
            if (!string.Equals(visitTest.Status, newStatus, StringComparison.OrdinalIgnoreCase))
            {
                visitTest.Status = newStatus;
                await _db.SaveChangesAsync();
            }
        }

        public async Task VerifyVisitTestAsync(int visitTestId, int verifiedByUserId)
        {
            var visitTest = await _db.VisitTests.FirstOrDefaultAsync(vt => vt.VisitTestId == visitTestId);
            if (visitTest == null)
            {
                throw new InvalidOperationException("Visit test not found.");
            }

            var parametersCount = await _db.TestParameters.CountAsync(p => p.TestId == visitTest.TestId);
            var results = await _db.ResultValues.Where(rv => rv.VisitTestId == visitTestId).ToListAsync();
            if (results.Count == 0)
            {
                throw new InvalidOperationException("No results to verify.");
            }

            var hasMissing = parametersCount > 0 && results.Count < parametersCount;
            if (hasMissing || results.Any(r => string.IsNullOrWhiteSpace(r.Value)))
            {
                throw new InvalidOperationException("All parameters must have values before verification.");
            }

            var now = DateTime.Now;
            foreach (var result in results)
            {
                result.VerifiedBy = verifiedByUserId;
                result.VerifiedAt = now;
            }

            visitTest.Status = "Verified";
            await _db.SaveChangesAsync();
        }

        public async Task ReopenVisitTestAsync(int visitTestId)
        {
            var visitTest = await _db.VisitTests.FirstOrDefaultAsync(vt => vt.VisitTestId == visitTestId);
            if (visitTest == null)
            {
                throw new InvalidOperationException("Visit test not found.");
            }

            if (!string.Equals(visitTest.Status, "Verified", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var results = await _db.ResultValues.Where(rv => rv.VisitTestId == visitTestId).ToListAsync();
            foreach (var result in results)
            {
                result.VerifiedBy = null;
                result.VerifiedAt = null;
            }

            visitTest.Status = "InProgress";
            await _db.SaveChangesAsync();
        }

        public async Task MarkVisitTestsCompletedAsync(IEnumerable<int> visitTestIds)
        {
            var ids = visitTestIds.Distinct().ToList();
            if (ids.Count == 0)
            {
                return;
            }

            var tests = await _db.VisitTests
                .Where(vt => ids.Contains(vt.VisitTestId))
                .ToListAsync();

            foreach (var test in tests)
            {
                if (!string.Equals(test.Status, "Verified", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(test.Status, "Delivered", StringComparison.OrdinalIgnoreCase))
                {
                    test.Status = "Completed";
                }
            }

            await _db.SaveChangesAsync();
        }

        public async Task MarkVisitTestsVerifiedAsync(IEnumerable<int> visitTestIds, int verifiedByUserId)
        {
            foreach (var id in visitTestIds.Distinct())
            {
                await VerifyVisitTestAsync(id, verifiedByUserId);
            }
        }

        public async Task MarkVisitTestsPrintedAsync(IEnumerable<int> visitTestIds, int userId)
        {
            var ids = visitTestIds.Distinct().ToList();
            if (ids.Count == 0)
            {
                return;
            }

            var visitIds = await _db.VisitTests
                .AsNoTracking()
                .Where(vt => ids.Contains(vt.VisitTestId))
                .Select(vt => vt.VisitId)
                .Distinct()
                .ToListAsync();

            foreach (var visitId in visitIds)
            {
                await LogVisitReportPrintedAsync(visitId, userId);
            }
        }

        public async Task LogVisitReportPrintedAsync(int visitId, int userId)
        {
            _db.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                Action = "PRINT_REPORT",
                TableName = "Visits",
                RecordId = visitId.ToString(),
                Timestamp = DateTime.UtcNow,
                NewValues = "Visit report generated and printed."
            });
            await _db.SaveChangesAsync();
        }

        public async Task<ReferenceRangeResult> ValidateResultAsync(int testId, string value, string gender, int age)
        {
            var result = new ReferenceRangeResult { OriginalValue = value };

            if (decimal.TryParse(value, out decimal numericValue))
            {
                result.NumericValue = numericValue;

                // 4.1 Automated Range Comparison
                var range = await _db.TestReferenceRanges
                    .Where(r => r.TestId == testId)
                    .Where(r => r.Gender == null || r.Gender == gender)
                    .Where(r => (r.AgeFrom == null || age >= r.AgeFrom) && (r.AgeTo == null || age <= r.AgeTo))
                    .OrderByDescending(r => r.Gender != null) // Prefer specific gender over null
                    .FirstOrDefaultAsync();

                if (range != null)
                {
                    result.LowThreshold = range.LowValue;
                    result.HighThreshold = range.HighValue;

                    if (range.LowValue.HasValue && numericValue < range.LowValue.Value)
                    {
                        result.IsLow = true;
                        result.Flag = "L";
                    }
                    else if (range.HighValue.HasValue && numericValue > range.HighValue.Value)
                    {
                        result.IsHigh = true;
                        result.Flag = "H";
                    }
                    else
                    {
                        result.IsNormal = true;
                        result.Flag = null;
                    }
                }
            }

            // 4.1 Automated Comments (High/Low)
            var comment = await _db.TestComments
                .Where(c => c.TestId == testId)
                .OrderByDescending(c => c.IsDefault)
                .FirstOrDefaultAsync();

            if (comment != null)
            {
                result.DefaultComment = comment.CommentText;
                result.LowComment = comment.LowComment;
                result.HighComment = comment.HighComment;
            }

            return result;
        }

        public async Task<VisitTest?> GetPreviousResultAsync(int patientId, int testId, int excludeVisitTestId)
        {
            return await _db.VisitTests
                .AsNoTracking()
                .Include(vt => vt.Visit)
                .Include(vt => vt.ResultValues)
                .ThenInclude(rv => rv.Parameter)
                .Where(vt => vt.Visit.PatientId == patientId
                    && vt.TestId == testId
                    && vt.VisitTestId != excludeVisitTestId
                    && vt.ResultValues.Any(rv => !string.IsNullOrWhiteSpace(rv.Value)))
                .OrderByDescending(vt => vt.Visit.VisitDate)
                .FirstOrDefaultAsync();
        }

        private static string BuildResultSnapshot(ResultValue result)
        {
            return BuildResultSnapshot(result.Value, result.Flag, result.Comment);
        }

        private static string BuildResultSnapshot(string? value, string? flag, string? comment)
        {
            return string.Join(" | ", new[]
            {
                $"Value={value}",
                $"Flag={flag}",
                $"Comment={comment}"
            });
        }
    }
}
