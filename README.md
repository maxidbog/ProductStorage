REST и gRPC API для работы с товарами на ASP.NET Core.

## Требования

- .NET 9
- Docker Desktop

## Запуск PostgreSQL

Файл docker-compose.yml находится в корне проекта. Он создаёт базу
product_storage.

```powershell
docker compose up -d postgres
```

Остановка:

```powershell
docker compose stop postgres
```

Удаление контейнера и данных:

```powershell
docker compose down -v
```

Адрес postgres 127.0.0.1:5432.

При запуске MigrationRunner применяет все миграции и создаёт
таблицу products, если её нет.

- Swagger: https://localhost:7006
- REST API: https://localhost:7006
- gRPC API: https://localhost:7007

## gRPC-клиент

Консольный клиент запускается отдельно:

```powershell
dotnet run --project ProductStorage.GrpcClient
```

Запускайте приложение и gRPC-клиент одновременно.

## Подключение к базе

Строка подключения берётся из `ConnectionStrings:Postgres` в
`ProductStorage.Presentation/appsettings.json`. Данные пользователя и базы должны
совпадать с настройками `docker-compose.yml`.

## Тесты

Интеграционные тесты запускают отдельный контейнер через Testcontainers,
поэтому Docker Desktop должен быть запущен.
