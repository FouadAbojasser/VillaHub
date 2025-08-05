using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VillaHub.Domain.Entities
{
    public class Floor
    {
       
        [Display(Name = "Floor Number")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public required int FloorNumber { get; set; }
        public string? Description { get; set; }
        public double Price { get; set; }
        public double Area { get; set; }
        public int Capacity { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }

        //===> Relations
        public int VillaId { get; set; }
        [ValidateNever]
        public Villa Villa { get; set; } = null!;

        public int VillageId { get; set; }
        [ValidateNever]
        public Village Village { get; set; } = null!;

        public ICollection<Image> Images { get; set; } = [];
        public ICollection<Review> Reviews { get; set; } = [];
        public ICollection<Amenity> Amenities { get; set; } = [];

        [NotMapped]
        public bool isAvailable { get; set; } = true;
        [NotMapped]
        public bool isInPriceRange { get; set; } = true;

        
    }
}
