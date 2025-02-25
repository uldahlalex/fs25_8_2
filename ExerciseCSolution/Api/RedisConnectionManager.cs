using System.Collections.Concurrent;
using System.Text.Json;
using Fleck;
using StackExchange.Redis;
using WebSocketBoilerplate;

namespace Api;

public class RedisConnectionManager : IConnectionManager
{
    private const string CONNECTION_TO_SOCKET = "conn:socket:"; // conn:socket:{clientId} -> socketId
    private const string SOCKET_TO_CONNECTION = "socket:conn:"; // socket:conn:{socketId} -> clientId
    private const string TOPIC_MEMBERS = "topic:members:"; // topic:members:{topicId} -> Set<memberId>
    private const string MEMBER_TOPICS = "member:topics:"; // member:topics:{memberId} -> Set<topicId>
    private readonly IDatabase _db;
    private readonly ILogger<RedisConnectionManager> _logger;
    private readonly IConnectionMultiplexer _redis;

    private readonly ConcurrentDictionary<string, IWebSocketConnection> ConnectionIdToSocket = new();

    public RedisConnectionManager(IConnectionMultiplexer redis, ILogger<RedisConnectionManager> logger)
    {
        _redis = redis;
        _db = redis.GetDatabase();
        _logger = logger;

        var server = redis.GetServer(redis.GetEndPoints().First());
        server.Keys(pattern: "*").ToList().ForEach(key => _db.KeyDelete(key));
    }

    public async Task<ConcurrentDictionary<string, HashSet<string>>> GetAllTopicsWithMembers()
    {
        var result = new ConcurrentDictionary<string, HashSet<string>>();
        var server = _redis.GetServer(_redis.GetEndPoints().First());

        var topicKeys = server.Keys(pattern: $"{TOPIC_MEMBERS}*");

        foreach (var key in topicKeys)
        {
            var topic = key.ToString().Replace(TOPIC_MEMBERS, "");
            var members = await _db.SetMembersAsync(key);
            var memberSet = new HashSet<string>(members.Select(m => m.ToString()));
            result.TryAdd(topic, memberSet);
        }

        return result;
    }

    public async Task<ConcurrentDictionary<string, HashSet<string>>> GetAllMembersWithTopics()
    {
        var result = new ConcurrentDictionary<string, HashSet<string>>();
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var keys = server.Keys(pattern: $"{MEMBER_TOPICS}*");

        foreach (var key in keys)
        {
            var member = key.ToString().Replace(MEMBER_TOPICS, "");
            var topics = await _db.SetMembersAsync(key);
            result[member] = new HashSet<string>(topics.Select(t => t.ToString()));
        }

        return result;
    }

    public async Task<Dictionary<string, string>> GetAllConnectionIdsWithSocketId()
    {
        var result = new Dictionary<string, string>();
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var keys = server.Keys(pattern: $"{CONNECTION_TO_SOCKET}*");

        foreach (var key in keys)
        {
            var clientId = key.ToString().Replace(CONNECTION_TO_SOCKET, "");
            var socketId = await _db.StringGetAsync(key);
            if (!socketId.IsNull) result[clientId] = socketId.ToString();
        }

        return result;
    }

    public async Task<Dictionary<string, string>> GetAllSocketIdsWithConnectionId()
    {
        var result = new Dictionary<string, string>();
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var keys = server.Keys(pattern: $"{SOCKET_TO_CONNECTION}*");

        foreach (var key in keys)
        {
            var socketId = key.ToString().Replace(SOCKET_TO_CONNECTION, "");
            var clientId = await _db.StringGetAsync(key);
            if (!clientId.IsNull) result[socketId] = clientId.ToString();
        }

        return result;
    }

    public async Task AddToTopic(string topic, string memberId, TimeSpan? expiry = null)
    {
        if (expiry is null)
            expiry = TimeSpan.FromMinutes(60);

        var tx = _db.CreateTransaction();

        tx.SetAddAsync($"{TOPIC_MEMBERS}{topic}", memberId);
        tx.SetAddAsync($"{MEMBER_TOPICS}{memberId}", topic);

        tx.KeyExpireAsync($"{TOPIC_MEMBERS}{topic}", expiry.Value);
        tx.KeyExpireAsync($"{MEMBER_TOPICS}{memberId}", expiry.Value);

        await tx.ExecuteAsync();
        await LogCurrentState();
    }

    public async Task RemoveFromTopic(string topic, string memberId)
    {
        var tx = _db.CreateTransaction();
        tx.SetRemoveAsync($"{TOPIC_MEMBERS}{topic}", memberId);
        tx.SetRemoveAsync($"{MEMBER_TOPICS}{memberId}", topic);
        await tx.ExecuteAsync();
    }

