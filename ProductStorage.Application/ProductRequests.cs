using ProductStorage.Domain;

namespace ProductStorage.Application;

public record CreateProductRequest(
    string Name,
    decimal Price,
    decimal Weight,
    ProductType Type,
    int WarehouseNumber);

public record ProductFilter(
    DateTimeOffset? CreatedFrom = null,
    DateTimeOffset? CreatedTo = null,
    ProductType? Type = null,
    int? WarehouseNumber = null,
    int Page = 1,
    int PageSize = 20);
public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
