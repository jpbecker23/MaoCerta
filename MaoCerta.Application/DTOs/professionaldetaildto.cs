namespace MaoCerta.Application.DTOs
{
    /// <summary>
    /// Detailed DTO for Professional Profile View
    /// Includes complete information with reviews and statistics
    /// </summary>
    public class ProfessionalDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? CategoryIcon { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        // Rating Statistics
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        
        // Detailed Rating Breakdown
        public double AveragePriceRating { get; set; }
        public double AverageQualityRating { get; set; }
        public double AverageTimeRating { get; set; }
        public double AverageCommunicationRating { get; set; }
        public double AverageProfessionalismRating { get; set; }

        // Service Statistics
        public int TotalServicesCompleted { get; set; }
        public int TotalServicesPending { get; set; }

        // Reviews (limited to most recent)
        public List<ReviewSummaryDto> RecentReviews { get; set; } = new();
    }

    /// <summary>
    /// Summary DTO for Reviews in Professional Profile
    /// </summary>
    public class ReviewSummaryDto
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public double OverallRating { get; set; }
        public int PriceRating { get; set; }
        public int QualityRating { get; set; }
        public int TimeRating { get; set; }
        public int CommunicationRating { get; set; }
        public int ProfessionalismRating { get; set; }
        public string? Comment { get; set; }
        public string? PositivePoints { get; set; }
        public string? NegativePoints { get; set; }
        public DateTime ReviewDate { get; set; }
        public string ServiceTitle { get; set; } = string.Empty;
    }
}
