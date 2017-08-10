using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SIAH.Models;
namespace SIAH.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            using (SIAH.Context.SIAHContext db = new Context.SIAHContext())
            {
                return View(db.UserAccounts.ToList());
            }
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(UserAccount account)
        {
            if(ModelState.IsValid)
            {
                using (SIAH.Context.SIAHContext db = new Context.SIAHContext())
                {
                    db.UserAccounts.Add(account);
                    db.SaveChanges();
                }
                ModelState.Clear();
                ViewBag.Message = account.nombre + " " + account.apellido + " Fue registrado correctamente";
            }
            return View();
        }

        //Login
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(UserAccount user)
        {
            using (Context.SIAHContext db = new Context.SIAHContext())
            {
                var usr = db.UserAccounts.Single(u => u.email == user.email && u.password == user.password);
                if (usr != null)
                {
                    Session["id"] = usr.id.ToString();
                    Session["email"] = usr.email.ToString();
                    return RedirectToAction("LoggedIn");
                } else
                {
                    ModelState.AddModelError("", "Usuario y/o contraseña incorrecto");
                }
            }
            return View();
        }

        public ActionResult LoggedIn()
        {
            if(Session["userId"]!=null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
    }
}