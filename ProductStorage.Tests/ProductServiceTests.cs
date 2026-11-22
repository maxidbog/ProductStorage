using FluentValidation;
using Moq;
using ProductStorage.Application;
using ProductStorage.Application.Exceptions;
using ProductStorage.Domain;
using ProductStorage.Infrastructure;
using Xunit;

namespace ProductStorage.Tests;

public class ProductServiceTests
{
    [Fact]
    public async Task CreateAsync_SavesProductAndReturnsId()
    {
        var repository = new Mock<IProductRepository>();
        repository.Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product product, CancellationToken _) => product);
        var service = CreateService(repository.Object);

        var id = await service.CreateAsync(new CreateProductRequest(
            "  Test product  ", 67m, 2m, ProductType.General, 1), default);

        Assert.NotEqual(Guid.Empty, id);
        repository.Verify(x => x.AddAsync(
            It.Is<Product>(p => p.Name == "Test product" && p.Id == id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsProduct()
    {
        var id = Guid.NewGuid();
        var product = new Product(id, "Test product", 67m, 2m,
            ProductType.General, DateTimeOffset.UtcNow, 1);
        var repository = new Mock<IProductRepository>();
        repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        var service = CreateService(repository.Object);

        var result = await service.GetByIdAsync(id, default);

        Assert.Equal(product, result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ThrowsNotFound()
    {
        var repository = new Mock<IProductRepository>();
        repository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);
        var service = CreateService(repository.Object);

        await Assert.ThrowsAsync<ProductNotFoundException>(
            () => service.GetByIdAsync(Guid.NewGuid(), default));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingProduct()
    {
        var id = Guid.NewGuid();
        var existing = new Product(id, "TestOld", 67m, 1m, ProductType.General,
            DateTimeOffset.UtcNow, 6);
        var repository = new Mock<IProductRepository>();
        repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        repository.Setup(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product product, CancellationToken _) => product);
        var service = CreateService(repository.Object);

        var updated = await service.UpdateAsync(id, new UpdateProductRequest(
            "TestNew", 69m, 2m, ProductType.Appliances, 7), default);

        Assert.Equal("TestNew", updated.Name);
        Assert.Equal(ProductType.Appliances, updated.Type);
        Assert.Equal(69m, updated.Price);
        Assert.Equal(7, updated.WarehouseNumber);
    }

    [Fact]
    public async Task UpdateAsync_WhenMissingBeforeUpdate_ThrowsNotFound()
    {
        var id = Guid.NewGuid();
        var repository = new Mock<IProductRepository>();
        repository.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);
        var service = CreateService(repository.Object);

        var exception = await Assert.ThrowsAsync<ProductNotFoundException>(
            () => service.UpdateAsync(id, new UpdateProductRequest(
                "TestMissing", 404m, 404m, ProductType.Appliances, 404), default));

        Assert.Equal(id, exception.Id);
        repository.Verify(x => x.UpdateAsync(
            It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static ProductService CreateService(IProductRepository repository) => new(
        repository,
        new CreateProductRequestValidator(),
        new ProductFilterValidator(),
        new UpdateProductRequestValidator());
}
