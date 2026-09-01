using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class TipsterConfiguration : IEntityTypeConfiguration<Tipster>
{
    public void Configure(
     EntityTypeBuilder<Tipster> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.WinRate)
            .HasPrecision(5, 2);

        builder.Property(x => x.ROI)
            .HasPrecision(5, 2);

        builder.Property(x => x.Profit)
            .HasPrecision(18, 2);

        builder.Property(x => x.Weight)
            .HasPrecision(5, 2);
    }
}
