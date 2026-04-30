using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class UnitTestCoverageEnforcementTests
    {
        [Fact]
        public void CoverageTracker_When_FunctionMarkedComplete_Should_List_TestNames()
        {
            var path = GetCoverageTrackerPath();
            var lines = File.ReadAllLines(path);

            var completeRows = lines
                .Where(l => l.Contains("|") && l.Contains("✅") && !l.Contains("رقم الوظيفة"))
                .ToList();

            foreach (var row in completeRows)
            {
                var parts = row.Split('|').Select(p => p.Trim()).ToArray();
                parts.Length.Should().BeGreaterThan(6, $"row should have all columns: {row}");

                var functionId = parts[1];
                var statusColumn = parts[5];
                var testsColumn = parts[6];

                if (statusColumn.StartsWith("✅", StringComparison.Ordinal))
                {
                    testsColumn.Should().NotBeNullOrWhiteSpace($"function {functionId} is ✅ but has no listed tests");
                }
            }
        }

        private static string GetCoverageTrackerPath()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                var sln = Path.Combine(dir.FullName, "Open_lab.sln");
                if (File.Exists(sln))
                {
                    return Path.Combine(dir.FullName, "Open_lab", "Docs", "unit_test_result.md");
                }

                dir = dir.Parent;
            }

            throw new DirectoryNotFoundException("Could not locate repository root (Open_lab.sln).");
        }
    }
}

