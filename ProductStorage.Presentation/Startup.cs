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
        services.AddControllers();
        services.AddSwaggerGen();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGrpcService<ProductGrpcService>();
            endpoints.MapControllers();
        });
        app.UseSwagger();
        app.UseSwaggerUI();
    }
}