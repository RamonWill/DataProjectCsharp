using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataProjectCsharp.Models
{
    public class User:IdentityUser
    {
        // If i need to create more identity fields i can add them here.
        public ICollection<Portfolio> Portfolios { get; set; }
        public ICollection<Trade> Trades { get; set; }
    }
}
