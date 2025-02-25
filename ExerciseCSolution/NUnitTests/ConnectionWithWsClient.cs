using System.Text.Json;
using System.Text.Json.Serialization;
using Api;
using Api.EventHandlers;
using Api.EventHandlers.Dtos;
using EFScaffold;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WebSocketBoilerplate;

namespace NUnit;

[TestFixture(typeof(RedisConnectionManager))]
[TestFixture(typeof(DictionaryConnectionManager))]
public class ConnectionWithWsClient(Type connectionManagerType) : WebApplicationFactory<Program>
{
    private ILogger<ConnectionWithWsClient> _logger;
    private HttpClient _httpClient;
    private KahootContext _dbContext;
    private IConnectionManager _connectionManager;
    private string _wsClientId;
    private WsRequestClient _wsClient;
    private IServiceScope _scope;


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.Remove(services.SingleOrDefault(d => d.ServiceType == typeof(IConnectionManager)) ??
                            throw new Exception("Could not find instance of " + nameof(IConnectionManager)));
            services.AddSingleton(typeof(IConnectionManager), connectionManagerType);

            services.Remove(services.SingleOrDefault(d => d.ServiceType == typeof(IGameTimeProvider)) ??
                            throw new Exception("Could not find instance of " + nameof(IGameTimeProvider)));
            var mockGameTimeProvider = new Mock<IGameTimeProvider>();
            //set default milliseconds to 2000
            mockGameTimeProvider.Setup(s => s.MilliSeconds).Returns(2000);

