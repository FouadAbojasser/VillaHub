using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using VillaHub.Application.Common.Interfaces;
using VillaHub.Application.Common.Utility;
using VillaHub.Domain.Entities;
using VillaHub.Web.SignalR;


public class NotificationService : INotificationService
{
    private readonly IHubContext<BookingHub> _hubContext;

    public NotificationService(IHubContext<BookingHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyNewBooking(Booking booking)
    {
        await _hubContext.Clients.Group(SD.Role_SuperAdmin).SendAsync("ReceiveBookingNotification",
            new
            {
                Id = booking?.Id,
                Floor = booking?.Floor?.FloorNumber,
                VillaName = booking?.Villa?.Name,
                CheckInDate = booking?.CheckInDate.ToString("d"),
                //Nights = booking?.Nights,
                TotalCost = booking?.TotalCost
            });
    }


    public async Task NotifyNewComment(Review review)
    {
        await _hubContext.Clients.Group(SD.Role_SuperAdmin).SendAsync("ReceiveReviewNotification",
            new
            {
                Id = review?.Id,

            });
    }
}
