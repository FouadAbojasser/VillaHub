using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        // ===> Relations
        // Foreign key
        public int VillaId { get; set; }
        // Navigation property: Each Villa belongs to one Village
        public Villa Villa { get; set; } = null!;
        
    }
}
