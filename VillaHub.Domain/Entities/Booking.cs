using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VillaHub.Domain.Entities
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }


        // User Related Fields
        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;




        // Village , Villa and Floor Related Fields
        public int VillageId { get; set; }
        [NotMapped]
        public Village Village { get; set; } = null!;
        [Required]
        public int VillaId { get; set; }
        [ForeignKey("VillaId")]
        public Villa Villa { get; set; } = null!;
        public int FloorNumber { get; set; }
        [NotMapped]
        public Floor Floor { get; set; } = null!;
        [Required]
        public double TotalCost { get; set; }
        public int Nights { get; set; }
        public string? Status { get; set; }




        //Booking Related Fields
        [Required]
        public DateTime BookingDate { get; set; }
        [Required]
        public DateOnly CheckInDate { get; set; }
        [Required]
        public DateOnly CheckOutDate { get; set; }
        //[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ActualCheckInDate { get; set; }
        //[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ActualCheckOutDate { get; set; }




        //Payment Related Fields
        public bool IsPaymentSuccessful { get; set; } = false;
        public DateTime PaymentDate { get; set; }
        public string? StripeSessionId { get; set; }
        public string? StripePaymentIntentId { get; set; }
               

    }
}
