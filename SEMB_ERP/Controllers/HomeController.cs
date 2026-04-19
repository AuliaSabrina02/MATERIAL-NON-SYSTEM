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
             //string name = User.FindFirst("semb_erp_name")?.Value;
            //string level = User.FindFirst("semb_erp_level")?.Value;
            //if (name == null || level == null)
            //{
            //    return RedirectToAction("Index", "Auth");
            //}
            //else if (level == "no_access")
            //{
            //    var claimsIdentity = (ClaimsIdentity)User.Identity;

            //    var existLevel = claimsIdentity?.FindFirst("semb_erp_level");
            //    if (existLevel != null)
            //    {
            //        claimsIdentity.RemoveClaim(existLevel);
            //    }
            //    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
            //}
            //ViewBag.changeLevel = _configuration["ChangeLevel"];
            return View();
        }
        [Authorize]
        public async Task<IActionResult> Login()
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
            //return View();
            return RedirectToAction("Open", "Home");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginManual(LoginModel user)
        {
            var hashpassword = new Authentication();

            if (ModelState.IsValid)
            {
                List<LoginModel> userInfo = new List<LoginModel>();
                using (SqlConnection conn = new SqlConnection(DbConnection()))
                {
                    string passwordHash = hashpassword.MD5Hash(user.password);
                    string query = "SELECT * FROM mst_users WHERE sesa_id = '" + user.sesa_id + "' AND password = '" + passwordHash + "' ";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        var db = new DatabaseAccessLayer();
                        List<UserDetailModel> userDetail = db.GetUserDetail(user.sesa_id);
                        List<UserDetailModel> userRole = db.GetUserRole(user.sesa_id);

                        var claimsIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

                        if (!claimsIdentity.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
                        {
                            claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.sesa_id));
                        }

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

                        // Check if role retrieval was successful
                        if (userDetail.Any() && userRole.Any())
                        {
                            var user_db = userDetail.First();
                            claimsIdentity.AddClaim(new Claim("semb_erp_name", user_db.name));
                            if (!string.IsNullOrEmpty(user_db.level))
                            {
                                // Create a new claim for the user role
                                claimsIdentity.AddClaim(new Claim("semb_erp_level", user_db.level));
                                foreach (var role in userRole)
                                {
                                    claimsIdentity.AddClaim(new Claim("semb_erp_role", Convert.ToString(role.role) ?? ""));
                                }
                            }
                            else
                            {
                                claimsIdentity.AddClaim(new Claim("semb_erp_level", "no_access"));
                                TempData["AccessDenied"] = "You dont have access";
                                return RedirectToAction("Index", "Home");
                            }
                        }
                        else
                        {
                            claimsIdentity.AddClaim(new Claim("semb_erp_level", "no_access"));

                            TempData["AccessDenied"] = "You dont have access";
                            return RedirectToAction("Index", "Home");
                        }

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                        //return Redirect(originalPath);
                        return RedirectToAction("Open", "Home");
                    }
                    else
                    {
                        ViewData["Message"] = "User and Password not Registered !";
                    }
                    conn.Close();

                }
            }

            //return RedirectToAction("Index", "Home");
            return View("Index");
        }
        [Authorize]
        public IActionResult Open()
        {
            string user_level = User.FindFirst("semb_erp_level")?.Value;
            List<string> user_roles = User.Claims
                                       .Where(c => c.Type == "semb_erp_role")
                                       .Select(c => c.Value)
                                       .ToList();

            if (user_level != null)
            {
                // Prioritas pengecekan role
                if (user_roles.Contains("plant_receiver"))
                {
                    return RedirectToAction("DashboardPalletReceiver", "User");
                }
                else if (user_roles.Contains("receiver"))
                {
                    return RedirectToAction("DashboardReceiver", "User");
                }
                else if (user_roles.Contains("requestor"))
                {
                    return RedirectToAction("DashboardRequestor", "User");
                }
                else
                {
                    // Default jika role tidak terdaftar di atas tapi memiliki level
                    return RedirectToAction("OrderList", "User");
                }
            }
            else
            {
                // Jika tidak punya level/akses
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize]
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
        [Authorize]
        public IActionResult Unauthorize()
        {
            return View();
        }
        [Authorize]
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

            //HttpContext.Session.Clear();
            //foreach (var cookie in Request.Cookies.Keys)
            //{
            //    Response.Cookies.Delete(cookie);
            //}
            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
