using ProductStorage.Domain;

namespace ProductStorage.Application;

public interface IProductService
{
   Task<PagedResult<Product>> GetPagedAsync(ProductFilter filter, CancellationToken cancellationToken);
   Task<Product> GetByIdAsync(Guid id, CancellationToken cancellationToken);
   Task<Guid> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken);
   Task<Product> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken);
}