using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using VillaHub.Application.Common.Interfaces;
using VillaHub.Application.Common.Utility;
using VillaHub.Infrastructure.Repository;
using VillaHub.Web.ViewModels.Dashboard;

namespace VillaHub.Web.Controllers
{
    public class DashboardController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;

        static int previousMonth = DateTime.Now.Month == 1 ? 12 : DateTime.Now.Month - 1;

        private readonly DateTime previousMonthStartDate = new(DateTime.Now.Year, previousMonth, 1);

        private readonly DateTime currentMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);

        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public IActionResult Index()
        {
            return View();
        }



        public IActionResult BookingsRadialChartData()
        {
            // Total Bookings that are not Pendding or Cancelled
            var totalBookings = _unitOfWork.Booking.Get(b => b.Status != SD.StatusPending || b.Status != SD.StatusCancelled);

            var currentMonthBookings = totalBookings.Count(b => b.BookingDate >= currentMonthStartDate && b.BookingDate <= DateTime.Now);

            var previousMonthBookings = totalBookings.Count(b => b.BookingDate >= previousMonthStartDate && b.BookingDate < currentMonthStartDate);

            RadialBarChartVM radialBarChartVM = new();

            int IncreaseDecreaseRatio = 100;

            if (previousMonthBookings != 0)
            {
                IncreaseDecreaseRatio = Convert.ToInt32(((currentMonthBookings - previousMonthBookings) / (previousMonthBookings)) * 100);
            }

            radialBarChartVM.TotalCount = totalBookings.Count();
            radialBarChartVM.CountInCurrentMonth = currentMonthBookings;
            radialBarChartVM.HasRatioIncreased = currentMonthBookings > previousMonthBookings;
            radialBarChartVM.Series = new List<int> { IncreaseDecreaseRatio };


            return Json(radialBarChartVM);
        }




        public IActionResult UsersRadialChartData()
        {
            // Total Bookings that are not Pendding or Cancelled
            var totalUsers = _unitOfWork.ApplicationUser.Get();

            var currentMonthUsers = totalUsers.Count(b => b.CreatedAt >= currentMonthStartDate && b.CreatedAt <= DateTime.Now);

            var previousMonthUsers = totalUsers.Count(b => b.CreatedAt >= previousMonthStartDate && b.CreatedAt < currentMonthStartDate);

            RadialBarChartVM radialBarChartVM = new();

            int IncreaseDecreaseRatio = 100;

            if (previousMonthUsers != 0)
            {
                IncreaseDecreaseRatio = Convert.ToInt32(((currentMonthUsers - previousMonthUsers) / (previousMonthUsers)) * 100);
            }

            radialBarChartVM.TotalCount = totalUsers.Count();
            radialBarChartVM.CountInCurrentMonth = currentMonthUsers;
            radialBarChartVM.HasRatioIncreased = currentMonthUsers > previousMonthUsers;
            radialBarChartVM.Series = new List<int> { IncreaseDecreaseRatio };

            return Json(radialBarChartVM);
        }




        public IActionResult RevenuesRadialChartData()
        {
            // Total Bookings that are not Pendding or Cancelled
            var totalBookings = _unitOfWork.Booking.Get(b => b.Status == SD.StatusApproved);

            var currentMonthRevenues = totalBookings.Where(b => b.BookingDate >= currentMonthStartDate && b.BookingDate <= DateTime.Now).Sum(b => b.TotalCost);

            var previousMonthRevenues = totalBookings.Where(b => b.BookingDate >= previousMonthStartDate && b.BookingDate < currentMonthStartDate).Sum(b => b.TotalCost);

            RadialBarChartVM radialBarChartVM = new();

            int IncreaseDecreaseRatio = 100;

            if (previousMonthRevenues != 0)
            {
                IncreaseDecreaseRatio = Convert.ToInt32(((currentMonthRevenues - previousMonthRevenues) / (previousMonthRevenues)) * 100);
            }

            radialBarChartVM.TotalCount = totalBookings.Sum(b => b.TotalCost);
            radialBarChartVM.CountInCurrentMonth = currentMonthRevenues;
            radialBarChartVM.HasRatioIncreased = currentMonthRevenues > previousMonthRevenues;
            radialBarChartVM.Series = new List<int> { IncreaseDecreaseRatio };

            return Json(radialBarChartVM);
        }




        public IActionResult NewAndOldUsersPieChartData()
        {
            var totalBookings = _unitOfWork.Booking.Get(b => b.BookingDate >= DateTime.Now.AddDays(-30) && (b.Status != SD.StatusPending || b.Status != SD.StatusCancelled));

            var usersWithOneTimeBooking = totalBookings.GroupBy(b => b.UserId).Where(x => x.Count() == 1).Select(x => x.Key).ToList();

            int usersWithOneTimeBookingCount = usersWithOneTimeBooking.Count();

            int usersWithMoreThanOneBookingCount = totalBookings.Count() - usersWithOneTimeBookingCount;

            PieChartVM pieChartVM = new()
            {
                Labels = ["New Users Bookings", "Return Users Bookings"],

                Series = [usersWithOneTimeBookingCount, usersWithMoreThanOneBookingCount],
            };

            return Json(pieChartVM);

        }


        public IActionResult UsersAndBookingsPerDateLineChartData()
        {
            var bookingData = _unitOfWork.Booking.Get(u => u.BookingDate >= DateTime.Now.AddDays(-30) &&
             u.BookingDate.Date <= DateTime.Now)
                 .GroupBy(b => b.BookingDate.Date)  // .Date will group them by Date only not DateTime
                 .Select(u => new
                 { // Projection to select only Date and No. of Bookings at that date = Count value
                     DateTime = u.Key,
                     NewBookingCount = u.Count()
                 });


            var UsersData = _unitOfWork.ApplicationUser.Get(u => u.CreatedAt >= DateTime.Now.AddDays(-30) &&
            u.CreatedAt.Date <= DateTime.Now)
                .GroupBy(b => b.CreatedAt.Date)
                .Select(u => new
                {
                    DateTime = u.Key,
                    NewUsersCount = u.Count()
                });


            //Left Join to get All Dates of bookings First
            var leftJoin = bookingData.GroupJoin(UsersData, booking => booking.DateTime, user => user.DateTime,
                (booking, user) => new
                {
                    booking.DateTime,
                    booking.NewBookingCount,
                    NewUsersCount = user.Select(x => x.NewUsersCount).FirstOrDefault()
                });


            //Right Join to get All Dates of User Registeration Second
            var rightJoin = UsersData.GroupJoin(bookingData, user => user.DateTime, booking => booking.DateTime,
                (user, booking) => new
                {
                    user.DateTime,
                    NewBookingCount = booking.Select(x => x.NewBookingCount).FirstOrDefault(),
                    user.NewUsersCount
                });


            // (Date is common) So Merge to Remove dublicated Dates and keep only Uniqe ones with their data
            var mergedData = leftJoin.Union(rightJoin).OrderBy(x => x.DateTime).ToList();

            var newBookingData = mergedData.Select(x => x.NewBookingCount).ToArray();
            var newUsersData = mergedData.Select(x => x.NewUsersCount).ToArray();
            var categories = mergedData.Select(x => x.DateTime.ToString("MM/dd/yyyy")).ToArray();

            List<ChartData> chartDataList = new()
            {
                new ChartData
                {
                    Name = "New Bookings",
                    Data = newBookingData
                },
                new ChartData
                {
                    Name = "New Members",
                    Data = newUsersData
                },
            };


            LineChartVM lineChartVM = new()
            {
                Series = chartDataList,

                Categories = categories,
            };

            return Json(lineChartVM);

        }


    }
}