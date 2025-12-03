using MaoCerta.Application.Interfaces;
using MaoCerta.Application.DTOs;
using MaoCerta.Domain.Interfaces;
using MaoCerta.Domain.Entities;
using MaoCerta.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace MaoCerta.Application.Services
{
    /// <summary>
    /// Service for managing service request operations
    /// Implements business logic for service request-related functionality
    /// </summary>
    public class ServiceRequestService : IServiceRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ServiceRequestService> _logger;
        private readonly Random _random = new Random();

        public ServiceRequestService(IUnitOfWork unitOfWork, ILogger<ServiceRequestService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<ServiceRequestDto>> GetAllAsync()
        {
            try
            {
                var serviceRequests = await _unitOfWork.ServiceRequests.GetAllAsync();
                var serviceRequestsList = serviceRequests.ToList();
                
                // Load related entities for mapping
                var clients = await _unitOfWork.Clients.GetAllAsync();
                var professionals = await _unitOfWork.Professionals.GetAllAsync();
                
                return serviceRequestsList.Select(sr => MapToDto(sr, clients, professionals));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all service requests");
                throw;
            }
        }

        public async Task<ServiceRequestDto?> GetByIdAsync(int id)
        {
            try
            {
                var serviceRequest = await _unitOfWork.ServiceRequests.GetByIdAsync(id);
                if (serviceRequest == null)
                    return null;

                var clients = await _unitOfWork.Clients.GetAllAsync();
                var professionals = await _unitOfWork.Professionals.GetAllAsync();
                
                return MapToDto(serviceRequest, clients, professionals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service request with ID {ServiceRequestId}", id);
                throw;
            }
        }

        public async Task<ServiceRequestDto> CreateAsync(CreateServiceRequestDto createServiceRequestDto)
        {
            try
            {
                // Validate that client exists
                var client = await _unitOfWork.Clients.GetByIdAsync(createServiceRequestDto.ClientId);
                if (client == null)
                    throw new ArgumentException($"Client with ID {createServiceRequestDto.ClientId} not found");

                // Validate that professional exists
                var professional = await _unitOfWork.Professionals.GetByIdAsync(createServiceRequestDto.ProfessionalId);
                if (professional == null)
                    throw new ArgumentException($"Professional with ID {createServiceRequestDto.ProfessionalId} not found");

                var serviceRequest = new ServiceRequest
                {
                    ClientId = createServiceRequestDto.ClientId,
                    ProfessionalId = createServiceRequestDto.ProfessionalId,
                    Title = createServiceRequestDto.Title,
                    Description = createServiceRequestDto.Description,
                    ServiceAddress = createServiceRequestDto.ServiceAddress,
                    ScheduledDate = createServiceRequestDto.ScheduledDate,
                    ProposedValue = createServiceRequestDto.ProposedValue,
                    Status = ServiceStatus.Pending,
                    Observations = createServiceRequestDto.Observations,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.ServiceRequests.AddAsync(serviceRequest);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Service request created successfully with ID {ServiceRequestId}", serviceRequest.Id);

                var clients = await _unitOfWork.Clients.GetAllAsync();
                var professionals = await _unitOfWork.Professionals.GetAllAsync();
                
                return MapToDto(serviceRequest, clients, professionals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service request");
                throw;
            }
        }

        public async Task<ServiceRequestDto> UpdateAsync(UpdateServiceRequestDto updateServiceRequestDto)
        {
            try
            {
                var serviceRequest = await _unitOfWork.ServiceRequests.GetByIdAsync(updateServiceRequestDto.Id);
                if (serviceRequest == null)
                    throw new ArgumentException($"Service request with ID {updateServiceRequestDto.Id} not found");

                serviceRequest.Title = updateServiceRequestDto.Title;
                serviceRequest.Description = updateServiceRequestDto.Description;
                serviceRequest.ServiceAddress = updateServiceRequestDto.ServiceAddress;
                serviceRequest.ScheduledDate = updateServiceRequestDto.ScheduledDate;
                serviceRequest.ProposedValue = updateServiceRequestDto.ProposedValue;
                serviceRequest.Status = updateServiceRequestDto.Status;
                serviceRequest.Observations = updateServiceRequestDto.Observations;
                serviceRequest.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.ServiceRequests.UpdateAsync(serviceRequest);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Service request updated successfully with ID {ServiceRequestId}", updateServiceRequestDto.Id);

                var clients = await _unitOfWork.Clients.GetAllAsync();
                var professionals = await _unitOfWork.Professionals.GetAllAsync();
                
                return MapToDto(serviceRequest, clients, professionals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service request with ID {ServiceRequestId}", updateServiceRequestDto.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var serviceRequest = await _unitOfWork.ServiceRequests.GetByIdAsync(id);
                if (serviceRequest == null)
                    return false;

                await _unitOfWork.ServiceRequests.DeleteAsync(serviceRequest);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Service request deleted successfully with ID {ServiceRequestId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service request with ID {ServiceRequestId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                var serviceRequest = await _unitOfWork.ServiceRequests.GetByIdAsync(id);
                return serviceRequest != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if service request exists with ID {ServiceRequestId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ServiceRequestDto>> GetByClientIdAsync(int clientId)
        {
            try
            {
                var serviceRequests = await _unitOfWork.ServiceRequests.GetAllAsync();
                var serviceRequestsList = serviceRequests.Where(sr => sr.ClientId == clientId).ToList();
                
                var clients = await _unitOfWork.Clients.GetAllAsync();
                var professionals = await _unitOfWork.Professionals.GetAllAsync();
                
                return serviceRequestsList.Select(sr => MapToDto(sr, clients, professionals));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service requests for client with ID {ClientId}", clientId);
                throw;
            }
        }

        public async Task<IEnumerable<ServiceRequestDto>> GetByProfessionalIdAsync(int professionalId)
        {
            try
            {
                var serviceRequests = await _unitOfWork.ServiceRequests.GetAllAsync();
                var serviceRequestsList = serviceRequests.Where(sr => sr.ProfessionalId == professionalId).ToList();
                
                var clients = await _unitOfWork.Clients.GetAllAsync();
                var professionals = await _unitOfWork.Professionals.GetAllAsync();
                
                return serviceRequestsList.Select(sr => MapToDto(sr, clients, professionals));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service requests for professional with ID {ProfessionalId}", professionalId);
                throw;
            }
        }

        public async Task<ServiceRequestDto> UpdateStatusAsync(UpdateServiceRequestStatusDto updateStatusDto)
        {
            try
            {
                var serviceRequest = await _unitOfWork.ServiceRequests.GetByIdAsync(updateStatusDto.Id);
                if (serviceRequest == null)
                    throw new ArgumentException($"Service request with ID {updateStatusDto.Id} not found");

                serviceRequest.Status = updateStatusDto.Status;

                if (serviceRequest.Status == ServiceStatus.Accepted && string.IsNullOrEmpty(serviceRequest.VerificationCode))
                {
                    serviceRequest.VerificationCode = GenerateCodeValue();
                }

                if (serviceRequest.Status == ServiceStatus.Completed)
                {
                    if (string.IsNullOrWhiteSpace(serviceRequest.VerificationCode))
                        throw new ArgumentException("Gere um codigo de verificacao antes de concluir o servico.");

                    if (string.IsNullOrWhiteSpace(updateStatusDto.VerificationCode))
                        throw new ArgumentException("Informe o codigo de confirmacao fornecido pelo cliente.");

                    if (!string.Equals(serviceRequest.VerificationCode, updateStatusDto.VerificationCode, StringComparison.Ordinal))
                        throw new ArgumentException("Codigo de confirmacao incorreto.");

                    serviceRequest.CompletionDate = DateTime.UtcNow;
                }
                else
                {
                    serviceRequest.CompletionDate = null;

                    if (!string.IsNullOrWhiteSpace(updateStatusDto.VerificationCode) && string.IsNullOrEmpty(serviceRequest.VerificationCode))
                    {
                        serviceRequest.VerificationCode = updateStatusDto.VerificationCode.Trim();
                    }
                }

                serviceRequest.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.ServiceRequests.UpdateAsync(serviceRequest);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Service request status updated successfully with ID {ServiceRequestId}", updateStatusDto.Id);

                var clients = await _unitOfWork.Clients.GetAllAsync();
                var professionals = await _unitOfWork.Professionals.GetAllAsync();
                
                return MapToDto(serviceRequest, clients, professionals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service request status with ID {ServiceRequestId}", updateStatusDto.Id);
                throw;
            }
        }

        public async Task<string> GenerateVerificationCodeAsync(int serviceRequestId)
        {
            try
            {
                var serviceRequest = await _unitOfWork.ServiceRequests.GetByIdAsync(serviceRequestId);
                if (serviceRequest == null)
                    throw new ArgumentException($"Service request with ID {serviceRequestId} not found");

                var code = GenerateCodeValue();
                serviceRequest.VerificationCode = code;
                serviceRequest.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.ServiceRequests.UpdateAsync(serviceRequest);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Verification code generated for service request with ID {ServiceRequestId}", serviceRequestId);
                return code;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating verification code for service request with ID {ServiceRequestId}", serviceRequestId);
                throw;
            }
        }

        public async Task<bool> VerifyCodeAsync(int serviceRequestId, string code)
        {
            try
            {
                var serviceRequest = await _unitOfWork.ServiceRequests.GetByIdAsync(serviceRequestId);
                if (serviceRequest == null)
                    return false;

                return serviceRequest.VerificationCode == code;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying code for service request with ID {ServiceRequestId}", serviceRequestId);
                throw;
            }
        }

        private static ServiceRequestDto MapToDto(ServiceRequest serviceRequest, IEnumerable<Client> clients, IEnumerable<Professional> professionals)
        {
            var client = clients.FirstOrDefault(c => c.Id == serviceRequest.ClientId);
            var professional = professionals.FirstOrDefault(p => p.Id == serviceRequest.ProfessionalId);

            return new ServiceRequestDto
            {
                Id = serviceRequest.Id,
                ClientId = serviceRequest.ClientId,
                ClientName = client?.Name ?? "Unknown",
                ProfessionalId = serviceRequest.ProfessionalId,
                ProfessionalName = professional?.Name ?? "Unknown",
                Title = serviceRequest.Title,
                Description = serviceRequest.Description,
                ServiceAddress = serviceRequest.ServiceAddress,
                ScheduledDate = serviceRequest.ScheduledDate,
                ProposedValue = serviceRequest.ProposedValue,
                Status = serviceRequest.Status,
                Observations = serviceRequest.Observations,
                VerificationCode = serviceRequest.VerificationCode,
                CompletionDate = serviceRequest.CompletionDate,
                CreatedAt = serviceRequest.CreatedAt,
                IsActive = serviceRequest.IsActive
            };
        }

        private string GenerateCodeValue()
        {
            return _random.Next(100000, 999999).ToString();
        }
    }
}

