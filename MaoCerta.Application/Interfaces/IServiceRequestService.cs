using MaoCerta.Application.DTOs;

namespace MaoCerta.Application.Interfaces
{
    /// <summary>
    /// Service interface for ServiceRequest operations
    /// Defines the contract for service request business logic
    /// </summary>
    public interface IServiceRequestService
    {
        Task<ServiceRequestDto?> GetByIdAsync(int id);
        Task<IEnumerable<ServiceRequestDto>> GetAllAsync();
        Task<ServiceRequestDto> CreateAsync(CreateServiceRequestDto createServiceRequestDto);
        Task<ServiceRequestDto> UpdateAsync(UpdateServiceRequestDto updateServiceRequestDto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<ServiceRequestDto>> GetByClientIdAsync(int clientId);
        Task<IEnumerable<ServiceRequestDto>> GetByProfessionalIdAsync(int professionalId);
        Task<ServiceRequestDto> UpdateStatusAsync(UpdateServiceRequestStatusDto updateStatusDto);
        Task<string> GenerateVerificationCodeAsync(int serviceRequestId);
        Task<bool> VerifyCodeAsync(int serviceRequestId, string code);
    }
}
