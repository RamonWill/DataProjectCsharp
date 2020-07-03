using DataProjectCsharp.Data;
using DataProjectCsharp.Models;
using DataProjectCsharp.Models.Repository;
using Microsoft.Data.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace DataProjectCsharp.Services
{
    public class BusinessService : IBusinessService
    {
        private readonly IRepository _repo;
        public BusinessService(IRepository repo)
        {
            this._repo = repo;
        }

        public PortfolioData GetPortfolioData(string portfolioName, List<Trade> allTrades)
        {
            List<string> distinctTickers = allTrades.Select(t => t.Ticker).Distinct().ToList();

            PortfolioData userPortfolio = new PortfolioData(portfolioName);
            foreach (string ticker in distinctTickers)
            {
                PositionFormulas position = new PositionFormulas(ticker);
                foreach (Trade trade in allTrades)
                {
                    if (trade.Ticker == ticker)
                    {
                        position.AddTransaction(trade);
                    }
                }
                List<SecurityPrices> marketPrices = _repo.GetSecurityPrices(ticker);
                DataFrame marketPricesFrame = CreatePriceTable(marketPrices);

                position.CalculateDailyValuation(marketPricesFrame);
                userPortfolio.AddPositon(position);
            }
            return userPortfolio;
        }

        public PositionFormulas GetPositionData(int? portfolioId, string userId, string symbol)
        {
            
            List<Trade> allTradesBySymbol = _repo.GetTradesBySymbol(portfolioId, userId, symbol); 
            if (allTradesBySymbol.Count == 0)
            {
                return null;
            }
            
            // add the trades to the positon object.
            PositionFormulas position = new PositionFormulas(symbol);
            foreach(Trade trade in allTradesBySymbol)
            {
                position.AddTransaction(trade);
            }
            // get symbol prices from DB
            List<SecurityPrices> marketPrices = _repo.GetSecurityPrices(symbol);
            // make the below a private method - build price table
            // build DataFrame from Database Prices #
            DataFrame marketPricesFrame = CreatePriceTable(marketPrices);
            // #

            position.CalculateDailyPerformance(marketPricesFrame);
            return position;
        }

        public DataFrame GetPortfolioHPR(int? portfolioId, string userId)
        {
            List<Trade> allTrades = _repo.GetAllUserTrades(portfolioId, userId);
            //Creating flows for each trade at a position level.
            // Flows and Cash
            if (allTrades.Count == 0)
            {
                return null;
            }
            DataFrame flowFrame = CreateFlowTable(allTrades);


            //distinct tickers;
            string portfolioName = _repo.GetPortfolioName(portfolioId);
            PortfolioData userPortfolio = GetPortfolioData(portfolioName, allTrades);

            //#######
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
            int inflowIndex = portfolioValuation.Columns.Count - 1;
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
                decimal HPR = (((decimal)portfolioValuation[row, PortfolioValIndex]) / ((decimal)portfolioValuation[prevRow, PortfolioValIndex] + (decimal)portfolioValuation[row, inflowIndex]) - 1) * 100;
                portfolioValuation[row, hprIndex] = Math.Round(HPR, 3);
            }


            // This is HPR performance indexed
            PrimitiveDataFrameColumn<decimal> HPRindexed = new PrimitiveDataFrameColumn<decimal>("Holding Period Return Indexed", pvSize);
            portfolioValuation.Columns.Add(HPRindexed);
            int HPRi = portfolioValuation.Columns.Count - 1;

            portfolioValuation[0, HPRi] = 100m; //initial index.
            for (int row = 1; row < pvSize; row++)
            {
                int prevRow = row - 1;
                decimal HPRx = (decimal)portfolioValuation[prevRow,HPRi] *(((decimal)portfolioValuation[row,hprIndex]/100)+1);
                portfolioValuation[row, HPRi] = Math.Round(HPRx, 3);
            }

            System.Diagnostics.Debug.WriteLine(portfolioValuation);
            return portfolioValuation;
        }

        private DataFrame CreatePriceTable(List<SecurityPrices> securityPrices)
        {
            List<DateTime> dates = securityPrices.Select(sp => sp.Date).ToList();
            List<decimal> prices = securityPrices.Select(sp => sp.ClosePrice).ToList();

            PrimitiveDataFrameColumn<DateTime> dateCol = new PrimitiveDataFrameColumn<DateTime>("date", dates);
            PrimitiveDataFrameColumn<decimal> priceCol = new PrimitiveDataFrameColumn<decimal>("price", prices);
            DataFrame marketPricesFrame = new DataFrame(dateCol, priceCol);
            return marketPricesFrame;
        }

        private DataFrame CreateFlowTable(List<Trade> allTrades)
        {
            int otherlength = allTrades.Count;
            PrimitiveDataFrameColumn<DateTime> flowDate = new PrimitiveDataFrameColumn<DateTime>("date", otherlength);
            PrimitiveDataFrameColumn<decimal> cashCol = new PrimitiveDataFrameColumn<decimal>("cash", otherlength);
            PrimitiveDataFrameColumn<decimal> inflowCol = new PrimitiveDataFrameColumn<decimal>("inflow", otherlength);
            DataFrame flowFrame = new DataFrame(flowDate, cashCol, inflowCol);
            int rowRef = 0;
            decimal lastcashvalue = Decimal.Zero; // cumsum for cash column
            foreach (Trade trade in allTrades)
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
                    flowFrame[rowRef, 1] = Math.Abs(TradeAmount) + lastcashvalue;
                    lastcashvalue = (decimal)flowFrame[rowRef, 1];
                    flowFrame[rowRef, 2] = Decimal.Zero;
                }
                rowRef++;
            }
            GroupBy groupBy = flowFrame.GroupBy("date");
            flowFrame = groupBy.Sum();
            return flowFrame;
        }

    }
}
