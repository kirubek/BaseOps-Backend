using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AviationMaintenance.Domain.Entities;

namespace AviationMaintenance.Infrastructure.Configurations
{
    /// <summary>
    /// Entity Framework configuration for CarryOverReview
    /// </summary>
    public class CarryOverReviewConfiguration : IEntityTypeConfiguration<CarryOverReview>
    {
        public void Configure(EntityTypeBuilder<CarryOverReview> builder)
        {
            builder.ToTable("CarryOverReviews");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Comment)
                .HasMaxLength(1000);

            // Indexes for performance
            builder.HasIndex(x => x.CarryOverReportId);

            builder.HasIndex(x => x.ReviewerUserId);

            builder.HasIndex(x => x.ReviewerRole);

            builder.HasIndex(x => x.Action);

            builder.HasIndex(x => x.ReviewedAt);

            // Relationships
            builder.HasOne(x => x.CarryOverReport)
                .WithMany(x => x.Reviews)
                .HasForeignKey(x => x.CarryOverReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ReviewerUser)
                .WithMany()
                .HasForeignKey(x => x.ReviewerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Global query filter for soft deletes if needed
            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
