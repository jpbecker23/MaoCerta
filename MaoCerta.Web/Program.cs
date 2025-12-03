using MaoCerta.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection(ApiSettings.SectionName));
builder.Services.PostConfigure<ApiSettings>(settings =>
{
    if (string.IsNullOrWhiteSpace(settings.BaseUrl))
    {
        settings.BaseUrl = builder.Configuration["API_BASE_URL"]?.TrimEnd('/') ?? "https://localhost:5001/api";
    }
    else
    {
        settings.BaseUrl = settings.BaseUrl.TrimEnd('/');
    }
});

builder.Services.AddResponseCaching();
builder.Services.AddMemoryCache();

builder.Services.AddControllersWithViews(options =>
{
    options.CacheProfiles.Add("StaticContent", new CacheProfile { Duration = 60 });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        const int durationInSeconds = 60 * 60;
        ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=" + durationInSeconds;
    }
});

app.UseRouting();
app.UseResponseCaching();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

