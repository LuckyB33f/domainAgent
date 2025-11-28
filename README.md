# Domain Agent

A .NET 10 background service that automatically purchases expired .au domains from TPP Wholesale.

## Overview

This application runs as a background service and executes a domain purchase workflow at 1:31 AM Australian Eastern Time every day. It:

1. Fetches the .au domain drop list from TPP Wholesale API
2. Saves the drop list to a MySQL database for tracking
3. Selects domains based on configurable criteria
4. Executes purchase orders for selected domains
5. Records all purchase attempts and their outcomes in the database

## Requirements

- .NET 10 SDK
- MySQL Server 8.0 or later
- TPP Wholesale API credentials
- Valid registrant contact ID

## Configuration

Configure the application using `appsettings.json` or environment variables.

**Required configuration values (validated at startup):**
- `Database:ConnectionString` - MySQL database connection string
- `TppWholesale:ApiKey` - Your TPP Wholesale API key
- `TppWholesale:BaseUrl` - The API base URL
- `DomainSelection:DefaultRegistrantContactId` - Contact ID for domain registration

### Database Settings

```json
{
  "Database": {
    "ConnectionString": "Server=localhost;Database=domain_agent;User=root;Password=your-password;"
  }
}
```

The application uses MySQL to store:
- **Drop list entries**: All domains from the .au drop list (from API and CSV imports)
- **Purchase records**: All purchase attempts with status (Pending, Success, Failed)

The database tables are created automatically on first run.

### TPP Wholesale API Settings

```json
{
  "TppWholesale": {
    "BaseUrl": "https://www.tppwholesale.com.au/api/",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret",
    "ResellerId": "your-reseller-id"
  }
}
```

### Domain Selection Settings

```json
{
  "DomainSelection": {
    "AllowedTlds": [".au", ".com.au", ".net.au", ".org.au"],
    "MaxDomainsPerDay": 10,
    "MinDomainLength": 3,
    "MaxDomainLength": 63,
    "PriorityKeywords": ["business", "tech"],
    "ExcludeKeywords": ["spam", "xxx"],
    "DefaultRegistrantContactId": "your-contact-id",
    "DefaultNameservers": ["ns1.example.com", "ns2.example.com"]
  }
}
```

## CSV Ingestion

The application supports importing domain drop lists from CSV files. This is useful for:
- Loading historical drop list data
- Importing lists from alternative sources
- Testing with custom domain lists

### CSV Format

The CSV file should have a header row with the following supported column names:

| Column Name | Required | Description |
|------------|----------|-------------|
| `DomainName`, `Domain Name`, `domain_name`, `domain`, `name`, `DOMAIN`, `DOMAINNAME` | Yes | The domain name |
| `DropDate`, `Drop Date`, `drop_date`, `date`, `expiry`, `expiry_date`, `ExpiryDate`, `DATE`, `DROPDATE` | No | The drop date (defaults to today if not provided) |

### Supported Date Formats

- `yyyy-MM-dd` (e.g., 2024-01-15)
- `dd/MM/yyyy` (e.g., 15/01/2024)
- `MM/dd/yyyy` (e.g., 01/15/2024)
- `d/M/yyyy` (e.g., 15/1/2024)
- `yyyy/MM/dd` (e.g., 2024/01/15)
- `dd-MM-yyyy` (e.g., 15-01-2024)

### Example CSV

```csv
DomainName,DropDate
example.au,2024-01-15
mysite.com.au,2024-01-16
business.net.au,2024-01-17
```

### Using CSV Ingestion Programmatically

Inject `ICsvIngestionService` and use it to import from a file or stream:

```csharp
// From file
int count = await csvIngestionService.IngestFromFileAsync("/path/to/droplist.csv");

// From stream
using var stream = File.OpenRead("/path/to/droplist.csv");
int count = await csvIngestionService.IngestFromStreamAsync(stream);
```

## Running the Application

### Development

