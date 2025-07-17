using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillaHub.Application.Common.Interfaces;
using VillaHub.Application.Common.Utility;
using VillaHub.Domain.Entities;
using VillaHub.Infrastructure.Data;

namespace VillaHub.Infrastructure.Repository
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        private readonly ApplicationDbContext dbContext;
        public BookingRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }

        public void UpdateStatus(int bookingId, string bookingStatus)
        {
            var bookingIdDb = dbContext.Bookings.FirstOrDefault(b => b.Id == bookingId);
            if (bookingIdDb is not null) 
            { 
                bookingIdDb.Status = bookingStatus;

                if (bookingStatus == SD.StatusCheckedIn)
                {
                    bookingIdDb.ActualCheckInDate = DateTime.UtcNow;
                }
                if (bookingStatus == SD.StatusCompleted)
                {
                    bookingIdDb.ActualCheckOutDate = DateTime.UtcNow;
                }
            }

        }

        public void UpdateStripPaymentId(int bookingId, string sessionId, string paymentIntentId)
        {
            var bookingIdDb = dbContext.Bookings.FirstOrDefault(b => b.Id == bookingId);
            if (bookingIdDb is not null)
            {
                // فهذا يعني أنه تم فتح نافذة للدفع مع طريقة الدفع sessionId إذا حصلت على 
                if (!string.IsNullOrEmpty(sessionId))
                {
                  bookingIdDb.StripeSessionId = sessionId;
                }
                // فهذا يعني أن عملية الدفع تمت بشكل صحيح paymentIntentId  إذا حصلت على 
                if (!string.IsNullOrEmpty(paymentIntentId))
                {
                    bookingIdDb.StripePaymentIntentId = paymentIntentId;
                    bookingIdDb.PaymentDate = DateTime.UtcNow;
                    bookingIdDb.IsPaymentSuccessful = true;
                }
                
            }


        }
    }
}
