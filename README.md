# Domain Agent

A .NET 10 background service that automatically purchases expired .au domains from TPP Wholesale.

## Overview

This application runs as a background service and executes a domain purchase workflow at 1:31 AM Australian Eastern Time every day. It:

1. Fetches the .au domain drop list from TPP Wholesale API
2. Selects domains based on configurable criteria
3. Executes purchase orders for selected domains

## Requirements

- .NET 10 SDK
- TPP Wholesale API credentials
- Valid registrant contact ID

## Configuration

Configure the application using `appsettings.json` or environment variables.

**Required configuration values (validated at startup):**
- `TppWholesale:ApiKey` - Your TPP Wholesale API key
- `TppWholesale:BaseUrl` - The API base URL
- `DomainSelection:DefaultRegistrantContactId` - Contact ID for domain registration

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

## Schedule

The domain purchase job runs at **1:31 AM Australian Eastern Time** daily.

This timing is configured in `Program.cs` using a Quartz cron expression with the Australia/Sydney timezone.

## Project Structure

```
src/DomainAgent/
├── Configuration/
│   ├── TppWholesaleOptions.cs     # TPP Wholesale API configuration
│   └── DomainSelectionOptions.cs  # Domain selection criteria
├── Jobs/
│   └── DomainPurchaseJob.cs       # Quartz scheduled job
├── Models/
│   ├── ApiResponse.cs             # Generic API response wrapper
│   ├── DropListDomain.cs          # Drop list domain model
│   ├── DomainOrderRequest.cs      # Domain order request model
│   └── DomainOrderResponse.cs     # Domain order response model
├── Services/
│   ├── ITppWholesaleApiClient.cs  # API client interface
│   ├── TppWholesaleApiClient.cs   # TPP Wholesale API client
│   ├── IDomainSelectionService.cs # Selection service interface
│   ├── DomainSelectionService.cs  # Domain selection logic
│   ├── IDomainPurchaseService.cs  # Purchase service interface
│   └── DomainPurchaseService.cs   # Purchase workflow orchestration
├── Program.cs                      # Application entry point
└── appsettings.json               # Configuration file
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
