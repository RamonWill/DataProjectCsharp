using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DataProjectCsharp.Models
{
    public class Portfolio
    {
        [Key]
        public int PortfolioId { get; set; }
        [Required(ErrorMessage = "You must give your portfolio a name."), StringLength(50)]
        public string Name { get; set; }

        public ICollection<Trade> Trades { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
