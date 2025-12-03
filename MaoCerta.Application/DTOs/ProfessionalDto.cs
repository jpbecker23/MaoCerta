namespace MaoCerta.Application.DTOs
{
    /// <summary>
    /// Data Transfer Object for Professional entity
    /// Includes calculated properties for ratings and reviews
    /// </summary>
    public class ProfessionalDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO for creating a new professional
    /// </summary>
    public class CreateProfessionalDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing professional
    /// </summary>
    public class UpdateProfessionalDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
    }

    /// <summary>
    /// DTO for professional search and filtering
    /// </summary>
    public class ProfessionalSearchDto
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public double? MinRating { get; set; }
        public string? SortBy { get; set; } // "rating", "name", "created"
        public string? SortDirection { get; set; } // "asc", "desc"
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
