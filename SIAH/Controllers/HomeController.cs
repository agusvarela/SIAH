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

            ViewBag.error = TempData["message"];
            if (Session["rol"] == null)
            {
                Session["rol"] = "Admin";
            }
            return View();
        }

        public ActionResult Home()
        {
            ViewBag.error = TempData["message"].ToString();
            return RedirectToAction((String)Session["rol"]);
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
        [AuthorizeUserAccessLevel(UserRole = "RespAutorizacion")]
        public ActionResult RespAutorizacion()
        {
            ViewBag.Message = "Your index Page.";

            return View();
        }
        [AuthorizeUserAccessLevel(UserRole = "RespFarmacia")]
        public ActionResult RespFarmacia()
        {
            ViewBag.Message = "Your index Page.";

            return RedirectToAction("RespFarmaciaDashboard");
        }
        [AuthorizeUserAccessLevel(UserRole = "RespFarmacia")]
        public ActionResult RespFarmaciaDashboard()
        {
            ViewBag.Message = "Your index Page.";

            return View();
        }
        [AuthorizeUserAccessLevel(UserRole = "DirectorArea")]
        public ActionResult DirectorArea(string param)
        {
            if (param != null)
            {
                if (param.CompareTo("Success") == 0)
                {
                    ViewBag.success = true;
                }
                else
                {
                    ViewBag.success = false;
                    ViewBag.problem = param;
                };

                return View();
            }
            else
            {
                return View();
            }
        }
    }
}