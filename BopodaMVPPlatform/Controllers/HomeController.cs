﻿using Aiursoft.Pylon.Attributes;
using Aiursoft.Pylon.Services;
using BopodaMVPPlatform.Data;
using BopodaMVPPlatform.Models.HomeViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BopodaMVPPlatform.Controllers
{
    [LimitPerMin]
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ServiceLocation _serviceLocation;
        private readonly MVPDbContext _dbContext;

        public HomeController(
            IConfiguration configuration,
            ServiceLocation serviceLocation,
            MVPDbContext dbContext)
        {
            _configuration = configuration;
            _serviceLocation = serviceLocation;
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var model = new IndexViewModel();
            return View(model);
        }

        [Route("Account/Signout")]
        public IActionResult SignOut()
        {
            return RedirectToAction(nameof(Index));
        }
    }
}
