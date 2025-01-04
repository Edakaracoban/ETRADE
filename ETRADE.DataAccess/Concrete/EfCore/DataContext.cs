using ETRADE.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETRADE.DataAccess.Concrete.EfCore
{
    public class DataContext:DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {    // connection string e bağlanmak için Oncongifuring metodu kullanılır.
            optionsBuilder.UseSqlServer(@"Server=EDANIN-DESKTOPU\SQLEXPRESS;Database=ETRADE;uid=sa;pwd=1;TrustServerCertificate=True");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductCategory>().HasKey(c => new { c.ProductId, c.CategoryId });
        }

        public DbSet<Product> Products { get; set; } // tablolarda primary keye gelen id olmak zorundadır.
        public DbSet<Category> Categories { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Order> Orders { get; set; }

    }
}
