using Microsoft.AspNetCore.SignalR;
using TrelloJiraCore.Core.Entities;

namespace TrelloJiraCore.Web.Hubs;

public class BoardHub : Hub
{
    public async Task JoinBoard(int boardId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Board_{boardId}");
    }

    public async Task LeaveBoard(int boardId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Board_{boardId}");
    }

    public async Task CardMoved(int boardId, int cardId, int fromListId, int toListId)
    {
        await Clients.Group($"Board_{boardId}").SendAsync("OnCardMoved", cardId, fromListId, toListId);
    }

    public async Task CardUpdated(int boardId, int cardId)
    {
        await Clients.Group($"Board_{boardId}").SendAsync("OnCardUpdated", cardId);
    }
}
