using System.Reflection;
using System.Text;
using BenefitService.Migrations;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")),
    ServiceLifetime.Singleton);
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName: builder.Environment.ApplicationName))
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddPrometheusExporter();
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
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
        options.AccessDeniedPath = "/external-login";
    });
builder.Services.AddAuthorization();

    


builder.Services.AddControllers();
builder.Services.Configure<KafkaSettings>(
    builder.Configuration.GetSection("Kafka"));
    
builder.Services.AddSingleton<IBenefitsProcessor, BenefitsProcessor>();
builder.Services.AddHostedService<KafkaConsumerService>();
var app = builder.Build();
app.UseCors("AllowAll");
app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


var authServiceUrl = "http://localhost:8082";
app.MapGet("/external-login", async (HttpContext context) =>
{
    var returnUrl = context.Request.Query["returnUrl"].FirstOrDefault() ?? "/";
    var loginUrl = $"{authServiceUrl}/login?returnUrl={Uri.EscapeDataString(returnUrl)}";
    context.Response.Redirect(loginUrl);
});
app.UseAuthentication();
app.UseAuthorization();
app.Run();