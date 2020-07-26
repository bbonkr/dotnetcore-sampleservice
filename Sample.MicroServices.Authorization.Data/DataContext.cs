using Microsoft.EntityFrameworkCore;

using Sample.MicroServices.Entities;

using System;

namespace Sample.MicroServices.Authorization.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
