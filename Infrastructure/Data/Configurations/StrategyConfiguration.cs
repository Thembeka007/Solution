using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Entities;

namespace Infrastructure.Data.Configurations;

public class StrategyConfiguration
    : IEntityTypeConfiguration<Strategy>
{
    public void Configure(
        EntityTypeBuilder<Strategy> builder)
    {
        builder.ToTable("strategies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Version)
            .HasMaxLength(50);

        builder.HasIndex(x => x.IsActive);

        builder.HasIndex(x => x.ROI);
    }
}