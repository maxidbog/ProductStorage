using ProductStorage.Domain;

namespace ProductStorage.Infrastructure;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        DateTimeOffset? createdFrom,
        DateTimeOffset? createdTo,
        ProductType? type,
        int? warehouseNumber,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default);
    Task<Product?> UpdateAsync(Product product, CancellationToken cancellationToken = default);
}