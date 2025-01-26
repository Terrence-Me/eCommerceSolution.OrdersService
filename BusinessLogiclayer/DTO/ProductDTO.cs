namespace BusinessLogiclayer.DTO;
public record ProductDTO(Guid ProductId, string ProductName, string? Category, double UnitPrice, int QuantityInStock);

