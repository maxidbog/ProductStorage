using System.Globalization;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;

const string address = "https://localhost:7007";

using var channel = GrpcChannel.ForAddress(address);
var client = new ProductService.ProductServiceClient(channel);

Console.WriteLine($"Server: {address}");

while (true)
{
    PrintMenu();
    var command = Console.ReadLine()?.Trim();

    if (command is null || command == "0")
        break;

    try
    {
        switch (command)
        {
            case "1":
                await CreateProductAsync(client);
                break;
            case "2":
                await GetProductAsync(client);
                break;
            case "3":
                await ListProductsAsync(client);
                break;
            default:
                Console.WriteLine("Неизвестная команда.");
                break;
        }
    }
    catch (RpcException exception)
    {
        Console.WriteLine($"gRPC error: {exception.StatusCode} - {exception.Status.Detail}");
    }
    catch (Exception exception)
    {
        Console.WriteLine($"Ошибка: {exception.Message}");
    }

    Console.WriteLine();
}

static void PrintMenu()
{
    Console.WriteLine(
        "\nВыберите запрос:" +
        "\n  1. Создать товар" +
        "\n  2. Получить товар по ID" +
        "\n  3. Получить список товаров" +
        "\n  0. Выход");
    Console.Write("Команда: ");
}

static async Task CreateProductAsync(ProductService.ProductServiceClient client)
{
    var request = new CreateProductRequest
    {
        Name = ReadRequiredString("Название: "),
        Price = ReadPositiveDouble("Цена: "),
        Weight = ReadPositiveDouble("Вес: "),
        Type = ReadProductType(),
        WarehouseNumber = ReadPositiveInt("Номер склада: ")
    };

    var product = await client.CreateProductAsync(request);
    Console.WriteLine("Товар создан:");
    PrintProduct(product);
}

static async Task GetProductAsync(ProductService.ProductServiceClient client)
{
    var id = ReadGuid("ID товара: ");
    var product = await client.GetProductAsync(new GetProductRequest { Id = id.ToString() });
    PrintProduct(product);
}

static async Task ListProductsAsync(ProductService.ProductServiceClient client)
{
    var request = new ListProductsRequest
    {
        Page = ReadPositiveInt("Страница (по умолчанию 1): ", 1),
        PageSize = ReadPositiveInt("Размер страницы (по умолчанию 20): ", 20)
    };

    var createdFrom = ReadOptionalDateTimeOffset("Дата создания от (yyyy-MM-dd HH:mm, пусто — без фильтра): ");
    if (createdFrom.HasValue)
        request.CreatedFrom = Timestamp.FromDateTimeOffset(createdFrom.Value);

    var createdTo = ReadOptionalDateTimeOffset("Дата создания до (yyyy-MM-dd HH:mm, пусто — без фильтра): ");
    if (createdTo.HasValue)
        request.CreatedTo = Timestamp.FromDateTimeOffset(createdTo.Value);

    if (ReadYesNo("Фильтровать по типу? (y/n): "))
        request.Type = ReadProductType();

    if (ReadYesNo("Фильтровать по складу? (y/n): "))
        request.WarehouseNumber = ReadPositiveInt("Номер склада: ");

    var response = await client.ListProductsAsync(request);

    Console.WriteLine($"\nСтраница: {response.Page}, размер: {response.PageSize}, всего товаров: {response.TotalCount}");

    if (response.Products.Count == 0)
    {
        Console.WriteLine("Товары не найдены.");
        return;
    }

    foreach (var product in response.Products)
        PrintProduct(product);
}

static ProductType ReadProductType()
{
    while (true)
    {
        Console.WriteLine(
            "Тип товара:" +
            "\n  1. General" +
            "\n  2. HouseholdChemicals" +
            "\n  3. Appliances" +
            "\n  4. Food");
        Console.Write("Тип: ");

        switch (Console.ReadLine()?.Trim())
        {
            case "1":
                return ProductType.General;
            case "2":
                return ProductType.HouseholdChemicals;
            case "3":
                return ProductType.Appliances;
            case "4":
                return ProductType.Food;
            default:
                Console.WriteLine("Выберите тип от 1 до 4.");
                break;
        }
    }
}

static string ReadRequiredString(string message)
{
    while (true)
    {
        Console.Write(message);
        var value = Console.ReadLine()?.Trim();
        if (!string.IsNullOrWhiteSpace(value))
            return value;

        Console.WriteLine("Значение не может быть пустым.");
    }
}

static Guid ReadGuid(string message)
{
    while (true)
    {
        Console.Write(message);
        if (Guid.TryParse(Console.ReadLine(), out var value))
            return value;

        Console.WriteLine("Введите корректный GUID.");
    }
}

static int ReadPositiveInt(string message, int defaultValue = 0)
{
    while (true)
    {
        Console.Write(message);
        var input = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(input) && defaultValue > 0)
            return defaultValue;

        if (int.TryParse(input, out var value) && value > 0)
            return value;

        Console.WriteLine("Введите положительное целое число.");
    }
}

static double ReadPositiveDouble(string message)
{
    while (true)
    {
        Console.Write(message);
        var input = Console.ReadLine()?.Trim()?.Replace(',', '.');
        if (double.TryParse(input, NumberStyles.Float,
                CultureInfo.InvariantCulture, out var value) && value > 0)
            return value;

        Console.WriteLine("Введите положительное число.");
    }
}

static DateTimeOffset? ReadOptionalDateTimeOffset(string message)
{
    while (true)
    {
        Console.Write(message);
        var input = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(input))
            return null;

        if (DateTimeOffset.TryParse(input, out var value))
            return value;

        Console.WriteLine("Введите дату в формате yyyy-MM-dd HH:mm.");
    }
}

static bool ReadYesNo(string message)
{
    while (true)
    {
        Console.Write(message);
        switch (Console.ReadLine()?.Trim().ToLowerInvariant())
        {
            case "y":
                return true;
            case "n":
                return false;
            default:
                Console.WriteLine("Введите y/n");
                break;
        }
    }
}

static void PrintProduct(Product product)
{
    Console.WriteLine(
        $"{product.Id} | {product.Name} | price={product.Price} | weight={product.Weight} | " +
        $"type={product.Type} | warehouse={product.WarehouseNumber} | " +
        $"created={product.CreatedAt.ToDateTimeOffset():O}");
}
