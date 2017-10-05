using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

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
            if (rol.CompareTo(UserRole) == 0)
            {
                return true;
            }
            else return false;
        }
        //protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        //{
        //    if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
        //    {
        //        base.HandleUnauthorizedRequest(filterContext);
        //    }
        //    else
        //    {
        //        filterContext.Result = new RedirectToRouteResult(new
        //        RouteValueDictionary(new { controller = "Error", action = "AccessDenied" }));
        //    }
        //}
    }
}