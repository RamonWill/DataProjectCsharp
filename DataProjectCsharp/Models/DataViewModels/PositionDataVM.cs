using DataProjectCsharp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataProjectCsharp.Models.DataViewModels
{
    public class PositionDataVM
    {
        public int? PortfolioId { get; set; }
        public string PortfolioName { get; set; }
        public PositionFormulas PositionObject { get; set; }

        public TradeableSecurities PositionSymbolData { get; set; }
    }
}
