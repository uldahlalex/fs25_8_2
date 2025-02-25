using System.Reflection;
using Api.Websocket.Documentation;
using EFScaffold;
using EFScaffold.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using StackExchange.Redis;
using Startup.Extensions;
using WebSocketBoilerplate;

namespace Api;

public class Program
{
    public static void Main()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddOptionsWithValidateOnStart<AppOptions>()
            .Bind(builder.Configuration.GetSection(nameof(AppOptions)));
        var appOptions = builder.Services.BuildServiceProvider()
            .GetRequiredService<IOptionsMonitor<AppOptions>>()
            .CurrentValue;

        builder.Services.AddDbContext<KahootContext>(options =>
        {
            options.UseNpgsql(appOptions.DbConnectionString);
            options.EnableSensitiveDataLogging();
        });

        var redisConfig = new ConfigurationOptions
        {
            AbortOnConnectFail = false,
            ConnectTimeout = 5000,
            SyncTimeout = 5000,
            Ssl = true,
            DefaultDatabase = 0,
            ConnectRetry = 5,
            ReconnectRetryPolicy = new ExponentialRetry(5000),
            EndPoints = { { appOptions.REDIS_HOST, 6379 } },
            User = appOptions.REDIS_USERNAME,
            Password = appOptions.REDIS_PASSWORD
        };
        builder.Services.AddSingleton<IGameTimeProvider, GameTimeProvider>();
        builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConfig));
        builder.Services.AddSingleton<IConnectionManager, RedisConnectionManager>();
        builder.Services.AddSingleton<CustomWebSocketServer>();
        builder.Services.InjectEventHandlers(Assembly.GetExecutingAssembly());
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApiDocument(conf =>
        {
            conf.DocumentProcessors.Add(new AddAllDerivedTypesProcessor());
            conf.DocumentProcessors.Add(new AddStringConstantsProcessor());
        });
        var app = builder.Build();
        app.UseOpenApi();
        app.MapScalarApiReference();
        app.GenerateTypeScriptClient("/../client/src/generated-client.ts").GetAwaiter().GetResult();
        using (var scope = app.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<KahootContext>();
            new Seeder(ctx).SeedTestData().GetAwaiter().GetResult();

        }
        app.Services.GetRequiredService<CustomWebSocketServer>().Start(app);

        app.Run();
    }
}