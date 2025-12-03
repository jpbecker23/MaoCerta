using System.ComponentModel.DataAnnotations;

namespace MaoCerta.Domain.Entities
{
    /// <summary>
    /// Represents a client in the system
    /// Clients can request services and review professionals
    /// </summary>
    public class Client : BaseEntity
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

        [MaxLength(200)]
        public string? Address { get; set; }

        public int? Age { get; set; }

        // Navigation properties
        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
