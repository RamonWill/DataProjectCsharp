using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace DataProjectCsharp.Models
{
    public class AccountLogin
    {
        [Required]
        [Display(Name ="Username/Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DisplayAttribute(Name="Remember me?")]
        public bool RememberMe { get; set; }
    }
}
