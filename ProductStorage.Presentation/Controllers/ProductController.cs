using Microsoft.AspNetCore.Mvc;

namespace ProductStorage.Presentation.Controllers;

public class ProductController(Application.ProductService service) : ControllerBase
{
    [HttpGet("products/{productId}")]
    public async Task<Domain.Product> GetProduct(Guid productId)
    {
        Console.WriteLine("HttpGetProduct");
        var product = await service.GetByIdAsync(productId, CancellationToken.None);
        return product!;
    }
}