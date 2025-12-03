using MaoCerta.Domain.Enums;

namespace MaoCerta.Application.DTOs
{
    /// <summary>
    /// Data Transfer Object for ServiceRequest entity
    /// Includes all service request details and status information
    /// </summary>
    public class ServiceRequestDto
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public int ProfessionalId { get; set; }
        public string ProfessionalName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ServiceAddress { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public decimal? ProposedValue { get; set; }
        public ServiceStatus Status { get; set; }
        public string? Observations { get; set; }
        public string? VerificationCode { get; set; }
        public DateTime? CompletionDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO for creating a new service request
    /// </summary>
    public class CreateServiceRequestDto
    {
        public int ClientId { get; set; }
        public int ProfessionalId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ServiceAddress { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public decimal? ProposedValue { get; set; }
        public string? Observations { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing service request
    /// </summary>
    public class UpdateServiceRequestDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ServiceAddress { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public decimal? ProposedValue { get; set; }
        public ServiceStatus Status { get; set; }
        public string? Observations { get; set; }
    }

    /// <summary>
    /// DTO for updating service request status
    /// </summary>
    public class UpdateServiceRequestStatusDto
    {
        public int Id { get; set; }
        public ServiceStatus Status { get; set; }
        public string? VerificationCode { get; set; }
    }
}
