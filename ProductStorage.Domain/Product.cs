namespace ProductStorage.Domain;

public record Product(
    Guid Id,
    string Name,
    decimal Price,
    decimal Weight,
    ProductType Type,
    DateTimeOffset CreatedAt,
    int WarehouseNumber);