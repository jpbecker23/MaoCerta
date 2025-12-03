// using Microsoft.EntityFrameworkCore;
// using MaoCerta.Models;
// using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

// namespace MaoCerta.Data
// {
//     public class AppDbContext : IdentityDbContext
//     {
//         public AppDbContext(DbContextOptions<AppDbContext> options)
//             : base(options)
//         {
//         }

//         public DbSet<Cliente> Clientes { get; set; }
//         public DbSet<Profissional> Profissionais { get; set; }
//         public DbSet<Categoria> Categorias { get; set; }
//         public DbSet<Avaliacao> Avaliacoes { get; set; }
//         public DbSet<SolicitacaoServico> SolicitacoesServico { get; set; }

//         protected override void OnModelCreating(ModelBuilder modelBuilder)
//         {
//             base.OnModelCreating(modelBuilder);

//             // Configuração de relacionamentos
//             modelBuilder.Entity<Avaliacao>()
//                 .HasOne(a => a.Cliente)
//                 .WithMany(c => c.Avaliacoes)
//                 .HasForeignKey(a => a.ClienteId)
//                 .OnDelete(DeleteBehavior.Restrict);

//             modelBuilder.Entity<Avaliacao>()
//                 .HasOne(a => a.Profissional)
//                 .WithMany(p => p.Avaliacoes)
//                 .HasForeignKey(a => a.ProfissionalId)
//                 .OnDelete(DeleteBehavior.Restrict);

//             modelBuilder.Entity<Avaliacao>()
//                 .HasOne(a => a.SolicitacaoServico)
//                 .WithOne(s => s.Avaliacao)
//                 .HasForeignKey<Avaliacao>(a => a.SolicitacaoServicoId)
//                 .OnDelete(DeleteBehavior.Restrict);

//             modelBuilder.Entity<SolicitacaoServico>()
//                 .HasOne(s => s.Cliente)
//                 .WithMany(c => c.Solicitacoes)
//                 .HasForeignKey(s => s.ClienteId)
//                 .OnDelete(DeleteBehavior.Restrict);

//             modelBuilder.Entity<SolicitacaoServico>()
//                 .HasOne(s => s.Profissional)
//                 .WithMany(p => p.Solicitacoes)
//                 .HasForeignKey(s => s.ProfissionalId)
//                 .OnDelete(DeleteBehavior.Restrict);

//             modelBuilder.Entity<Profissional>()
//                 .HasOne(p => p.Categoria)
//                 .WithMany(c => c.Profissionais)
//                 .HasForeignKey(p => p.CategoriaId)
//                 .OnDelete(DeleteBehavior.Restrict);

//             // Configuração de índices para melhor performance
//             modelBuilder.Entity<Profissional>()
//                 .HasIndex(p => p.Email)
//                 .IsUnique();

//             modelBuilder.Entity<Cliente>()
//                 .HasIndex(c => c.Email)
//                 .IsUnique();

//             modelBuilder.Entity<Avaliacao>()
//                 .HasIndex(a => new { a.ClienteId, a.ProfissionalId, a.SolicitacaoServicoId })
//                 .IsUnique();

//             // Seed data para categorias
//             modelBuilder.Entity<Categoria>().HasData(
//                 new Categoria { Id = 1, Nome = "Manutenção de Chuveiros", Descricao = "Serviços de manutenção e reparo de chuveiros", Icone = "🚿" },
//                 new Categoria { Id = 2, Nome = "Jardinagem", Descricao = "Serviços de jardinagem e paisagismo", Icone = "🌱" },
//                 new Categoria { Id = 3, Nome = "Limpeza Residencial", Descricao = "Serviços de limpeza doméstica", Icone = "🧹" },
//                 new Categoria { Id = 4, Nome = "Elétrica", Descricao = "Serviços elétricos residenciais", Icone = "⚡" },
//                 new Categoria { Id = 5, Nome = "Hidráulica", Descricao = "Serviços hidráulicos e encanamento", Icone = "🔧" },
//                 new Categoria { Id = 6, Nome = "Pintura", Descricao = "Serviços de pintura residencial", Icone = "🎨" }
//             );
//         }
//     }
// }
