using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillaHub.Domain.Entities;

namespace VillaHub.Application.Common.Interfaces
{
    public interface IBookingRepository : IRepository<Booking>
    {
        void UpdateStatus(int bookingId, string bookingStatus);
        void UpdateStripPaymentId(int bookingId, string sessionId, string paymentIntentId);
    }
}
