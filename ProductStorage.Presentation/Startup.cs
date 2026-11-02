using FluentValidation;
using ProductStorage.Application;
using ProductStorage.Infrastructure;
using ProductStorage.Presentation.Interceptors;
using ProductStorage.Presentation.Middleware;

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
        services.AddScoped<IProductService, Application.ProductService>();
        services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();
        services.AddGrpc(options => options.Interceptors.Add<GrpcLoggingInterceptor>()).AddJsonTranscoding();
        services.AddGrpcReflection();
        services.AddControllers();
        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(type => type.FullName!.Replace("+", "."));
        });
        services.AddGrpcSwagger();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseRouting();
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGrpcService<ProductGrpcService>();
            endpoints.MapGrpcReflectionService();
            endpoints.MapControllers();
            endpoints.MapGet("/", () => Results.Redirect("/swagger"));
        });
    }
}