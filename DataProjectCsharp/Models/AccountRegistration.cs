using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DataProjectCsharp.Models
{
    public class AccountRegistration
    {
        [Required(ErrorMessage ="You need to create a Username")]
        public string UserName { get; set; }
        [Required(ErrorMessage ="An Email address is required.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage="A password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Your passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
