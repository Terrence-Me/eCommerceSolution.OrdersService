namespace BusinessLogiclayer.RabbitMQ;

public interface IRabbitMQProductDeletionConsumer
{
    void Consume();
    void Dispose();
}