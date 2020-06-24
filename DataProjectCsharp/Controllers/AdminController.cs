using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataProjectCsharp.Models.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataProjectCsharp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly IAdminRepository _adminRepo;
        public AdminController(IAdminRepository adminRepo)
        {
            this._adminRepo = adminRepo;
        }

        public IActionResult AdminPanel()
        {
            //run an admin service
            return View();
        }
    }
}