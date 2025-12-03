using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaoCerta.Models
{
    public class Profissional
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(40)]
        public string Nome { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(15)]
        public string Telefone { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Endereco { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descricao { get; set; }

        [Required]
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        [Required]
        public bool Ativo { get; set; } = true;

        [Required]
        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public virtual Categoria Categoria { get; set; } = null!;

        public virtual ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();
        public virtual ICollection<SolicitacaoServico> Solicitacoes { get; set; } = new List<SolicitacaoServico>();

        // Propriedades calculadas
        [NotMapped]
        public double AvaliacaoMedia => Avaliacoes.Any() ? Avaliacoes.Average(a => a.NotaGeral) : 0;

        [NotMapped]
        public int TotalAvaliacoes => Avaliacoes.Count;
    }
}
