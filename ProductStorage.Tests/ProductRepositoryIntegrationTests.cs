using ProductStorage.Domain;
using ProductStorage.Infrastructure;
using ProductStorage.Infrastructure.Migrations;
using Testcontainers.PostgreSql;
using Xunit;

namespace ProductStorage.Tests;

public class ProductRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithDatabase("product_storage_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        MigrationRunner.RunMigrations(_container.GetConnectionString());
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    [Fact]
    public async Task AddGetUpdateAndPagedQueries_WorkWithPostgres()
    {
        var repository = new ProductRepository(_container.GetConnectionString());
        var product = new Product(Guid.NewGuid(), "Test product", 69m, 1000m,
            ProductType.General, DateTimeOffset.UtcNow, 67);

        await repository.AddAsync(product);
        var found = await repository.GetByIdAsync(product.Id);
        Assert.NotNull(found);

        var updated = found! with { Name = "Updated", WarehouseNumber = 52 };
        Assert.NotNull(await repository.UpdateAsync(updated));

        var page = await repository.GetPagedAsync(
            null, null, ProductType.General, 52, 1, 20);
        Assert.Equal(1, page.TotalCount);
        Assert.Single(page.Items);
        Assert.Equal("Updated", page.Items[0].Name);
    }
}