```bash
cd src/DomainAgent
dotnet run
```

### Production

```bash
cd src/DomainAgent
dotnet publish -c Release
./bin/Release/net10.0/publish/DomainAgent
```

### Docker

Build and run using Docker (create a Dockerfile as needed).

## Database Schema

The application automatically creates the following tables:

### `drop_list_entries`

| Column | Type | Description |
|--------|------|-------------|
| `id` | INT | Primary key |
| `domain_name` | VARCHAR(255) | Domain name |
| `drop_date` | DATETIME | When the domain drops |
| `tld` | VARCHAR(50) | TLD (e.g., .au, .com.au) |
| `source` | VARCHAR(50) | Data source (API or CSV) |
| `created_at` | DATETIME | Record creation time |
| `updated_at` | DATETIME | Last update time |

### `purchases`

| Column | Type | Description |
|--------|------|-------------|
| `id` | INT | Primary key |
| `domain_name` | VARCHAR(255) | Domain name |
| `tld` | VARCHAR(50) | TLD (e.g., .au, .com.au) |
| `order_id` | VARCHAR(100) | TPP Wholesale order ID |
| `status` | VARCHAR(20) | Status (Pending, Success, Failed) |
| `error_message` | VARCHAR(1000) | Error details if failed |
| `purchase_date` | DATETIME | When purchase was attempted |
| `created_at` | DATETIME | Record creation time |
| `updated_at` | DATETIME | Last update time |

## Schedule

The domain purchase job runs at **1:31 AM Australian Eastern Time** daily.

This timing is configured in `Program.cs` using a Quartz cron expression with the Australia/Sydney timezone.

## Project Structure

```
src/DomainAgent/
├── Configuration/
│   ├── DatabaseOptions.cs          # MySQL database configuration
│   ├── TppWholesaleOptions.cs      # TPP Wholesale API configuration
│   └── DomainSelectionOptions.cs   # Domain selection criteria
├── Data/
│   ├── DomainAgentDbContext.cs     # Entity Framework DbContext
│   ├── Entities/
│   │   ├── DropListEntry.cs        # Drop list entity
│   │   └── Purchase.cs             # Purchase entity
│   └── Repositories/
│       ├── IDropListRepository.cs  # Drop list repository interface
│       ├── DropListRepository.cs   # Drop list repository
│       ├── IPurchaseRepository.cs  # Purchase repository interface
│       └── PurchaseRepository.cs   # Purchase repository
├── Jobs/
│   └── DomainPurchaseJob.cs        # Quartz scheduled job
├── Models/
│   ├── ApiResponse.cs              # Generic API response wrapper
│   ├── DropListDomain.cs           # Drop list domain model
│   ├── DomainOrderRequest.cs       # Domain order request model
│   └── DomainOrderResponse.cs      # Domain order response model
├── Services/
│   ├── ITppWholesaleApiClient.cs   # API client interface
│   ├── TppWholesaleApiClient.cs    # TPP Wholesale API client
│   ├── IDomainSelectionService.cs  # Selection service interface
│   ├── DomainSelectionService.cs   # Domain selection logic
│   ├── IDomainPurchaseService.cs   # Purchase service interface
│   ├── DomainPurchaseService.cs    # Purchase workflow orchestration
│   ├── ICsvIngestionService.cs     # CSV ingestion interface
│   └── CsvIngestionService.cs      # CSV import functionality
├── Program.cs                       # Application entry point
└── appsettings.json                # Configuration file
```

## Customizing Domain Selection

The `DomainSelectionService` uses the following criteria to select domains:

1. **TLD Filter**: Only domains with allowed TLDs are considered
2. **Length Filter**: Domain name must be within min/max length
3. **Keyword Exclusion**: Domains containing excluded keywords are filtered out
4. **Priority Scoring**:
   - Shorter domains get higher priority
   - Domains containing priority keywords get boosted
   - `.au` TLD preferred over `.com.au`

## License

Private - All rights reserved
