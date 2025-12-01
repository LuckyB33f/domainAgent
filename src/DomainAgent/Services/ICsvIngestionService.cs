using DomainAgent.Data.Entities;

namespace DomainAgent.Services;

/// <summary>
/// Interface for CSV ingestion service.
/// </summary>
public interface ICsvIngestionService
{
    /// <summary>
    /// Ingests domain drop list data from a CSV file.
    /// </summary>
    /// <param name="filePath">Path to the CSV file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of entries successfully ingested.</returns>
    Task<int> IngestFromFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ingests domain drop list data from a CSV stream.
    /// </summary>
    /// <param name="stream">The stream containing CSV data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of entries successfully ingested.</returns>
    Task<int> IngestFromStreamAsync(Stream stream, CancellationToken cancellationToken = default);
}
