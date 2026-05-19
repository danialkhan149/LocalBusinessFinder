using LocalBusinessFinder.Constants;
using LocalBusinessFinder.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace LocalBusinessFinder.Hubs;

[Authorize]
public class TrackingHub(RequestService requestService) : Hub
{
    public async Task JoinTracking(int requestId)
    {
        if (!await CanAccessAsync(requestId))
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, TrackGroup(requestId));
    }

    public async Task UpdateLocation(int requestId, double latitude, double longitude)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !await CanAccessAsync(requestId)) return;

        await requestService.SaveLocationPingAsync(requestId, userId, latitude, longitude);
        await Clients.Group(TrackGroup(requestId)).SendAsync("LocationUpdated", new
        {
            userId,
            latitude,
            longitude,
            timestamp = DateTime.UtcNow
        });
    }

    private async Task<bool> CanAccessAsync(int requestId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return false;
        var isAdmin = Context.User?.IsInRole(AppRoles.Admin) == true;
        return await requestService.CanUserAccessAsync(requestId, userId, isAdmin);
    }

    private static string TrackGroup(int requestId) => $"track-{requestId}";
}
