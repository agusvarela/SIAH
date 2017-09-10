using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace System.Web.Mvc
{
    public class AuthorizeUserAccessLevel: AuthorizeAttribute
    {
        public string UserRole {get; set;}

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            /*var isAuthorized = base.AuthorizeCore(httpContext);
            if(!isAuthorized)
            {
                return false;
            }*/

      /*
            if (this.UserRole.Contains(CurrentUserRole))
            {
                return true;
            }
            else return false;
            */
            String rol = HttpContext.Current.Session["rol"].ToString();
            if ( rol.CompareTo(UserRole) == 0)
            {
                return true;
            }
            else return false;
        }
    }
}