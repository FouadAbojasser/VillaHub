using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillaHub.Application.Common.Interfaces;
using VillaHub.Infrastructure.Data;

namespace VillaHub.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbcontext;
        
        public IOTPRepository OTP { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }
        public IVillageRepository Village { get; private set; }
        public IVillaRepository Villa { get; private set; }
        public IFloorRepository Floor { get; private set; }
        public IAmenityRepository Amenity { get; private set; }
        public IImageRepository Image { get; private set; }
        public IBookingRepository Booking { get; private set; }
        public IReviewRepository Review { get; private set; }

        public UnitOfWork(ApplicationDbContext dbContext)
        {
            _dbcontext = dbContext;

            OTP = new OTPRepository(_dbcontext);
            ApplicationUser = new ApplicationUserRepository(_dbcontext);
            Village = new VillageRepository(_dbcontext);
            Villa = new VillaRepository(_dbcontext);
            Floor = new FloorRepository(_dbcontext);
            Amenity = new AmenityRepository(_dbcontext);
            Image = new ImageRepository(_dbcontext);
            Booking = new BookingRepository(_dbcontext);
            Review = new ReviewRepository(_dbcontext);
        }

    }
}
