using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Analysis;

namespace DataProjectCsharp.Data
{
    public class PortfolioData
    {
        /*
         * A portfolio is a collection of position objects
         * A big assumption made is that the first position that is added will determine the portfolios date range
         * Therefore portfolios with multiple securities should be added in ascending order
         */

        public readonly string PortfolioName; 
        private List<Position> positions;
        private DataFrame PortfolioTable;

        public PortfolioData(string name)
        {
            this.PortfolioName = name;
            this.positions = new List<Position>();
            this.PortfolioTable = new DataFrame();
        }

        public List<Position> GetPositions()
        {
            return this.positions;
        }

        public void AddPositon(PositionFormulas position)
        {
            // adds the position to the position list.
            // adds the positon to the list of positions.
            this.positions.Add(position);

            // append the position.GetDailyValuation to the PortfolioTable initially.
            // then every table appended after that is added on.
            DataFrame positionValuation = position.GetDailyValuation();

            if (this.PortfolioTable.Columns.Count == 0)
            {
                this.PortfolioTable = positionValuation.Clone();
            }
            else
            {
                // If the portfolio already contains securities then add a new column
                int numberOfRows = this.PortfolioTable.Rows.Count();
                string NewColName = $"{position.symbol}_MarketValue";
                PrimitiveDataFrameColumn<decimal> newCol = new PrimitiveDataFrameColumn<decimal>(NewColName, numberOfRows);
                this.PortfolioTable.Columns.Add(newCol);

                int dateCol = 0;
                int secondaryRow = 0;

                int newColIndex = PortfolioTable.Columns.Count-1;
                for(int row = 0; row < numberOfRows; row++)
                {
                    if (secondaryRow==positionValuation.Rows.Count)
                    {
                        break;
                    }

                    if(this.PortfolioTable[row, dateCol].Equals(positionValuation[secondaryRow, dateCol]))
                    {
                        // if the dates between the tables match then assign the positions valuation at that date to the Portfolios Table
                        this.PortfolioTable[row, newColIndex] = positionValuation[secondaryRow, 1];
                        secondaryRow++;
                    }
                    else
                    {
                        this.PortfolioTable[row, newColIndex] = Decimal.Zero;
                    }
                }
            }
        }

        public DataFrame GetValuation()
        {
            if (this.PortfolioTable.Columns.Count < 2)
            {
                //return empty dataframe
                return this.PortfolioTable;
            }
            DataFrame ResultTable = this.PortfolioTable.Clone();
            int lastColumnIndex = ResultTable.Columns.Count-1;
            int numberOfRows = ResultTable.Rows.Count();
            PrimitiveDataFrameColumn<decimal> totalMarketVal = new PrimitiveDataFrameColumn<decimal>("TotalMarketValue", numberOfRows);
            ResultTable.Columns.Add(totalMarketVal);

            int marketValCol = lastColumnIndex+1;

            for (int row=0; row < numberOfRows; row++)
            {
                decimal total = 0;
                //for each column that isnt a date and isnt the total get the sum.
                for (int col = 1; col <= lastColumnIndex; col++)
                {
                    decimal? value = (decimal?)ResultTable[row, col];
                    if (value != null)
                    {
                        total += (decimal)value;
                    }
                    
                }
                ResultTable[row, marketValCol] = Math.Round(total,2);
            }
            return ResultTable;
        }

    }
}
