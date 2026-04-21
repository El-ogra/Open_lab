namespace Open_lab.Models
{
    /// <summary>
    /// Represents the result of comparing a test value against reference ranges.
    /// </summary>
    public class ReferenceRangeResult
    {
        /// <summary>
        /// The original value entered.
        /// </summary>
        public string? OriginalValue { get; set; }

        /// <summary>
        /// The numeric value parsed (if applicable).
        /// </summary>
        public decimal? NumericValue { get; set; }

        /// <summary>
        /// The calculated flag based on reference ranges: L (Low), H (High), N (Normal), or null.
        /// </summary>
        public string? Flag { get; set; }

        /// <summary>
        /// Indicates if the value is within the normal range.
        /// </summary>
        public bool IsNormal { get; set; }

        /// <summary>
        /// Indicates if the value is below the low threshold.
        /// </summary>
        public bool IsLow { get; set; }

        /// <summary>
        /// Indicates if the value is above the high threshold.
        /// </summary>
        public bool IsHigh { get; set; }

        /// <summary>
        /// The low threshold value from reference ranges.
        /// </summary>
        public decimal? LowThreshold { get; set; }

        /// <summary>
        /// The high threshold value from reference ranges.
        /// </summary>
        public decimal? HighThreshold { get; set; }

        /// <summary>
        /// The comment to display when value is low (from TestComment.LowComment).
        /// </summary>
        public string? LowComment { get; set; }

        /// <summary>
        /// The comment to display when value is high (from TestComment.HighComment).
        /// </summary>
        public string? HighComment { get; set; }

        /// <summary>
        /// The default comment for this test (from TestComment.CommentText).
        /// </summary>
        public string? DefaultComment { get; set; }

        /// <summary>
        /// The appropriate comment based on the flag (Low, High, or Default).
        /// </summary>
        public string? RecommendedComment => IsLow ? LowComment : (IsHigh ? HighComment : DefaultComment);
    }
}
