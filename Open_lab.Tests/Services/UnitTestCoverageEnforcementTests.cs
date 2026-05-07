using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class UnitTestCoverageEnforcementTests
    {
        [Fact]
        public void CoverageTracker_When_FunctionMarkedComplete_Should_List_TestNames()
        {
            // Function: 13.8 — Set System Password
            // Arrange
            // Act
            var path = GetCoverageTrackerPath();
            var lines = File.ReadAllLines(path);

            var functionRowRegex = new Regex(@"^\|\s*\d+\.\d+\s*\|", RegexOptions.Compiled);
            var functionRows = lines
                .Where(l => functionRowRegex.IsMatch(l))
                .ToList();

            foreach (var row in functionRows)
            {
                var cells = row.Trim().Trim('|').Split('|').Select(p => p.Trim()).ToArray();
                // Assert
                cells.Length.Should().BeGreaterOrEqualTo(6, $"row should have all columns: {row}");

                var functionId = cells[0];
                var statusColumn = cells[4];
                var testsColumn = cells[5];

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
