using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaoCerta.Models
{
    public class Avaliacao
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

        [Required]
        public int SolicitacaoServicoId { get; set; }

        [ForeignKey("SolicitacaoServicoId")]
        public virtual SolicitacaoServico SolicitacaoServico { get; set; } = null!;

        // Avaliações por critérios (1-5 estrelas)
        [Required, Range(1, 5)]
        public int NotaPreco { get; set; }

        [Required, Range(1, 5)]
        public int NotaQualidade { get; set; }

        [Required, Range(1, 5)]
        public int NotaTempoExecucao { get; set; }

        [Required, Range(1, 5)]
        public int NotaComunicacao { get; set; }

        [Required, Range(1, 5)]
        public int NotaProfissionalismo { get; set; }

        // Nota geral calculada
        [NotMapped]
        public double NotaGeral => (NotaPreco + NotaQualidade + NotaTempoExecucao + NotaComunicacao + NotaProfissionalismo) / 5.0;

        [MaxLength(1000)]
        public string? Comentario { get; set; }

        [MaxLength(500)]
        public string? PontosPositivos { get; set; }

        [MaxLength(500)]
        public string? PontosNegativos { get; set; }

        [Required]
        public DateTime DataAvaliacao { get; set; } = DateTime.Now;

        [Required]
        public bool Ativa { get; set; } = true;
    }
}
