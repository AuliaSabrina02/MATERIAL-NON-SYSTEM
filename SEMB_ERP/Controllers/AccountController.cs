using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEMB_ERP.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace SEMB_ERP.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        public IActionResult AccessDenied() {
            return Redirect("/"); 
        }
    }
}
