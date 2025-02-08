namespace BusinessLogiclayer.RabbitMQ;
public record ProductNameUpdateMessage(Guid ProductId, string NewName);
