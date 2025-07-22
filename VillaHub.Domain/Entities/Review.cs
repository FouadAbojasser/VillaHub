using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VillaHub.Domain.Entities
{
    public class Review
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string Comment { get; set; } = string.Empty;
        public int Rate { get; set; }
        public DateOnly CreatedAt { get; set; }
        public DateOnly UpdatedAt { get; set; }

        // ===> Foreign Keys
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int? FloorNumber { get; set; }       // changed from string? to int
        public int? FloorVillaId { get; set; }      // changed from int? to int
        public int? FloorVillageId { get; set; }    // changed from int? to int

        // ===> Navigation Properties
        public ApplicationUser? User { get; set; }
        public Floor Floor { get; set; } = null!;
    }
}
