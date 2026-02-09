using System.Text;
using DndApp.Media.Data;
using DndApp.Media.Options;
using DndApp.Media.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Minio;

var builder = WebApplication.CreateBuilder(args);

const string defaultJwtSigningKey = "replace-with-a-minimum-32-character-signing-key";

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException("ConnectionStrings:Default (or ConnectionStrings:Database) is required.");

var jwtSection = builder.Configuration.GetSection("Auth:Jwt");
var jwtIssuer = jwtSection["Issuer"] ?? "dnd-app";
var jwtAudience = jwtSection["Audience"] ?? "dnd-app-clients";
var jwtSigningKey = jwtSection["SigningKey"] ?? defaultJwtSigningKey;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddDbContext<MediaDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.Configure<S3StorageOptions>(builder.Configuration.GetSection("Storage:S3"));
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IMinioClient>(serviceProvider =>
{
    var options = serviceProvider.GetRequiredService<IOptions<S3StorageOptions>>().Value;
    ValidateS3Options(options);

    var minioClient = new MinioClient()
        .WithEndpoint(options.Endpoint)
        .WithCredentials(options.AccessKey, options.SecretKey)
        .WithSSL(options.UseSsl)
        .Build();

    if (!string.IsNullOrWhiteSpace(options.Region))
    {
        minioClient = minioClient.WithRegion(options.Region);
    }

    return minioClient;
});
builder.Services.AddSingleton<IMediaObjectStorage, MediaObjectStorage>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await ApplyMigrationsAsync(app.Services, app.Logger);
    await EnsureBucketExistsAsync(app.Services, app.Logger);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    service = "media",
    version = "1.0.0"
})).RequireAuthorization();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = _ => true
});

app.Run();

static void ValidateS3Options(S3StorageOptions options)
{
    if (string.IsNullOrWhiteSpace(options.Endpoint))
    {
        throw new InvalidOperationException("Storage:S3:Endpoint is required.");
    }

    if (string.IsNullOrWhiteSpace(options.AccessKey))
    {
        throw new InvalidOperationException("Storage:S3:AccessKey is required.");
    }

    if (string.IsNullOrWhiteSpace(options.SecretKey))
    {
        throw new InvalidOperationException("Storage:S3:SecretKey is required.");
    }

    if (string.IsNullOrWhiteSpace(options.Bucket))
    {
        throw new InvalidOperationException("Storage:S3:Bucket is required.");
    }

    if (!string.IsNullOrWhiteSpace(options.PublicBaseUrl)
        && !Uri.TryCreate(options.PublicBaseUrl, UriKind.Absolute, out _))
    {
        throw new InvalidOperationException("Storage:S3:PublicBaseUrl must be an absolute URL when configured.");
    }
}

static async Task ApplyMigrationsAsync(IServiceProvider services, ILogger logger)
{
    const int maxAttempts = 10;
    var delay = TimeSpan.FromSeconds(3);

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
            await dbContext.Database.MigrateAsync();
            return;
        }
        catch (Exception exception) when (attempt < maxAttempts)
        {
            logger.LogWarning(
                exception,
                "Failed to apply media migrations on attempt {Attempt}/{MaxAttempts}. Retrying in {DelaySeconds}s.",
                attempt,
                maxAttempts,
                delay.TotalSeconds);

            await Task.Delay(delay);
        }
    }

    using var finalScope = services.CreateScope();
    var finalDbContext = finalScope.ServiceProvider.GetRequiredService<MediaDbContext>();
    await finalDbContext.Database.MigrateAsync();
}

static async Task EnsureBucketExistsAsync(IServiceProvider services, ILogger logger)
{
    const int maxAttempts = 10;
    var delay = TimeSpan.FromSeconds(3);

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            using var scope = services.CreateScope();
            var objectStorage = scope.ServiceProvider.GetRequiredService<IMediaObjectStorage>();
            await objectStorage.EnsureBucketExistsAsync(CancellationToken.None);
            return;
        }
        catch (Exception exception) when (attempt < maxAttempts)
        {
            logger.LogWarning(
                exception,
                "Failed to ensure media bucket on attempt {Attempt}/{MaxAttempts}. Retrying in {DelaySeconds}s.",
                attempt,
                maxAttempts,
                delay.TotalSeconds);

            await Task.Delay(delay);
        }
    }

    using var finalScope = services.CreateScope();
    var finalObjectStorage = finalScope.ServiceProvider.GetRequiredService<IMediaObjectStorage>();
    await finalObjectStorage.EnsureBucketExistsAsync(CancellationToken.None);
}

public partial class Program;
