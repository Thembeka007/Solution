namespace Ingestion.Services.IngestionEngine.Abstractions;

public interface ITeamFormGenerator
{
    Task GenerateAsync(Guid teamId);
}
