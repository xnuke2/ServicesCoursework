using System.Text;
using EmployeeService;
using EmployeeService.Migrations;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName: builder.Environment.ApplicationName))
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddPrometheusExporter();
    });
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<KafkaSettings>(
    builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddControllers();
var authServiceUrl = "http://localhost:8082";

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
        
    });
    
    options.AddPolicy("AllowLocalhost8081",
        builder =>
        {
            builder.WithOrigins("http://localhost:8081")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
        
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["access_token"];
                if (!string.IsNullOrEmpty(token))
                    context.Token = token;
                return Task.CompletedTask;
            }
        };
    }).AddCookie(options => 
    {
        options.LoginPath = "/external-login"; 
        options.AccessDeniedPath = "/access-denied";
    });
builder.Services.AddAuthorization();
var app = builder.Build();



app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowLocalhost8081");
app.UseAuthentication();
app.UseAuthorization();
// Middleware/RedirectUnauthorizedMiddleware.cs




app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapGet("/external-login", async (HttpContext context) =>
{
    var returnUrl = context.Request.Query["returnUrl"].FirstOrDefault() ?? "/";
    var loginUrl = $"{authServiceUrl}/login?returnUrl={Uri.EscapeDataString(returnUrl)}";
    context.Response.Redirect(loginUrl);
});
app.MapFallbackToFile("index.html").RequireAuthorization();
app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.UseHttpMetrics();
app.MapMetrics();
app.Run();