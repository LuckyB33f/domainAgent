using DomainAgent.Configuration;
using DomainAgent.Jobs;
using DomainAgent.Services;
using Microsoft.Extensions.Options;
using Quartz;

var builder = Host.CreateApplicationBuilder(args);

// Configure options with validation
builder.Services.AddOptions<TppWholesaleOptions>()
    .Bind(builder.Configuration.GetSection(TppWholesaleOptions.SectionName))
    .Validate(options =>
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            return false;
        }
        if (string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            return false;
        }
        return true;
    }, "TppWholesale:ApiKey and TppWholesale:BaseUrl are required.")
    .ValidateOnStart();

builder.Services.AddOptions<DomainSelectionOptions>()
    .Bind(builder.Configuration.GetSection(DomainSelectionOptions.SectionName))
    .Validate(options =>
    {
        if (string.IsNullOrWhiteSpace(options.DefaultRegistrantContactId))
        {
            return false;
        }
        return true;
    }, "DomainSelection:DefaultRegistrantContactId is required for domain registration.")
    .ValidateOnStart();

// Register HTTP client for TPP Wholesale API
builder.Services.AddHttpClient<ITppWholesaleApiClient, TppWholesaleApiClient>(client =>
{
    var baseUrl = builder.Configuration.GetSection(TppWholesaleOptions.SectionName)["BaseUrl"]
        ?? "https://www.tppwholesale.com.au/api/";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register services
builder.Services.AddScoped<IDomainSelectionService, DomainSelectionService>();
builder.Services.AddScoped<IDomainPurchaseService, DomainPurchaseService>();

// Configure Quartz
builder.Services.AddQuartz(q =>
{
    // Create job key
    var jobKey = new JobKey("DomainPurchaseJob");

    // Register the job
    q.AddJob<DomainPurchaseJob>(opts => opts.WithIdentity(jobKey));

    // Create a trigger to run at 1:31 AM Australian Eastern Time every day
    // AEST is UTC+10, AEDT (daylight saving) is UTC+11
    // We use Australia/Sydney timezone to handle daylight saving automatically
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("DomainPurchaseJob-trigger")
        .WithCronSchedule(
            "0 31 1 * * ?", // Every day at 1:31 AM
            x => x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Australia/Sydney")))
        .WithDescription("Trigger for domain purchase job at 1:31 AM Australian Eastern Time"));
});

// Add the Quartz hosted service
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var host = builder.Build();
host.Run();
