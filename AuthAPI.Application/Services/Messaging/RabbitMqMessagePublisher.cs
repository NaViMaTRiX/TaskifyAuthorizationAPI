using System.Text;
using System.Text.Json;
using AuthAPI.Application.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using IModel = RabbitMQ.Client.IModel;

namespace AuthAPI.Application.Services.Messaging;

public class RabbitMqMessagePublisher : IMessagePublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqMessagePublisher> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RabbitMqMessagePublisher(IConfiguration configuration, ILogger<RabbitMqMessagePublisher> logger)
    {
        _logger = logger;
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:Host"] ?? throw new ArgumentNullException(nameof(configuration), "RabbitMQ host is not configured"),
                UserName = configuration["RabbitMQ:Username"] ?? throw new ArgumentNullException(nameof(configuration), "RabbitMQ username is not configured"),
                Password = configuration["RabbitMQ:Password"] ?? throw new ArgumentNullException(nameof(configuration), "RabbitMQ password is not configured"),
                DispatchConsumersAsync = true 
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            _logger.LogInformation("RabbitMQ connection established successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка подключения к RabbitMQ.");
            throw;
        }
    }

    public async Task PublishAsync<T>(string queueName, T message)
    {
        if (string.IsNullOrWhiteSpace(queueName))
        {
            _logger.LogError("Ошибка: имя очереди не может быть пустым.");
            throw new ArgumentException("Queue name cannot be empty.", nameof(queueName));
        }

        if (message == null)
        {
            _logger.LogError("Ошибка: сообщение не может быть null.");
            throw new ArgumentNullException(nameof(message), "Message cannot be null.");
        }

        try
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message, _jsonOptions));

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true; 

            _channel.BasicPublish(
                exchange: "",
                routingKey: queueName,
                basicProperties: properties,
                body: body
            );

            _logger.LogInformation("Сообщение отправлено в очередь {QueueName}.", queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке сообщения в очередь {QueueName}.", queueName);
            throw;
        }

        await ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        try
        {
            _channel.Close();
            _channel.Dispose();
            _connection.Close();
            _connection.Dispose();

            _logger.LogInformation("RabbitMQ соединение закрыто.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при закрытии соединения RabbitMQ.");
        }
    }
}

