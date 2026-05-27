using System.Collections.Generic;

namespace Open_lab.AdminCli.Commands
{
    internal static class TestSeedData
    {
        public const string UnitMgDl = "[TEST] mg/dL";
        public const string UnitGDl = "[TEST] g/dL";
        public const string UnitPercent = "[TEST] %";
        public const string UnitMillionPerUl = "[TEST] x10^6/uL";
        public const string UnitThousandPerUl = "[TEST] x10^3/uL";
        public const string UnitFl = "[TEST] fL";
        public const string UnitPg = "[TEST] pg";

        public const string SampleSerum = "[TEST] Serum";
        public const string SampleWholeBlood = "[TEST] Whole Blood";
        public const string SampleUrine = "[TEST] Urine";

        public const string GroupChemistry = "[TEST] Chemistry";
        public const string GroupHematology = "[TEST] Hematology";
        public const string GroupUrinalysis = "[TEST] Urinalysis";

        public static IReadOnlyList<string> Units => new[]
        {
            UnitMgDl, UnitGDl, UnitPercent, UnitMillionPerUl, UnitThousandPerUl, UnitFl, UnitPg
        };

        public static IReadOnlyList<string> SampleTypes => new[]
        {
            SampleSerum, SampleWholeBlood, SampleUrine
        };

        public static IReadOnlyList<string> Groups => new[]
        {
            GroupChemistry, GroupHematology, GroupUrinalysis
        };

        public static IReadOnlyList<SeedTest> Tests => new[]
        {
            new SeedTest
            {
                Code = "[TEST]UA_ACID",
                NameReport = "[TEST] Uric Acid",
                NameReceipt = "[TEST] Uric Acid",
                Group = GroupChemistry,
                SampleType = SampleSerum,
                Unit = UnitMgDl,
                Price = 35m,
                CostPrice = 10m,
                PatientPrice = 35m,
                TurnaroundHours = 4,
                IsRoutine = true,
                ReportOrder = 10,
                Description = "Fixture single-component uric acid test.",
                Parameters = new[]
                {
                    new SeedParameter { Name = "Uric Acid", Unit = UnitMgDl, OrderNo = 1,
                        Ranges = new[]
                        {
                            new SeedRange { Gender = "Male", LowValue = 3.4m, HighValue = 7.0m, NormalText = "3.4-7.0 mg/dL" },
                            new SeedRange { Gender = "Female", LowValue = 2.4m, HighValue = 6.0m, NormalText = "2.4-6.0 mg/dL" }
                        } }
                }
            },
            new SeedTest
            {
                Code = "[TEST]CHOL",
                NameReport = "[TEST] Cholesterol",
                NameReceipt = "[TEST] Cholesterol",
                Group = GroupChemistry,
                SampleType = SampleSerum,
                Unit = UnitMgDl,
                Price = 45m,
                CostPrice = 12m,
                PatientPrice = 45m,
                TurnaroundHours = 4,
                IsRoutine = true,
                ReportOrder = 15,
                Description = "Fixture single-component cholesterol test.",
                Parameters = new[]
                {
                    new SeedParameter { Name = "Cholesterol", Unit = UnitMgDl, OrderNo = 1,
                        Ranges = new[]
                        {
                            new SeedRange { LowValue = 0m, HighValue = 200m, NormalText = "< 200 mg/dL" }
                        } }
                }
            },
            new SeedTest
            {
                Code = "[TEST]CBC",
                NameReport = "[TEST] Complete Blood Count",
                NameReceipt = "[TEST] CBC",
                Group = GroupHematology,
                SampleType = SampleWholeBlood,
                Unit = null,
                Price = 60m,
                CostPrice = 18m,
                PatientPrice = 60m,
                TurnaroundHours = 6,
                IsRoutine = true,
                ReportOrder = 20,
                Description = "Fixture multi-component CBC panel.",
                Parameters = new[]
                {
                    new SeedParameter { Name = "Hemoglobin", Unit = UnitGDl, OrderNo = 1,
                        Ranges = new[]
                        {
                            new SeedRange { Gender = "Male",   LowValue = 13.5m, HighValue = 17.5m, NormalText = "13.5-17.5 g/dL" },
                            new SeedRange { Gender = "Female", LowValue = 12.0m, HighValue = 15.5m, NormalText = "12.0-15.5 g/dL" }
                        } },
                    new SeedParameter { Name = "WBC", Unit = UnitThousandPerUl, OrderNo = 2,
                        Ranges = new[]
                        {
                            new SeedRange { LowValue = 4.0m, HighValue = 11.0m, NormalText = "4.0-11.0 x10^3/uL" }
                        } },
                    new SeedParameter { Name = "RBC", Unit = UnitMillionPerUl, OrderNo = 3,
                        Ranges = new[]
                        {
                            new SeedRange { Gender = "Male",   LowValue = 4.5m, HighValue = 5.9m, NormalText = "4.5-5.9 x10^6/uL" },
                            new SeedRange { Gender = "Female", LowValue = 4.1m, HighValue = 5.1m, NormalText = "4.1-5.1 x10^6/uL" }
                        } },
                    new SeedParameter { Name = "Hematocrit", Unit = UnitPercent, OrderNo = 4,
                        Ranges = new[]
                        {
                            new SeedRange { Gender = "Male",   LowValue = 41m, HighValue = 53m, NormalText = "41-53 %" },
                            new SeedRange { Gender = "Female", LowValue = 36m, HighValue = 46m, NormalText = "36-46 %" }
                        } },
                    new SeedParameter { Name = "MCV", Unit = UnitFl, OrderNo = 5,
                        Ranges = new[]
                        {
                            new SeedRange { LowValue = 80m, HighValue = 100m, NormalText = "80-100 fL" }
                        } },
                    new SeedParameter { Name = "MCH", Unit = UnitPg, OrderNo = 6,
                        Ranges = new[]
                        {
                            new SeedRange { LowValue = 27m, HighValue = 33m, NormalText = "27-33 pg" }
                        } },
                    new SeedParameter { Name = "Platelets", Unit = UnitThousandPerUl, OrderNo = 7,
                        Ranges = new[]
                        {
                            new SeedRange { LowValue = 150m, HighValue = 450m, NormalText = "150-450 x10^3/uL" }
                        } }
                }
            },
            new SeedTest
            {
                Code = "[TEST]U_ANAL",
                NameReport = "[TEST] Urine Analysis",
                NameReceipt = "[TEST] Urinalysis",
                Group = GroupUrinalysis,
                SampleType = SampleUrine,
                Unit = null,
                Price = 30m,
                CostPrice = 9m,
                PatientPrice = 30m,
                TurnaroundHours = 2,
                IsRoutine = true,
                ReportOrder = 30,
                Description = "Fixture urinalysis panel.",
                Parameters = new[]
                {
                    new SeedParameter { Name = "Color",   Unit = null, OrderNo = 1 },
                    new SeedParameter { Name = "Glucose", Unit = null, OrderNo = 2 },
                    new SeedParameter { Name = "Protein", Unit = null, OrderNo = 3 }
                }
            }
        };

