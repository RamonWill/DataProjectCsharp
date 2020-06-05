using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DataProjectCsharp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using DataProjectCsharp.Data;

namespace DataProjectCsharp.Controllers
{
    //[Authorize] < redirects user to login page if they are not authorised.
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AlphaVantageConnection _AVConn;
        public HomeController(ILogger<HomeController> logger, AlphaVantageConnection AVConn)
        {
            _logger = logger;
            _AVConn = AVConn;

        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
