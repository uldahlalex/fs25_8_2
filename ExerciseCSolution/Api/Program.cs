using System.Reflection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
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

        builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConfig));
        builder.Services.AddSingleton<IConnectionManager, RedisConnectionManager>();
        builder.Services.AddSingleton<CustomWebSocketServer>();
        builder.Services.InjectEventHandlers(Assembly.GetExecutingAssembly());
        var app = builder.Build();
        app.Services.GetRequiredService<CustomWebSocketServer>().Start(app);
        app.Run();
    }
}