        internal sealed class SeedTest
        {
            public string Code { get; init; } = string.Empty;
            public string NameReport { get; init; } = string.Empty;
            public string NameReceipt { get; init; } = string.Empty;
            public string? Group { get; init; }
            public string? SampleType { get; init; }
            public string? Unit { get; init; }
            public decimal Price { get; init; }
            public decimal? CostPrice { get; init; }
            public decimal? PatientPrice { get; init; }
            public int TurnaroundHours { get; init; }
            public bool IsRoutine { get; init; }
            public bool IsSendOut { get; init; }
            public int ReportOrder { get; init; }
            public string? Description { get; init; }
            public IReadOnlyList<SeedParameter> Parameters { get; init; } = System.Array.Empty<SeedParameter>();
            public IReadOnlyList<SeedRange> Ranges { get; init; } = System.Array.Empty<SeedRange>();
        }

        internal sealed class SeedParameter
        {
            public string Name { get; init; } = string.Empty;
            public string? Unit { get; init; }
            public int OrderNo { get; init; }
            public IReadOnlyList<SeedRange> Ranges { get; init; } = System.Array.Empty<SeedRange>();
        }

        internal sealed class SeedRange
        {
            public string? Gender { get; init; }
            public decimal? LowValue { get; init; }
            public decimal? HighValue { get; init; }
            public string? NormalText { get; init; }
            public int? AgeFromValue { get; init; }
            public string? AgeFromUnit { get; init; }
            public int? AgeFromDays { get; init; }
            public int? AgeToValue { get; init; }
            public string? AgeToUnit { get; init; }
            public int? AgeToDays { get; init; }
        }
    }
}
