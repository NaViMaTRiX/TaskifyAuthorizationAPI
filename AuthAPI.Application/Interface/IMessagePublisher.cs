namespace AuthAPI.Application.Interface;

public interface IMessagePublisher
{
    Task PublishAsync<T>(string queueName, T message);
}
