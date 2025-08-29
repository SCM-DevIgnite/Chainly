using Chainly.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chainly.Data
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public DbSet<Company> Companies { get; set; }
        public DbSet<ProductionLine> ProductionLines { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SupplierMaterial> SupplierMaterials { get; set; }
        public DbSet<DefectiveProduct> DefectiveProducts { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Configure one-to-many relationship between Company and User
            builder.Entity<Company>()
                .HasMany(c => c.Users)
                .WithOne(u => u.Company)
                .HasForeignKey(u => u.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
            // Configure one-to-many relationship between ProductionLine and Report
            builder.Entity<ProductionLine>()
                .HasMany(pl => pl.Reports)
                .WithOne(r => r.ProductionLine)
                .HasForeignKey(r => r.productionLineId)
                .OnDelete(DeleteBehavior.Cascade);
            // Configure one-to-many relationship between Report and DefectiveProduct
            builder.Entity<Report>()
                .HasMany(r => r.DefectiveProducts)
                .WithOne(dp => dp.Report)
                .HasForeignKey(dp => dp.ReportId)
                .OnDelete(DeleteBehavior.Cascade);
            // Configure many-to-many relationship between Supplier and Material through SupplierMaterial
            builder.Entity<SupplierMaterial>()
                .HasKey(sm => new { sm.SupplierId, sm.MaterialId });
            builder.Entity<SupplierMaterial>()
                .HasOne(sm => sm.Supplier)
                .WithMany(s => s.SupplierMaterials)
                .HasForeignKey(sm => sm.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<SupplierMaterial>()
                .HasOne(sm => sm.Material)
                .WithMany(m => m.SupplierMaterials)
                .HasForeignKey(sm => sm.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
