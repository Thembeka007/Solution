using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Entities;

namespace Infrastructure.Data.Configurations;

public class BetConfiguration
    : IEntityTypeConfiguration<Bet>
{
    public void Configure(
        EntityTypeBuilder<Bet> builder)
    {
        builder.ToTable("bets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Stake)
            .HasPrecision(10, 2);

        builder.Property(x => x.Odds)
            .HasPrecision(10, 2);

        builder.Property(x => x.PotentialPayout)
            .HasPrecision(10, 2);

        builder.Property(x => x.ActualPayout)
            .HasPrecision(10, 2);

        builder.HasIndex(x => x.Won);

        builder.HasIndex(x => x.Category);
    }
}