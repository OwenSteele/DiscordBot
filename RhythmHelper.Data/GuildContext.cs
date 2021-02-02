using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RhythmHelper.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhythmHelper.Data
{
    public class GuildContext : DbContext
    {
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.UseSqlServer(
                @"Server=(localdb)\MSSQLLocalDB;Database=OwenSteeleBot;Integrated Security=True;Connect Timeout=30;");
        }

        protected override void OnModelCreating(ModelBuilder bd)
        {
            bd.Entity<Guild>().HasMany(g => g.Users).WithOne(u => u.Guild);

            bd.Entity<Guild>().HasMany(g => g.Roles);
            bd.Entity<User>().HasMany(u => u.Roles);

        }
    }
}
