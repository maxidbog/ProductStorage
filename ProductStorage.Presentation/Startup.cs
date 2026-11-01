using FluentValidation;
using ProductStorage.Application;
using ProductStorage.Infrastructure;

namespace ProductStorage.Presentation;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IProductRepository, ProductRepository>();
        services.AddScoped<ProductStorage.Application.ProductService>();
        services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();
        services.AddGrpc();
        services.AddGrpcReflection();
        services.AddControllers();
        services.AddSwaggerGen(options =>
            options.CustomSchemaIds(type => type.FullName!.Replace("+", ".")));
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGrpcService<ProductGrpcService>();
            endpoints.MapGrpcReflectionService();
            endpoints.MapControllers();
        });
    }
}