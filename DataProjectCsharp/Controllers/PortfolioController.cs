using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DataProjectCsharp.Data;
using DataProjectCsharp.Models;
using DataProjectCsharp.Models.Repository;
using DataProjectCsharp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Analysis;
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
        private readonly IRepository _repo;
        private IBusinessService _service;
        public PortfolioController(IRepository repo, ApplicationDbContext db, UserManager<User> userManager, 
                            IHttpContextAccessor httpContextAccessor)
        {
            this._service = new BusinessService(repo);
            this._repo = repo;
            this._db = db;
            this._userManager = userManager;
            this._userId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
        public IActionResult Portfolios()
        {
            bool hasPortfolio = _repo.PortfoliosExists(_userId);
            if (!hasPortfolio)
            {
                return View();
            }
            else
            {
                List<Portfolio> allUserPortfolios = _repo.GetAllUserPortfolios(_userId);
                return View(allUserPortfolios);
            }
        }
        public  IActionResult PositionBreakdown(int? portfolioId, string positionSymbol)
        {

            if (portfolioId == null || positionSymbol==null)
            {
                return NotFound();
            }

            DataFrame positionPerformance = _service.GetPositionPerformance(portfolioId, _userId, positionSymbol);
            if (positionPerformance == null)
            {
                return NotFound();
            }
            return View(positionPerformance);
        }

        public async Task<IActionResult> PortfolioBreakdown(int? id)
        {
            
            if (id == null)
            {
                return NotFound();
            }
            // eager loading
            Portfolio portfolio = await _db.Portfolios
                         .Where(p => p.PortfolioId == id && p.UserId == _userId)
                         .Include(p => p.Trades)
                         .FirstOrDefaultAsync();
            if (portfolio == null)
            {
                return NotFound();
            }
            // will i need to load in all the trades(ordered by date and then ticker), create a position obj and then all the prices for the calcs?
            // the add them all into a portfolio? and then the valuation for the portfolio..
            // get a list of tradenames. create a position with it. for each name get all trades. add them to posiiton, then get security price and thus valuation, then add to portfolio object

            //###################################################################
            // builds the market valuation
            // maybe make this an object that you put in a view model.
            DataFrame portfolioHPR = _service.GetPortfolioHPR(id, _userId);

            // i might just have to pass in the portfolio object to the view... maybe i can use a viewmodel.

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
            bool isDuplicatePortfolio = _repo.IsDuplicatePortfolio(portfolio.Name, _userId);
            if (!isDuplicatePortfolio)
            {
                _repo.AddPortfolio(portfolio);
                await _repo.SaveChangesAsync();
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