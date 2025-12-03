using MaoCerta.Application.DTOs;

namespace MaoCerta.Application.Interfaces
{
    /// <summary>
    /// Service interface for Professional operations
    /// Defines the contract for professional business logic
    /// </summary>
    public interface IProfessionalService
    {
        Task<ProfessionalDto?> GetByIdAsync(int id);
        Task<IEnumerable<ProfessionalDto>> GetAllAsync();
        Task<ProfessionalDto> CreateAsync(CreateProfessionalDto createProfessionalDto);
        Task<ProfessionalDto> UpdateAsync(UpdateProfessionalDto updateProfessionalDto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<ProfessionalDto?> GetByEmailAsync(string email);
        Task<IEnumerable<ProfessionalDto>> SearchAsync(ProfessionalSearchDto searchDto);
        Task<IEnumerable<ProfessionalDto>> GetByCategoryAsync(int categoryId);
        Task<IEnumerable<ProfessionalDto>> GetTopRatedAsync(int count = 10);
        Task<ProfessionalDetailDto?> GetDetailAsync(int id);
    }
}
