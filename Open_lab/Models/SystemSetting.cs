using System.ComponentModel.DataAnnotations;

namespace Open_lab.Models
{
    /// <summary>
    /// System-wide configuration settings stored as key-value pairs.
    /// </summary>
    public class SystemSetting
    {
        [Key]
        public int SettingId { get; set; }

        [Required]
        [MaxLength(100)]
        public string SettingKey { get; set; } = string.Empty;

        public string? SettingValue { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string SettingType { get; set; } = "String"; // String, Int, Decimal, Bool, Json

        public DateTime? LastModified { get; set; }
    }
}
