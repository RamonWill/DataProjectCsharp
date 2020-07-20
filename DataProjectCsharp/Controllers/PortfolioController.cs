using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DataProjectCsharp.Data;
using DataProjectCsharp.Models;
using DataProjectCsharp.Models.DataViewModels;
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
        
        private readonly string _userId;
        private readonly IRepository _repo;
        private readonly IBusinessService _service;
        public PortfolioController(IRepository repo, IHttpContextAccessor httpContextAccessor)
        {
            this._service = new BusinessService(repo);
            this._repo = repo;
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

            PositionFormulas position = _service.GetPositionData(portfolioId, _userId, positionSymbol);
            if (position == null)
            {
                return NotFound();
            }
            string portfolioName = _repo.GetPortfolioName(portfolioId);
            TradeableSecurities securityDetail = _repo.GetSecurityDetails(positionSymbol);

            PositionDataVM positionVM = new PositionDataVM { PortfolioId = portfolioId, 
                                                             PortfolioName = portfolioName, 
                                                             PositionObject = position,
                                                             PositionSymbolData = securityDetail};

            return View(positionVM);
        }

        public IActionResult PortfolioBreakdown(int? id)
        {
            
            if (id == null)
            {
                return NotFound();
            }

            bool portfolioCheck = _repo.UserPortfolioValidation(id, _userId);

            if (!portfolioCheck)
            {
                return NotFound();
            }

            List<Trade> allTrades = _repo.GetAllUserTrades(id, _userId);
            string portfolioName = _repo.GetPortfolioName(id);
            PortfolioData userPortfolio = _service.GetPortfolioData(portfolioName, allTrades);

            DataFrame portfolioHPR = _service.GetPortfolioHPR(id, _userId); 


            PortfolioDataVM portfolioVM = new PortfolioDataVM { PortfolioId = id, PortfolioObject = userPortfolio, HoldingPeriodReturn=portfolioHPR };


            return View(portfolioVM);
        }


        [HttpGet]
        public IActionResult ShowTradeableSecurities()
        {
            List<TradeableSecurities> allSecurities = _repo.GetTradeableSecurities();
            return PartialView("_TradeableSecuritiesModalPartial", allSecurities);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePortfolio(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Portfolio portfolio = _repo.GetUserPortfolio(id, _userId);
            if (portfolio == null)
            {
                return NotFound();
            }


            _repo.RemovePortfolio(portfolio);
            await _repo.SaveChangesAsync();

            return RedirectToAction("Portfolios", "Portfolio");
        }

    }
}