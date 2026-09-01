using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel.Entities;

public class TeamFormConfiguration
    : IEntityTypeConfiguration<TeamForm>
{
    public void Configure(
        EntityTypeBuilder<TeamForm> builder)
    {
        builder.ToTable("team_forms");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TeamName)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.XG)
            .HasPrecision(5, 2);

        builder.Property(x => x.OpponentXG)
            .HasPrecision(5, 2);

        builder.HasIndex(x => x.TeamId);

        builder.HasIndex(x => x.MatchDateUtc);

        builder.HasIndex(x =>
            new
            {
                x.TeamId,
                x.MatchDateUtc
            });

        builder.HasIndex(x => x.BTTS);

        builder.HasIndex(x => x.Over25);
    }
}