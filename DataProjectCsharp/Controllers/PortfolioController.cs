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
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DataProjectCsharp.Controllers
{
    [Authorize]
    public class PortfolioController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly string _userId;
        public PortfolioController(ApplicationDbContext db, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            this._db = db;
            this._userManager = userManager;
            this._userId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
        public IActionResult Portfolios()
        {
            var firstPortfolio = _db.Portfolios.FirstOrDefault(p => p.UserId == _userId);
            if (firstPortfolio == null)
            {
                return View();
            }
            else
            {
                List<Portfolio> allUserPortfolios = _db.Portfolios.Where(p => p.UserId == _userId).ToList();
                return View(allUserPortfolios);
            }
        }

        public async Task<IActionResult> PortfolioBreakdown(int? id)
        {
            
            if (id == null)
            {
                return NotFound();
            }

            Portfolio portfolio = await _db.Portfolios.FirstOrDefaultAsync(p => p.PortfolioId==id && p.UserId==_userId);
            if (portfolio == null)
            {
                return NotFound();
            }

            // At some point i will need to pass trades into here also.
            // Or maybe I WONT NEED TO. Portfolio has an icollection i can iterate thorugh
            return View(portfolio);
        }

        [HttpGet]
        public IActionResult AddPortfolio()
        {
            Portfolio portfolio = new Portfolio { };
            return PartialView("_PortfolioModalPartial", portfolio);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPortfolio([Bind("PortfolioId, Name, UserId")] Portfolio portfolio)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_PortfolioModalPartial", portfolio);
            }
            
            portfolio.UserId = _userId;
            var isDuplicatePortfolio = _db.Portfolios.Any(p => p.Name == portfolio.Name && p.UserId == _userId);
            if (!isDuplicatePortfolio)
            {
                _db.Portfolios.Add(portfolio);
                await _db.SaveChangesAsync();
                return PartialView("_PortfolioModalPartial", portfolio);
            }
            else
            {
                ModelState.AddModelError("Name", "You can't have two portfolios with the same name.");
                return PartialView("_PortfolioModalPartial", portfolio);
            }
        }

    }
}