using LocalBusinessFinder.Data;
using LocalBusinessFinder.Models;
using LocalBusinessFinder.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace LocalBusinessFinder.Hubs;

[Authorize]
public class ChatHub(
    RequestService requestService,
    UserManager<ApplicationUser> userManager) : Hub
{
    public async Task JoinRequest(int requestId)
    {
        if (!await CanAccessAsync(requestId))
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, RequestGroup(requestId));
    }

    public async Task SendMessage(int requestId, string content, bool isOffer = false, decimal? offerAmount = null)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !await CanAccessAsync(requestId)) return;

        var message = new ChatMessage
        {
            ServiceRequestId = requestId,
            SenderId = userId,
            Content = content,
            IsOffer = isOffer,
            OfferAmount = offerAmount
        };
        var saved = await requestService.SendMessageAsync(message);
        var sender = await userManager.FindByIdAsync(userId);

        await Clients.Group(RequestGroup(requestId)).SendAsync("ReceiveMessage", new
        {
            saved.Id,
            saved.SenderId,
            SenderName = sender?.FullName ?? Context.User?.Identity?.Name,
            saved.Content,
            saved.IsOffer,
            saved.OfferAmount,
            saved.SentAt
        });
    }

    private async Task<bool> CanAccessAsync(int requestId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return false;
        var isAdmin = Context.User?.IsInRole(Constants.AppRoles.Admin) == true;
        return await requestService.CanUserAccessAsync(requestId, userId, isAdmin);
    }

    private static string RequestGroup(int requestId) => $"request-{requestId}";
}
