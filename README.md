Swagger на https://localhost:7006
Также есть консольный grpc клиент

## PostgreSQL

По умолчанию приложение использует connection string `Postgres` из `appsettings.json`.
Создайте локальную базу `product_storage`, затем запустите приложение: миграция FluentMigrator создаст таблицу `products` автоматически.

Пример Docker:

```powershell
docker run --name product-storage-postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=product_storage -p 5432:5432 -d postgres
```

Интеграционные тесты запускают PostgreSQL через Testcontainers и требуют запущенный Docker Desktop:

```powershell
dotnet test ProductStorage.Tests/ProductStorage.Tests.csproj
```
