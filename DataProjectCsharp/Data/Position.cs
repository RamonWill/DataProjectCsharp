using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace DataProjectCsharp.Data
{
    public class Transaction
    {
        //this is temporary, delete soon
        public string ticker = "VENOM";
        public long quantity = 99;
        public decimal price = 12.3m;
        public DateTime TradeDate = DateTime.Now;
    }

    public class PositionSnapshot
    {
        public DateTime date;
        public long quantity;
        public decimal averageCost;
        public PositionSnapshot(DateTime date, long quantity, decimal averageCost)
        {
            this.date = date;
            this.quantity = quantity;
            this.averageCost = averageCost;
        }
    }

    public class OpenLots
    {
        public DateTime date;
        public long quantity;
        public decimal price;
        public OpenLots(DateTime date, long quantity, decimal price)
        {
            this.date = date;
            this.quantity = quantity;
            this.price = price;
        }

        public decimal GetMarketValue()
        {
            return this.quantity * this.price;
        }
    }

    public class Position
    {
        // this object is comprised of a list of transactions. I can update the position object with transactions. Position.AddTransaction(Transaction)
        // current limitation transactions will need to be added by the correct date for this to work.
        private string symbol { get; set; }
        private decimal averageCost { get; set; }
        private long netQuantity { get; set; }
        private bool isLong { get; set; }

        private List<PositionSnapshot> positionBreakdown;
        private Stack<OpenLots> openLots;

        public Position(string symbol)
        {
            this.symbol = symbol;
            this.averageCost = 0;
            this.netQuantity = 0;
            this.positionBreakdown = new List<PositionSnapshot>();
            this.openLots = new Stack<OpenLots>();
        }


        public void AddTransaction(Transaction transaction)
        {
            if (transaction.ticker != this.symbol)
            {
                throw new InvalidOperationException("The transaction ticker does not match this positions ticker");
            }

            OpenLots lot = new OpenLots(transaction.TradeDate, transaction.quantity, transaction.price);

            // make sure a transaction cannot equal zero
            if(transaction.quantity * netQuantity == 0)
            {
                this.isLong = (transaction.quantity >= 0);
                openLots.Push(lot);
            }
            //trades in diff direction
            else if (transaction.quantity * netQuantity < 0)
            {
                // long 100
                while (openLots.Count > 0 && transaction.quantity != 0)
                {
                    if(Math.Abs(transaction.quantity) >= Math.Abs(openLots.Peek().quantity))
                    {
                        transaction.quantity -= openLots.Peek().quantity;
                        openLots.Pop();
                    }
                    else
                    {
                        openLots.Peek().quantity -= transaction.quantity;
                        transaction.quantity = 0;
                    }
                }
                if (transaction.quantity != 0)
                {
                    lot.quantity = transaction.quantity;
                }

            }
            if (transaction.quantity != 0)
            {
                openLots.Push(lot);
            }
            /// i need to update position regardless, 
            /// i need update the closed lots regardless(give it another name like trade summary), 
            /// if transaction.quantity!=0 need to push the lots)
            UpdatePosition(transaction);
            //when all said and done, net position should equal total open lots
        }

        public decimal GetTotalMarketValue()
        {
            decimal result = 0;
            if (this.openLots.Count > 0)
            {
                foreach (OpenLots lot in this.openLots)
                {
                    result += lot.GetMarketValue();
                }
            }
            return result;
        }

        private void UpdatePosition(Transaction transaction)
        {
            this.netQuantity = this.openLots.Sum(lots => lots.quantity);
            this.averageCost = GetAverageCost();
            CheckDirection();
            AppendBreakdown(transaction.TradeDate);
            
        }

        private void AppendBreakdown(DateTime tradeDate)
        {
            PositionSnapshot snapshot = new PositionSnapshot(tradeDate, this.netQuantity, this.averageCost);
            this.positionBreakdown.Add(snapshot);
        }

        private decimal GetAverageCost()
        {
            decimal marketValue = GetTotalMarketValue();

            return marketValue/this.netQuantity;
        }

        private void CheckDirection()
        {
            if (this.netQuantity < 0 && this.isLong)
            {
                this.isLong = false;
            }
            else if (this.netQuantity > 0 && !this.isLong)
            {
                this.isLong = true;
            }
        }
    }
}
