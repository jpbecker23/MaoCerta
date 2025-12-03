using MaoCerta.Application.Interfaces;
using MaoCerta.Application.DTOs;
using MaoCerta.Domain.Interfaces;
using MaoCerta.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace MaoCerta.Application.Services
{
    /// <summary>
    /// Service for managing client operations
    /// Implements business logic for client-related functionality
    /// </summary>
    public class ClientService : IClientService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ClientService> _logger;

        public ClientService(IUnitOfWork unitOfWork, ILogger<ClientService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<ClientDto>> GetAllAsync()
        {
            try
            {
                var clients = await _unitOfWork.Clients.GetAllAsync();
                return clients.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all clients");
                throw;
            }
        }

        public async Task<ClientDto?> GetByIdAsync(int id)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByIdAsync(id);
                return client != null ? MapToDto(client) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client with ID {ClientId}", id);
                throw;
            }
        }

        public async Task<ClientDto> CreateAsync(CreateClientDto createClientDto)
        {
            try
            {
                // Verificar se cliente já existe pelo email
                var existingClient = await GetByEmailAsync(createClientDto.Email);
                if (existingClient != null)
                {
                    _logger.LogInformation("Client with email {Email} already exists, returning existing client with ID {ClientId}", 
                        createClientDto.Email, existingClient.Id);
                    return existingClient;
                }

                var client = new Client
                {
                    Name = createClientDto.Name,
                    Email = createClientDto.Email,
                    Phone = createClientDto.Phone,
                    Address = createClientDto.Address,
                    Age = createClientDto.Age,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Clients.AddAsync(client);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Client created successfully with ID {ClientId}", client.Id);
                return MapToDto(client);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                // Verificar se é erro de duplicidade (SQL State 23505)
                if (ex.InnerException != null && 
                    (ex.InnerException.Message.Contains("23505") || 
                     ex.InnerException.Message.Contains("IX_Clients_Email") ||
                     ex.InnerException.Message.Contains("duplicar valor")))
                {
                    // Se ainda assim houver duplicidade (condição de corrida), buscar o cliente existente
                    _logger.LogWarning("Duplicate email detected for {Email}, attempting to retrieve existing client", createClientDto.Email);
                    var existingClient = await GetByEmailAsync(createClientDto.Email);
                    if (existingClient != null)
                    {
                        return existingClient;
                    }
                    _logger.LogError(ex, "Error creating client - duplicate email and could not retrieve existing client");
                    throw new ArgumentException($"Cliente com email {createClientDto.Email} já existe.");
                }
                // Se não for erro de duplicidade, re-lançar
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client");
                throw;
            }
        }

        public async Task<ClientDto> UpdateAsync(UpdateClientDto updateClientDto)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByIdAsync(updateClientDto.Id);
                if (client == null)
                    throw new ArgumentException($"Client with ID {updateClientDto.Id} not found");

                client.Name = updateClientDto.Name;
                client.Email = updateClientDto.Email;
                client.Phone = updateClientDto.Phone;
                client.Address = updateClientDto.Address;
                client.Age = updateClientDto.Age;
                client.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Clients.UpdateAsync(client);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Client updated successfully with ID {ClientId}", updateClientDto.Id);
                return MapToDto(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client with ID {ClientId}", updateClientDto.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByIdAsync(id);
                if (client == null)
                    return false;

                await _unitOfWork.Clients.DeleteAsync(client);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Client deleted successfully with ID {ClientId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting client with ID {ClientId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByIdAsync(id);
                return client != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if client exists with ID {ClientId}", id);
                throw;
            }
        }

        public async Task<ClientDto?> GetByEmailAsync(string email)
        {
            try
            {
                var clients = await _unitOfWork.Clients.GetAllAsync();
                var client = clients.FirstOrDefault(c => c.Email == email);
                return client != null ? MapToDto(client) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client with email {Email}", email);
                throw;
            }
        }

        private static ClientDto MapToDto(Client client)
        {
            return new ClientDto
            {
                Id = client.Id,
                Name = client.Name,
                Email = client.Email,
                Phone = client.Phone,
                Address = client.Address,
                Age = client.Age,
                IsActive = client.IsActive,
                CreatedAt = client.CreatedAt
            };
        }
    }
}
