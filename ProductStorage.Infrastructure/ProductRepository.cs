using System.Collections.Concurrent;
using ProductStorage.Domain;

namespace ProductStorage.Infrastructure;

public class ProductRepository : IProductRepository
{
    private readonly ConcurrentDictionary<Guid, Product> products = new();

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        products.TryGetValue(id, out var product);
        return Task.FromResult(product);
    }

    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<Product>>(products.Values.ToArray());
    }

    public Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!products.TryAdd(product.Id, product))
            throw new InvalidOperationException($"Product {product.Id} already exists.");
        return Task.FromResult(product);
    }

}