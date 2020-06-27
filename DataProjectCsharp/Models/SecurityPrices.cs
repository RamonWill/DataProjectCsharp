using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DataProjectCsharp.Models
{
    public class SecurityPrices
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string Ticker { get; set; }
        [Required, Column(TypeName = "decimal(18,4)"), Range(0, long.MaxValue, ErrorMessage = "Prices can not be negative numbers.")]
        public decimal ClosePrice { get; set; }
    }
}
