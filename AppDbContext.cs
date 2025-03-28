﻿using Microsoft.EntityFrameworkCore;

namespace sdt_backend.net.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; } // Admins table

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.Property(u => u.Name)
                    .HasMaxLength(100)
                    .IsRequired()
                    .HasColumnType("VARCHAR(100)");

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnType("VARCHAR(255)");

                entity.Property(u => u.Password)
                    .IsRequired()
                    .HasColumnType("TEXT");

                entity.HasIndex(u => u.Email).IsUnique();
            });

            // Define the Admin entity with explicit column mappings
            modelBuilder.Entity<Admin>(entity =>
            {
                entity.ToTable("admins");

                entity.Property(a => a.Id)
                    .HasColumnName("id") // Map to lowercase column name
                    .IsRequired();

                entity.Property(a => a.Username)
                    .HasColumnName("username") // Map to lowercase column name
                    .HasMaxLength(50)
                    .IsRequired()
                    .HasColumnType("VARCHAR(50)");

                entity.Property(a => a.PasswordHash)
                    .HasColumnName("password_hash") // Map to lowercase column name
                    .IsRequired()
                    .HasColumnType("TEXT");

                entity.HasIndex(a => a.Username).IsUnique();
            });
        }
    }
}