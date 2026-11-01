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
                listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
                listenOptions.UseHttps();
            });
        });
    })
    .Build()
    .Run();