using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AviationMaintenance.Domain.Entities;

namespace AviationMaintenance.Infrastructure.Configurations
{
    /// <summary>
    /// Entity Framework configuration for CarryOverTask
    /// </summary>
    public class CarryOverTaskConfiguration : IEntityTypeConfiguration<CarryOverTask>
    {
        public void Configure(EntityTypeBuilder<CarryOverTask> builder)
        {
            builder.ToTable("CarryOverTasks");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TaskName)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.TaskBarcode)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.CustomDeferralReason)
                .HasMaxLength(500);

            builder.Property(x => x.StockItem)
                .HasMaxLength(100);

            builder.Property(x => x.PartNumber)
                .HasMaxLength(100);

            builder.Property(x => x.StockId)
                .HasMaxLength(50);

            builder.Property(x => x.Remark)
                .HasMaxLength(1000);

            builder.Property(x => x.DelayComment)
                .HasMaxLength(500);

            // Indexes for performance
            builder.HasIndex(x => x.CarryOverReportId);

            builder.HasIndex(x => x.TaskBarcode);

            builder.HasIndex(x => x.DeferralReason);

            builder.HasIndex(x => x.TaskType);

            builder.HasIndex(x => x.AircraftDelayImpact);

            builder.HasIndex(x => x.IsRepeatTask);

            builder.HasIndex(x => new { x.CarryOverReportId, x.TaskBarcode })
                .IsUnique();

            // Relationships
            builder.HasOne(x => x.Report)
                .WithMany(x => x.Tasks)
                .HasForeignKey(x => x.CarryOverReportId)
                .OnDelete(DeleteBehavior.Cascade);

            // Global query filter for soft deletes if needed
            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
