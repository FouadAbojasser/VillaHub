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
        public DbSet<Floor> Floors { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<OTP> OTPs { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ✅ Villa → Village (safe to cascade)
            builder.Entity<Villa>()
                .HasOne(v => v.Village)
                .WithMany(vg => vg.Villas)
                .HasForeignKey(v => v.VillageId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Floor>()
                .HasOne(f => f.Villa)
                .WithMany(v => v.Floors)
                .HasForeignKey(f => f.VillaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Floor>()
                .HasOne(f => f.Village)
                .WithMany(v => v.Floors)
                .HasForeignKey(f => f.VillageId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ Image → Villa (Restrict to avoid multiple cascade paths)
            builder.Entity<Image>()
                .HasOne(i => i.Villa)
                .WithMany(v => v.Images)
                .HasForeignKey(i => i.VillaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Image → Floor (Composite Foreign Key)
            builder.Entity<Image>()
                .HasOne(i => i.Floor)
                .WithMany(f => f.Images)
                .HasForeignKey(i => new { i.FloorVillageId, i.FloorVillaId, i.FloorNumber })
                .HasPrincipalKey(f => new { f.VillageId, f.VillaId, f.FloorNumber })
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Floor>()
                .HasKey(e => new { e.VillageId, e.VillaId, e.FloorNumber });
        }


    }

}
