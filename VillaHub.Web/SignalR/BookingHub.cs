using Microsoft.AspNetCore.SignalR;
using VillaHub.Application.Common.Utility;

namespace VillaHub.Web.SignalR
{
    public class BookingHub : Hub
    {
        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, SD.Role_SuperAdmin);
        }

        public async Task LeaveAdminGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, SD.Role_SuperAdmin);
        }
    }
}
