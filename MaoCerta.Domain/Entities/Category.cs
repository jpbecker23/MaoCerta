using System.ComponentModel.DataAnnotations;

namespace MaoCerta.Domain.Entities
{
    /// <summary>
    /// Represents a service category in the system
    /// Categories help organize and filter services offered by professionals
    /// </summary>
    public class Category : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public string? Icon { get; set; }

        // Navigation properties
        public virtual ICollection<Professional> Professionals { get; set; } = new List<Professional>();
    }
}
