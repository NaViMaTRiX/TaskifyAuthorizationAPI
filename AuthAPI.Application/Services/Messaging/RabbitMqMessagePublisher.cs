using System.Text;
using System.Text.Json;
using AuthAPI.Application.Interface;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using IModel = RabbitMQ.Client.IModel;

namespace AuthAPI.Application.Services.Messaging;

public class RabbitMqMessagePublisher : IMessagePublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqMessagePublisher(IConfiguration configuration)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:Host"] ?? string.Empty,
            UserName = configuration["RabbitMQ:Username"] ?? string.Empty,
            Password = configuration["RabbitMQ:Password"] ?? string.Empty
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public Task PublishAsync<T>(string queueName, T message)
    {
        _channel.QueueDeclare(queueName, durable: true, exclusive: false);
        
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        
        _channel.BasicPublish(
            exchange: "",
            routingKey: queueName,
            basicProperties: null,
            body: body
        );

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
