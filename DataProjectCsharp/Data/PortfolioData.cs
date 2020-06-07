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
        private List<Position> positions;
        private DataFrame PortfolioTable;

        public PortfolioData()
        {
            this.positions = new List<Position>();
            this.PortfolioTable = new DataFrame();
        }

        public void AddPositon(PositionFormulas position)
        {
            // adds the position to the position list.
            // append the position.GetDailyPerformance to the PortfolioTable initially.
            // then every table appended after that is added on.
            this.positions.Add(position);

        }
    }
}
