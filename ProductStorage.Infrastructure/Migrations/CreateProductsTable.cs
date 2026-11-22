using FluentMigrator;

namespace ProductStorage.Infrastructure.Migrations;

[Migration(202611210001)]
public class CreateProductsTable : Migration
{
    public override void Up()
    {
        Create.Table("products")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("name").AsString(200).NotNullable()
            .WithColumn("price").AsDecimal(18, 2).NotNullable()
            .WithColumn("weight").AsDecimal(18, 3).NotNullable()
            .WithColumn("type").AsInt32().NotNullable()
            .WithColumn("created_at").AsDateTimeOffset().NotNullable()
            .WithColumn("warehouse_number").AsInt32().NotNullable();

        Create.Index("ix_products_created_at").OnTable("products").OnColumn("created_at").Descending();
        Create.Index("ix_products_warehouse_number").OnTable("products").OnColumn("warehouse_number");
    }

    public override void Down()
    {
        if (Schema.Table("products").Exists())
            Delete.Table("products");
    }
}
