using Microsoft.Extensions.Hosting;

namespace BusinessLogiclayer.RabbitMQ;
public class RabbitMQProductDeletionHostedService(IRabbitMQProductDeletionConsumer rabbitMQProductDeletionConsumer) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        rabbitMQProductDeletionConsumer.Consume();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        rabbitMQProductDeletionConsumer.Dispose();
        return Task.CompletedTask;
    }
}
