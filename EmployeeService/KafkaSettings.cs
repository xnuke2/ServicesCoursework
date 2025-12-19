using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace EmployeeService;

public class KafkaSettings
    {
        public string BootstrapServers { get; set; }
        public string Topic { get; set; }
    }

    public interface IKafkaProducerService
    {
        Task SendEmployeeCreatedAsync(int id, string name, string position);
        Task SendEmployeeUpdatedAsync(int id, string name, string position);
    }

    public class KafkaProducerService : IKafkaProducerService, IDisposable
    {
        private readonly IProducer<Null, string> _producer;
        private readonly KafkaSettings _settings;
        private readonly ILogger<KafkaProducerService> _logger;

        public KafkaProducerService(
            IOptions<KafkaSettings> settings,
            ILogger<KafkaProducerService> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            var config = new ProducerConfig
            {
                BootstrapServers = _settings.BootstrapServers
            };

            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public async Task SendEmployeeCreatedAsync(int id, string name, string position)
        {
            var message = new
            {
                EventType = "EmployeeCreated",
                EmployeeId = id,
                Name = name,
                Position = position,
                Timestamp = DateTime.Now
            };

            await SendMessageAsync(message);
            _logger.LogInformation($"Отправлено событие создания сотрудника: {name}");
        }

        public async Task SendEmployeeUpdatedAsync(int id, string name, string position)
        {
            var message = new
            {
                EventType = "EmployeeUpdated",
                EmployeeId = id,
                Name = name,
                Position = position,
                Timestamp = DateTime.Now
            };

            await SendMessageAsync(message);
            _logger.LogInformation($"Отправлено событие обновления сотрудника: {name}");
        }

        private async Task SendMessageAsync(object message)
        {
            var json = JsonSerializer.Serialize(message);
            
            try
            {
                var result = await _producer.ProduceAsync(
                    _settings.Topic,
                    new Message<Null, string> { Value = json }
                );

                _logger.LogDebug($"Сообщение отправлено в Kafka: {result.TopicPartitionOffset}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка отправки в Kafka");
            }
        }

        public void Dispose()
        {
            _producer?.Dispose();
        }
    }