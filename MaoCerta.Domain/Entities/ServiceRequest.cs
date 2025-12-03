using MaoCerta.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace MaoCerta.Domain.Entities
{
    /// <summary>
    /// Represents a service request made by a client to a professional
    /// Includes all details about the requested service and its status
    /// </summary>
    public class ServiceRequest : BaseEntity
    {
        [Required]
        public int ClientId { get; set; }

        [Required]
        public int ProfessionalId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(200)]
        public string? ServiceAddress { get; set; }

        public DateTime? ScheduledDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? ProposedValue { get; set; }

        public ServiceStatus Status { get; set; } = ServiceStatus.Pending;

        [MaxLength(1000)]
        public string? Observations { get; set; }

        [MaxLength(10)]
        public string? VerificationCode { get; set; }

        public DateTime? CompletionDate { get; set; }

        // Navigation properties
        public virtual Client Client { get; set; } = null!;
        public virtual Professional Professional { get; set; } = null!;
        public virtual Review? Review { get; set; }
    }
}