    public async Task<List<string>> GetMembersFromTopicId(string topic)
    {
        var members = await _db.SetMembersAsync($"{TOPIC_MEMBERS}{topic}");
        return members.Select(m => m.ToString()).ToList();
    }

    public async Task<List<string>> GetTopicsFromMemberId(string memberId)
    {
        var topics = await _db.SetMembersAsync($"{MEMBER_TOPICS}{memberId}");
        return topics.Select(t => t.ToString()).ToList();
    }

    public async Task<string> GetClientIdFromSocketId(string socketId)
    {
        var result = await _db.StringGetAsync($"{SOCKET_TO_CONNECTION}{socketId}");
        return result.ToString();
    }

    public async Task OnOpen(IWebSocketConnection socket, string clientId)
    {
        _logger.LogInformation($"OnOpen called with clientId: {clientId} and socketId: {socket.ConnectionInfo.Id}");

        var socketId = socket.ConnectionInfo.Id.ToString();

        // Clean up old connection if exists
        var oldSocketId = await _db.StringGetAsync($"{CONNECTION_TO_SOCKET}{clientId}");
        if (!oldSocketId.IsNull)
        {
            await _db.KeyDeleteAsync($"{SOCKET_TO_CONNECTION}{oldSocketId}");
            ConnectionIdToSocket.TryRemove(clientId, out _);
            _logger.LogInformation($"Removed old connection {oldSocketId} for client {clientId}");
        }

        // Store new connection
        var tx = _db.CreateTransaction();
        tx.StringSetAsync($"{CONNECTION_TO_SOCKET}{clientId}", socketId);
        tx.StringSetAsync($"{SOCKET_TO_CONNECTION}{socketId}", clientId);
        await tx.ExecuteAsync();

        ConnectionIdToSocket[clientId] = socket;

        _logger.LogInformation($"Added new connection {socketId} for client {clientId}");
        await LogCurrentState();
    }

    public async Task OnClose(IWebSocketConnection socket, string clientId)
    {
        var socketId = socket.ConnectionInfo.Id.ToString();

        // Verify this is still the current socket for this client
        var currentSocketId = await _db.StringGetAsync($"{CONNECTION_TO_SOCKET}{clientId}");
        if (!currentSocketId.IsNull && currentSocketId.ToString() == socketId)
        {
            var tx = _db.CreateTransaction();
            tx.KeyDeleteAsync($"{CONNECTION_TO_SOCKET}{clientId}");
            tx.KeyDeleteAsync($"{SOCKET_TO_CONNECTION}{socketId}");
            await tx.ExecuteAsync();

            ConnectionIdToSocket.TryRemove(clientId, out _);
            _logger.LogInformation($"Removed connection for client {clientId}");

            // Clean up topics
            var topics = await GetTopicsFromMemberId(clientId);
            foreach (var topic in topics) await RemoveFromTopic(topic, clientId);
            await _db.KeyDeleteAsync($"{MEMBER_TOPICS}{clientId}");
        }
    }

    public async Task BroadcastToTopic<T>(string topic, T message) where T : BaseDto
    {
        var members = await GetMembersFromTopicId(topic);
        if (members.Count == 0)
        {
            _logger.LogWarning($"No members found for topic: {topic}");
            return;
        }

        foreach (var memberId in members) await BroadcastToMember(topic, memberId, message);
    }

    private async Task BroadcastToMember<T>(string topic, string memberId, T message) where T : BaseDto
    {
        if (!ConnectionIdToSocket.TryGetValue(memberId, out var socket))
        {
            _logger.LogWarning($"No socket found for member: {memberId}");
            await RemoveFromTopic(topic, memberId);
            return;
        }

        if (!socket.IsAvailable)
        {
            _logger.LogWarning($"Socket not available for {memberId}");
            await RemoveFromTopic(topic, memberId);
            return;
        }

        try
        {
            socket.SendDto(message);
            _logger.LogInformation($"Successfully sent message to {memberId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send message to {memberId}");
            await RemoveFromTopic(topic, memberId);
        }
    }

    public async Task LogCurrentState()
    {
        _logger.LogDebug(JsonSerializer.Serialize(new
        {
            ConnectionIdToSocket = await GetAllConnectionIdsWithSocketId(),
            SocketToConnectionId = await GetAllSocketIdsWithConnectionId(),
            TopicsWithMembers = await GetAllTopicsWithMembers(),
            MembersWithTopics = await GetAllMembersWithTopics()
        }, new JsonSerializerOptions { WriteIndented = true }));
    }
}