using Microsoft.AspNetCore.Mvc;
using ProductStorage.Application;
using ProductStorage.Domain;
using ApplicationCreateProductRequest = ProductStorage.Application.CreateProductRequest;

namespace ProductStorage.Presentation.Controllers;

[ApiController]
[Route("products")]
public class ProductController(IProductService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<Product>>> ListProducts(
        [FromQuery] ProductFilter filter,
        CancellationToken cancellationToken)
    {
        var result = await service.GetPagedAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{productId:guid}")]
    public async Task<ActionResult<Product>> GetProduct(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var product = await service.GetByIdAsync(productId, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(
        [FromBody] ApplicationCreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetProduct), new { productId = product.Id }, product);
    }
}