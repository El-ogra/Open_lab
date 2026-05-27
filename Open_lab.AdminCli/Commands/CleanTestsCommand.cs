using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.AdminCli.Commands
{
    internal static class CleanTestsCommand
    {
        public static async Task<int> ExecuteAsync()
        {
            if (!Confirm())
            {
                Console.Error.WriteLine("Clean cancelled.");
                return 2;
            }

            await using var db = new OpenLabDbContextFactory().CreateDbContext(Array.Empty<string>());
            await db.Database.OpenConnectionAsync();
            await using var transaction = await db.Database.BeginTransactionAsync();

            SessionContext.Current.IsSystemOperation = true;
            try
            {
                var prefix = SeedConstants.TestPrefix;

                var referencedVisitTestCount = await db.VisitTests
                    .CountAsync(vt => vt.Test.Code.StartsWith(prefix));
                if (referencedVisitTestCount > 0)
                {
                    Console.Error.WriteLine(
                        $"Refusing to clean: {referencedVisitTestCount} VisitTest row(s) still reference [TEST] tests. " +
                        "Detach them before re-running.");
                    await transaction.RollbackAsync();
                    return 4;
                }

                var ranges = await db.TestReferenceRanges
                    .Where(r => r.Test.Code.StartsWith(prefix))
                    .ToListAsync();
                db.TestReferenceRanges.RemoveRange(ranges);

                var comments = await db.TestComments
                    .Where(c => c.Test.Code.StartsWith(prefix))
                    .ToListAsync();
                db.TestComments.RemoveRange(comments);

                await db.SaveChangesAsync();

                var parameters = await db.TestParameters
                    .Where(p => p.Test.Code.StartsWith(prefix))
                    .ToListAsync();
                db.TestParameters.RemoveRange(parameters);
                await db.SaveChangesAsync();

                var tests = await db.Tests
                    .Where(t => t.Code.StartsWith(prefix))
                    .ToListAsync();
                db.Tests.RemoveRange(tests);
                await db.SaveChangesAsync();

                int groupsRemoved = await RemoveUnreferencedGroupsAsync(db, prefix);
                int sampleTypesRemoved = await RemoveUnreferencedSampleTypesAsync(db, prefix);
                int unitsRemoved = await RemoveUnreferencedUnitsAsync(db, prefix);

                db.AuditLogs.Add(new AuditLog
                {
                    UserId = 1,
                    Action = "TestFixtureClean",
                    TableName = nameof(Test),
                    RecordId = tests.Count.ToString(),
                    Timestamp = DateTime.UtcNow
                });

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                Console.WriteLine(
                    $"Clean completed. Ranges: {ranges.Count}, Comments: {comments.Count}, " +
                    $"Parameters: {parameters.Count}, Tests: {tests.Count}, " +
                    $"Groups: {groupsRemoved}, SampleTypes: {sampleTypesRemoved}, Units: {unitsRemoved}.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Clean failed: {ex.Message}");
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
            Console.Write($"Type {SeedConstants.CleanConfirmToken} to confirm deletion of fixture tests: ");
            var confirmation = Console.ReadLine();
            return string.Equals(confirmation, SeedConstants.CleanConfirmToken, StringComparison.Ordinal);
        }

        private static async Task<int> RemoveUnreferencedGroupsAsync(OpenLabDbContext db, string prefix)
        {
            var groups = await db.TestGroups
                .Where(g => g.GroupName.StartsWith(prefix))
                .Where(g => !db.Tests.Any(t => t.GroupId == g.GroupId))
                .ToListAsync();
            db.TestGroups.RemoveRange(groups);
            await db.SaveChangesAsync();
            return groups.Count;
        }

        private static async Task<int> RemoveUnreferencedSampleTypesAsync(OpenLabDbContext db, string prefix)
        {
            var sampleTypes = await db.SampleTypes
                .Where(s => s.Name.StartsWith(prefix))
                .Where(s => !db.Tests.Any(t => t.SampleTypeId == s.SampleTypeId))
                .ToListAsync();
            db.SampleTypes.RemoveRange(sampleTypes);
            await db.SaveChangesAsync();
            return sampleTypes.Count;
        }

        private static async Task<int> RemoveUnreferencedUnitsAsync(OpenLabDbContext db, string prefix)
        {
            var units = await db.Units
                .Where(u => u.Name.StartsWith(prefix))
                .Where(u => !db.Tests.Any(t => t.UnitId == u.UnitId))
                .Where(u => !db.TestParameters.Any(p => p.UnitId == u.UnitId))
                .ToListAsync();
            db.Units.RemoveRange(units);
            await db.SaveChangesAsync();
            return units.Count;
        }
    }
}
