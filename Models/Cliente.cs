using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaoCerta.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(40)]
        public string Nome { get; set; } = string.Empty;

        [Required, MaxLength(60)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(15)]
        public string Telefone { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Endereco { get; set; }

        public int? Idade { get; set; }

        [Required]
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        [Required]
        public bool Ativo { get; set; } = true;

        public virtual ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();
        public virtual ICollection<SolicitacaoServico> Solicitacoes { get; set; } = new List<SolicitacaoServico>();
    }
}
