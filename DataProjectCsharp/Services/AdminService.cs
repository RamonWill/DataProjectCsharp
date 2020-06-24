using DataProjectCsharp.Data;
using DataProjectCsharp.Models.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataProjectCsharp.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepo;
        private readonly AlphaVantageConnection _avConn;
        public AdminService(IAdminRepository adminRepo, AlphaVantageConnection avConn)
        {
            this._adminRepo = adminRepo;
            this._avConn = avConn;
        }
        public void UpdateSecurityPrices(string symbol)
        {
            // so basically use the GetOpenTradeTickers() to get a list of the open tickers.
            // then for each ticker GetMostRecentPrice(symbol) which returns a date datetime.
            // then connect get the prices from the alphavantage connection and add prices for that ticker if the date is more recent than the most recent date.

            throw new NotImplementedException();
        }
    }
}
