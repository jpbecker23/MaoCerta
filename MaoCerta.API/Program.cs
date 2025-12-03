using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MaoCerta.Infrastructure.Data;
using MaoCerta.API.Data;
using MaoCerta.Infrastructure.Repositories;
using MaoCerta.Application.Interfaces;
using MaoCerta.Domain.Interfaces;
using MaoCerta.Application.Services;
using Serilog;
using FluentValidation;
using DotNetEnv;
using Microsoft.AspNetCore.Identity;
using MaoCerta.Domain.Entities;

// Load .env file even when the process is running from a nested directory
Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/maocerta-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Database Configuration
var connectionString = BuildConnectionStringFromEnvironment();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, 
        b => b.MigrationsAssembly("MaoCerta.API")));

// ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Repository Pattern
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application Services
builder.Services.AddScoped<IClientService, MaoCerta.Application.Services.ClientService>();
builder.Services.AddScoped<IProfessionalService, MaoCerta.Application.Services.ProfessionalService>();
builder.Services.AddScoped<IAuthService, MaoCerta.Application.Services.AuthService>();
builder.Services.AddScoped<IReviewService, MaoCerta.Application.Services.ReviewService>();
builder.Services.AddScoped<IServiceRequestService, MaoCerta.Application.Services.ServiceRequestService>();

// FluentValidation - TODO: Implement validators
// builder.Services.AddFluentValidationAutoValidation();
// builder.Services.AddValidatorsFromAssemblyContaining<CreateClientDtoValidator>();

// JWT Authentication
var secretKey = GetRequiredEnvVar("JWT_SECRET_KEY");
var issuer = GetRequiredEnvVar("JWT_ISSUER");
var audience = GetRequiredEnvVar("JWT_AUDIENCE");
var expiryMinutesStr = GetRequiredEnvVar("JWT_EXPIRY_MINUTES");
var expiryMinutes = int.Parse(expiryMinutesStr);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();

// CORS Configuration
var allowedOrigins = new[]
{
    "http://localhost:3000",
    "https://localhost:3000",
    "http://localhost:3001",
    "http://localhost:5088",
    "https://localhost:5088",
    "http://localhost:5089",
    "https://localhost:5089",
    "http://localhost:7080",
    "https://localhost:7080"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Mão Certa API", Version = "v1" });
    
    // JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mão Certa API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Database Migration
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
    try
    {
        context.Database.Migrate();
        await IdentitySchemaHelper.EnsureIdentityProfileColumnsAsync(context);
        Log.Information("Database migration completed successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while migrating the database");
    }
}

app.Run();

static string BuildConnectionStringFromEnvironment()
{
    var host = GetRequiredEnvVar("DB_HOST");
    var port = GetRequiredEnvVar("DB_PORT");
    var database = GetRequiredEnvVar("DB_NAME");
    var username = GetRequiredEnvVar("DB_USERNAME");
    var password = GetRequiredEnvVar("DB_PASSWORD");

    return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
}

static string GetRequiredEnvVar(string key)
{
    var value = Environment.GetEnvironmentVariable(key);
    if (string.IsNullOrWhiteSpace(value))
    {
        throw new InvalidOperationException($"Environment variable '{key}' is missing. Please configure it in your .env file.");
    }

    return value;
}
