using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Services;
using GreenSuppliers.Worker.Jobs;
using GreenSuppliers.Worker.Services;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// Register DbContext (same connection string as API)
builder.Services.AddDbContext<GreenSuppliersDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services from API project
builder.Services.AddScoped<EsgScoringService>();
builder.Services.AddScoped<VerificationService>();

// Register email sender (swap ConsoleEmailSender for a real implementation in production)
builder.Services.AddScoped<IEmailSender, ConsoleEmailSender>();

// Register hosted services (background jobs)
builder.Services.AddHostedService<CertExpiryScanner>();
builder.Services.AddHostedService<NightlyRescore>();
builder.Services.AddHostedService<EmailDispatch>();

var host = builder.Build();
host.Run();
