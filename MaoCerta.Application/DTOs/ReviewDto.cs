namespace MaoCerta.Application.DTOs
{
    /// <summary>
    /// Data Transfer Object for Review entity
    /// Implements the detailed multi-criteria evaluation system
    /// </summary>
    public class ReviewDto
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public int ProfessionalId { get; set; }
        public string ProfessionalName { get; set; } = string.Empty;
        public int ServiceRequestId { get; set; }
        
        // Detailed rating criteria
        public int PriceRating { get; set; }
        public int QualityRating { get; set; }
        public int SpeedRating { get; set; }
        public int CommunicationRating { get; set; }
        public int ProfessionalismRating { get; set; }
        
        // Calculated overall rating
        public double OverallRating { get; set; }
        
        public string? Comment { get; set; }
        public string? PositivePoints { get; set; }
        public string? NegativePoints { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO for creating a new review
    /// </summary>
    public class CreateReviewDto
    {
        public int ClientId { get; set; }
        public int ProfessionalId { get; set; }
        public int ServiceRequestId { get; set; }
        public int PriceRating { get; set; }
        public int QualityRating { get; set; }
        public int SpeedRating { get; set; }
        public int CommunicationRating { get; set; }
        public int ProfessionalismRating { get; set; }
        public string? Comment { get; set; }
        public string? PositivePoints { get; set; }
        public string? NegativePoints { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing review
    /// </summary>
    public class UpdateReviewDto
    {
        public int Id { get; set; }
        public int PriceRating { get; set; }
        public int QualityRating { get; set; }
        public int SpeedRating { get; set; }
        public int CommunicationRating { get; set; }
        public int ProfessionalismRating { get; set; }
        public string? Comment { get; set; }
        public string? PositivePoints { get; set; }
        public string? NegativePoints { get; set; }
    }
}
