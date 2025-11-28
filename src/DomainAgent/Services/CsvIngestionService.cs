using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using DomainAgent.Data.Entities;
using DomainAgent.Data.Repositories;

namespace DomainAgent.Services;

/// <summary>
/// Service for ingesting domain drop list data from CSV files.
/// </summary>
public class CsvIngestionService : ICsvIngestionService
{
    private readonly IDropListRepository _dropListRepository;
    private readonly ILogger<CsvIngestionService> _logger;

    public CsvIngestionService(
        IDropListRepository dropListRepository,
        ILogger<CsvIngestionService> logger)
    {
        _dropListRepository = dropListRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<int> IngestFromFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ingesting CSV file: {FilePath}", filePath);

        if (!File.Exists(filePath))
        {
            _logger.LogError("CSV file not found: {FilePath}", filePath);
            throw new FileNotFoundException("CSV file not found", filePath);
        }

        using var stream = File.OpenRead(filePath);
        return await IngestFromStreamAsync(stream, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> IngestFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting CSV ingestion from stream");

        var entries = new List<DropListEntry>();
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null,
            TrimOptions = TrimOptions.Trim
        };

        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, config);

        // Register class map for flexible column mapping
        csv.Context.RegisterClassMap<DropListCsvMap>();

        var records = csv.GetRecords<DropListCsvRecord>();

        foreach (var record in records)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(record.DomainName))
            {
                continue;
            }

            var entry = new DropListEntry
            {
                DomainName = record.DomainName.Trim().ToLowerInvariant(),
                DropDate = ParseDropDate(record.DropDate),
                Tld = ExtractTld(record.DomainName),
                Source = "CSV"
            };

            // Skip duplicates
            if (!await _dropListRepository.ExistsAsync(entry.DomainName, cancellationToken))
            {
                entries.Add(entry);
            }
        }

        if (entries.Count > 0)
        {
            await _dropListRepository.AddRangeAsync(entries, cancellationToken);
            await _dropListRepository.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("Successfully ingested {Count} entries from CSV", entries.Count);
        return entries.Count;
    }

    private static DateTime ParseDropDate(string? dropDate)
    {
        if (string.IsNullOrWhiteSpace(dropDate))
        {
            return DateTime.UtcNow.Date;
        }

        // Try various date formats commonly used in .au drop lists
        string[] formats =
        [
            "yyyy-MM-dd",
            "dd/MM/yyyy",
            "MM/dd/yyyy",
            "d/M/yyyy",
            "yyyy/MM/dd",
            "dd-MM-yyyy"
        ];

        if (DateTime.TryParseExact(dropDate.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
        {
            return result;
        }

        if (DateTime.TryParse(dropDate.Trim(), out result))
        {
            return result;
        }

        return DateTime.UtcNow.Date;
    }

    private static string? ExtractTld(string domainName)
    {
        var parts = domainName.Split('.');
        if (parts.Length >= 2)
        {
            var tldParts = parts.Skip(1);
            return "." + string.Join(".", tldParts);
        }
        return null;
    }
}

/// <summary>
/// CSV record model for drop list data.
/// </summary>
public class DropListCsvRecord
{
    public string? DomainName { get; set; }
    public string? DropDate { get; set; }
}

/// <summary>
/// CSV mapping for flexible column name handling.
/// </summary>
public sealed class DropListCsvMap : ClassMap<DropListCsvRecord>
{
    public DropListCsvMap()
    {
        // Map various possible column names for domain name
        Map(m => m.DomainName).Name("DomainName", "Domain Name", "domain_name", "domain", "name", "DOMAIN", "DOMAINNAME");
        
        // Map various possible column names for drop date
        Map(m => m.DropDate).Name("DropDate", "Drop Date", "drop_date", "date", "expiry", "expiry_date", "ExpiryDate", "DATE", "DROPDATE").Optional();
    }
}
