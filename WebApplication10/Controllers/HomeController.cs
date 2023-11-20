using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Stilbaai_Tourism_Web_Portal.Models;

namespace Stilbaai_Tourism_Web_Portal.Controllers
{
   public class HomeController : Controller
   {
      private readonly ILogger<HomeController> _logger;

      //---------------------------------------------------------------------------------------
      //default constructor
      public HomeController(ILogger<HomeController> logger)
      {
         _logger = logger;
      }

      //---------------------------------------------------------------------------------------
      [Authorize]
      public IActionResult Index()
      {
         var logo = Properties.Resources.logo;
         ViewBag.LogoImage = Convert.ToBase64String(logo);

         var welcomeImage = Properties.Resources.welcome_image;
         ViewBag.welcomeImage = Convert.ToBase64String(welcomeImage);

         return View();
      }

      //---------------------------------------------------------------------------------------
      [Authorize]
      public IActionResult Privacy()
      {
         return View();
      }

      //---------------------------------------------------------------------------------------
      [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
      public IActionResult Error()
      {
         return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
      }
   }
}
//-------------------------------------====END OF FILE====-------------------------------------