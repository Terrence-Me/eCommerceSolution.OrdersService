namespace BusinessLogiclayer.RabbitMQ;
public record ProductDeletionMessage(Guid ProductId, string? ProductName);
