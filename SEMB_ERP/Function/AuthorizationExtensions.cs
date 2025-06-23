using System.Collections.Generic;
using System.Linq;

namespace SEMB_ERP.Function
{
    public static class AuthorizationExtensions
    {
        public static bool HasAnyRole(this List<string> userRoles, params string[] requiredRoles)
        {
            if (userRoles == null || !userRoles.Any())
            {
                return false;
            }

            if (requiredRoles == null || !requiredRoles.Any())
            {
                // If no required roles are specified, assume access (or return false based on your policy)
                return true;
            }

            return userRoles.Any(role => requiredRoles.Contains(role));
        }
    }
}
