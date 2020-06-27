using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DataProjectCsharp.Models
{
    public class Trade: IValidatableObject
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

        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            if(TradeDate < CreatedTimeStamp.AddDays(-99))
            {
                // the reason for this is that with my current setup i can only get the last 100 prices from AlphaVantage
                yield return new ValidationResult("The Trade Date cannot be 100 days older that the date the trade was created.", new[] { "TradeDate"});
            }
            if(TradeDate >= DateTime.UtcNow.AddDays(1))
            {
                yield return new ValidationResult("The Trade Date cannot be a date in the future.", new[] { "TradeDate" });
            }
            // Add another check that the Trade date itself cannot be a weekend
            if(TradeDate.DayOfWeek==DayOfWeek.Saturday|| TradeDate.DayOfWeek== DayOfWeek.Sunday)
            {
                yield return new ValidationResult("The Trade Date cannot fall on a weekend.", new[] { "TradeDate" });
            }
        }
    }
}
