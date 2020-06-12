using DataProjectCsharp.Data;
using DataProjectCsharp.Models;
using Microsoft.Data.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataProjectCsharp.Services
{
    public interface IBusinessService
    {
        PositionFormulas GetPositionData(int? portfolioId, string userId, string symbol);

        DataFrame GetPortfolioHPR(int? portfolioId, string userId);

        PortfolioData GetPortfolioData(string portfolioName, List<Trade> allTrades);
    }
}