            services.AddSingleton<IGameTimeProvider>(mockGameTimeProvider.Object);
        });
    }

    [SetUp]
    public async Task Setup()
    {
        _httpClient = CreateClient();

        //Singletons
        _logger = Services.GetRequiredService<ILogger<ConnectionWithWsClient>>();
        _connectionManager = Services.GetRequiredService<IConnectionManager>();

        //Scoped services
        using var scope = Services.CreateScope();
        {
            _scope = Services.CreateScope();
            _dbContext = _scope.ServiceProvider.GetRequiredService<KahootContext>();
        }


        var wsPort = Environment.GetEnvironmentVariable("PORT");
        if (string.IsNullOrEmpty(wsPort)) throw new Exception("Environment variable WS_PORT is not set");
        _wsClientId = "client" + Guid.NewGuid();
        var url = "ws://localhost:" + wsPort + "?id=" + _wsClientId;
        _wsClient = new WsRequestClient(
            new[] { typeof(ClientWantsToAuthenticateDto).Assembly },
            url
        );
        await _wsClient.ConnectAsync();
        await Task.Delay(1000);
    }


    [Theory]
    public async Task Api_Can_Successfully_Add_Connection()
    {
        var pairForClientId = _connectionManager.GetAllConnectionIdsWithSocketId().Result
            .First(pair => pair.Key == _wsClientId);
        if (pairForClientId.Key != _wsClientId && pairForClientId.Value.Length > 5)
            throw new Exception("ConnectionIdToSocket should have client ID key and a socket ID, but state was: " +
                                "" + JsonSerializer.Serialize(
                                    await _connectionManager.GetAllConnectionIdsWithSocketId()));
        if (_connectionManager.GetAllSocketIdsWithConnectionId().Result.Keys.Count != 1)
            throw new Exception("SocketToConnectionId should have 1 value, but state was: " +
                                "" + JsonSerializer.Serialize(
                                    await _connectionManager.GetAllSocketIdsWithConnectionId()));
    }

    [Theory]
    public async Task Client_Can_Join_Room_Using_Topic_Subscription()
    {
        var roomId = "Room" + Guid.NewGuid();


        var dto = new ClientWantsToSubscribeToTopicDto
        {
            TopicId = roomId,
            requestId = Guid.NewGuid().ToString()
        };
        var result = await _wsClient.SendMessage<ClientWantsToSubscribeToTopicDto, ServerConfirmsDto>(dto);

        if (result.Success == false)
            throw new Exception("The response DTO should indicate success when joining a room");

        var manager = Services.GetRequiredService<IConnectionManager>();
        var memberDictionaryEntry =
            _connectionManager.GetAllMembersWithTopics().Result.First(key => key.Key == _wsClientId);
        if (memberDictionaryEntry.Value.First() != roomId)
            throw new Exception(
                "Exepected " + roomId + " to be in the hashset: " + memberDictionaryEntry.Value.ToList());

        var topicDictionaryEntry = _connectionManager.GetAllTopicsWithMembers().Result.First(key => key.Key == roomId);
        if (topicDictionaryEntry.Value.First() != _wsClientId)
            throw new Exception("Expected " + _wsClientId + " to be in the hashset: " +
                                memberDictionaryEntry.Value.ToList());
    }


    // [Theory]
    // public async Task Client_Can_Start_Game()
    // {
    //     var startGameDto = new AdminWantsToStartGameDto()
    //     {
    //         requestId = Guid.NewGuid().ToString(),
    //     };
    //     var result =
    //         await _wsClient.SendMessage<AdminWantsToStartGameDto, ServerAddsClientToGameDto>(startGameDto, 20);
    //     if (result.GameId.Length != 36)
    //         throw new Exception("Response DTO's Game ID is invalid: Probably did not correctly create game");
    //     var manager = Services.GetRequiredService<IConnectionManager>();
    //     var state = await _connectionManager.GetAllTopicsWithMembers();
    //     var gameKey = state.Keys.FirstOrDefault(key => key == "games/" + result.GameId)
    //                   ?? throw new Exception("Did not correctly add games/" + result.GameId + " to state!");
    //     var clientIdInState = state[gameKey].ToList().FirstOrDefault(c => c == _wsClientId)
    //                           ?? throw new Exception("Client is not foud in state!");
    // }
    //
    // [Theory]
    // public async Task Client_Can_Start_Game_And_Start_Question()
    // {
    //     var startGameDto = new AdminWantsToStartGameDto()
    //     {
    //         requestId = Guid.NewGuid().ToString(),
    //     };
    //     var startGameResult =
    //         await _wsClient.SendMessage<AdminWantsToStartGameDto, ServerAddsClientToGameDto>(startGameDto);
    //
    //     var startQuestionPhaseDto = new ClientWantsToGoToQuestionPhaseDto()
    //     {
    //         requestId = Guid.NewGuid().ToString(),
    //         GameId = startGameResult.GameId,
    //     };
    //
    //
    //     await _wsClient.SendMessage(startQuestionPhaseDto);
    //     await Task.Delay(1000);
    //
    //     if (!_wsClient.ReceivedMessages.Any(m => m.eventType == nameof(ServerSendsQuestionDto).Replace("Dto", "")))
    //         throw new Exception("Did not receive any dto of type " + nameof(ServerSendsQuestionDto) +
    //                             ", all received messages: " + JsonSerializer.Serialize(_wsClient.ReceivedMessages));
    // }
    //
    // [Theory]
    // public async Task Client_Can_Start_Game_And_Start_Question_And_Answer_Question()
    // {
    //     var startGameDto = new AdminWantsToStartGameDto()
    //     {
    //         requestId = Guid.NewGuid().ToString(),
    //     };
    //     var startGameResult =
    //         await _wsClient.SendMessage<AdminWantsToStartGameDto, ServerAddsClientToGameDto>(startGameDto);
    //
    //     var startQuestionPhaseDto = new ClientWantsToGoToQuestionPhaseDto()
    //     {
    //         requestId = Guid.NewGuid().ToString(),
    //         GameId = startGameResult.GameId,
    //     };
    //
    //
    //     await _wsClient.SendMessage(startQuestionPhaseDto);
    //     await Task.Delay(1000);
    //
    //     var serverSendsQuestionDto = _wsClient.GetMessagesOfType<ServerSendsQuestionDto>().First();
    //
    //     var answer = new ClientAnswersQuestionDto()
    //     {
    //         requestId = Guid.NewGuid().ToString(),
    //         gameId = startGameResult.GameId,
    //         optionId = serverSendsQuestionDto.Question.QuestionOptions.First().Id,
    //         questionId = serverSendsQuestionDto.Question.Id
    //     };
    //     var answerResult = await _wsClient.SendMessage<ClientAnswersQuestionDto, ServerConfirmsDto>(answer);
    //     if (answerResult.Success != true)
    //         throw new Exception("Success property indicates a failure to answer the question");
    //     await Task.Delay(Services.GetRequiredService<IGameTimeProvider>().MilliSeconds*2);
    //     var serverEndsGameRoundDto = _wsClient.GetMessagesOfType<ServerEndsGameRoundDto>().First();
    //     _logger.LogInformation("Result: "+JsonSerializer.Serialize(serverEndsGameRoundDto, new JsonSerializerOptions()
    //     {
    //         ReferenceHandler = ReferenceHandler.IgnoreCycles,
    //         WriteIndented = true
    //     }));
    //     if (serverEndsGameRoundDto.GameStateDto.Questions.First().PlayerAnswers.Count != 2)
    //         throw new Exception("There should be exactly 2 answers. State:"+nameof(ServerEndsGameRoundDto)+" contains state: " +
    //                             ""+JsonSerializer.Serialize(serverEndsGameRoundDto, new JsonSerializerOptions()
    //                             {
    //                                 WriteIndented = true
    //                             }));
    //
    // }
}
