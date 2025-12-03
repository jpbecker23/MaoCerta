using System.ComponentModel.DataAnnotations;

namespace MaoCerta.Models
{
    public class Categoria
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descricao { get; set; }

        [MaxLength(100)]
        public string? Icone { get; set; }

        [Required]
        public bool Ativa { get; set; } = true;

        public virtual ICollection<Profissional> Profissionais { get; set; } = new List<Profissional>();
    }
}
