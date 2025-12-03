using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MaoCerta.Domain.Entities;

namespace MaoCerta.Infrastructure.Data.Configurations
{
    public class ServiceRequestConfiguration : IEntityTypeConfiguration<ServiceRequest>
    {
        public void Configure(EntityTypeBuilder<ServiceRequest> builder)
        {
            builder.ToTable("ServiceRequests");

            builder.HasKey(sr => sr.Id);

            builder.Property(sr => sr.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(sr => sr.Description)
                .HasMaxLength(1000);

            builder.Property(sr => sr.ServiceAddress)
                .HasMaxLength(200);

            builder.Property(sr => sr.Observations)
                .HasMaxLength(1000);

            builder.Property(sr => sr.VerificationCode)
                .HasMaxLength(10);

            builder.Property(sr => sr.ProposedValue)
                .HasColumnType("decimal(10,2)");

            builder.Property(sr => sr.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(sr => sr.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Relationships
            builder.HasOne(sr => sr.Client)
                .WithMany(c => c.ServiceRequests)
                .HasForeignKey(sr => sr.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sr => sr.Professional)
                .WithMany(p => p.ServiceRequests)
                .HasForeignKey(sr => sr.ProfessionalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sr => sr.Review)
                .WithOne(r => r.ServiceRequest)
                .HasForeignKey<Review>(r => r.ServiceRequestId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
