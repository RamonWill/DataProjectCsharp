using DataProjectCsharp.Data;
using DataProjectCsharp.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DataProjectCsharp.Tests.DataObjectsTesting
{
    public class PositionTests
    {
        private readonly string  testSymbol = "MSFT";

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

        [Fact]
        public void TestTwoLongTrades()
        {
            // The user executes 2 buy trades
            Position position = new Position(testSymbol);
            Trade tradeA = CreateTransaction(500, testSymbol, 12.5m, new DateTime (2020, 5, 4));
            Trade tradeB = CreateTransaction(300, testSymbol, 17.6m, new DateTime(2020, 5, 5));
            position.AddTransaction(tradeA);
            position.AddTransaction(tradeB);
            Assert.Equal(800, position.NetQuantity);
            Assert.Equal(14.4125m, position.AverageCost);

            // expected breakdown
            List<PositionSnapshot> breakdown = new List<PositionSnapshot>
            {
                new PositionSnapshot(new DateTime(2020,5,4), 500, 12.5m),
                new PositionSnapshot(new DateTime(2020,5,5), 800, 14.4125m)
            };
            Assert.Equal(breakdown, position.GetBreakdown());
        }

        [Fact]
        public void TestTwoShortTradesSameDay()
        {
            // the user executes 2 sell trades on the same day
            Position position = new Position(testSymbol);
            Trade tradeA = CreateTransaction(-450, testSymbol, 15.1m, new DateTime(2020, 5, 4));
            Trade tradeB = CreateTransaction(-157, testSymbol, 10.22m, new DateTime(2020, 5, 4));
            position.AddTransaction(tradeA);
            position.AddTransaction(tradeB);
            Assert.Equal(-607, position.NetQuantity);
            Assert.Equal(13.83779242m, Math.Round(position.AverageCost,8));

            // expected breakdown
            List<PositionSnapshot> breakdown = new List<PositionSnapshot>
            {
                new PositionSnapshot(new DateTime(2020,5,4), -607, 13.837792421746293245469522241m)
            };
            Assert.Equal(breakdown, position.GetBreakdown());
        }

        [Fact]
        public void TestOneLongTradeThenShort()
        {
            // the user executes a buy trade and then a sell trade
            Position position = new Position(testSymbol);
            Trade tradeA = CreateTransaction(300, testSymbol, 15m, new DateTime(2020, 5, 4));
            Trade tradeB = CreateTransaction(-200, testSymbol, 5.5m, new DateTime(2020, 5, 5));
            position.AddTransaction(tradeA);
            position.AddTransaction(tradeB);
            Assert.Equal(100, position.NetQuantity);
            Assert.Equal(15m, position.AverageCost);

            // expected breakdown
            List<PositionSnapshot> breakdown = new List<PositionSnapshot>
            {
                new PositionSnapshot(new DateTime(2020,5,4), 300, 15m),
                new PositionSnapshot(new DateTime(2020,5,5), 100, 15m)
            };
            Assert.Equal(breakdown, position.GetBreakdown());
        }

        [Fact]
        public void TestOneShortTradeThenLong()
        {
            // the user executes a sell trade and then a buy trade
            Position position = new Position(testSymbol);
            Trade tradeA = CreateTransaction(-300, testSymbol, 15m, new DateTime(2020, 5, 4));
            Trade tradeB = CreateTransaction(100, testSymbol, 10m, new DateTime(2020, 5, 5));
            position.AddTransaction(tradeA);
            position.AddTransaction(tradeB);
            Assert.Equal(-200, position.NetQuantity);
            Assert.Equal(15m, position.AverageCost);

            // expected breakdown
            List<PositionSnapshot> breakdown = new List<PositionSnapshot>
            {
                new PositionSnapshot(new DateTime(2020,5,4), -300, 15m),
                new PositionSnapshot(new DateTime(2020,5,5), -200, 15m)
            };
            Assert.Equal(breakdown, position.GetBreakdown());
        }

        [Fact]
        public void TestTradeToFlattenPosition()
        {
            // the user executes a buy trade then a sell trade to flatten the position
            Position position = new Position(testSymbol);
            Trade tradeA = CreateTransaction(300, testSymbol, 25m, new DateTime(2020, 5, 4));
            Trade tradeB = CreateTransaction(-300, testSymbol, 10m, new DateTime(2020, 5, 5));
            position.AddTransaction(tradeA);
            position.AddTransaction(tradeB);
            Assert.Equal(0, position.NetQuantity);
            Assert.Equal(0, position.AverageCost);

            // expected breakdown
            List<PositionSnapshot> breakdown = new List<PositionSnapshot>
            {
                new PositionSnapshot(new DateTime(2020,5,4), 300, 25m),
                new PositionSnapshot(new DateTime(2020,5,5), 0, 0m)
            };
            Assert.Equal(breakdown, position.GetBreakdown());
        }

        [Fact]
        public void TestLongPositionToShortPosition()
        {
            /* the user executes a buy trade. 
             * the user then executes a sell trade to make the overall position negative.
             * the user then executes another buy trade to make the overall position positive
            */
            Position position = new Position(testSymbol);
            Trade tradeA = CreateTransaction(300, testSymbol, 25m, new DateTime(2020, 5, 4));
            Trade tradeB = CreateTransaction(-500, testSymbol, 10m, new DateTime(2020, 5, 5));
            Trade tradeC = CreateTransaction(600, testSymbol, 15m, new DateTime(2020, 5, 6));
            position.AddTransaction(tradeA);
            position.AddTransaction(tradeB);
            position.AddTransaction(tradeC);
            Assert.Equal(400, position.NetQuantity);
            Assert.Equal(15m, position.AverageCost);

            // expected breakdown
            List<PositionSnapshot> breakdown = new List<PositionSnapshot>
            {
                new PositionSnapshot(new DateTime(2020,5,4), 300, 25m),
                new PositionSnapshot(new DateTime(2020,5,5), -200, 10m),
                new PositionSnapshot(new DateTime(2020,5,6), 400, 15m),
            };
            Assert.Equal(breakdown, position.GetBreakdown());
        }

        [Fact]
        public void TestMismatchedTradeSymbol()
        {
            // if for some reason code is written that tries to add a trade from a different security into a position it should throw an exception
            Position position = new Position(testSymbol);
            Trade tradeA = CreateTransaction(300, "AAPL", 25m, new DateTime(2020, 5, 4));
            Assert.Throws<InvalidOperationException>(()=>position.AddTransaction(tradeA));
        }
    }
}
