using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaoCerta.Models
{
    public class SolicitacaoServico
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public virtual Cliente Cliente { get; set; } = null!;

        [Required]
        public int ProfissionalId { get; set; }

        [ForeignKey("ProfissionalId")]
        public virtual Profissional Profissional { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Descricao { get; set; }

        [Required]
        public DateTime DataSolicitacao { get; set; } = DateTime.Now;

        public DateTime? DataAgendamento { get; set; }

        [MaxLength(200)]
        public string? EnderecoServico { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? ValorProposto { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Pendente"; // Pendente, Aceita, Recusada, Concluida, Cancelada

        [MaxLength(1000)]
        public string? Observacoes { get; set; }

        [MaxLength(10)]
        public string? CodigoVerificacao { get; set; }

        public DateTime? DataConclusao { get; set; }

        public virtual Avaliacao? Avaliacao { get; set; }
    }
}
