using FluentValidation;
using ProductStorage.Domain;
using ProductStorage.Infrastructure;

namespace ProductStorage.Application;

public sealed class ProductNotFoundException(Guid id) : Exception($"Product '{id}' was not found.")
{
    public Guid Id { get; } = id;
}

public sealed class ProductService(
    IProductRepository repository,
    IValidator<CreateProductRequest> createValidator,
    IValidator<ProductFilter> filterValidator)
{
    public async Task<PagedResult<Product>> GetPagedAsync(ProductFilter filter, CancellationToken cancellationToken = default)
    {
        await filterValidator.ValidateAndThrowAsync(filter, cancellationToken);
        var products = await repository.GetAllAsync(cancellationToken);
        var query = products.AsEnumerable();
        if (filter.CreatedFrom.HasValue)
            query = query.Where(x => x.CreatedAt >= filter.CreatedFrom.Value);
        if (filter.CreatedTo.HasValue)
            query = query.Where(x => x.CreatedAt <= filter.CreatedTo.Value);
        if (filter.Type.HasValue)
            query = query.Where(x => x.Type == filter.Type.Value);
        if (filter.WarehouseNumber.HasValue)
            query = query.Where(x => x.WarehouseNumber == filter.WarehouseNumber.Value);

        var total = query.Count();
        var items = query.OrderBy(x => x.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToArray();
        return new PagedResult<Product>(items, filter.Page, filter.PageSize, total);
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.GetByIdAsync(id, cancellationToken);

    public async Task<Product> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        return await repository.AddAsync(new Product(Guid.NewGuid(), request.Name.Trim(), request.Price,
            request.Weight, request.Type, DateTimeOffset.UtcNow, request.WarehouseNumber), cancellationToken);
    }
}
