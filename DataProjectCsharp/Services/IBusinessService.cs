using DataProjectCsharp.Data;
using Microsoft.Data.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataProjectCsharp.Services
{
    public interface IBusinessService
    {
        DataFrame GetPositionPerformance(int? portfolioId, string userId, string symbol);

        DataFrame GetPortfolioHPR(int? portfolioId, string userId);
    
    }
}
