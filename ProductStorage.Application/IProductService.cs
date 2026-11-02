using ProductStorage.Domain;

namespace ProductStorage.Application;

public interface IProductService
{
   Task<PagedResult<Product>> GetPagedAsync(ProductFilter filter, CancellationToken cancellationToken);
   Task<Product> GetByIdAsync(Guid id, CancellationToken cancellationToken);
   Task<Guid> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken);
}