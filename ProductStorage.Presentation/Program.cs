using Microsoft.AspNetCore.Server.Kestrel.Core;
using ProductStorage.Presentation;

Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(builder =>
    {
        builder.UseStartup<Startup>();
        builder.ConfigureKestrel(options =>
        {
            options.ListenLocalhost(7006, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                listenOptions.UseHttps();
            });
            options.ListenLocalhost(7007, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
                listenOptions.UseHttps();
            });
        });
    })
    .Build()
    .Run();