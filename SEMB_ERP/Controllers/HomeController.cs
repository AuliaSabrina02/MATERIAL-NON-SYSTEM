using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SEMB_ERP.Function;
using SEMB_ERP.Models;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;

namespace SEMB_ERP.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private string DbConnection()
        {
            var dbAccess = new DatabaseAccessLayer();
            string dbString = dbAccess.ConnectionString;
            return dbString;
        }
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        public IActionResult Dashboard()
        {
            return View();
        }
        public async Task<IActionResult> Index()
        {
            string name = User.FindFirst("semb_erp_name")?.Value;
            string level = User.FindFirst("semb_erp_level")?.Value;
            if (name == null || level == null)
            {
                return RedirectToAction("Index", "Auth");
            }
            else if (level == "no_access")
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;

                var existLevel = claimsIdentity?.FindFirst("semb_erp_level");
                if (existLevel != null)
                {
                    claimsIdentity.RemoveClaim(existLevel);
                }
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
            }
            //ViewBag.changeLevel = _configuration["ChangeLevel"];
            return View();
        }

        public IActionResult Open()
        {
            string user_level = User.FindFirst("semb_erp_level")?.Value;
            if (user_level != null)
            {
                switch (user_level.ToLower())
                {
                    case "user":
                        return RedirectToAction("OrderList", "User");
                    case "admin":
                        return RedirectToAction("Index", "Admin");
                    default:
                        return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> ChangeLevel(string level = "approver", string role = "hod") {
            var identity = User.Identity as ClaimsIdentity;
            var existingClaim = identity?.FindFirst("semb_erp_level");
            if (existingClaim != null)
            {
                identity.RemoveClaim(existingClaim);
                identity.AddClaim(new Claim("semb_erp_level", level));
            }
            else
            {
                identity.AddClaim(new Claim("semb_erp_level", level));
            }

            var existingClaim2 = identity?.FindFirst("semb_erp_role");
            if (existingClaim2 != null)
            {
                identity.RemoveClaim(existingClaim2);
                identity.AddClaim(new Claim("semb_erp_role", role));
            }
            else
            {
                identity.AddClaim(new Claim("semb_erp_role", role));
            }
            // Step 4: Create a new ClaimsPrincipal
            var claimsPrincipal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(claimsPrincipal);

            return Ok( new { 
                level = User.FindFirst("semb_erp_level")?.Value,
                role = User.FindFirst("semb_erp_role")?.Value
            });
        }
        public IActionResult Unauthorize()
        {
            return View();
        }
        public async Task<IActionResult> Logout()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;

            var existName = claimsIdentity?.FindFirst("semb_erp_name");
            if (existName != null)
            {
                claimsIdentity.RemoveClaim(existName);
            }
            var existLevel = claimsIdentity?.FindFirst("semb_erp_level");
            if (existLevel != null)
            {
                claimsIdentity.RemoveClaim(existLevel);
            }

            //var existRole = claimsIdentity?.FindFirst("semb_erp_role");
            //if (existRole != null)
            //{
            //    claimsIdentity.RemoveClaim(existRole);
            //}

            if (claimsIdentity != null)
            {
                // Get all claims with the specified claim type
                var rolesToRemove = claimsIdentity.Claims
                    .Where(c => c.Type == "semb_erp_role")
                    .ToList(); // Convert to a list to avoid modifying the collection while iterating

                // Remove each claim
                foreach (var roleClaim in rolesToRemove)
                {
                    claimsIdentity.RemoveClaim(roleClaim);
                }
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
