
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using VillaHub.Application.Common.Interfaces;
using VillaHub.Application.Common.Utility;
using VillaHub.Domain.Entities;
using Color = Syncfusion.Drawing.Color;


namespace VillaHub.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly StripeSettings _stripeSettings;

        public BookingController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IOptions<StripeSettings> stripeSettings)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _stripeSettings = stripeSettings.Value;
        }



        [Authorize]
        public IActionResult Index()
        {
            return View();
        }




        [Authorize]
        public async Task<IActionResult> FinalizeBookingAsync(int villageId, int villaId, int floorNumber, DateOnly checkInDate, int noOfNights)
        {
            var claimIdentity = (ClaimsIdentity)User.Identity!;
            var UserId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            var applicationUser = await _userManager.FindByIdAsync(UserId);

            if (applicationUser is not null)
            {
                var FloorToBook = _unitOfWork.Floor.GetOne(
                    f => f.FloorNumber == floorNumber &&
                         f.VillaId == villaId &&
                         f.VillageId == villageId,
                    [f => f.Village, f => f.Villa, f => f.Images, f => f.Amenities],
                    false);

                if (FloorToBook is not null)
                {
                    Booking booking = new()
                    {
                        Name = applicationUser.Name,
                        Email = applicationUser.Email!,
                        Phone = applicationUser.PhoneNumber!,

                        FloorNumber = floorNumber,
                        Floor = FloorToBook,
                        VillaId = villaId,
                        Villa = FloorToBook.Villa,
                        VillageId = villageId,
                        Village = FloorToBook.Village,

                        CheckInDate = checkInDate,
                        Nights = noOfNights,
                        CheckOutDate = checkInDate.AddDays(noOfNights),
                        TotalCost = FloorToBook.Price * noOfNights,
                    };

                    return View(booking);
                }
            }

            return BadRequest();
        }




        [Authorize]
        [HttpPost]
        public async Task<IActionResult> FinalizeBookingAsync(Booking booking)
        {
            
            var claimIdentity = (ClaimsIdentity)User.Identity!;

            var UserId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            var applicationUser = await _userManager.FindByIdAsync(UserId);

            if (applicationUser is not null)
            {
                var FloorToBook = _unitOfWork.Floor.GetOne(
                    f => f.FloorNumber == booking.FloorNumber &&
                            f.VillaId == booking.VillaId &&
                            f.VillageId == booking.VillageId,
                    [f => f.Village, f => f.Villa, f => f.Images, f => f.Amenities],
                    false);


                if (FloorToBook is not null)
                {
                    //Check Availability Before Placing Booking

                    booking.UserId = UserId;
                    booking.User = applicationUser;
                    booking.TotalCost = FloorToBook.Price * booking.Nights;
                    booking.Status = SD.StatusPending;
                    booking.BookingDate = DateTime.UtcNow;

                    if (booking.Id == 0)
                    {
                        await _unitOfWork.Booking.CreateAsync(booking);
                        await _unitOfWork.Booking.CommitAsync();
                    }

                    string stripeSessionUrl = await CreateStripeSessionUrl(
                            booking.TotalCost,
                            booking.Id,
                            booking.VillageId,
                            booking.VillaId,
                            booking.FloorNumber,
                            booking.CheckInDate,
                            booking.Nights);

                    //Go to payment
                    return Redirect(stripeSessionUrl);
                }
            }

        return BadRequest(); 
                      
        }





        private async Task<string> CreateStripeSessionUrl(double amount, int bookingId, int villageId, int villaId, int floorNumber, DateOnly checkInDate, int noOfNights)
        {
            var domain = $"{Request.Scheme}://{Request.Host}";

            //var formattedCheckIn = checkInDate.ToString("dd-MM-yyyy");

            // If payment canceled, return back to booking confirm
            var cancelUrl = $"{domain}/Booking/FinalizeBooking" +
                            $"?villageId={villageId}" +
                            $"&villaId={villaId}" +
                            $"&floorNumber={floorNumber}" +
                            $"&checkInDate={checkInDate}" +
                            $"&noOfNights={noOfNights}";

            // Retrieve floor with image, villa, and village info
            var bookingFloor = _unitOfWork.Floor.GetOne(
                        f => f.FloorNumber == floorNumber
                        && f.VillaId == villaId
                        && f.VillageId == villageId,
                        [f => f.Images, f => f.Village, f => f.Villa]);
            

            var options = new Stripe.Checkout.SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },

                Mode = "payment",

                SuccessUrl = $"{domain}/Booking/BookingConfirmation?bookingId={bookingId}",

                CancelUrl = cancelUrl,

                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>
                {
                    new Stripe.Checkout.SessionLineItemOptions
                    {
                        PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(amount * 100), // Amount in cents
                            Currency = "usd",
                            ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"VillaHub Booking - Floor Number: {floorNumber}",

                                Description = $" Check-In: {checkInDate:yyyy-MM-dd} |" +
                                              $" Nights: {noOfNights} |" +
                                              $" Villa: {bookingFloor?.Villa?.Name} |" +
                                              $" Village: {bookingFloor?.Village?.Name}",
                        
                            }
                        },
                        Quantity = 1
                    }
                }
            };

            var service = new Stripe.Checkout.SessionService();
            var session = service.Create(options);

            // Save Stripe session ID to booking
            // in new version of strip session.PaymentIntentId will be null here. it will have value after the payment succeeded
            _unitOfWork.Booking.UpdateStripPaymentId(bookingId, session.Id, session.PaymentIntentId);
            await _unitOfWork.Booking.CommitAsync();

            return session.Url;
        }





        public async Task<IActionResult> BookingConfirmation(int bookingId)
        {
            var bookingInDb = _unitOfWork.Booking.GetOne(b => b.Id == bookingId);

            if (bookingInDb is null || string.IsNullOrEmpty(bookingInDb.StripeSessionId))
            {
                return NotFound();
            }

            var service = new Stripe.Checkout.SessionService();

            var session = service.Get(bookingInDb.StripeSessionId);

            if (session.PaymentStatus == "paid")
            {
                // Strip من جلسة  PaymentIntentId  بعد نجاح عملية الدفع نحصل على 
                // يستخدم في حالة اعادة المبلغ المدفوع للمشتري
                _unitOfWork.Booking.UpdateStripPaymentId(bookingId, session.Id, session.PaymentIntentId);

                _unitOfWork.Booking.UpdateStatus(bookingId, SD.StatusApproved);

                await _unitOfWork.Booking.CommitAsync();
            }

            return View(bookingId);
        }




        public IActionResult BookingDetails(int bookingId)
        {
            if(bookingId == 0)
            {
                return NotFound();
            }

            var BookingInDb = _unitOfWork.Booking.GetOne(b=>b.Id == bookingId,[b=>b.User]);

            if(BookingInDb is not null)
            {
                var BookedFloor = _unitOfWork.Floor.GetOne(
                                        f => f.FloorNumber == BookingInDb.FloorNumber
                                        && f.VillaId == BookingInDb.VillaId
                                        && f.VillageId == BookingInDb.VillageId,
                                        [f => f.Images, f => f.Village, f => f.Villa, f=>f.Amenities]);

                if (BookedFloor is not null) 
                {
                    BookingInDb.Village = BookedFloor.Village;
                    BookingInDb.Villa = BookedFloor.Villa;
                    BookingInDb.Floor = BookedFloor;
                }
                                
            }

            return View(BookingInDb);

        }





        [Authorize(Roles =SD.Role_SuperAdmin)]
        [HttpPost]
        public async Task<IActionResult> BookingCheckInAsync(Booking booking)
        {
            var bookingInDb = _unitOfWork.Booking.GetOne(b => b.Id == booking.Id);

            if (bookingInDb is not null)
            {
                bookingInDb.Status = SD.StatusCheckedIn;
                bookingInDb.ActualCheckInDate = DateTime.UtcNow;
                _unitOfWork.Booking.Update(bookingInDb);
                await _unitOfWork.Booking.CommitAsync();
                TempData["success"] = "Booking has ben changed to Checked-In";
            }

           return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
           
        }





        [Authorize(Roles = SD.Role_SuperAdmin)]
        [HttpPost]
        public async Task<IActionResult> BookingCheckOutAsync(Booking booking)
        {
            var bookingInDb = _unitOfWork.Booking.GetOne(b => b.Id == booking.Id);

            if (bookingInDb is not null)
            {
                bookingInDb.Status = SD.StatusCompleted;
                bookingInDb.ActualCheckOutDate = DateTime.UtcNow;
                _unitOfWork.Booking.Update(bookingInDb);
                await _unitOfWork.Booking.CommitAsync();
                TempData["success"] = "Booking has ben changed to Completed";
            }
            
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }





        //[Authorize(Roles = SD.Role_SuperAdmin)]
        //ممكن نخلي إلغاء الحجز يتم فقط من الأدمن
        [HttpPost]
        public async Task<IActionResult> BookingCancelAsync(Booking booking)
        {
            var bookingInDb = _unitOfWork.Booking.GetOne(b => b.Id == booking.Id);

            if (bookingInDb is not null) 
            { 
                bookingInDb.Status = SD.StatusCancelled;
                _unitOfWork.Booking.Update(bookingInDb);
                await _unitOfWork.Booking.CommitAsync();
                TempData["success"] = "Booking has ben changed to Cancelled";
            }

            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }





        


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> GenerateInvoiceAsync(int bookingId, string downloadType)
        {
            var bookingInDb = _unitOfWork.Booking.GetOne(b => b.Id == bookingId);

            var applicationUser = new ApplicationUser();

            if (bookingInDb is not null)
            {
                applicationUser = await _userManager.FindByIdAsync(bookingInDb.UserId);
                var bookingVilla = _unitOfWork.Villa.GetOne(v => v.Id == bookingInDb.VillaId);
                var bookingVillage = _unitOfWork.Village.GetOne(v=>v.Id == bookingInDb.VillageId);

                //Loading the Template Document
                WordDocument documnet = new WordDocument();
                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "BookingDetails.docx");
                if (!System.IO.File.Exists(templatePath))
                    throw new FileNotFoundException($"Template not found at: {templatePath}");
                using FileStream fileStream = new(templatePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                documnet.Open(fileStream, FormatType.Automatic);


                //Update The Template with Booking Data
                TextSelection textSelection = documnet.Find("XX_BOOKING_NUMBER", false, true);
                WTextRange textRange = textSelection.GetAsOneRange();
                textRange.Text = bookingInDb.Id.ToString();

                textSelection = documnet.Find("XX_BOOKING_DATE", false, true);
                textRange = textSelection.GetAsOneRange();
                textRange.Text = bookingInDb.BookingDate.ToShortDateString();

                textSelection = documnet.Find("xx_payment_date", false, true);
                textRange = textSelection.GetAsOneRange();
                textRange.Text = bookingInDb.PaymentDate.ToShortDateString();

                textSelection = documnet.Find("xx_checkin_date", false, true);
                textRange = textSelection.GetAsOneRange();
                textRange.Text = bookingInDb.CheckInDate.ToShortDateString();

                textSelection = documnet.Find("xx_checkout_date", false, true);
                textRange = textSelection.GetAsOneRange();
                textRange.Text = bookingInDb.CheckOutDate.ToShortDateString();

                textSelection = documnet.Find("xx_booking_total", false, true);
                textRange = textSelection.GetAsOneRange();
                textRange.Text = bookingInDb.TotalCost.ToString("c");


                if (applicationUser is not null)
                {
                    textSelection = documnet.Find("xx_customer_name", false, true);
                    textRange = textSelection.GetAsOneRange();
                    textRange.Text = applicationUser.Name;

                    textSelection = documnet.Find("xx_customer_phone", false, true);
                    textRange = textSelection.GetAsOneRange();
                    textRange.Text = applicationUser.PhoneNumber!;


                    textSelection = documnet.Find("xx_customer_email", false, true);
                    textRange = textSelection.GetAsOneRange();
                    textRange.Text = applicationUser.Email!;
                }

                //Insert A table into the Template
                WTable table = new(documnet);
                table.TableFormat.Borders.LineWidth = 1f;
                table.TableFormat.Borders.Color = Color.Black;
                table.TableFormat.Paddings.Top = 7f;
                table.TableFormat.Paddings.Bottom = 7f;
                table.TableFormat.Borders.Horizontal.LineWidth = 7f;

                table.ResetCells(2, 4);

                WTableRow row_0 = table.Rows[0];
                row_0.Cells[0].AddParagraph().AppendText("Floor Number");
                row_0.Cells[0].Width = 80f;
                row_0.Cells[1].AddParagraph().AppendText("Villa Name");
                row_0.Cells[1].Width = 220f;
                row_0.Cells[2].AddParagraph().AppendText("Village Name");
                row_0.Cells[2].Width = 80f;
                row_0.Cells[3].AddParagraph().AppendText("No. of Nights");

                WTableRow row_1 = table.Rows[1];
                row_1.Cells[0].AddParagraph().AppendText(bookingInDb.FloorNumber.ToString());
                row_1.Cells[0].Width = 80f;
                
                row_1.Cells[1].AddParagraph().AppendText(bookingVilla!.Name);
                row_1.Cells[1].Width = 220f;
                row_1.Cells[2].AddParagraph().AppendText(bookingVillage!.Name);
                row_1.Cells[2].Width = 80f;
                row_1.Cells[3].AddParagraph().AppendText(bookingInDb.Nights.ToString());


                TextBodyPart textBodyPart = new(documnet);
                textBodyPart.BodyItems.Add(table);
                documnet.Replace("<ADDTABLEHERE>", textBodyPart,false,true);
                

                //Saving the Files
                using DocIORenderer docIORenderer = new();
                MemoryStream documnetStream = new();

                if (downloadType == "pdf")
                {
                    PdfDocument pdfDocument = docIORenderer.ConvertToPDF(documnet);
                    pdfDocument.Save(documnetStream);
                    documnetStream.Position = 0;
                    return File(documnetStream, "application/pdf", $"Invoice_{bookingId}.pdf");
                }
                else
                {
                  documnet.Save(documnetStream, FormatType.Docx);
                  documnetStream.Position = 0;
                  return File(documnetStream, "application/docx", $"Invoice_{bookingId}.docx");
                }

                
            }

            return NotFound();
        }
      



        #region API Calls
        [HttpGet]
        [Authorize]
        public IActionResult GetAllBookings(string status)
        {
            IEnumerable<Booking> objBooking;

            if (User.IsInRole(SD.Role_SuperAdmin))
            {
                //Admin User Can Get all Booking Records
                objBooking = _unitOfWork.Booking.Get(null, [b => b.User]);
            }
            else
            {
                //Get booking for this user only
                var claimIdentity = (ClaimsIdentity)User.Identity!;
                var UserId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier)!.Value;

                objBooking=_unitOfWork.Booking.Get(b=>b.UserId == UserId);

            }

            if (!string.IsNullOrEmpty(status))
            {
                objBooking = objBooking.Where(b=>b.Status!.ToLower().Equals(status.ToLower()));
            }

            return Json(new {data=objBooking});

        }

        #endregion 



    }
}
