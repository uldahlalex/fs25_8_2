using System.Text.Json;
using Api;
using Api.EventHandlers;
using Api.EventHandlers.Dtos;
using EFScaffold;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using WebSocketBoilerplate;

namespace NUnit;

[TestFixture(typeof(RedisConnectionManager))]
[TestFixture(typeof(DictionaryConnectionManager))]
public class ConnectionWithWsClient(Type connectionManagerType) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.Remove(services.SingleOrDefault(d => d.ServiceType == typeof(IConnectionManager)) ??
                            throw new Exception("Could not find instance of " + nameof(IConnectionManager)));
            services.AddSingleton(typeof(IConnectionManager), connectionManagerType);
        });
    }

    [Theory]
    public async Task Api_Can_Successfully_Add_Connection()
    {
        _ = CreateClient();
        var wsPort = Environment.GetEnvironmentVariable("PORT");

        if (string.IsNullOrEmpty(wsPort)) throw new Exception("Environment variable WS_PORT is not set");

        var clientId = "client" + Guid.NewGuid();
        var url = "ws://localhost:" + wsPort + "?id=" + clientId;


        var client = new WsRequestClient(
            new[] { typeof(ClientWantsToAuthenticateDto).Assembly },
            url
        );

        await client.ConnectAsync();

        await Task.Delay(1000);

        var manager = Services.GetRequiredService<IConnectionManager>();

        var pairForClientId = manager.GetAllConnectionIdsWithSocketId().Result.First(pair => pair.Key == clientId);

        if (pairForClientId.Key != clientId && pairForClientId.Value.Length > 5)
            throw new Exception("ConnectionIdToSocket should have client ID key and a socket ID, but state was: " +
                                "" + JsonSerializer.Serialize(await manager.GetAllConnectionIdsWithSocketId()));
        if (manager.GetAllSocketIdsWithConnectionId().Result.Keys.Count != 1)
            throw new Exception("SocketToConnectionId should have 1 value, but state was: " +
                                "" + JsonSerializer.Serialize(await manager.GetAllSocketIdsWithConnectionId()));
    }

    [Theory]
    public async Task Client_Can_Join_Room_Using_Topic_Subscription()
    {
        _ = CreateClient();
        var wsPort = Environment.GetEnvironmentVariable("PORT");

        if (string.IsNullOrEmpty(wsPort)) throw new Exception("Environment variable WS_PORT is not set");

        var clientId = "client" + Guid.NewGuid();
        var url = "ws://localhost:" + wsPort + "?id=" + clientId;


        var client = new WsRequestClient(
            new[] { typeof(ClientWantsToAuthenticateDto).Assembly },
            url
        );

        await client.ConnectAsync();
        await Task.Delay(1000);

        var roomId = "Room" + Guid.NewGuid();


        var dto = new ClientWantsToSubscribeToTopicDto
        {
            TopicId = roomId,
            requestId = Guid.NewGuid().ToString()
        };
        var result = await client.SendMessage<ClientWantsToSubscribeToTopicDto, ServerConfirmsDto>(dto);

        if (result.Success == false)
            throw new Exception("The response DTO should indicate success when joining a room");

        var manager = Services.GetRequiredService<IConnectionManager>();
        var memberDictionaryEntry = manager.GetAllMembersWithTopics().Result.First(key => key.Key == clientId);
        if (memberDictionaryEntry.Value.First() != roomId)
            throw new Exception(
                "Exepected " + roomId + " to be in the hashset: " + memberDictionaryEntry.Value.ToList());

        var topicDictionaryEntry = manager.GetAllTopicsWithMembers().Result.First(key => key.Key == roomId);
        if (topicDictionaryEntry.Value.First() != clientId)
            throw new Exception("Expected " + clientId + " to be in the hashset: " +
                                memberDictionaryEntry.Value.ToList());
    }


    [Theory]
    public async Task Client_Can_Start_Game()
    {
        _ = CreateClient();
        var wsPort = Environment.GetEnvironmentVariable("PORT");

        if (string.IsNullOrEmpty(wsPort)) throw new Exception("Environment variable WS_PORT is not set");

        var clientId = "client" + Guid.NewGuid();
        var url = "ws://localhost:" + wsPort + "?id=" + clientId;


        var client = new WsRequestClient(
            new[] { typeof(ClientWantsToAuthenticateDto).Assembly },
            url
        );

        await client.ConnectAsync();
        
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<KahootContext>();

        var startGameDto = new ClientWantsToStartAGameDto()
        {
            requestId = Guid.NewGuid().ToString(),
            TemplateId = dbContext.Gametemplates.First().Id,
        };
        var result = await client.SendMessage<ClientWantsToStartAGameDto, ServerAddsClientToGameDto>(startGameDto, 20);
        if (result.GameId.Length != 36)
            throw new Exception("Response DTO's Game ID is invalid: Probably did not correctly create game");
        var manager = Services.GetRequiredService<IConnectionManager>();
        var state = await manager.GetAllTopicsWithMembers();
        var gameKey = state.Keys.FirstOrDefault(key => key == "games/" + result.GameId) 
                ?? throw new Exception("Did not correctly add games/"+result.GameId +" to state!");
        var clientIdInState = state[gameKey].ToList().FirstOrDefault(c => c == clientId)
            ?? throw new Exception("Client is not foud in state!");
    }
    
    [Theory]
    public async Task Client_Can_Start_Game_And_Start_Question()
    {
        _ = CreateClient();
        var wsPort = Environment.GetEnvironmentVariable("PORT");

        if (string.IsNullOrEmpty(wsPort)) throw new Exception("Environment variable WS_PORT is not set");

        var clientId = "client" + Guid.NewGuid();
        var url = "ws://localhost:" + wsPort + "?id=" + clientId;


        var client = new WsRequestClient(
            new[] { typeof(ClientWantsToAuthenticateDto).Assembly },
            url
        );

        await client.ConnectAsync();
        
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<KahootContext>();

        var startGameDto = new ClientWantsToStartAGameDto()
        {
            requestId = Guid.NewGuid().ToString(),
            TemplateId = dbContext.Gametemplates.First().Id,
        };
        var startGameResult = await client.SendMessage<ClientWantsToStartAGameDto, ServerAddsClientToGameDto>(startGameDto);

        var startQuestionPhaseDto = new ClientWantsToGoToQuestionPhaseDto()
        {
            requestId = Guid.NewGuid().ToString(),
            GameId = startGameResult.GameId,
        };

      
            await client.SendMessage(startQuestionPhaseDto);
  
            // client.ReceivedMessages
        

    }
}