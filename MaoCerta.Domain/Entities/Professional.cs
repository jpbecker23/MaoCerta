using System.ComponentModel.DataAnnotations;

namespace MaoCerta.Domain.Entities
{
    /// <summary>
    /// Represents a professional service provider in the system
    /// Extends User with professional-specific properties
    /// </summary>
    public class Professional : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(15)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public int CategoryId { get; set; }

        // Navigation properties
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

        // Calculated properties
        public double AverageRating => Reviews.Any() ? Reviews.Average(r => r.OverallRating) : 0;
        public int TotalReviews => Reviews.Count;
    }
}
