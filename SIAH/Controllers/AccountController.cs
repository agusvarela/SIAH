using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SIAH.Models;
using SIAH.Context;

namespace SIAH.Controllers
{
    public class AccountController : Controller
    {
        private SIAHContext db = new SIAHContext();
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
            ViewBag.rolID = new SelectList(db.Roles, "id", "nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Include = "nombre, apellido, email, rolID, password, confirmPassword")]UserAccount account)
        {
            if (ModelState.IsValid)
            {
                //account.rol = null;
                //account.rolID = null;
                using (SIAH.Context.SIAHContext db = new Context.SIAHContext())
                {
                    db.UserAccounts.Add(account);
                    try
                    {
                        if (db.SaveChanges() > 0)
                        {
                            return RedirectToAction("Index", new { param = "Success" });
                        }
                    }
                    catch (Exception e)
                    {
                        return RedirectToAction("Index", new { param = e.Message });
                    }
                }
                ModelState.Clear();
                //  ViewBag.Message = account.nombre + " " + account.apellido + " Fue registrado correctamente";
            }
            return View();
        }

        //Login
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login( UserAccount user)
        {
            using (Context.SIAHContext db = new Context.SIAHContext())
            {
                //var usr = db.UserAccounts.Single(u => u.email == user.email && u.password == user.password);
                var usr = db.UserAccounts.Where(u => u.email == user.email && u.password == user.password).FirstOrDefault();
                string rol = "";
                if (usr != null)
                {
                    Session["userid"] = usr.id.ToString();
                    Session["email"] = usr.email.ToString();
                    Session["nombre"] = usr.nombre.ToString();
                    rol = db.Roles.Find(usr.rolID).nombre;
                    Session["rol"] = rol;
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
            if(Session["userid"]!=null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        //Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}