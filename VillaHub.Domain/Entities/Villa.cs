using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VillaHub.Domain.Entities
{
    public class Villa
    {
        public int Id { get; set; }
        [Display(Name="Villa Name")]
        public required string Name { get; set; }
        public string? Description { get; set; }
        [Display(Name="No. Of Floors")]
        public int NumberOfFloors { get; set; }
        public double Area { get; set; }
        [Display(Name="Capacity (Person)")]
        public int Capacity { get; set; }
        [Display(Name="Main Image")]
        public string? MainImg { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }

        // ===> Relations
        // Foreign key
        public int VillageId { get; set; }

        //Navigation property: Each Villa belongs to one Village
        public Village Village { get; set; } = null!;
        [Display(Name="Villa Images")]
        public ICollection<Image> Images { get; set; } = [];
        public ICollection<Floor> Floors { get; set; } = [];  // Navigation property: One Villa has many Floors

    }
}
