using System.ComponentModel.DataAnnotations;

namespace MaoCerta.Domain.Entities
{
    /// <summary>
    /// Represents a detailed review of a professional service
    /// Implements the multi-criteria evaluation system as specified in requirements
    /// </summary>
    public class Review : BaseEntity
    {
        [Required]
        public int ClientId { get; set; }

        [Required]
        public int ProfessionalId { get; set; }

        [Required]
        public int ServiceRequestId { get; set; }

        // Detailed rating criteria (1-5 stars each)
        [Required]
        [Range(1, 5)]
        public int PriceRating { get; set; }

        [Required]
        [Range(1, 5)]
        public int QualityRating { get; set; }

        [Required]
        [Range(1, 5)]
        public int SpeedRating { get; set; }

        [Required]
        [Range(1, 5)]
        public int CommunicationRating { get; set; }

        [Required]
        [Range(1, 5)]
        public int ProfessionalismRating { get; set; }

        // Calculated overall rating
        public double OverallRating => (PriceRating + QualityRating + SpeedRating + CommunicationRating + ProfessionalismRating) / 5.0;

        [MaxLength(1000)]
        public string? Comment { get; set; }

        [MaxLength(500)]
        public string? PositivePoints { get; set; }

        [MaxLength(500)]
        public string? NegativePoints { get; set; }

        // Navigation properties
        public virtual Client Client { get; set; } = null!;
        public virtual Professional Professional { get; set; } = null!;
        public virtual ServiceRequest ServiceRequest { get; set; } = null!;
    }
}
