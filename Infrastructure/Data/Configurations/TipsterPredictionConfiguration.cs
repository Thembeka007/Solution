using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class TipsterPredictionConfiguration
    : IEntityTypeConfiguration<TipsterPrediction>
{
    public void Configure(
    EntityTypeBuilder<TipsterPrediction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PredictionType)
            .HasMaxLength(100);

        builder.Property(x => x.PredictedOutcome)
            .HasMaxLength(100);

        builder.Property(x => x.Odds)
            .HasPrecision(8, 2);

        builder
            .HasOne(x => x.Tipster)
            .WithMany()
            .HasForeignKey(x => x.TipsterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
