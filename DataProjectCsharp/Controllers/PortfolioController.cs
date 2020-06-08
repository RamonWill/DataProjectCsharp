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
                List<Portfolio> allUserPortfolios =  _db.Portfolios
                                         .Where(p =>  p.UserId == _userId)
                                         .Include(p => p.Trades)
                                         .ToList();
                return View(allUserPortfolios);
            }
        }
        public async Task<IActionResult> PositionBreakdown(int? portfolioId, string positionSymbol)
        {

            if (portfolioId == null || positionSymbol==null)
            {
                return NotFound();
            }
            // eager loading

            // I will need to check if the trades are available first at some point i.e. use first or default.
            // needs to include order by date
            List<Trade> allTrades =  await _db.Trades
                         .Where(t => t.PortfolioId == portfolioId && t.UserId == _userId && t.Ticker == positionSymbol)
                         .ToListAsync();
            if (allTrades == null)
            {
                return NotFound();
            }

            PositionFormulas position = new PositionFormulas(positionSymbol);
            foreach (Trade trade in allTrades)
            {
                position.AddTransaction(trade);
            }
            // ######################
            // creating my dataframe for the prices
            List<SecurityPrices> prices = await _db.SecurityPrices.Where(t => t.ticker == positionSymbol).OrderBy(t=> t.date).ToListAsync();
            int size = prices.Count;
            PrimitiveDataFrameColumn<DateTime> dateCol = new PrimitiveDataFrameColumn<DateTime>("date", size);
            PrimitiveDataFrameColumn<decimal> priceCol = new PrimitiveDataFrameColumn<decimal>("price", size);
            DataFrame pricesFrame = new DataFrame(dateCol, priceCol);
            int counter = 0;
            foreach(var row in pricesFrame.Rows)
            {
                row[0] = prices[counter].date;
                row[1] = prices[counter].ClosePrice;
                counter++;
            }
            position.CalculateDailyPerformance(pricesFrame);
           
            // #################################
            return View(position.GetDailyPerformance());
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

            List<Trade> allTrades = await _db.Trades
                                             .Where(t => t.PortfolioId == id && t.UserId == _userId)
                                             .OrderBy(t => t.TradeDate)
                                             .ToListAsync();
            // distinct tickers
            List<string> tickers = new List<string>();
            foreach (Trade trade in allTrades)
            {
                if (!tickers.Contains(trade.Ticker))
                {
                    tickers.Add(trade.Ticker);
                }
            }
            PortfolioData userPortfolio = new PortfolioData();
            foreach(string ticker in tickers)
            {
                PositionFormulas position = new PositionFormulas(ticker);
                foreach(Trade trade in allTrades)
                {
                    if (trade.Ticker == ticker)
                    {
                        position.AddTransaction(trade);
                    }
                }
                List<SecurityPrices> prices = await _db.SecurityPrices.Where(t => t.ticker == ticker).OrderBy(t => t.date).ToListAsync();
                int size = prices.Count;
                PrimitiveDataFrameColumn<DateTime> dateCol = new PrimitiveDataFrameColumn<DateTime>("date", size);
                PrimitiveDataFrameColumn<decimal> priceCol = new PrimitiveDataFrameColumn<decimal>("price", size);
                DataFrame pricesFrame = new DataFrame(dateCol, priceCol);
                int counter = 0;
                foreach (var row in pricesFrame.Rows)
                {
                    row[0] = prices[counter].date;
                    row[1] = prices[counter].ClosePrice;
                    counter++;
                }
                position.CalculateDailyValuation(pricesFrame);
                userPortfolio.AddPositon(position);
            }
            System.Diagnostics.Debug.WriteLine(userPortfolio.GetValuation());
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