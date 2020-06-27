using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataProjectCsharp.Data;
using DataProjectCsharp.Models;
using DataProjectCsharp.Models.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataProjectCsharp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly IAdminRepository _adminRepo;
        private readonly AlphaVantageConnection _avConn;
        public AdminController(IAdminRepository adminRepo, AlphaVantageConnection avConn)
        {
            this._adminRepo = adminRepo;
            this._avConn = avConn;
        }

        public IActionResult AdminPanel()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDatabasePrices()
        {
            // 1. Get the tickers for all open trades. (tickers where net quantity !=0)
            // 2. get a hashset of the date each ticker is priced at our db.
            // 3. get latest prices from alphavantage and update the prices for each ticker if we dont have it in our system.

            // I can make 5 api calls a minute. so after every 5 calls. pause for 60 seconds before resuming
            List<string> openTickers = _adminRepo.GetOpenTradeTickers();
            int requestsMade = 0; 
            foreach (string ticker in openTickers)
            {
                requestsMade++;
                if (requestsMade % 6 == 0)
                {
                    System.Threading.Thread.Sleep(60000);
                }
                HashSet<DateTime> pricedDates = _adminRepo.GetPriceDates(ticker);
                List<AlphaVantageSecurityData> avPrices = _avConn.GetDailyPrices(ticker);
                foreach (var price in avPrices)
                {
                    if (!pricedDates.Contains(price.Timestamp))
                    {
                        SecurityPrices newPrice = new SecurityPrices { Date = price.Timestamp, ClosePrice = price.Close, Ticker = ticker };
                        _adminRepo.AddSecurityPrice(newPrice);
                        System.Diagnostics.Debug.WriteLine($"Storing: {ticker}| {price.Timestamp}| {price.Close}");
                    }
                }
            }
            await _adminRepo.SaveChangesAsync();
            return RedirectToAction("AdminPanel", "Admin");
        }
    }
}