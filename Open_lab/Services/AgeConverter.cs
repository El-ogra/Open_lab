namespace Open_lab.Services
{
    /// <summary>
    /// Single source of truth for converting between (value + unit) age input and
    /// the internal "days" representation used for storage and range comparisons.
    /// Keeping all conversions here prevents unit drift across services / VMs.
    /// </summary>
    public static class AgeConverter
    {
        public const int DaysPerMonth = 30;
        public const int DaysPerYear = 365;

        public const string UnitDay = "Day";
        public const string UnitMonth = "Month";
        public const string UnitYear = "Year";

        public static int? ToDays(int? value, string? unit)
        {
            return value.HasValue ? ToDays(value.Value, unit) : (int?)null;
        }

        public static int ToDays(int value, string? unit)
        {
            return NormalizeUnit(unit) switch
            {
                UnitDay => value,
                UnitMonth => value * DaysPerMonth,
                _ => value * DaysPerYear
            };
        }

        public static bool IsValidUnit(string? unit)
        {
            var normalized = NormalizeUnit(unit);
            return normalized == UnitDay || normalized == UnitMonth || normalized == UnitYear;
        }

        /// <summary>
        /// Accepts both legacy plural forms ("Years"/"Months"/"Days") and the canonical
        /// singular form. Returns the canonical singular, or "Year" when null/empty.
        /// </summary>
        public static string NormalizeUnit(string? unit)
        {
            if (string.IsNullOrWhiteSpace(unit))
            {
                return UnitYear;
            }

            var trimmed = unit.Trim();
            return trimmed switch
            {
                "Day" or "Days" or "day" or "days" => UnitDay,
                "Month" or "Months" or "month" or "months" => UnitMonth,
                "Year" or "Years" or "year" or "years" => UnitYear,
                _ => UnitYear
            };
        }
    }
}
