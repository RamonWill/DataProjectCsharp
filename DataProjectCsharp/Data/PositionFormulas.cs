using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Analysis;

namespace DataProjectCsharp.Data
{
    public class PositionFormulas:Position
    {
        private DataFrame PerformanceTable;
        private DataFrame ValuationTable;
        public PositionFormulas(string symbol) : base(symbol)
        {
            this.PerformanceTable = new DataFrame();
            this.ValuationTable = new DataFrame();
        }

        public DataFrame GetDailyPerformance()
        {
            return this.PerformanceTable;
        }

        public DataFrame GetDailyValuation()
        {
            return this.ValuationTable;
        }
        // Via inheritance i have all the position class attributes and methods.
        public void CalculateDailyPerformance(DataFrame prices)
        {
            if (this.PerformanceTable.Columns.Count != 0)
            {
                this.PerformanceTable = new DataFrame();
            }


            int pbRows = this.positionBreakdown.Count;
            if (pbRows == 0) //position is empty
            {
                return; //this.PerformanceTable;
            }

            /// Take all prices from Database and columns with them and add those columns to the performance table
            // then add new columns for the dataframe
            // modify dataframe inplace.
            //this.PerformanceTable = prices;
            DataFrame NewTable = prices;
            long numberOfRows = NewTable.Rows.Count;
            PrimitiveDataFrameColumn<long> quantity = new PrimitiveDataFrameColumn<long>("Quantity", numberOfRows);
            PrimitiveDataFrameColumn<decimal> averageCost = new PrimitiveDataFrameColumn<decimal>("AverageCost", numberOfRows);
            PrimitiveDataFrameColumn<int> LongShort = new PrimitiveDataFrameColumn<int>("Long/Short", numberOfRows);
            PrimitiveDataFrameColumn<decimal> pctChange = new PrimitiveDataFrameColumn<decimal>("pct_change", numberOfRows);

            NewTable.Columns.Add(quantity);
            NewTable.Columns.Add(averageCost);
            NewTable.Columns.Add(LongShort);
            NewTable.Columns.Add(pctChange);
            
            // Column indices
            int dateCol = 0;
            int priceCol = 1;
            int quantityCol = 2;
            int averageCostCol = 3;
            int signCol = 4;
            int percentChangeCol = 5;


            // since merge and join arent ready yet use the below.
            int counter = 0;
            for(int row=0; row<numberOfRows; row++)
            {
                if (counter == pbRows)
                {
                    break;
                }

                if(NewTable[row, dateCol].Equals(this.positionBreakdown[counter].date))
                {
                    NewTable[row, quantityCol] = this.positionBreakdown[counter].quantity;
                    NewTable[row, averageCostCol] = Math.Round(this.positionBreakdown[counter].averageCost, 4);
                    NewTable[row, signCol] = (this.positionBreakdown[counter].quantity > 0) ? 1 : -1;
                    counter++;
                }
            }

            // since fill nulls cant forwardfill use the below.
            bool toFill = false;
            long prevQuantity = 0;
            decimal prevAverageCost = 0;
            int prevLongShort = 0;

            for(int row=0; row < numberOfRows; row++)
            {
                if ((NewTable[row, quantityCol] != null && !toFill) || (NewTable[row, quantityCol] != null && toFill))
                {
                    toFill = true;
                    prevQuantity = (long)NewTable[row, quantityCol];
                    prevAverageCost = (decimal)NewTable[row, averageCostCol];
                    prevLongShort = (int)NewTable[row, signCol];
                }
                else if (NewTable[row, quantityCol] == null && toFill)
                {
                    NewTable[row, quantityCol] = prevQuantity;
                    NewTable[row, averageCostCol] = prevAverageCost;
                    NewTable[row, signCol] = prevLongShort;
                }
            }


            // if the quantity is still null. make it long.MaxValue and then creates a new dataframe with that filtered info
            PrimitiveDataFrameColumn<bool> boolFilter = NewTable.Columns.GetPrimitiveColumn<long>("Quantity").FillNulls(long.MaxValue,true).ElementwiseNotEquals(long.MaxValue);
            NewTable = NewTable.Filter(boolFilter);
            numberOfRows = NewTable.Rows.Count;
            for(int row = 0; row < numberOfRows; row++)
            {
                // if cost is zero (user closed position) then i need to change this to null...
                decimal price = Convert.ToDecimal(NewTable[row, priceCol]);
                decimal cost = (decimal)NewTable[row, averageCostCol];
                int sign = (int)NewTable[row, signCol];
                NewTable[row, percentChangeCol] = Math.Round(((price / cost) - 1) * 100 * sign, 3);
            }

            this.PerformanceTable = NewTable;
        }

        public void CalculateDailyValuation(DataFrame prices)
        {
            if (this.ValuationTable.Columns.Count != 0)
            {
                this.ValuationTable = new DataFrame();
            }

            int pbRows = this.positionBreakdown.Count;
            if (pbRows == 0) //position is empty
            {
                return;
            }

            /// Take all prices from Database and columns with them and add those columns to the valuation table
            // then add new columns for the dataframe
            // modifies dataframe inplace.

            DataFrame NewTable = prices;
            long numberOfRows = NewTable.Rows.Count;
            //long rowsnumber = prices.Rows.Count;
            // if i was using an actual datetime from the database i could use the filter.
            string title = $"{this.symbol}_MarketValue";

            PrimitiveDataFrameColumn<long> quantity = new PrimitiveDataFrameColumn<long>("Quantity", numberOfRows);
            PrimitiveDataFrameColumn<decimal> marketValue = new PrimitiveDataFrameColumn<decimal>(title, numberOfRows);
            NewTable.Columns.Add(quantity);
            NewTable.Columns.Add(marketValue);

            // since merge and join arent ready yet use the below.
            int counter = 0;

            int dateCol = 0;
            int priceCol = 1;
            int quantityCol = 2;
            int marketValueCol = 3;

            for(int row = 0; row < numberOfRows; row++)
            {
                if (counter == pbRows)
                {
                    break;
                }
                if (NewTable[row, dateCol].Equals(this.positionBreakdown[counter].date))
                {
                    NewTable[row, quantityCol] = this.positionBreakdown[counter].quantity;
                    counter++;
                }
            }

            // since fill nulls cant forwardfill use the below.
            bool toFill = false;
            long prevQuantity = 0;

            for(int row=0; row < numberOfRows; row++)
            {
                if((NewTable[row,quantityCol] != null && !toFill) || (NewTable[row, quantityCol] != null && toFill))
                {
                    toFill = true;
                    prevQuantity = (long)NewTable[row, quantityCol];
                }
                else if (NewTable[row, quantityCol] == null && toFill)
                {
                    NewTable[row, quantityCol] = prevQuantity;
                }
            }

            // if the quantity is still null. make it long.MaxValue and then creates a new dataframe with that filtered info
            PrimitiveDataFrameColumn<bool> boolFilter = NewTable.Columns.GetPrimitiveColumn<long>("Quantity").FillNulls(long.MaxValue).ElementwiseNotEquals(long.MaxValue);
            NewTable = NewTable.Filter(boolFilter);
            numberOfRows = NewTable.Rows.Count;
            for(int row=0; row < numberOfRows; row++)
            {
                decimal price = Convert.ToDecimal(NewTable[row, priceCol]);
                long units = (long)NewTable[row, quantityCol]; //quantity
                NewTable[row, marketValueCol] = Math.Round(price * units, 4);
            }

            NewTable.Columns.Remove("price");
            NewTable.Columns.Remove("Quantity");
            this.ValuationTable = NewTable;
        }
    }
}
