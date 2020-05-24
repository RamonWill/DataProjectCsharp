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
            Portfolio portfolio = _db.Portfolios.FirstOrDefault(p => p.PortfolioId == id && p.UserId == _userId);
            if (portfolio == null)
            {
                return NotFound();
            }
            List<Trade> allUserTrades = _db.Trades.Where(p => p.PortfolioId == id && p.UserId == _userId).ToList();
            // In the future i might need to use a portfolio instead and iterate over the icollection to form a table.
            return PartialView("_TradeViewModalPartial", allUserTrades);
        }
    }
}
