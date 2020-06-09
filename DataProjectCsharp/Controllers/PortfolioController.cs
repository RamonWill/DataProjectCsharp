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
                         .OrderBy(t=>t.TradeDate)
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
           
            // creating my dataframe for the position
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

            //###################################################################
            // builds the market valuation
            List<Trade> allTrades = await _db.Trades
                                             .Where(t => t.PortfolioId == id && t.UserId == _userId)
                                             .OrderBy(t => t.TradeDate)
                                             .ToListAsync();

            //Creating flows for each trade at a position level.
            // Flows and Cash
            int otherlength = allTrades.Count;
            PrimitiveDataFrameColumn<DateTime> flowDate = new PrimitiveDataFrameColumn<DateTime>("date", otherlength);
            PrimitiveDataFrameColumn<decimal> cashCol = new PrimitiveDataFrameColumn<decimal>("cash", otherlength);
            PrimitiveDataFrameColumn<decimal> inflowCol = new PrimitiveDataFrameColumn<decimal>("inflow", otherlength);
            DataFrame flowFrame = new DataFrame(flowDate, cashCol, inflowCol);
            int rowRef = 0;
            decimal lastcashvalue = Decimal.Zero; // cumsum for cash column
            foreach(Trade trade in allTrades)
            {
                decimal TradeAmount = trade.Quantity * trade.Price;
                flowFrame[rowRef, 0] = trade.TradeDate;
                if (TradeAmount > 0)
                {
                    //trade is a purchase, set inflow
                    flowFrame[rowRef, 1] = lastcashvalue;
                    flowFrame[rowRef, 2] = TradeAmount;
                }
                else
                {
                    //trade is a sell, set cash
                    flowFrame[rowRef, 1] = Math.Abs(TradeAmount)+ lastcashvalue;
                    lastcashvalue = (decimal)flowFrame[rowRef, 1];
                    flowFrame[rowRef, 2] = Decimal.Zero;
                }
                rowRef++;
            }
            GroupBy groupBy = flowFrame.GroupBy("date");
            flowFrame = groupBy.Sum();
            System.Diagnostics.Debug.WriteLine(flowFrame);

            // cash column is a cumulative sum
            // group by date then sum

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

            //############################################################################
            DataFrame portfolioValuation = userPortfolio.GetValuation();
            int pvSize = portfolioValuation.Rows.Count();
            
            PrimitiveDataFrameColumn<decimal> cashCol2 = new PrimitiveDataFrameColumn<decimal>("cash", pvSize);
            PrimitiveDataFrameColumn<decimal> inflowCol2 = new PrimitiveDataFrameColumn<decimal>("inflow", pvSize);
            portfolioValuation.Columns.Add(cashCol2);
            portfolioValuation.Columns.Add(inflowCol2);
            //populate with flowFrame data

            int secondaryRow = 0;
            int valueIndex = portfolioValuation.Columns.Count - 3;
            int cashIndex = portfolioValuation.Columns.Count - 2;
            int inflowIndex = portfolioValuation.Columns.Count-1;
            for (int row = 0; row < pvSize; row++)
            {
                if (secondaryRow == flowFrame.Rows.Count)
                {
                    break;
                }
                if (secondaryRow < flowFrame.Rows.Count)
                {
                    if (portfolioValuation[row, 0].Equals(flowFrame[secondaryRow, 0]))
                    {
                        // if the dates match
                        portfolioValuation[row, cashIndex] = flowFrame[secondaryRow, 1];
                        portfolioValuation[row, inflowIndex] = flowFrame[secondaryRow, 2];
                        secondaryRow++;
                    }
                }
                else
                {
                    portfolioValuation[row, cashIndex] = Decimal.Zero;
                    portfolioValuation[row, inflowIndex] = Decimal.Zero;
                }
            }
            //forwardfill cashcolumn then replace null with Decimal.Zero
            bool toFill = false;
            decimal prevCash = decimal.Zero;
            System.Diagnostics.Debug.WriteLine(portfolioValuation);
            for (int row = 0; row < pvSize; row++)
            {

                if (((portfolioValuation[row, cashIndex] != null && (decimal?)portfolioValuation[row, cashIndex] != Decimal.Zero) && !toFill) || 
                    ((portfolioValuation[row, cashIndex] != null && (decimal?)portfolioValuation[row, cashIndex] != Decimal.Zero) && toFill))
                {
                    toFill = true;
                    prevCash = (decimal)portfolioValuation[row, cashIndex];

                }
                else if ((portfolioValuation[row, cashIndex] == null || (decimal?)portfolioValuation[row, cashIndex] == Decimal.Zero) && toFill)
                {
                    portfolioValuation[row, cashIndex] = prevCash;

                }
            }
            portfolioValuation.Columns.GetPrimitiveColumn<decimal>("cash").FillNulls(Decimal.Zero, true);
            portfolioValuation.Columns.GetPrimitiveColumn<decimal>("inflow").FillNulls(Decimal.Zero, true);
            // get total portfolio value

            // 
            
            PrimitiveDataFrameColumn<decimal> PortfolioVal = new PrimitiveDataFrameColumn<decimal>("PortfolioValue", pvSize);
            portfolioValuation.Columns.Add(PortfolioVal);
            int PortfolioValIndex = portfolioValuation.Columns.Count - 1;
            for (int row = 0; row < pvSize; row++)
            {
                 portfolioValuation[row, PortfolioValIndex] = (decimal)portfolioValuation[row, cashIndex] + (decimal)portfolioValuation[row, valueIndex];
            }

            PrimitiveDataFrameColumn<decimal> HPRcol = new PrimitiveDataFrameColumn<decimal>("Holding Period Return", pvSize);
            portfolioValuation.Columns.Add(HPRcol);
            int hprIndex = portfolioValuation.Columns.Count - 1;

            for (int row = 1; row < pvSize; row++)
            {
                int prevRow = row - 1;
                portfolioValuation[row, hprIndex] = (((decimal)portfolioValuation[row, PortfolioValIndex]) / ((decimal)portfolioValuation[prevRow, PortfolioValIndex] + (decimal)portfolioValuation[prevRow, cashIndex])-1)*100;
            }

            System.Diagnostics.Debug.WriteLine(portfolioValuation);

            // So im thinking is to have a repository where i can access the database. then have a business logic interface where i can build objects based on repository..
            // ..using helper methods
            // then i can simply call these objects from the controller.

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