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
        public ActionResult Index(string param)
        {
            if (param != null)
            {
                ViewBag.success = true;
            }

            ViewBag.UserError = TempData["UserMessage"];
            ViewBag.PassError = TempData["PassMessage"];

            if (Session["rol"] == null)
            {
                Session["rol"] = "Admin";
            }
            return View();
        }

        [AuthorizeUserAccessLevel(UserRole = "RespAutorizacion", UserRole2 = "DirectorArea", UserRole3 = "RespFarmacia")]
        public ActionResult Home()
        {
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
                else if (param.CompareTo("Reclamo") == 0)
                {
                    ViewBag.reclamo = true;
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
        [AuthorizeUserAccessLevel(UserRole = "Admin")]
        public ActionResult Admin()
        {
            ViewBag.Message = "Your index Page.";

            return RedirectToAction("Index");
        }
    }
}