using DataProjectCsharp.Models.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace DataProjectCsharp.Models
{
    public class ApplicationDbContext: IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // use the below to input seed data
            base.OnModelCreating(builder);
            /*
            builder.ApplyConfiguration(new RoleConfigurations());
            builder.ApplyConfiguration(new TradeConfigurations());
            builder.ApplyConfiguration(new PortfolioConfigurations());
            
            builder.ApplyConfiguration(new AdminConfiguration());
            builder.ApplyConfiguration(new UsersWithRolesConfig());
            */
        }
        public DbSet<Portfolio> Portfolios { get; set; }
        public DbSet<Trade> Trades { get; set; }
        public DbSet<TradeableSecurities> TradeableSecurities { get; set; }
        public DbSet<SecurityPrices> SecurityPrices { get; set; }
    }
}
