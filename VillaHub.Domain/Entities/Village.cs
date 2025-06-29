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
        public required string Name { get; set; }
        public string? Description { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        [Display(Name = "Village Image")]
        public string? ImgUrl { get; set; }
    }
}
