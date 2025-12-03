using System.ComponentModel.DataAnnotations;

namespace MaoCerta.Domain.Entities
{
    /// <summary>
    /// Base entity class that provides common properties for all entities
    /// Implements the base contract for domain entities
    /// </summary>
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}
