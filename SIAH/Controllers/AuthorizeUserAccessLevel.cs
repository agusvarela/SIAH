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
        public string UserRole2 { get; set; }
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
            String rol = "";
            if (HttpContext.Current.Session != null)
            {
                rol = HttpContext.Current.Session["rol"].ToString();
            }
            else
            {
                return false;
            }

            
            if (rol.CompareTo(UserRole) == 0 || rol.CompareTo(UserRole2) == 0)
            {
                return true;
            }
            else return false;
        }
       
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectToRouteResult(new
                RouteValueDictionary(new { controller = "Account", action = "Denied" }));
                //base.HandleUnauthorizedRequest(filterContext);
            }
            else
            {
              /*  filterContext.Result = new RedirectToRouteResult(new
                RouteValueDictionary(new { controller = "AccountController", action = "Denied" }));*/
            }
        }
    }
}