using MaoCerta.Application.DTOs;

namespace MaoCerta.Application.Interfaces
{
    /// <summary>
    /// Service interface for Review operations
    /// Defines the contract for review business logic
    /// </summary>
    public interface IReviewService
    {
        Task<ReviewDto?> GetByIdAsync(int id);
        Task<IEnumerable<ReviewDto>> GetAllAsync();
        Task<ReviewDto> CreateAsync(CreateReviewDto createReviewDto);
        Task<ReviewDto> UpdateAsync(UpdateReviewDto updateReviewDto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<ReviewDto>> GetByProfessionalIdAsync(int professionalId);
        Task<IEnumerable<ReviewDto>> GetByClientIdAsync(int clientId);
        Task<ReviewDto?> GetByServiceRequestIdAsync(int serviceRequestId);
        Task<double> GetAverageRatingAsync(int professionalId);
        Task<Dictionary<string, double>> GetRatingBreakdownAsync(int professionalId);
    }
}
