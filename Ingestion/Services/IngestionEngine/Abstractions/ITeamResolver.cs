namespace Ingestion.Services.IngestionEngine.Abstractions;

public interface ITeamResolver
{
    Task<Guid> ResolveTeamIdAsync(
        string teamName);
}