using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace DataProjectCsharp.Data
{
    public class Transaction
    {
        //this is temporary, delete soon it is just a reference to work with
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
        // current limitation transactions will need to be added by the correct date order for this to work.
        public readonly string symbol;
        private decimal averageCost { get; set; }
        private long netQuantity { get; set; }
        private bool isLong { get; set; }

        protected List<PositionSnapshot> positionBreakdown;
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
                throw new InvalidOperationException("The transaction ticker does not match the ticker of this position");
            }

            OpenLots lot = new OpenLots(transaction.TradeDate, transaction.quantity, transaction.price);

            // make sure a transaction cannot equal zero in my models
            // position has no trades
            if(transaction.quantity * netQuantity == 0)
            {
                this.isLong = (transaction.quantity >= 0);
            }

            //trades in diff direction to position
            else if (transaction.quantity * netQuantity < 0)
            {
                while (openLots.Count > 0 && transaction.quantity != 0)
                {
                    if(Math.Abs(transaction.quantity) >= Math.Abs(openLots.Peek().quantity))
                    {
                        transaction.quantity += openLots.Peek().quantity;
                        openLots.Pop();
                    }
                    else
                    {
                        openLots.Peek().quantity += transaction.quantity;
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

            UpdatePosition(transaction);
            // i need to update position regardless, 
            // i need update the closed lots regardless(give it another name like trade summary), 
            // if transaction.quantity!=0 need to push the lots)

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
        public List<PositionSnapshot> GetBreakdown()
        {
            return this.positionBreakdown;
        }

        private void UpdatePosition(Transaction transaction)
        {
            this.netQuantity = this.openLots.Sum(lots => lots.quantity);
            if (transaction.quantity==0 && this.openLots.Count > 0)
            {
                this.averageCost = this.openLots.Peek().price;
            }
            else
            {
                this.averageCost = GetAverageCost();
            }            
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
