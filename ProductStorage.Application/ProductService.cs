using FluentValidation;
using ProductStorage.Application.Exceptions;
using ProductStorage.Domain;
using ProductStorage.Infrastructure;

namespace ProductStorage.Application;

public class ProductService(
    IProductRepository repository,
    IValidator<CreateProductRequest> createValidator,
    IValidator<ProductFilter> filterValidator,
    IValidator<UpdateProductRequest> updateValidator) : IProductService
{
    public async Task<PagedResult<Product>> GetPagedAsync(ProductFilter filter, CancellationToken cancellationToken = default)
    {
        await filterValidator.ValidateAndThrowAsync(filter, cancellationToken);
        var (items, totalCount) = await repository.GetPagedAsync(
            filter.CreatedFrom,
            filter.CreatedTo,
            filter.Type,
            filter.WarehouseNumber,
            filter.Page,
            filter.PageSize,
            cancellationToken);
        return new PagedResult<Product>(items, filter.Page, filter.PageSize, totalCount);
    }

    public async Task<Product> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);

        return product ?? throw new ProductNotFoundException(id);
    }

    public async Task<Guid> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var product = await repository.AddAsync(new Product(Guid.NewGuid(), request.Name.Trim(), request.Price,
            request.Weight, request.Type, DateTimeOffset.UtcNow, request.WarehouseNumber), cancellationToken);
        return product.Id;
    }

    public async Task<Product> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        var existing = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new ProductNotFoundException(id);

        var updated = await repository.UpdateAsync(existing with
        {
            Name = request.Name.Trim(),
            Price = request.Price,
            Weight = request.Weight,
            Type = request.Type,
            WarehouseNumber = request.WarehouseNumber
        }, cancellationToken);

        return updated ?? throw new ProductNotFoundException(id);
    }
}
