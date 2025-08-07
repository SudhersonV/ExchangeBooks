// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using IdSrv.Host.Filters;
using IdSrv.Host.ViewModels;
using IdSrv.Cosmos.Data.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace IdSrv.Host.Controllers
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger _logger;
        private readonly IEmailService _emailService;

        public HomeController(IIdentityServerInteractionService interaction, IWebHostEnvironment environment
        , ILogger<HomeController> logger, IEmailService emailService)
        {
            _interaction = interaction;
            _environment = environment;
            _logger = logger;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            if (_environment.IsDevelopment())
            {
                // only show in development
                return View();
            }

            _logger.LogInformation("Homepage is disabled in production. Returning 404.");
            return NotFound();
        }

        /// <summary>
        /// Shows the error page
        /// </summary>
        public async Task<IActionResult> Error(string errorId)
        {
            var vm = new ErrorViewModel();

            // retrieve error details from identityserver
            var message = await _interaction.GetErrorContextAsync(errorId);
            if (message != null)
            {
                vm.Error = message;

                if (!_environment.IsDevelopment())
                {
                    // only show in development
                    message.ErrorDescription = null;
                }
            }

            return View("Error", vm);
        }

        [HttpGet]
        public IActionResult PrivacyPolicy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Eula()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Support()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Email()
        {
            return View(new EmailViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Email(EmailViewModel emailViewModel)
        {
            #region Validate request
            ValidationContext vc = new ValidationContext(emailViewModel);
            ICollection<ValidationResult> results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(emailViewModel, vc, results, true);
            if (isValid)
            {
                await _emailService.SendSupportEmail(emailViewModel.FromEmail, emailViewModel.FromName, emailViewModel.Body);
                return View(new EmailViewModel { IsEmailSent = true });
            }
            else
            {
                return View(new EmailViewModel());
            }
            #endregion

        }
    }
}