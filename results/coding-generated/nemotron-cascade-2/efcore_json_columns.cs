using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CompanyDemo
{
    // 1. Complex type representing an address
    [ComplexType]
    public class Address
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostCode { get; set; } = string.Empty;
    }

    // 2. Entity that owns the address
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Headquarters address is stored as JSON in the database
        public Address HeadquartersAddress { get; set; } = null!;
    }

    // 3. DbContext with a DbSet<Company> and JSON mapping for the address
    public class CompanyDbContext : DbContext
    {
        public DbSet<Company> Companies => Set<Company>();

        public CompanyDbContext(DbContextOptions<CompanyDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Map HeadquartersAddress as a JSON column
            modelBuilder.Entity<Company>()
                .Property(c => c.HeadquartersAddress)
                .HasJsonConversion();

            base.OnModelCreating(modelBuilder);
        }
    }

    // 4. Query helper
    public static class CompanyQueries
    {
        /// <summary>
        /// Returns all companies whose headquarters city matches the supplied city name.
        /// </summary>
        public static List<Company> FindCompaniesByCity(CompanyDbContext db, string city)
        {
            return db.Companies
                .Where(c => c.HeadquartersAddress != null &&
                            c.HeadquartersAddress.City == city)
                .ToList();
        }
    }
}