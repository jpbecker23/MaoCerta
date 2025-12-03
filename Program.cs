using Microsoft.EntityFrameworkCore;
using MaoCerta.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using DotNetEnv;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

var connectionString = BuildConnectionStringFromEnvironment();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
);

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddRazorPages();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", context =>
{
    context.Response.Redirect("/Home");
    return Task.CompletedTask;
});

app.MapRazorPages();

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
