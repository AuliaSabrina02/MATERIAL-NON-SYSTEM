using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEMB_ERP.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace SEMB_ERP.Controllers
{
    public class ErrorController : Controller
    {
        [Route("AccessDenied")] // Custom route for cleaner URL
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
