using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DataProjectCsharp.Models
{
    public class TradeableSecurities
    {
        [Required]
        public string Name { get; private set; }
        [Key, Required]
        public string Ticker { get; private set; }
        [Required]
        public string Country { get; private set; }
        [Required]
        public string BenchmarkIndex { get; private set; }
        [Required]
        public string ISOAlphaCode { get; private set; }
    }
}
