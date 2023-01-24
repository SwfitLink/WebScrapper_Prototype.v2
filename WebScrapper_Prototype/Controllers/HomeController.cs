﻿using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebScrapper_Prototype.Data;
using WebScrapper_Prototype.Models;

namespace WebScrapper_Prototype.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly WebScrapper_PrototypeContext _context;

        public HomeController(ILogger<HomeController> logger, WebScrapper_PrototypeContext context)
        {
            _logger = logger;
            _context = context;

        }

        public IActionResult Index()
        {
            List<Product> products = _context.Product.ToList();
            return View(products);
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