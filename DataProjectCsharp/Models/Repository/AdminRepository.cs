using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataProjectCsharp.Models.Repository
{
    public class AdminRepository: IAdminRepository
    {
        private readonly ApplicationDbContext _db;

        public AdminRepository(ApplicationDbContext db)
        {
            this._db = db;
        }

        public void AddSecurityPrice(SecurityPrices price)
        {
            _db.SecurityPrices.Add(price);
        }

        public DateTime GetMostRecentPrice(string symbol)
        {
            SecurityPrices mostRecentPrice = _db.SecurityPrices.OrderByDescending(sp => sp.date).First();
            DateTime mostRecentDate = mostRecentPrice.date;
            return mostRecentDate;
        }

        public List<string> GetOpenTradeTickers()
        {
            // if the quantity sum of the grouped tickers doesnt equal zero then it means that the trade is open...
            var openTrades = _db.Trades
                            .GroupBy(t => t.Ticker)
                            .Select(t => new { t.Key, quantity = t.Sum(i => i.Quantity) })
                            .Where(t => t.quantity != 0)
                            .ToList();
            List<string> openTickers = openTrades.Select(t=>t.Key).ToList();
            return openTickers;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _db.SaveChangesAsync() > 0);
        }
    }
}
