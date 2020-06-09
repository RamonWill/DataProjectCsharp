using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataProjectCsharp.Models.Repository
{
    public interface IRepository
    {
        List<Portfolio> GetAllUserPortfolios(string userId);
        Portfolio GetUserPortfolio(int? portfolioId, string userId);

        bool IsDuplicatePortfolio(string name, string userId);
        bool PortfoliosExists(string userId);
        void AddPortfolio(Portfolio portfolio);
        // void RemovePortfolio
        // void UpdatePortfolio

            //getalltradesbyticker
        List<Trade> GetAllUserTrades(int portfolioId, string userId);
        Trade GetTrade(int? tradeId);
        void AddTrade(Trade trade);
        void UpdateTrade(Trade trade);
        void RemoveTrade(Trade trade);

        Task<bool> SaveChangesAsync();

        List<SecurityPrices> GetSecurityPrices(string symbol);
        bool IsSecurityStored(string symbol);
        void AddSecurityPrice(SecurityPrices price);

    }
}
