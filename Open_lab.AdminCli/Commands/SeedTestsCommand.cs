using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.AdminCli.Commands
{
    internal static class SeedTestsCommand
    {
        public static async Task<int> ExecuteAsync()
        {
            if (!Confirm())
            {
                Console.Error.WriteLine("Seed cancelled.");
                return 2;
            }

            await using var db = new OpenLabDbContextFactory().CreateDbContext(Array.Empty<string>());
            await db.Database.OpenConnectionAsync();
            await using var transaction = await db.Database.BeginTransactionAsync();

            SessionContext.Current.IsSystemOperation = true;
            try
            {
                var units = await UpsertUnitsAsync(db);
                var sampleTypes = await UpsertSampleTypesAsync(db);
                var groups = await UpsertGroupsAsync(db);

                var touched = await UpsertTestsAsync(db, units, sampleTypes, groups);

                db.AuditLogs.Add(new AuditLog
                {
                    UserId = 1,
                    Action = "TestFixtureSeed",
                    TableName = nameof(Test),
                    RecordId = touched.ToString(),
                    Timestamp = DateTime.UtcNow
                });

                await db.SaveChangesAsync();
                await transaction.CommitAsync();
                Console.WriteLine($"Seed completed. Tests touched: {touched}.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Seed failed: {ex.Message}");
                await transaction.RollbackAsync();
                return 3;
            }
            finally
            {
                SessionContext.Current.IsSystemOperation = false;
            }
        }

        private static bool Confirm()
        {
            Console.Write($"Type {SeedConstants.SeedConfirmToken} to confirm seeding fixture tests: ");
            var confirmation = Console.ReadLine();
            return string.Equals(confirmation, SeedConstants.SeedConfirmToken, StringComparison.Ordinal);
        }

        private static async Task<Dictionary<string, int>> UpsertUnitsAsync(OpenLabDbContext db)
        {
            var existing = await db.Units
                .Where(u => TestSeedData.Units.Contains(u.Name))
                .ToListAsync();

            var map = existing.ToDictionary(u => u.Name, u => u.UnitId);
            foreach (var name in TestSeedData.Units)
            {
                if (map.ContainsKey(name)) continue;
                var entity = new Unit { Name = name };
                db.Units.Add(entity);
                await db.SaveChangesAsync();
                map[name] = entity.UnitId;
            }
            return map;
        }

        private static async Task<Dictionary<string, int>> UpsertSampleTypesAsync(OpenLabDbContext db)
        {
            var existing = await db.SampleTypes
                .Where(s => TestSeedData.SampleTypes.Contains(s.Name))
                .ToListAsync();

            var map = existing.ToDictionary(s => s.Name, s => s.SampleTypeId);
            foreach (var name in TestSeedData.SampleTypes)
            {
                if (map.ContainsKey(name)) continue;
                var entity = new SampleType { Name = name };
                db.SampleTypes.Add(entity);
                await db.SaveChangesAsync();
                map[name] = entity.SampleTypeId;
            }
            return map;
        }

        private static async Task<Dictionary<string, int>> UpsertGroupsAsync(OpenLabDbContext db)
        {
            var existing = await db.TestGroups
                .Where(g => TestSeedData.Groups.Contains(g.GroupName))
                .ToListAsync();

            var map = existing.ToDictionary(g => g.GroupName, g => g.GroupId);
            foreach (var name in TestSeedData.Groups)
            {
                if (map.ContainsKey(name)) continue;
                var entity = new TestGroup { GroupName = name };
                db.TestGroups.Add(entity);
                await db.SaveChangesAsync();
                map[name] = entity.GroupId;
            }
            return map;
        }

        private static async Task<int> UpsertTestsAsync(
            OpenLabDbContext db,
            IReadOnlyDictionary<string, int> units,
            IReadOnlyDictionary<string, int> sampleTypes,
            IReadOnlyDictionary<string, int> groups)
        {
            int touched = 0;
            foreach (var fixture in TestSeedData.Tests)
            {
                var test = await db.Tests
                    .Include(t => t.Parameters)
                    .Include(t => t.ReferenceRanges)
                    .FirstOrDefaultAsync(t => t.Code == fixture.Code);

                if (test == null)
                {
                    test = new Test { Code = fixture.Code };
                    db.Tests.Add(test);
                }

                test.NameReport = fixture.NameReport;
                test.NameReceipt = fixture.NameReceipt;
                test.GroupId = fixture.Group is null ? null : groups[fixture.Group];
                test.SampleTypeId = fixture.SampleType is null ? null : sampleTypes[fixture.SampleType];
                test.UnitId = fixture.Unit is null ? null : units[fixture.Unit];
                test.Price = fixture.Price;
                test.CostPrice = fixture.CostPrice;
                test.PatientPrice = fixture.PatientPrice;
                test.TurnaroundHours = fixture.TurnaroundHours;
                test.IsRoutine = fixture.IsRoutine;
                test.IsSendOut = fixture.IsSendOut;
                test.ReportOrder = fixture.ReportOrder;
                test.Description = fixture.Description;

                await db.SaveChangesAsync();

                var parameterIdsByName = await UpsertParametersAsync(db, test, fixture, units);
                await UpsertRangesAsync(db, test, fixture, parameterIdsByName);

                touched++;
            }
            return touched;
        }

        private static async Task<Dictionary<string, int>> UpsertParametersAsync(
            OpenLabDbContext db,
            Test test,
            TestSeedData.SeedTest fixture,
            IReadOnlyDictionary<string, int> units)
        {
            var existing = await db.TestParameters.Where(p => p.TestId == test.TestId).ToListAsync();
            var byName = existing.ToDictionary(p => p.Name, p => p);

            foreach (var p in fixture.Parameters)
            {
                if (!byName.TryGetValue(p.Name, out var parameter))
                {
                    parameter = new TestParameter { TestId = test.TestId, Name = p.Name };
                    db.TestParameters.Add(parameter);
                    byName[p.Name] = parameter;
                }
                parameter.UnitId = p.Unit is null ? null : units[p.Unit];
                parameter.OrderNo = p.OrderNo;
            }

            await db.SaveChangesAsync();
            return byName.ToDictionary(kv => kv.Key, kv => kv.Value.ParameterId);
        }

        private static async Task UpsertRangesAsync(
            OpenLabDbContext db,
            Test test,
            TestSeedData.SeedTest fixture,
            IReadOnlyDictionary<string, int> parameterIdsByName)
        {
            var existing = await db.TestReferenceRanges.Where(r => r.TestId == test.TestId).ToListAsync();
            db.TestReferenceRanges.RemoveRange(existing);
            await db.SaveChangesAsync();

            foreach (var r in fixture.Ranges)
            {
                db.TestReferenceRanges.Add(BuildRange(test.TestId, null, r));
            }

            foreach (var p in fixture.Parameters)
            {
                if (p.Ranges.Count == 0) continue;
                var parameterId = parameterIdsByName[p.Name];
                foreach (var r in p.Ranges)
                {
                    db.TestReferenceRanges.Add(BuildRange(test.TestId, parameterId, r));
                }
            }

            await db.SaveChangesAsync();
        }

        private static TestReferenceRange BuildRange(int testId, int? parameterId, TestSeedData.SeedRange r)
        {
            return new TestReferenceRange
            {
                TestId = testId,
                ParameterId = parameterId,
                Gender = r.Gender,
                LowValue = r.LowValue,
                HighValue = r.HighValue,
                NormalText = r.NormalText,
                AgeFromValue = r.AgeFromValue,
                AgeFromUnit = r.AgeFromUnit,
                AgeFromDays = r.AgeFromDays,
                AgeToValue = r.AgeToValue,
                AgeToUnit = r.AgeToUnit,
                AgeToDays = r.AgeToDays
            };
        }
    }
}
