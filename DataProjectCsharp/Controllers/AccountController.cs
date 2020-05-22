using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DataProjectCsharp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DataProjectCsharp.Controllers
{
    public class AccountController : Controller
    {
        //this is an authentication manager.
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
        }
        [HttpGet]
        public IActionResult Registration()
        {
            //shows the view for the registration page
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] //CSRF
        public async Task<IActionResult> Registration(AccountRegistration userInput)
        {
            // post request logic
            if (!ModelState.IsValid)
            {
                return View(userInput);
            }
            // for now we will just take the username and email as they are.
            var user = new User {UserName=userInput.UserName, Email=userInput.Email};
            var result = await _userManager.CreateAsync(user, userInput.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Visitor");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.TryAddModelError(error.Code, error.Description);
            }
            return View(userInput);
        }

        [HttpGet]
        public IActionResult Login(string returnUrl=null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AccountLogin userInput, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(userInput);
            }


            var userName = userInput.Email; // User/Email field in form.
            User user;
            // If form has '@' it is an email otherwise it is a username.
            if (userName.IndexOf('@') > -1)
            {
                user = await _userManager.FindByEmailAsync(userName);
                if (user != null)
                {
                    userName = user.UserName;
                }
            }

            var result = await _signInManager.PasswordSignInAsync(userName, userInput.Password, userInput.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return RedirectToReturn(returnUrl);
            }
            else
            {
                ModelState.AddModelError("", "Invalid UserName or Password");
                return View();
            }           
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        private IActionResult RedirectToReturn(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}