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

        string GetPortfolioName(int? portfolioId);
        bool IsDuplicatePortfolio(string name, string userId);
        bool PortfoliosExists(string userId);

        bool UserPortfolioValidation(int? portfolioId, string userId);
        void AddPortfolio(Portfolio portfolio);

        void RemovePortfolio(Portfolio portfolio);


        List<Trade> GetAllUserTrades(int? portfolioId, string userId);
        Trade GetTrade(int? tradeId);
        void AddTrade(Trade trade);
        void UpdateTrade(Trade trade);
        void RemoveTrade(Trade trade);

        Task<bool> SaveChangesAsync();

        List<SecurityPrices> GetSecurityPrices(string symbol);
        bool IsSecurityStored(string symbol);
        bool IsValidTicker(string symbol);
        void AddSecurityPrice(SecurityPrices price);

        TradeableSecurities GetSecurityDetails(string symbol);

        List<Trade> GetTradesBySymbol(int? portfolioId, string userId, string symbol);

        List<TradeableSecurities> GetTradeableSecurities();
    }
}
