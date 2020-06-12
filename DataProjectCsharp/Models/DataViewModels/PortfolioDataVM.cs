using DataProjectCsharp.Data;
using Microsoft.Data.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataProjectCsharp.Models.DataViewModels
{
    public class PortfolioDataVM
    {
        public int? PortfolioId { get; set; }
        public PortfolioData PortfolioObject { get; set; }

        public DataFrame HoldingPeriodReturn { get; set; }
    }
}
