using MaoCerta.Application.Interfaces;
using MaoCerta.Application.DTOs;
using MaoCerta.Domain.Interfaces;
using MaoCerta.Domain.Entities;
using MaoCerta.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace MaoCerta.Application.Services
{
    /// <summary>
    /// Service for managing professional operations
    /// Implements business logic for professional-related functionality
    /// </summary>
    public class ProfessionalService : IProfessionalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProfessionalService> _logger;

        public ProfessionalService(IUnitOfWork unitOfWork, ILogger<ProfessionalService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<ProfessionalDto>> GetAllAsync()
        {
            try
            {
                var professionals = await _unitOfWork.Professionals.GetAllAsync();
                var professionalsList = professionals.ToList();
                
                // Load related entities for mapping
                var categories = await _unitOfWork.Categories.GetAllAsync();
                var reviews = await _unitOfWork.Reviews.GetAllAsync();
                
                return professionalsList.Select(p => MapToDto(p, categories, reviews));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all professionals");
                throw;
            }
        }

        public async Task<ProfessionalDto?> GetByIdAsync(int id)
        {
            try
            {
                var professional = await _unitOfWork.Professionals.GetByIdAsync(id);
                if (professional == null)
                    return null;

                var categories = await _unitOfWork.Categories.GetAllAsync();
                var reviews = await _unitOfWork.Reviews.GetAllAsync();
                
                return MapToDto(professional, categories, reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving professional with ID {ProfessionalId}", id);
                throw;
            }
        }

        public async Task<ProfessionalDto> CreateAsync(CreateProfessionalDto createProfessionalDto)
        {
            try
            {
                var professional = new Professional
                {
                    Name = createProfessionalDto.Name,
                    Email = createProfessionalDto.Email,
                    Phone = createProfessionalDto.Phone,
                    Address = createProfessionalDto.Address,
                    Description = createProfessionalDto.Description,
                    CategoryId = createProfessionalDto.CategoryId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Professionals.AddAsync(professional);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Professional created successfully with ID {ProfessionalId}", professional.Id);
                
                var categories = await _unitOfWork.Categories.GetAllAsync();
                var reviews = await _unitOfWork.Reviews.GetAllAsync();
                
                return MapToDto(professional, categories, reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating professional");
                throw;
            }
        }

        public async Task<ProfessionalDto> UpdateAsync(UpdateProfessionalDto updateProfessionalDto)
        {
            try
            {
                var professional = await _unitOfWork.Professionals.GetByIdAsync(updateProfessionalDto.Id);
                if (professional == null)
                    throw new ArgumentException($"Professional with ID {updateProfessionalDto.Id} not found");

                professional.Name = updateProfessionalDto.Name;
                professional.Email = updateProfessionalDto.Email;
                professional.Phone = updateProfessionalDto.Phone;
                professional.Address = updateProfessionalDto.Address;
                professional.Description = updateProfessionalDto.Description;
                professional.CategoryId = updateProfessionalDto.CategoryId;
                professional.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Professionals.UpdateAsync(professional);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Professional updated successfully with ID {ProfessionalId}", updateProfessionalDto.Id);
                
                var categories = await _unitOfWork.Categories.GetAllAsync();
                var reviews = await _unitOfWork.Reviews.GetAllAsync();
                
                return MapToDto(professional, categories, reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating professional with ID {ProfessionalId}", updateProfessionalDto.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var professional = await _unitOfWork.Professionals.GetByIdAsync(id);
                if (professional == null)
                    return false;

                await _unitOfWork.Professionals.DeleteAsync(professional);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Professional deleted successfully with ID {ProfessionalId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting professional with ID {ProfessionalId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                var professional = await _unitOfWork.Professionals.GetByIdAsync(id);
                return professional != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if professional exists with ID {ProfessionalId}", id);
                throw;
            }
        }

        public async Task<ProfessionalDto?> GetByEmailAsync(string email)
        {
            try
            {
                var professionals = await _unitOfWork.Professionals.GetAllAsync();
                var professional = professionals.FirstOrDefault(p => p.Email == email);
                if (professional == null)
                    return null;

                var categories = await _unitOfWork.Categories.GetAllAsync();
                var reviews = await _unitOfWork.Reviews.GetAllAsync();
                
                return MapToDto(professional, categories, reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving professional with email {Email}", email);
                throw;
            }
        }

        public async Task<IEnumerable<ProfessionalDto>> SearchAsync(ProfessionalSearchDto searchDto)
        {
            try
            {
                var professionals = await _unitOfWork.Professionals.GetAllAsync();
                var professionalsList = professionals.ToList();
                
                var categories = await _unitOfWork.Categories.GetAllAsync();
                var reviews = await _unitOfWork.Reviews.GetAllAsync();
                var reviewsList = reviews.ToList();
                
                // Filter by search term
                if (!string.IsNullOrEmpty(searchDto.SearchTerm))
                {
                    professionalsList = professionalsList.Where(p => 
                        p.Name.Contains(searchDto.SearchTerm, StringComparison.OrdinalIgnoreCase) || 
                        (p.Description != null && p.Description.Contains(searchDto.SearchTerm, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                // Filter by category
                if (searchDto.CategoryId.HasValue)
                {
                    professionalsList = professionalsList.Where(p => p.CategoryId == searchDto.CategoryId.Value).ToList();
                }

                // Filter by minimum rating
                if (searchDto.MinRating.HasValue)
                {
                    professionalsList = professionalsList.Where(p =>
                    {
                        var professionalReviews = reviewsList.Where(r => r.ProfessionalId == p.Id).ToList();
                        if (!professionalReviews.Any())
                            return false;
                        var avgRating = professionalReviews.Average(r => r.OverallRating);
                        return avgRating >= searchDto.MinRating.Value;
                    }).ToList();
                }

                // Apply sorting
                IEnumerable<Professional> sortedList;
                switch (searchDto.SortBy?.ToLower())
                {
                    case "rating":
                        if (searchDto.SortDirection?.ToLower() == "asc")
                        {
                            sortedList = professionalsList.OrderBy(p =>
                            {
                                var professionalReviews = reviewsList.Where(r => r.ProfessionalId == p.Id).ToList();
                                return professionalReviews.Any() ? professionalReviews.Average(r => r.OverallRating) : 0.0;
                            });
                        }
                        else
                        {
                            sortedList = professionalsList.OrderByDescending(p =>
                            {
                                var professionalReviews = reviewsList.Where(r => r.ProfessionalId == p.Id).ToList();
                                return professionalReviews.Any() ? professionalReviews.Average(r => r.OverallRating) : 0.0;
                            });
                        }
                        break;
                    case "name":
                        sortedList = searchDto.SortDirection?.ToLower() == "desc" 
                            ? professionalsList.OrderByDescending(p => p.Name)
                            : professionalsList.OrderBy(p => p.Name);
                        break;
                    case "created":
                        sortedList = searchDto.SortDirection?.ToLower() == "desc"
                            ? professionalsList.OrderByDescending(p => p.CreatedAt)
                            : professionalsList.OrderBy(p => p.CreatedAt);
                        break;
                    default:
                        sortedList = professionalsList.OrderBy(p => p.Name);
                        break;
                }

                // Apply pagination
                var skip = (searchDto.Page - 1) * searchDto.PageSize;
                var pagedResults = sortedList.Skip(skip).Take(searchDto.PageSize).ToList();

                return pagedResults.Select(p => MapToDto(p, categories, reviews));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching professionals");
                throw;
            }
        }

        public async Task<IEnumerable<ProfessionalDto>> GetByCategoryAsync(int categoryId)
        {
            try
            {
                var professionals = await _unitOfWork.Professionals.GetAllAsync();
                var professionalsList = professionals.Where(p => p.CategoryId == categoryId).ToList();
                
                var categories = await _unitOfWork.Categories.GetAllAsync();
                var reviews = await _unitOfWork.Reviews.GetAllAsync();
                
                return professionalsList.Select(p => MapToDto(p, categories, reviews));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving professionals by category {CategoryId}", categoryId);
                throw;
            }
        }

        public async Task<IEnumerable<ProfessionalDto>> GetTopRatedAsync(int count = 10)
        {
            try
            {
                var professionals = await _unitOfWork.Professionals.GetAllAsync();
                var professionalsList = professionals.ToList();
                
                var categories = await _unitOfWork.Categories.GetAllAsync();
                var reviews = await _unitOfWork.Reviews.GetAllAsync();
                
                var professionalsWithRatings = professionalsList.Select(p => new
                {
                    Professional = p,
                    AverageRating = reviews.Where(r => r.ProfessionalId == p.Id).Any()
                        ? reviews.Where(r => r.ProfessionalId == p.Id).Average(r => r.OverallRating)
                        : 0.0
                })
                .OrderByDescending(x => x.AverageRating)
                .Take(count)
                .ToList();

                return professionalsWithRatings.Select(x => MapToDto(x.Professional, categories, reviews));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top rated professionals");
                throw;
            }
        }

        public async Task<ProfessionalDetailDto?> GetDetailAsync(int id)
        {
            try
            {
                var professional = await _unitOfWork.Professionals.GetByIdAsync(id);
                if (professional == null)
                {
                    return null;
                }

                var category = await _unitOfWork.Categories.GetByIdAsync(professional.CategoryId);
                var reviews = (await _unitOfWork.Reviews.FindAsync(r => r.ProfessionalId == id)).ToList();
                var serviceRequests = (await _unitOfWork.ServiceRequests.FindAsync(s => s.ProfessionalId == id)).ToList();

                return MapToDetailDto(professional, category, reviews, serviceRequests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving professional detail for {ProfessionalId}", id);
                throw;
            }
        }

        private static ProfessionalDto MapToDto(Professional professional, IEnumerable<Category> categories, IEnumerable<Review> reviews)
        {
            var category = categories.FirstOrDefault(c => c.Id == professional.CategoryId);
            var professionalReviews = reviews.Where(r => r.ProfessionalId == professional.Id).ToList();
            
            return new ProfessionalDto
            {
                Id = professional.Id,
                Name = professional.Name,
                Email = professional.Email,
                Phone = professional.Phone,
                Address = professional.Address,
                Description = professional.Description,
                CategoryId = professional.CategoryId,
                CategoryName = category?.Name ?? "Unknown",
                AverageRating = professionalReviews.Any() ? professionalReviews.Average(r => r.OverallRating) : 0.0,
                TotalReviews = professionalReviews.Count,
                IsActive = professional.IsActive,
                CreatedAt = professional.CreatedAt
            };
        }

        private static ProfessionalDetailDto MapToDetailDto(
            Professional professional,
            Category? category,
            IList<Review> reviews,
            IList<ServiceRequest> requests)
        {
            var reviewCount = reviews.Count;
            double Average(Func<Review, int> selector) => reviewCount > 0 ? reviews.Average(r => selector(r)) : 0;

            var detail = new ProfessionalDetailDto
            {
                Id = professional.Id,
                Name = professional.Name,
                Email = professional.Email,
                Phone = professional.Phone,
                Address = professional.Address,
                Description = professional.Description,
                CategoryId = professional.CategoryId,
                CategoryName = category?.Name ?? "Categoria",
                CategoryIcon = category?.Icon,
                CreatedAt = professional.CreatedAt,
                IsActive = professional.IsActive,
                AverageRating = reviewCount > 0 ? reviews.Average(r => r.OverallRating) : 0,
                TotalReviews = reviewCount,
                AveragePriceRating = Average(r => r.PriceRating),
                AverageQualityRating = Average(r => r.QualityRating),
                AverageTimeRating = Average(r => r.SpeedRating),
                AverageCommunicationRating = Average(r => r.CommunicationRating),
                AverageProfessionalismRating = Average(r => r.ProfessionalismRating),
                TotalServicesCompleted = requests.Count(r => r.Status == ServiceStatus.Completed),
                TotalServicesPending = requests.Count(r => r.Status == ServiceStatus.Pending || r.Status == ServiceStatus.InProgress || r.Status == ServiceStatus.Accepted)
            };

            detail.RecentReviews = reviews
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .Select(r =>
                {
                    var relatedRequest = requests.FirstOrDefault(req => req.Id == r.ServiceRequestId);
                    return new ReviewSummaryDto
                    {
                        Id = r.Id,
                        ClientName = r.Client?.Name ?? $"Cliente #{r.ClientId}",
                        OverallRating = r.OverallRating,
                        PriceRating = r.PriceRating,
                        QualityRating = r.QualityRating,
                        TimeRating = r.SpeedRating,
                        CommunicationRating = r.CommunicationRating,
                        ProfessionalismRating = r.ProfessionalismRating,
                        Comment = r.Comment,
                        PositivePoints = r.PositivePoints,
                        NegativePoints = r.NegativePoints,
                        ReviewDate = r.CreatedAt,
                        ServiceTitle = relatedRequest?.Title ?? string.Empty
                    };
                })
                .ToList();

            return detail;
        }
    }
}
