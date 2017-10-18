using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SIAH.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if(Session["rol"] == null)
            {
                Session["rol"] = "Admin";
            } 
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}