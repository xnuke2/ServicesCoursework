using System.Text;
using AuthService.Migrations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
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
// Add services to the container.
//builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder =>
        {
            builder.WithOrigins("allowedOrigins")
                .AllowCredentials() // Важно для кук!
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed((host) => true); // Разрешаем любой origin
        });
});
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
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
        
        // Принимаем токен из куки
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Из заголовка Authorization: Bearer <token>
                var token = context.Request.Headers["Authorization"]
                    .FirstOrDefault()?.Split(" ").Last();
            
                // Или из куки
                if (string.IsNullOrEmpty(token))
                    token = context.Request.Cookies["access_token"];
            
                if (!string.IsNullOrEmpty(token))
                    context.Token = token;
            
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                if (context.Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    context.HandleResponse();
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(
                        System.Text.Json.JsonSerializer.Serialize(new 
                        { 
                            Message = "Unauthorized" 
                        }));
                }
                
                var loginUrl = $"/login";
                context.Response.Redirect(loginUrl);
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();


app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseRouting();
app.UseDefaultFiles(); 
app.UseStaticFiles(); 
app.UseAuthentication();   
app.UseAuthorization(); 

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.Use(async (context, next) =>
{
    await next();
    
    if (context.Response.StatusCode == 401 && 
        !context.User.Identity?.IsAuthenticated == true)
    {
        var isAjax = context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                     context.Request.Headers["Accept"].ToString().Contains("application/json");
        
        if (isAjax)
        {
            // Для AJAX запросов возвращаем JSON
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                System.Text.Json.JsonSerializer.Serialize(new 
                { 
                    Message = "Unauthorized",
                    RedirectUrl = $"https://localhost:8082/login?returnUrl={Uri.EscapeDataString(context.Request.Path + context.Request.QueryString)}"
                }));
        }
        else
        {
            // Для обычных запросов делаем JavaScript редирект
            context.Response.ContentType = "text/html";
            var redirectScript = $@"
                <script>
                    window.location.href = 'https://localhost:8082/login?returnUrl={Uri.EscapeDataString(context.Request.Path + context.Request.QueryString)}';
                </script>";
            await context.Response.WriteAsync(redirectScript);
        }
    }
});
app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.UseHttpMetrics(); // Добавляет метрики HTTP
app.MapMetrics();
app.Run();