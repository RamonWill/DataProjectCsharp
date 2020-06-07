using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DataProjectCsharp.Data;
using DataProjectCsharp.Models;
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
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly string _userId;
        private readonly AlphaVantageConnection _avConn;

        public TradeController(ApplicationDbContext db, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor, AlphaVantageConnection avConn)
        {
            this._db = db;
            this._userManager = userManager;
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
            
            // This is eager loading  trades will be in the trade section
            Portfolio portfolio = _db.Portfolios
                                     .Where(p => p.PortfolioId == id && p.UserId == _userId)
                                     .Include(p => p.Trades)
                                     .FirstOrDefault();
            
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
            _db.Trades.Add(trade);
            await _db.SaveChangesAsync();

            // test adding prices to DB
            SecurityPrices security = _db.SecurityPrices.Where(sp=> sp.ticker==trade.Ticker).FirstOrDefault();
            if (security == null)
            {
                List<AlphaVantageSecurityData> prices = _avConn.GetDailyPrices(trade.Ticker);
                
                foreach(AlphaVantageSecurityData price in prices)
                {
                    SecurityPrices newPrice = new SecurityPrices {date=price.Timestamp, ClosePrice=price.Close, ticker=trade.Ticker };
                    _db.SecurityPrices.Add(newPrice);
                }
                await _db.SaveChangesAsync();
            }
            return PartialView("_TradeEntryModalPartial", trade);
        }

        [HttpGet]
        public async Task<IActionResult> EditTrade(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Trade trade = await _db.Trades.FindAsync(id);
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
            _db.Update(trade);
            await _db.SaveChangesAsync();

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

            var trade = await _db.Trades.FindAsync(id);
            if (trade == null)
            {
                return NotFound();
            }
            _db.Trades.Remove(trade);
            await _db.SaveChangesAsync();

            return RedirectToAction("Portfolios", "Portfolio");
        }
    }
}
