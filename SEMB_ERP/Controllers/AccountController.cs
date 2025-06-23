using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using SEMB_ERP.Function;
using SEMB_ERP.Models;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Org.BouncyCastle.Ocsp;

namespace SEMB_ERP.Controllers
{
    public class AccountController : Controller
    {
        private string DbConnection()
        {
            var dbAccess = new DatabaseAccessLayer();

            string dbString = dbAccess.ConnectionString;

            return dbString;
        }
        public IActionResult AccessDenied() {
            return Redirect("/"); 
        }
    }
}
