using MaoCerta.Application.Interfaces;
using MaoCerta.Application.DTOs;
using MaoCerta.Domain.Interfaces;
using MaoCerta.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace MaoCerta.Application.Services
{
    /// <summary>
    /// Service for managing review operations
    /// Implements business logic for review-related functionality
    /// </summary>
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(IUnitOfWork unitOfWork, ILogger<ReviewService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<ReviewDto>> GetAllAsync()
        {
            try
            {
                var reviews = await _unitOfWork.Reviews.GetAllAsync();
                var reviewsList = reviews.ToList();
                
                // Load related entities for mapping
                var clients = await _unitOfWork.Clients.GetAllAsync();
                var professionals = await _unitOfWork.Professionals.GetAllAsync();
                
                return reviewsList.Select(r => MapToDto(r, clients, professionals));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all reviews");
                throw;
            }
        }

        public async Task<ReviewDto?> GetByIdAsync(int id)
        {
            try
            {
                var review = await _unitOfWork.Reviews.GetByIdAsync(id);
                if (review == null)
                    return null;

                var clients = await _unitOfWork.Clients.GetAllAsync();
                var professionals = await _unitOfWork.Professionals.GetAllAsync();
                
                return MapToDto(review, clients, professionals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review with ID {ReviewId}", id);
                throw;
            }
        }

        public async Task<ReviewDto> CreateAsync(CreateReviewDto createReviewDto)
        {
            try
            {
                // Validate that service request exists and is completed
                var serviceRequest = await _unitOfWork.ServiceRequests.GetByIdAsync(createReviewDto.ServiceRequestId);
                if (serviceRequest == null)
                    throw new ArgumentException($"Service request with ID {createReviewDto.ServiceRequestId} not found");

                // Check if review already exists for this service request
                var existingReview = await GetByServiceRequestIdAsync(createReviewDto.ServiceRequestId);
                if (existingReview != null)
                    throw new InvalidOperationException("A review already exists for this service request");

                var review = new Review
                {
                    ClientId = createReviewDto.ClientId,
                    ProfessionalId = createReviewDto.ProfessionalId,
                    ServiceRequestId = createReviewDto.ServiceRequestId,
                    PriceRating = createReviewDto.PriceRating,
                    QualityRating = createReviewDto.QualityRating,
                    SpeedRating = createReviewDto.SpeedRating,
                    CommunicationRating = createReviewDto.CommunicationRating,
                    ProfessionalismRating = createReviewDto.ProfessionalismRating,
                    Comment = createReviewDto.Comment,
                    PositivePoints = createReviewDto.PositivePoints,
                    NegativePoints = createReviewDto.NegativePoints,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Reviews.AddAsync(review);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Review created successfully with ID {ReviewId}", review.Id);

                var clients = await _unitOfWork.Clients.GetAllAsync();
                var professionals = await _unitOfWork.Professionals.GetAllAsync();
                
                return MapToDto(review, clients, professionals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review");
                throw;
            }
        }

        public async Task<ReviewDto> UpdateAsync(UpdateReviewDto updateReviewDto)
        {
            try
            {
                var review = await _unitOfWork.Reviews.GetByIdAsync(updateReviewDto.Id);
                if (review == null)
                    throw new ArgumentException($"Review with ID {updateReviewDto.Id} not found");

                review.PriceRating = updateReviewDto.PriceRating;
                review.QualityRating = updateReviewDto.QualityRating;
                review.SpeedRating = updateReviewDto.SpeedRating;
                review.CommunicationRating = updateReviewDto.CommunicationRating;
                review.ProfessionalismRating = updateReviewDto.ProfessionalismRating;
                review.Comment = updateReviewDto.Comment;
                review.PositivePoints = updateReviewDto.PositivePoints;
                review.NegativePoints = updateReviewDto.NegativePoints;
                review.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Reviews.UpdateAsync(review);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Review updated successfully with ID {ReviewId}", updateReviewDto.Id);

                var clients = await _unitOfWork.Clients.GetAllAsync();
                var professionals = await _unitOfWork.Professionals.GetAllAsync();
                
                return MapToDto(review, clients, professionals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review with ID {ReviewId}", updateReviewDto.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var review = await _unitOfWork.Reviews.GetByIdAsync(id);
                if (review == null)
                    return false;

                await _unitOfWork.Reviews.DeleteAsync(review);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Review deleted successfully with ID {ReviewId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review with ID {ReviewId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                var review = await _unitOfWork.Reviews.GetByIdAsync(id);
                return review != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if review exists with ID {ReviewId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ReviewDto>> GetByProfessionalIdAsync(int professionalId)
        {
            try
            {
                var reviews = await _unitOfWork.Reviews.GetAllAsync();
                var reviewsList = reviews.Where(r => r.ProfessionalId == professionalId).ToList();
                
                var clients = await _unitOfWork.Clients.GetAllAsync();
                var professionals = await _unitOfWork.Professionals.GetAllAsync();
                
                return reviewsList.Select(r => MapToDto(r, clients, professionals));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for professional with ID {ProfessionalId}", professionalId);
                throw;
            }
        }

        public async Task<IEnumerable<ReviewDto>> GetByClientIdAsync(int clientId)
        {
            try
            {
                var reviews = await _unitOfWork.Reviews.GetAllAsync();
                var reviewsList = reviews.Where(r => r.ClientId == clientId).ToList();
                
                var clients = await _unitOfWork.Clients.GetAllAsync();
                var professionals = await _unitOfWork.Professionals.GetAllAsync();
                
                return reviewsList.Select(r => MapToDto(r, clients, professionals));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for client with ID {ClientId}", clientId);
                throw;
            }
        }

        public async Task<ReviewDto?> GetByServiceRequestIdAsync(int serviceRequestId)
        {
            try
            {
                var reviews = await _unitOfWork.Reviews.GetAllAsync();
                var review = reviews.FirstOrDefault(r => r.ServiceRequestId == serviceRequestId);
                
                if (review == null)
                    return null;

                var clients = await _unitOfWork.Clients.GetAllAsync();
                var professionals = await _unitOfWork.Professionals.GetAllAsync();
                
                return MapToDto(review, clients, professionals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review for service request with ID {ServiceRequestId}", serviceRequestId);
                throw;
            }
        }

        public async Task<double> GetAverageRatingAsync(int professionalId)
        {
            try
            {
                var reviews = await _unitOfWork.Reviews.GetAllAsync();
                var professionalReviews = reviews.Where(r => r.ProfessionalId == professionalId).ToList();
                
                if (!professionalReviews.Any())
                    return 0.0;

                return professionalReviews.Average(r => r.OverallRating);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating average rating for professional with ID {ProfessionalId}", professionalId);
                throw;
            }
        }

        public async Task<Dictionary<string, double>> GetRatingBreakdownAsync(int professionalId)
        {
            try
            {
                var reviews = await _unitOfWork.Reviews.GetAllAsync();
                var professionalReviews = reviews.Where(r => r.ProfessionalId == professionalId).ToList();
                
                if (!professionalReviews.Any())
                {
                    return new Dictionary<string, double>
                    {
                        { "Price", 0.0 },
                        { "Quality", 0.0 },
                        { "Speed", 0.0 },
                        { "Communication", 0.0 },
                        { "Professionalism", 0.0 }
                    };
                }

                return new Dictionary<string, double>
                {
                    { "Price", professionalReviews.Average(r => r.PriceRating) },
                    { "Quality", professionalReviews.Average(r => r.QualityRating) },
                    { "Speed", professionalReviews.Average(r => r.SpeedRating) },
                    { "Communication", professionalReviews.Average(r => r.CommunicationRating) },
                    { "Professionalism", professionalReviews.Average(r => r.ProfessionalismRating) }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating rating breakdown for professional with ID {ProfessionalId}", professionalId);
                throw;
            }
        }

        private static ReviewDto MapToDto(Review review, IEnumerable<Client> clients, IEnumerable<Professional> professionals)
        {
            var client = clients.FirstOrDefault(c => c.Id == review.ClientId);
            var professional = professionals.FirstOrDefault(p => p.Id == review.ProfessionalId);

            return new ReviewDto
            {
                Id = review.Id,
                ClientId = review.ClientId,
                ClientName = client?.Name ?? "Unknown",
                ProfessionalId = review.ProfessionalId,
                ProfessionalName = professional?.Name ?? "Unknown",
                ServiceRequestId = review.ServiceRequestId,
                PriceRating = review.PriceRating,
                QualityRating = review.QualityRating,
                SpeedRating = review.SpeedRating,
                CommunicationRating = review.CommunicationRating,
                ProfessionalismRating = review.ProfessionalismRating,
                OverallRating = review.OverallRating,
                Comment = review.Comment,
                PositivePoints = review.PositivePoints,
                NegativePoints = review.NegativePoints,
                CreatedAt = review.CreatedAt,
                IsActive = review.IsActive
            };
        }
    }
}

