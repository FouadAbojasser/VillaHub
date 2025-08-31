using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VillaHub.Domain.Entities
{
    public class Amenity
    {
        public enum AmenityType
        {
            Village,
            Villa,
            Floor,
        }
        public int Id { get; set; }
        public string Name { get; set; }=string.Empty;
        [Display(Name = "Name (Arabic)")]
        public string Name_ar { get; set; }=string.Empty;
        public string Description { get; set; } = string.Empty;
        [Display(Name = "Description (Arabic)")]
        public string Description_ar { get; set; } = string.Empty;
        public AmenityType Type { get; set; } 
        public double Price { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }


        public ICollection<Village> Villages { get; set; } = [];
        public ICollection<Villa> Villas { get; set; } = [];
        public ICollection<Floor> Floors { get; set; } = [];
    }
}
