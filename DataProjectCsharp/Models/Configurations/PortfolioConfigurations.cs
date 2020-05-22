using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataProjectCsharp.Models.Configurations
{
    public class PortfolioConfigurations : IEntityTypeConfiguration<Portfolio>
    {
        public void Configure(EntityTypeBuilder<Portfolio> builder)
        {
            builder.HasData(
                new Portfolio
                {
                    PortfolioId = 9999,
                    Name = "Portfolio Tester"
                }); ;
        }
    }
}
