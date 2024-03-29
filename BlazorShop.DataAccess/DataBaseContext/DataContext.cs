﻿using BlazorShop.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorShop.DataAccess.DataContext
{
    public class DataContext : DbContext
    {

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CartItem>()
                .HasKey(ci => new { ci.UserId, ci.ProductId, ci.ProductTypeId });

            modelBuilder.Entity<ProductVariant>()
                .HasKey(p => new { p.ProductId, p.ProductTypeId });

            modelBuilder.Entity<OrderItem>()
                .HasKey(oi => new { oi.OrderId, oi.ProductId, oi.ProductTypeId });

            modelBuilder.Entity<ProductType>().HasData(
                    new ProductType { Id = 1, Name = "Nowe" },
                    new ProductType { Id = 2, Name = "Używane" },
                    new ProductType { Id = 3, Name = "Odnowione" }
                );

            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    Id = 1,
                    Name = "Hulajnogi elektryczne",
                    Url = "scooters"
                },
                new Category
                {
                    Id = 2,
                    Name = "Części",
                    Url = "parts"
                },
                 new Category
                 {
                     Id = 3,
                     Name = "Baterie",
                     Url = "batteries"
                 },
                new Category
                {
                    Id = 4,
                    Name = "Akcesoria",
                    Url = "accesories"
                },
                new Category
                {
                    Id = 5,
                    Name = "Serwis",
                    Url = "service"
                }
                );
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Image> Images { get; set; }
    }
}
