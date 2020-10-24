using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DataProjectCsharp.Data;
using DataProjectCsharp.Models;
using DataProjectCsharp.Models.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DataProjectCsharp.Controllers
{
    [Authorize]
    public class TradeController : Controller
    {
        
        private readonly string _userId;
        private readonly AlphaVantageConnection _avConn;
        private readonly IRepository _repo;

        public TradeController(IRepository repo, IHttpContextAccessor httpContextAccessor, AlphaVantageConnection avConn)
        {
            this._repo = repo;
            this._userId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            this._avConn = avConn;
        }

        // GET: /<controller>/
        [HttpGet]
        public IActionResult ViewTrades(int? id)
        {
            // The Id here is the portfolio id.
            if (id == null)
            {
                return NotFound();
            }
            // if this portfolio doesnt belong to the user return not found

            Portfolio portfolio = _repo.GetUserPortfolio(id, _userId);
            
            if (portfolio == null)
            {
                return NotFound();
            }            

            return PartialView("_TradeViewModalPartial", portfolio);
        }

        [HttpGet]
        public IActionResult AddTrade(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Trade trade = new Trade { };
            trade.PortfolioId = id.GetValueOrDefault();
            return PartialView("_TradeEntryModalPartial", trade);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTrade([Bind("TradeId, Ticker, Quantity, Price, TradeDate, Comments, CreatedTimeStamp, UserId, PortfolioId")] Trade trade)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_TradeEntryModalPartial", trade);
            }
            trade.UserId = _userId;

            // check if trade ticker is valid..
            bool validTicker = _repo.IsValidTicker(trade.Ticker);
            if (!validTicker)
            {
                ModelState.AddModelError("Ticker", $"{trade.Ticker} is not a tradeable instrument");
                return PartialView("_TradeEntryModalPartial", trade);
            }

            _repo.AddTrade(trade);
            await _repo.SaveChangesAsync();

            bool isSecurityStored = _repo.IsSecurityStored(trade.Ticker);
            if (!isSecurityStored)
            {
                // Later on create logic that stores the security price AND MAKE THIS ASYNC so THE SCREEN DOESNT FREEZE
                // also make this try catch in the event that prices are not found
                List<AlphaVantageSecurityData> prices = _avConn.GetDailyPrices(trade.Ticker);
                
                foreach(AlphaVantageSecurityData price in prices)
                {
                    SecurityPrices newPrice = new SecurityPrices {Date=price.Timestamp, ClosePrice=price.Close, Ticker=trade.Ticker };
                    _repo.AddSecurityPrice(newPrice);
                }
                await _repo.SaveChangesAsync();
            }
            return PartialView("_TradeEntryModalPartial", trade);
        }

        [HttpGet]
        public IActionResult EditTrade(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Trade trade = _repo.GetTrade(id);
            if (trade == null)
            {
                return NotFound();
            }

            // In the partialview i need to keep the userid, createdtimestamp of the trade intact
            return PartialView("_TradeEditModalPartial", trade);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTrade(int id, [Bind("TradeId, Ticker, Quantity, Price, TradeDate, Comments, CreatedTimeStamp, UserId, PortfolioId")] Trade trade)
        {
            if (id != trade.TradeId)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                return PartialView("_TradeEditModalPartial", trade);
            }

            _repo.UpdateTrade(trade);
            await _repo.SaveChangesAsync();

            return PartialView("_TradeEditModalPartial", trade);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTrade(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Trade trade = _repo.GetTrade(id);
            if (trade == null)
            {
                return NotFound();
            }

            _repo.RemoveTrade(trade);
            await _repo.SaveChangesAsync();

            return RedirectToAction("Portfolios", "Portfolio");
        }
    }
}
