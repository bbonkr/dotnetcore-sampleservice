using Microsoft.EntityFrameworkCore;

using SampleService.Entities;

using System;
using System.Reflection;

namespace SampleService.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<AuthorizationLog> AuthorizationLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.Load("SampleService.Data.EntityTypeConfiguration"));
        }
    }
}
