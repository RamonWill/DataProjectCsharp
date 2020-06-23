using DataProjectCsharp.Data;
using DataProjectCsharp.Models;
using Microsoft.Data.Analysis;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DataProjectCsharp.Tests.DataObjectsTesting
{
    public class PositionFormulaTests
    {
        private readonly string testSymbol = "MSFT";

        public Trade CreateTransaction(long quantity, string symbol, decimal price, DateTime tradeDate)
        {
            Trade newTransaction = new Trade
            {
                Quantity = quantity,
                Ticker = symbol,
                Price = price,
                TradeDate = tradeDate
            };
            return newTransaction;
        }

        public DataFrame CreatePriceTable()
        {   
            PrimitiveDataFrameColumn<DateTime> dates = new PrimitiveDataFrameColumn<DateTime>("date");
            PrimitiveDataFrameColumn<decimal> prices = new PrimitiveDataFrameColumn<decimal>("price");
            for (int i = 1; i <= 10; i++)
            {
                dates.Append(new DateTime(2020, 5, i));
                prices.Append(i + 10.27m);
            }
            DataFrame priceTable = new DataFrame(dates, prices);
            return priceTable;
        }

        [Fact]
        public void TestDailyLongPositionPerformance()
        {
            PositionFormulas position = new PositionFormulas(testSymbol);
            Trade tradeA = CreateTransaction(500, testSymbol, 12.5m, new DateTime(2020, 5, 4));
            Trade tradeB = CreateTransaction(600, testSymbol, 15m, new DateTime(2020, 5, 4));
            position.AddTransaction(tradeA);
            position.AddTransaction(tradeB);

            DataFrame priceTable = CreatePriceTable();

            position.CalculateDailyPerformance(priceTable);

            decimal[] performances = new decimal[] { 2.931m, 10.145m, 17.358m, 24.571m, 31.784m, 38.997m, 46.210m };
            PrimitiveDataFrameColumn<decimal> performance = new PrimitiveDataFrameColumn<decimal>("pct_change", performances);

            Assert.Equal(performance, position.GetDailyPerformance().Columns["pct_change"]);
        }

        [Fact]
        public void TestDailyShortPositionPerformance()
        {
            PositionFormulas position = new PositionFormulas(testSymbol);
            Trade tradeA = CreateTransaction(-500, testSymbol, 12.5m, new DateTime(2020, 5, 4));
            Trade tradeB = CreateTransaction(-600, testSymbol, 15m, new DateTime(2020, 5, 4));
            position.AddTransaction(tradeA);
            position.AddTransaction(tradeB);

            DataFrame priceTable = CreatePriceTable();

            position.CalculateDailyPerformance(priceTable);

            decimal[] performances = new decimal[] { -2.931m, -10.145m, -17.358m, -24.571m, -31.784m, -38.997m, -46.210m };
            PrimitiveDataFrameColumn<decimal> performance = new PrimitiveDataFrameColumn<decimal>("pct_change", performances);

            Assert.Equal(performance, position.GetDailyPerformance().Columns["pct_change"]);
        }


        [Fact]
        public void TestDailyValuation()
        {
            // Two purchases on different days
            PositionFormulas position = new PositionFormulas(testSymbol);
            Trade tradeA = CreateTransaction(500, testSymbol, 12.5m, new DateTime(2020, 5, 4));
            Trade tradeB = CreateTransaction(600, testSymbol, 15m, new DateTime(2020, 5, 8));
            position.AddTransaction(tradeA);
            position.AddTransaction(tradeB);

            DataFrame priceTable = CreatePriceTable();
            position.CalculateDailyValuation(priceTable);

            decimal[] dailyValues = new decimal[] { 7135m, 7635m, 8135m, 8635m, 20097m, 21197m, 22297m };
            PrimitiveDataFrameColumn<decimal> dailyVals = new PrimitiveDataFrameColumn<decimal>($"{testSymbol}_MarketValue", dailyValues);

            Assert.Equal(dailyVals, position.GetDailyValuation().Columns[$"{testSymbol}_MarketValue"]);
        }
    }
}
