using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillaHub.Domain.Entities;

namespace VillaHub.Application.Common.Interfaces
{
    public interface INotificationService
    {
        Task NotifyNewBooking(Booking booking);
        Task NotifyNewComment(Review review);
    }

}
