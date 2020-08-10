using Microsoft.EntityFrameworkCore;

using SampleServices.Entities;

using System;

namespace SampleServices.Authorization.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
