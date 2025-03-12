using System.ComponentModel.DataAnnotations;
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
        ICollection<ValidationResult> results = new List<ValidationResult>();
        var validated = Validator.TryValidateObject(appOptions, new ValidationContext(appOptions), results, true);
        if(!validated)
            throw new Exception($"hey buddy, alex here. You're probably missing an environment variable / appsettings.json stuff / repo secret on github. Here's the technical error: " +
                                $"{string.Join(", ", results.Select(r => r.ErrorMessage))}");
        
        builder.Services.AddDbContext<KahootContext>(options =>
        {
            options.UseNpgsql(appOptions.DbConnectionString);
            options.EnableSensitiveDataLogging();
        });
        builder.Services.AddScoped<Seeder>();


        builder.Services.AddSingleton<IGameTimeProvider, GameTimeProvider>();
        builder.Services.AddSingleton<IConnectionManager, DictionaryConnectionManager>();
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

        app.Services.GetRequiredService<CustomWebSocketServer>().Start(app);
        app.Urls.Clear();
        app.Urls.Add($"http://*:5000");

        app.Run();
    }
}