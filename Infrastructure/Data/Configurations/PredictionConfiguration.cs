using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Entities;

namespace Infrastructure.Data.Configurations;

public class PredictionConfiguration
    : IEntityTypeConfiguration<Prediction>
{
    public void Configure(
        EntityTypeBuilder<Prediction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x =>
                x.PredictionType)
            .HasMaxLength(50);

        builder.Property(x =>
                x.PredictedOutcome)
            .HasMaxLength(50);

        builder.Property(x =>
                x.Confidence)
            .HasPrecision(5, 2);

        builder
            .HasOne(x => x.Match)
            .WithMany()
            .HasForeignKey(x => x.MatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}