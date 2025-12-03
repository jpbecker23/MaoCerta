using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MaoCerta.Domain.Entities;

namespace MaoCerta.Infrastructure.Data.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.ToTable("Reviews");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.PriceRating)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(r => r.QualityRating)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(r => r.SpeedRating)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(r => r.CommunicationRating)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(r => r.ProfessionalismRating)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(r => r.Comment)
                .HasMaxLength(1000);

            builder.Property(r => r.PositivePoints)
                .HasMaxLength(500);

            builder.Property(r => r.NegativePoints)
                .HasMaxLength(500);

            builder.Property(r => r.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(r => r.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Unique constraint for one review per service request
            builder.HasIndex(r => r.ServiceRequestId)
                .IsUnique();

            // Relationships
            builder.HasOne(r => r.Client)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Professional)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProfessionalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.ServiceRequest)
                .WithOne(sr => sr.Review)
                .HasForeignKey<Review>(r => r.ServiceRequestId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
