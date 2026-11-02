using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Enum = System.Enum;
using ProductStorage.Application;
using ProductStorage.Application.Exceptions;

namespace ProductStorage.Presentation;

public class ProductGrpcService(IProductService service) : ProductService.ProductServiceBase
{
    public override async Task<Product> CreateProduct(CreateProductRequest request, ServerCallContext context)
    {
        var product = await service.CreateAsync(new Application.CreateProductRequest(
            request.Name, (decimal)request.Price, (decimal)request.Weight,
            ToDomainType(request.Type), request.WarehouseNumber), context.CancellationToken);
        return ToGrpc(product);
    }

    public override async Task<Product> GetProduct(GetProductRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "id must be a valid GUID."));

        try
        {
            var product = await service.GetByIdAsync(id, context.CancellationToken);
            return ToGrpc(product);
        }
        catch (ProductNotFoundException exception)
        {
            throw new RpcException(new Status(StatusCode.NotFound, exception.Message));
        }
    }

    public override async Task<ListProductsResponse> ListProducts(ListProductsRequest request, ServerCallContext context)
    {
        var result = await service.GetPagedAsync( new ProductFilter(
            request.CreatedFrom?.ToDateTimeOffset(),
            request.CreatedTo?.ToDateTimeOffset(),
            request.Type == ProductType.Unknown ? null : ToDomainType(request.Type),
            request.WarehouseNumber <= 0 ? null : request.WarehouseNumber,
            request.Page <= 0 ? 1 : request.Page,
            request.PageSize <= 0 ? 20 : request.PageSize), context.CancellationToken);
        
        var response = new ListProductsResponse
        {
          Products  = {result.Items.Select(ToGrpc)}, Page = result.Page, PageSize = result.PageSize, TotalCount = result.TotalCount
        };
        return response;
    }
    
    private static Product ToGrpc(Domain.Product product) => new()
    {
        Id = product.Id.ToString(),
        Name = product.Name,
        Price = (double)product.Price,
        Weight = (double)product.Weight,
        Type = (ProductType)(int)product.Type,
        CreatedAt = Timestamp.FromDateTimeOffset(product.CreatedAt),
        WarehouseNumber = product.WarehouseNumber
    };
    
    private static Domain.ProductType ToDomainType(ProductType type) =>
        Enum.IsDefined(typeof(Domain.ProductType), (int)type)
            ? (Domain.ProductType)(int)type
            : Domain.ProductType.Unknown;
}