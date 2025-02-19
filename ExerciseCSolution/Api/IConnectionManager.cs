using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fleck;
using WebSocketBoilerplate;

namespace Api;

public interface IConnectionManager
{
    Task<Dictionary<string, string>> GetAllSocketIdsWithConnectionId();
    ConcurrentDictionary<string, IWebSocketConnection> ConnectionIdToSocket { get; }
    ConcurrentDictionary<string, string> SocketToConnectionId { get; }
    public Task<ConcurrentDictionary<string, HashSet<string>>> GetAllTopicsWithMembers();
    public Task<ConcurrentDictionary<string, HashSet<string>>> GetAllMembersWithTopics();
    public Task<Dictionary<string, string>> GetAllConnectionIdsWithSocketId();
    
    Task AddToTopic(string topic, string memberId, TimeSpan? expiry = null);
    Task RemoveFromTopic(string topic, string memberId);
    Task<List<string>> GetMembersFromTopicId(string topic);
    Task<List<string>> GetTopicsFromMemberId(string memberId);
    Task OnOpen(IWebSocketConnection socket, string clientId);
    Task OnClose(IWebSocketConnection socket, string clientId);
    Task BroadcastToTopic<T>(string topic, T message) where T : BaseDto;
}