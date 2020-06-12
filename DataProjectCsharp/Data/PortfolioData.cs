using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Analysis;

namespace DataProjectCsharp.Data
{
    public class PortfolioData
    {
        // A portfolio is a collection of position objects
        // order database by date and then ticker.
        // then create position objects to add to this portfolio
        // assumptions made. the first position that is added will determine the date range.
        public string PortfolioName; 
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
            // append the position.GetDailyPerformance to the PortfolioTable initially.
            // then every table appended after that is added on.
            this.positions.Add(position);
            DataFrame positionValuation = position.GetDailyValuation();
            // fail if there is not daily valuation
            if (PortfolioTable.Columns.Count == 0)
            {
                this.PortfolioTable = positionValuation;
            }
            else
            {
                int numberOfRows = this.PortfolioTable.Rows.Count();
                string NewColName = $"{position.symbol}_MarketValue";
                PrimitiveDataFrameColumn<decimal> newCol = new PrimitiveDataFrameColumn<decimal>(NewColName, numberOfRows);
                this.PortfolioTable.Columns.Add(newCol);

                int dateCol = 0;
                int secondaryRow = 0;
                int newColIndex = PortfolioTable.Columns.Count-1;
                for(int row = 0; row < numberOfRows; row++)
                {
                    if(this.PortfolioTable[row, dateCol].Equals(positionValuation[secondaryRow, dateCol]))
                    {
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

        /*
         * to get the valuation i will need the sum of each row.
         * i can have a function that checks if the last coloumn is called total. if it is delete and replace, otherwise just add it.
         * I would iterate through each row and do nested for loop.
        */ 
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
                //for each column that isnt a date and isnt the total
                for (int col = 1; col <= lastColumnIndex; col++)
                {
                    total += (decimal)ResultTable[row, col];
                }
                ResultTable[row, marketValCol] = Math.Round(total,2);
            }
            return ResultTable;
        }

    }
}
