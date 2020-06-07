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
        // Via inheritance i have inherited all the of position class attributes and methods.
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
            // modifies dataframe inplace.

            long numberOfRows = prices.Rows.Count;
            PrimitiveDataFrameColumn<long> quantity = new PrimitiveDataFrameColumn<long>("Quantity", numberOfRows);
            PrimitiveDataFrameColumn<decimal> averageCost = new PrimitiveDataFrameColumn<decimal>("AverageCost", numberOfRows);
            PrimitiveDataFrameColumn<int> LongShort = new PrimitiveDataFrameColumn<int>("Long/Short", numberOfRows);
            PrimitiveDataFrameColumn<decimal> pctChange = new PrimitiveDataFrameColumn<decimal>("pct_change", numberOfRows);

            //temporary
            this.PerformanceTable = prices;
            //
            this.PerformanceTable.Columns.Add(quantity);
            this.PerformanceTable.Columns.Add(averageCost);
            this.PerformanceTable.Columns.Add(LongShort);
            this.PerformanceTable.Columns.Add(pctChange);

            // since merge and join arent ready yet use the below.
            int counter = 0;
            foreach (var row in PerformanceTable.Rows)
            {

                if (counter == pbRows)
                {
                    break;
                }
                if (row[0].Equals(this.positionBreakdown[counter].date))
                {
                    row[2] = this.positionBreakdown[counter].quantity;
                    row[3] = Math.Round(this.positionBreakdown[counter].averageCost, 4);
                    if (this.positionBreakdown[counter].quantity > 0)
                    {
                        row[4] = 1;
                    }
                    else
                    {
                        row[4] = -1;
                    }
                    counter++;
                }
            }

            // since fill nulls cant forwardfill use the below.
            bool toFill = false;
            long prevQuantity = 0;
            decimal prevAverageCost = 0;
            int prevLongShort = 0;
            foreach (var row in PerformanceTable.Rows)
            {
                if ((row[2] != null && !toFill) || (row[2] != null && toFill))
                {
                    toFill = true;
                    prevQuantity = (long)row[2];
                    prevAverageCost = (decimal)row[3];
                    prevLongShort = (int)row[4];
                }
                else if (row[2] == null && toFill)
                {
                    row[2] = prevQuantity;
                    row[3] = prevAverageCost;
                    row[4] = prevLongShort;
                }
            }
            // if the quantity is still null. make it long.MaxValue and then creates a new dataframe with that filtered info
            PrimitiveDataFrameColumn<bool> boolFilter = PerformanceTable.Columns.GetPrimitiveColumn<long>("Quantity").FillNulls(long.MaxValue).ElementwiseNotEquals(long.MaxValue);
            DataFrame FinalTable = PerformanceTable.Filter(boolFilter);

            foreach (var row in FinalTable.Rows)
            {

                decimal price = Convert.ToDecimal(row[1]);
                decimal cost = (decimal)row[3];
                int sign = (int)row[4];
                row[5] = Math.Round(((price / cost) - 1) * 100 * sign, 3);
            }
            this.PerformanceTable = FinalTable;
            //return FinalTable;
        }

        public DataFrame GetDailyValuation(DataFrame prices)
        {
            if (this.ValuationTable.Columns.Count != 0)
            {
                this.ValuationTable = new DataFrame();
            }

            int pbRows = this.positionBreakdown.Count;
            if (pbRows == 0) //position is empty
            {
                return this.ValuationTable;
            }

            /// Take all prices from Database and columns with them and add those columns to the performance table
            // then add new columns for the dataframe
            // modifies dataframe inplace.

            long rowsnumber = prices.Rows.Count;
            // if i was using an actual datetime from the database i could use the filter.
            string title = $"{this.symbol}_MarketValue";

            PrimitiveDataFrameColumn<long> quantity = new PrimitiveDataFrameColumn<long>("Quantity", rowsnumber);
            PrimitiveDataFrameColumn<decimal> marketValue = new PrimitiveDataFrameColumn<decimal>(title, rowsnumber);
            prices.Columns.Add(quantity);
            prices.Columns.Add(marketValue);

            // since merge and join arent ready yet use the below.
            int counter = 0;
            foreach (var row in prices.Rows)
            {

                if (counter == pbRows)
                {
                    break;
                }
                if ((string)row[0] == this.positionBreakdown[counter].date.ToString("dd/MM/yyyy"))
                {
                    row[2] = this.positionBreakdown[counter].quantity;
                    counter++;
                }
            }

            // since fill nulls cant forwardfill use the below.
            bool toFill = false;
            long prevQuantity = 0;
            foreach (var row in prices.Rows)
            {
                if ((row[2] != null && !toFill) || (row[2] != null && toFill))
                {
                    toFill = true;
                    prevQuantity = (long)row[2];
                }
                else if (row[2] == null && toFill)
                {
                    row[2] = prevQuantity;
                }
            }
            // if the quantity is still null. make it long.MaxValue and then creates a new dataframe with that filtered info
            PrimitiveDataFrameColumn<bool> boolFilter = prices.Columns.GetPrimitiveColumn<long>("Quantity").FillNulls(long.MaxValue).ElementwiseNotEquals(long.MaxValue);
            DataFrame FinalTable = prices.Filter(boolFilter);
            foreach (var row in FinalTable.Rows)
            {

                decimal price = Convert.ToDecimal(row[1]);
                long units = (long)row[2]; //quantity
                row[3] = Math.Round(price * units, 4);
            }
            Console.WriteLine(FinalTable);
            return FinalTable;
        }
    }
}
