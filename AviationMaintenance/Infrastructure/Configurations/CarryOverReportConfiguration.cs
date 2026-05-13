using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AviationMaintenance.Domain.Entities;

namespace AviationMaintenance.Infrastructure.Configurations
{
    /// <summary>
    /// Entity Framework configuration for CarryOverReport
    /// </summary>
    public class CarryOverReportConfiguration : IEntityTypeConfiguration<CarryOverReport>
    {
        public void Configure(EntityTypeBuilder<CarryOverReport> builder)
        {
            builder.ToTable("CarryOverReports");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ReportNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.AircraftRegistration)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.AircraftType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.FleetType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.WorkPackageDescription)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.MaintenanceLocation)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.CarryOverPercentage)
                .HasPrecision(5, 2);

            // Indexes for performance
            builder.HasIndex(x => x.ReportNumber)
                .IsUnique();

            builder.HasIndex(x => x.AircraftRegistration);

            builder.HasIndex(x => x.FleetType);

            builder.HasIndex(x => x.ReportDate);

            builder.HasIndex(x => x.Status);

            builder.HasIndex(x => x.CreatedByUserId);

            builder.HasIndex(x => x.SectionId);

            // Relationships
            builder.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Section)
                .WithMany()
                .HasForeignKey(x => x.SectionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Tasks)
                .WithOne(x => x.Report)
                .HasForeignKey(x => x.CarryOverReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Reviews)
                .WithOne(x => x.CarryOverReport)
                .HasForeignKey(x => x.CarryOverReportId)
                .OnDelete(DeleteBehavior.Cascade);

            // Global query filter for soft deletes if needed
            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
