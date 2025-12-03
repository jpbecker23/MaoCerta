using MaoCerta.Application.DTOs;

namespace MaoCerta.Application.Interfaces
{
    /// <summary>
    /// Service interface for Client operations
    /// Defines the contract for client business logic
    /// </summary>
    public interface IClientService
    {
        Task<ClientDto?> GetByIdAsync(int id);
        Task<IEnumerable<ClientDto>> GetAllAsync();
        Task<ClientDto> CreateAsync(CreateClientDto createClientDto);
        Task<ClientDto> UpdateAsync(UpdateClientDto updateClientDto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<ClientDto?> GetByEmailAsync(string email);
    }
}
