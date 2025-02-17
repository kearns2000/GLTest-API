using System.ComponentModel.DataAnnotations;

namespace GLTest.Domain.Entities
{
    public class Company
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string Exchange { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string Ticker { get; set; } = string.Empty;

        [Required, MaxLength(12)]
        public string Isin { get; set; } = string.Empty;

        [Url]
        public string? Website { get; set; }

        [Required]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public DateTime? DateUpdated { get; set; }

        [MaxLength(255)]
        public string? UpdatedBy { get; set; }
    }
}
