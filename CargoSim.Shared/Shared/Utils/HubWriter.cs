using Microsoft.AspNetCore.SignalR;
using Shared.Hubs;

namespace Shared.Utils;

public static class HubWriter
{
    public static async Task Write(IHubContext<MessageHub> hubContext,string method, string message)
    {
        await hubContext.Clients.All.SendAsync(method, message);
    }
}
