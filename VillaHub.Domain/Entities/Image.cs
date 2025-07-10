using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VillaHub.Domain.Entities
{
    public class Image
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }

        [Required]
        public string Url { get; set; } = string.Empty;

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }

        // ===> Foreign Keys
        public int? VillaId { get; set; }

        public int? FloorNumber { get; set; }       // changed from string? to int
        public int? FloorVillaId { get; set; }      // changed from int? to int
        public int? FloorVillageId { get; set; }    // changed from int? to int

        // ===> Navigation Properties
        public Villa? Villa { get; set; }
        public Floor Floor { get; set; } = null!;
    }


}
