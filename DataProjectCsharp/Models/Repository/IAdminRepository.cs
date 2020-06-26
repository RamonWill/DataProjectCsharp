using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataProjectCsharp.Models.Repository
{
    public interface IAdminRepository
    {
        HashSet<DateTime> GetPriceDates(string symbol);

        // we only price open positions
        List<string> GetOpenTradeTickers();

        void AddSecurityPrice(SecurityPrices price);


        Task<bool> SaveChangesAsync();
    }
}
