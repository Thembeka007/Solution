namespace Ingestion.Services.IngestionEngine.Abstractions;

public interface IIngestionService
{
    Task IngestMatchesAsync(DateTime date);
}
