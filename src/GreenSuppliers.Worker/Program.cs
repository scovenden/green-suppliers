using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Services;
using GreenSuppliers.Worker.Jobs;
using GreenSuppliers.Worker.Services;
using Microsoft.EntityFrameworkCore;
using Resend;

var builder = Host.CreateApplicationBuilder(args);

// Register DbContext (same connection string as API)
builder.Services.AddDbContext<GreenSuppliersDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services from API project
builder.Services.AddScoped<EsgScoringService>();
builder.Services.AddScoped<VerificationService>();

// Register email sender: use Resend when API key is configured, otherwise fall back to console
var resendApiKey = builder.Configuration["Resend:ApiKey"];
if (!string.IsNullOrWhiteSpace(resendApiKey))
{
    builder.Services.AddOptions<ResendClientOptions>()
        .Configure(o => o.ApiToken = resendApiKey);
    builder.Services.AddHttpClient<IResend, ResendClient>();
    builder.Services.AddScoped<IEmailSender, ResendEmailSender>();
}
else
{
    builder.Services.AddScoped<IEmailSender, ConsoleEmailSender>();
}

// Register hosted services (background jobs)
builder.Services.AddHostedService<CertExpiryScanner>();
builder.Services.AddHostedService<NightlyRescore>();
builder.Services.AddHostedService<EmailDispatch>();

var host = builder.Build();
host.Run();
