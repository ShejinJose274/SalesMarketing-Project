
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SalesMarketing.API.Data;
using SalesMarketing.API.Services;

var builder = WebApplication.CreateBuilder(args);

var frontendOrigins = builder.Configuration
    .GetSection("Frontend:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:4200", "https://localhost:4200"];

// Add controllers
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(frontendOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// PostgreSQL connection
builder.Services.AddDbContext<SalesMarketingDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Cookie authentication
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "SalesMarketing.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;

        options.ExpireTimeSpan = TimeSpan.FromHours(2);


        options.Events.OnRedirectToLogin = async context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            await context.Response.WriteAsJsonAsync(new
            {
                message = "You are not logged in. Please login as an admin to access this API."
            });
        };


        options.Events.OnRedirectToAccessDenied = async context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;

            string path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

            string message = "Access denied. You do not have permission.";

            if (path.StartsWith("/api/salesmen"))
            {
                message = "Access denied. Only SalesManagerAdmin can create, update, delete or change salesman records. MasterAdmin has read-only access.";
            }
            else if (path.StartsWith("/api/marketingteams"))
            {
                message = "Access denied. Only MasterAdmin can manage marketing teams.";
            }

            await context.Response.WriteAsJsonAsync(new
            {
                message
            });
        };


    });

// Register authorization
builder.Services.AddAuthorization();

// Register SalesmanService
builder.Services.AddScoped<SalesmanService>();

//Register the MarketingTeamService
builder.Services.AddScoped<MarketingTeamService>();

var app = builder.Build();

// Swagger for development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

// Correct middleware order
app.UseAuthentication();
app.UseAuthorization();

// Basic origin check for state-changing requests
app.Use(async (context, next) =>
{
    bool changesData =
        HttpMethods.IsPost(context.Request.Method) ||
        HttpMethods.IsPut(context.Request.Method) ||
        HttpMethods.IsPatch(context.Request.Method) ||
        HttpMethods.IsDelete(context.Request.Method);

    if (changesData)
    {
        var origin = context.Request.Headers.Origin.ToString();

        if (!string.IsNullOrEmpty(origin))
        {
            var requestOrigin =
                $"{context.Request.Scheme}://{context.Request.Host}";

            bool isAllowedFrontendOrigin = frontendOrigins.Any(
                allowedOrigin => string.Equals(
                    origin,
                    allowedOrigin,
                    StringComparison.OrdinalIgnoreCase));

            if (!string.Equals(
                    origin,
                    requestOrigin,
                    StringComparison.OrdinalIgnoreCase) &&
                !isAllowedFrontendOrigin)
            {
                context.Response.StatusCode = 403;
                return;
            }
        }
    }

    await next();
});

app.MapControllers();

app.Run();
