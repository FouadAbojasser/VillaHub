using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VillaHub.Application.Common.Interfaces
{
    public interface IUnitOfWork
    {
        IOTPRepository OTP { get; }
        IApplicationUserRepository ApplicationUser { get; }
        IVillageRepository Village { get; }
        IVillaRepository Villa { get; }
        IFloorRepository Floor { get; }
        IAmenityRepository Amenity { get; }
        IImageRepository Image { get; }
        IBookingRepository Booking { get; }
        IReviewRepository Review { get; }
    }
}
