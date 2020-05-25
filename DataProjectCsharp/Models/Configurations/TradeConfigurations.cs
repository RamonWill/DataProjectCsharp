using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataProjectCsharp.Models.Configurations
{
    public class TradeConfigurations:IEntityTypeConfiguration<Trade>
    {
        public void Configure(EntityTypeBuilder<Trade> builder)
        {
            builder.HasData(
                new Trade
                {
                    TradeId = 9999,
                    Ticker = "ITV.L",
                    Quantity = 500,
                    Price = 1.23m,
                    TradeDate = new DateTime(2020,05,22),
                    Comments = "This is just a test.",
                    PortfolioId = 9999
                });
                
        }
    }
}
