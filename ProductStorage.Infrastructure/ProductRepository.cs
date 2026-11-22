using Dapper;
using Npgsql;
using ProductStorage.Domain;
using System.Text;

namespace ProductStorage.Infrastructure;

public class ProductRepository(string connectionString) : IProductRepository
{
    private class ProductRow
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Weight { get; set; }
        public int Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public int WarehouseNumber { get; set; }

        public Product ToProduct() => new(
            Id,
            Name,
            Price,
            Weight,
            (ProductType)Type,
            new DateTimeOffset(DateTime.SpecifyKind(CreatedAt, DateTimeKind.Utc)),
            WarehouseNumber);
    }
    
    private const string BaseProductSelectSql = 
        """
            SELECT id AS "Id",
                   name AS "Name",
                   price AS "Price",
                   weight AS "Weight",
                   type AS "Type",
                   created_at AS "CreatedAt",
                   warehouse_number AS "WarehouseNumber"
            FROM products
        """;
    
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        var row = await connection.QuerySingleOrDefaultAsync<ProductRow>(
            new CommandDefinition(BaseProductSelectSql + " WHERE id = @Id", new { Id = id }, cancellationToken: cancellationToken));
        return row?.ToProduct();
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        DateTimeOffset? createdFrom,
        DateTimeOffset? createdTo,
        ProductType? type,
        int? warehouseNumber,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var (whereFilters, parameters) = BuildFilter(createdFrom, createdTo, type, warehouseNumber);
        parameters.Add("Offset", (page - 1) * pageSize);
        parameters.Add("PageSize", pageSize);

        var countSql = new StringBuilder("SELECT COUNT(*) FROM products");
        var selectSql = new StringBuilder(BaseProductSelectSql);

        if (!string.IsNullOrEmpty(whereFilters))
        {
            countSql.Append(" WHERE ").Append(whereFilters);
            selectSql.Append(" WHERE ").Append(whereFilters);
        }

        selectSql.Append(" ORDER BY created_at DESC LIMIT @PageSize OFFSET @Offset");

        await using var connection = new NpgsqlConnection(connectionString);
        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql.ToString(), parameters, cancellationToken: cancellationToken));
        var rows = await connection.QueryAsync<ProductRow>(
            new CommandDefinition(
                selectSql.ToString(),
                parameters,
                cancellationToken: cancellationToken));

        return (rows.Select(row => row.ToProduct()).ToArray(), totalCount);
    }

    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        const string sql = 
            """
                INSERT INTO products (id, name, price, weight, type, created_at, warehouse_number)
                VALUES (@Id, @Name, @Price, @Weight, @Type, @CreatedAt, @WarehouseNumber)
            """;

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            product.Id,
            product.Name,
            product.Price,
            product.Weight,
            Type = (int)product.Type,
            product.CreatedAt,
            product.WarehouseNumber
        }, cancellationToken: cancellationToken));
        return product;
    }

    public async Task<Product?> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        const string sql =
            """
                UPDATE products
                SET name = @Name,
                    price = @Price,
                    weight = @Weight,
                    type = @Type,
                    warehouse_number = @WarehouseNumber
                WHERE id = @Id
            """;

        await using var connection = new NpgsqlConnection(connectionString);
        var affected = await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            product.Id,
            product.Name,
            product.Price,
            product.Weight,
            Type = (int)product.Type,
            product.WarehouseNumber
        }, cancellationToken: cancellationToken));

        return affected == 0 ? null : product;
    }
    
    private static (string WhereFilters, DynamicParameters Parameters) BuildFilter(
        DateTimeOffset? createdFrom,
        DateTimeOffset? createdTo,
        ProductType? type,
        int? warehouseNumber)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        if (createdFrom.HasValue)
        {
            conditions.Add("created_at >= @CreatedFrom");
            parameters.Add("CreatedFrom", createdFrom.Value);
        }

        if (createdTo.HasValue)
        {
            conditions.Add("created_at <= @CreatedTo");
            parameters.Add("CreatedTo", createdTo.Value);
        }

        if (type.HasValue)
        {
            conditions.Add("type = @Type");
            parameters.Add("Type", (int)type.Value);
        }

        if (warehouseNumber.HasValue)
        {
            conditions.Add("warehouse_number = @WarehouseNumber");
            parameters.Add("WarehouseNumber", warehouseNumber.Value);
        }

        return (string.Join(" AND ", conditions), parameters);
    }
}