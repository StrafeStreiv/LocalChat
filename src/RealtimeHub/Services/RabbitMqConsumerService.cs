using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RealtimeHub.Hubs;
using Microsoft.Extensions.Logging;
using System;

namespace RealtimeHub.Services
{
    public class RabbitMqConsumerService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<RabbitMqConsumerService> _logger;
        private IConnection _connection;
        private IModel _channel;
        private const string _queueName = "chat_messages";
        private bool _rabbitMqEnabled = true;

        public RabbitMqConsumerService(IConfiguration configuration, IHubContext<ChatHub> hubContext, ILogger<RabbitMqConsumerService> logger)
        {
            _configuration = configuration;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RabbitMQ Consumer Service starting...");

            // Пытаемся подключиться к RabbitMQ с retry
            await TryConnectToRabbitMQWithRetry(stoppingToken);

            if (!_rabbitMqEnabled)
            {
                _logger.LogWarning("RabbitMQ is disabled. Service will run without message queue support.");
                // Сервис продолжает работу, но без RabbitMQ
                await Task.Delay(Timeout.Infinite, stoppingToken);
                return;
            }

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation($"Received message from RabbitMQ: {message}");

                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", "System", message);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing RabbitMQ message: {ex.Message}");
                }
            };

            _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
            _logger.LogInformation("RabbitMQ Consumer Service started successfully");

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task TryConnectToRabbitMQWithRetry(CancellationToken stoppingToken)
        {
            const int maxRetries = 5;
            const int retryDelaySeconds = 10;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var factory = new ConnectionFactory()
                    {
                        HostName = _configuration["RabbitMQ:Host"],
                        UserName = _configuration["RabbitMQ:Username"],
                        Password = _configuration["RabbitMQ:Password"],
                        DispatchConsumersAsync = true,
                        AutomaticRecoveryEnabled = true,
                        NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                    };

                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();
                    _channel.QueueDeclare(
                        queue: _queueName,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    _logger.LogInformation($"Successfully connected to RabbitMQ on attempt {attempt}");
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to connect to RabbitMQ (attempt {attempt}/{maxRetries}): {ex.Message}");

                    if (attempt == maxRetries)
                    {
                        _logger.LogError("Max retry attempts reached. RabbitMQ will be disabled.");
                        _rabbitMqEnabled = false;
                        return;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds), stoppingToken);
                }
            }
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}