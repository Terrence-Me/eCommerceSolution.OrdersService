using Microsoft.Extensions.Hosting;

namespace BusinessLogiclayer.RabbitMQ;
public class RabbitMQProductNameUpdateHostedService(IRabbitMQProductNameUpdateConsumer rabbitMQProductNameUpdateConsumer) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        rabbitMQProductNameUpdateConsumer.Consume();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        rabbitMQProductNameUpdateConsumer.Dispose();
        return Task.CompletedTask;
    }
}
