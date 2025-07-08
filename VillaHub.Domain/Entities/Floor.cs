using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VillaHub.Domain.Entities
{
    public class Floor
    {
        [Key]
        public int FoolrNumber { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public double Price { get; set; }
        public int Capacity { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }

        //===> Relations


    }
}
