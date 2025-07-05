using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VillaHub.Domain.Entities;


namespace VillaHub.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Village> Villages { get; set; }
        public DbSet<Villa> Villas { get; set; }
        public DbSet<Floor> floors { get; set; }
        public DbSet<Entertainment> entertainments { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<OTP> OTPs { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Villa>()
                    .HasOne(v => v.Village)
                    .WithMany(vg => vg.Villas)
                    .HasForeignKey(v => v.VillageId)
                    .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Image>()
                   .HasOne(v => v.Villa)
                   .WithMany(vg => vg.Images)
                   .HasForeignKey(v => v.VillaId)
                   .OnDelete(DeleteBehavior.Cascade);

        }



    }


}
