using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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

        public TradeController(ApplicationDbContext db, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            this._db = db;
            this._userManager = userManager;
            this._userId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
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
            return PartialView("_TradeEntryModalParial", trade);
        }
    }
}
