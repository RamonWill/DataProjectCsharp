using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DataProjectCsharp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DataProjectCsharp.Controllers
{
    [Authorize]
    public class PortfolioController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        public PortfolioController(ApplicationDbContext db, UserManager<User> userManager)
        {
            this._db = db;
            this._userManager = userManager;
        }
        public IActionResult Performance()
        {
            return View();
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

            portfolio.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isDuplicatePortfolio = _db.Portfolios.Any(x => x.Name == portfolio.Name && x.UserId == portfolio.UserId);
            if (!isDuplicatePortfolio)
            {
                _db.Portfolios.Add(portfolio);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Performance));
            }
            else
            {
                ModelState.AddModelError("Name", "You can't have two portfolios with the same name.");
                return PartialView("_PortfolioModalPartial", portfolio);
            }
        }
    }
}