// Services/KafkaConsumerService.cs
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;
using BenefitService.Migrations;
using Bixd.Models;
using Microsoft.EntityFrameworkCore;

public class KafkaSettings
{
    public string BootstrapServers { get; set; }
    public string Topic { get; set; }
    public string GroupId { get; set; }
}

public interface IBenefitsProcessor
{
    Task ProcessEmployeeCreated(int employeeId, string name, string position);
    Task ProcessEmployeeUpdated(int employeeId, string name, string position);
}

public class BenefitsProcessor : IBenefitsProcessor
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BenefitsProcessor> _logger;

    public BenefitsProcessor(ApplicationDbContext context, ILogger<BenefitsProcessor> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ProcessEmployeeCreated(int employeeId, string name, string position)
    {
        _logger.LogInformation($"Создание льгот для сотрудника: {name} (ID: {employeeId})");
        
        try
        {
            var benefits = new EmployeeBenefit
            {
                EmployeeId = employeeId,
                BenefitsId = 1,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddYears(1),
            };

            await _context.EmployeeBenefits.AddAsync(benefits);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Назначено льгота для {name}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка создания льгот для сотрудника {employeeId}");
        }
    }

    public async Task ProcessEmployeeUpdated(int employeeId, string name, string position)
    {
        _logger.LogInformation($"Обновление льгот для сотрудника: {name} (ID: {employeeId})");
        
        try
        {
            var existingBenefits = await _context.EmployeeBenefits
                .FirstOrDefaultAsync(b => b.EmployeeId == employeeId);
                
            if (existingBenefits != null)
            {
                // existingBenefits.Position = position;
                // existingBenefits.LastUpdated = DateTime.Now;
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Льготы обновлены для {name}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка обновления льгот для сотрудника {employeeId}");
        }
    }
}

public class KafkaConsumerService : BackgroundService
{
    private readonly KafkaSettings _settings;
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly IBenefitsProcessor _benefitsProcessor;
    private readonly IHostApplicationLifetime _appLifetime;

    public KafkaConsumerService(
        IOptions<KafkaSettings> settings,
        ILogger<KafkaConsumerService> logger,
        IBenefitsProcessor benefitsProcessor,
        IHostApplicationLifetime appLifetime)
    {
        _settings = settings.Value;
        _logger = logger;
        _benefitsProcessor = benefitsProcessor;
        _appLifetime = appLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        await WaitForAppStartup(_appLifetime, stoppingToken);
        
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        
        _logger.LogInformation("Kafka Consumer запущен");

        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnablePartitionEof = true,
            MaxPollIntervalMs = 300000, 
            SessionTimeoutMs = 10000
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(_settings.Topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {

                var result = consumer.Consume(TimeSpan.FromMilliseconds(1000));
                
                if (result != null && !result.IsPartitionEOF)
                {
                    _logger.LogDebug($"Получено сообщение: {result.Message.Value}");

                    _ = Task.Run(() => ProcessMessageAsync(result.Message.Value), stoppingToken);
                    
                    consumer.Commit(result);
                }
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, $"Ошибка потребления: {ex.Error.Reason}");
                await Task.Delay(1000, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неожиданная ошибка");
                await Task.Delay(1000, stoppingToken);
            }
        }

        consumer.Close();
        _logger.LogInformation("Kafka Consumer остановлен");
    }

    private async Task ProcessMessageAsync(string jsonMessage)
    {
        try
        {
            var jsonDoc = JsonDocument.Parse(jsonMessage);
            var root = jsonDoc.RootElement;

            if (root.TryGetProperty("EventType", out var eventType))
            {
                var employeeId = root.GetProperty("EmployeeId").GetInt32();
                var name = root.GetProperty("Name").GetString();
                var position = root.GetProperty("Position").GetString();

                switch (eventType.GetString())
                {
                    case "EmployeeCreated":
                        await _benefitsProcessor.ProcessEmployeeCreated(employeeId, name, position);
                        break;
                    case "EmployeeUpdated":
                        await _benefitsProcessor.ProcessEmployeeUpdated(employeeId, name, position);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка обработки сообщения");
        }
    }


    private static async Task WaitForAppStartup(IHostApplicationLifetime lifetime, CancellationToken stoppingToken)
    {
        var startSource = new TaskCompletionSource<bool>();
        using var registration = lifetime.ApplicationStarted.Register(() => startSource.SetResult(true));
        await startSource.Task.WaitAsync(stoppingToken);
    }
}