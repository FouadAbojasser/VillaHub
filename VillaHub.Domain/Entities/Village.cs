using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VillaHub.Domain.Entities
{
    public class Village
    {
        public int Id { get; set; }
        [Display(Name = "Village Name")]
        public required string Name { get; set; }
        public string? Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        [Display(Name = "Village Image")]
        public string? ImgUrl { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }

        // ===> Relations
        public ICollection<Villa> Villas { get; set; } = [];  // Navigation property: One Village has many Villas
        public ICollection<Floor> Floors { get; set; } = [];
    }
}
