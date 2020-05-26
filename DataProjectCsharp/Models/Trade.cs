using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DataProjectCsharp.Models
{
    public class Trade
    {
        [Key]
        public int TradeId { get; set; }
        [Required]
        public string Ticker { get; set; }
        [Required]
        public long Quantity { get; set; }
        [Required, Column(TypeName ="decimal(18,4)"), Range(0, long.MaxValue, ErrorMessage = "Prices can not be negative numbers.")]
        public decimal Price { get; set; }

        [Required]
        [DisplayFormat(ApplyFormatInEditMode =true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime TradeDate { get; set; }

        [DataType(DataType.MultilineText)]
        public string Comments { get; set; }
        public DateTime CreatedTimeStamp { get; set; }
        
        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("Portfolio")]
        public int PortfolioId { get; set; }
        public Portfolio Portfolio { get; set;}

        public Trade()
        {
            TradeDate = DateTime.UtcNow;
            CreatedTimeStamp = DateTime.UtcNow;
        }
    }
}
