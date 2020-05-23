using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SIAH.Models;
using SIAH.Context;
using System.Data.Entity;
namespace SIAH.Controllers
{
    public class AccountController : Controller
    {
        private SIAHContext db = new SIAHContext();
        // GET: Account
        [AuthorizeUserAccessLevel(UserRole = "DirectorArea")]
        public ActionResult Index()
        {
            using (SIAH.Context.SIAHContext db = new Context.SIAHContext())
            {
                return View(db.UserAccounts.Include(u => u.rol).ToList());
            }
        }

        //GET: Account/Profile
        [AuthorizeUserAccessLevel(UserRole = "DirectorArea", UserRole2 = "RespFarmacia", UserRole3 = "RespAutorizacion")]
        public ActionResult Profile()
        {
            ViewBag.HospitalRequired = "";
            ViewBag.rolID = new SelectList(db.Roles, "id", "nombre");
            ViewBag.hospitalID = new SelectList(db.Hospitales, "id", "nombre");
            return View();
        }
        
        //POST: Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeUserAccessLevel(UserRole = "DirectorArea", UserRole2 = "RespFarmacia", UserRole3 = "RespAutorizacion")]
        public ActionResult Profile([Bind(Include = "nombre, apellido, email, rolID, password, confirmPassword, hospitalID")]UserAccount account)
        {
            ViewBag.HospitalRequired = "";
            if (ModelState.IsValid)
            {
                if (string.Compare(db.Roles.Find(account.rolID.Value).nombre, "RespFarmacia") == 0 && account.hospitalID == null)
                {
                    ViewBag.HospitalRequired = "El hospital es obligatorio para los responsables de farmacia";
                }
                else
                {
                    String hash = Hashing.HashPassword(account.password);
                    account.password = hash;
                    account.confirmPassword = hash;
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
                }

            }
            ViewBag.rolID = new SelectList(db.Roles, "id", "nombre");
            ViewBag.hospitalID = new SelectList(db.Hospitales, "id", "nombre");
            return View();
        }

        //GET: Account/Register
        [AuthorizeUserAccessLevel(UserRole = "DirectorArea")]
        public ActionResult Register()
        {
            ViewBag.HospitalRequired = "";
            ViewBag.rolID = new SelectList(db.Roles.OrderBy(rol => rol.nombre), "id", "nombre");
            ViewBag.hospitalID = new SelectList(db.Hospitales.OrderBy(hospital => hospital.nombre), "id", "nombre");
            return View("Register", new UserAccount());
        }

        //POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeUserAccessLevel(UserRole = "DirectorArea")]
        public ActionResult Register([Bind(Include = "nombre, apellido, email, rolID, password, confirmPassword, hospitalID")]UserAccount account)
        {
            ViewBag.HospitalRequired = "";
            if (ModelState.IsValid)
            {
                if (string.Compare(db.Roles.Find(account.rolID.Value).nombre, "RespFarmacia") == 0 && account.hospitalID == null)
                {
                    ViewBag.HospitalRequired = "El hospital es obligatorio para los responsables de farmacia";
                }
                else
                {
                    String hash = Hashing.HashPassword(account.password);
                    account.password = hash;
                    account.confirmPassword = hash;
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
                }

            }
            ViewBag.rolID = new SelectList(db.Roles.OrderBy(rol => rol.nombre), "id", "nombre");
            ViewBag.hospitalID = new SelectList(db.Hospitales.OrderBy(hospital => hospital.nombre), "id", "nombre");
            return View(account);
        }

        //Login
        public ActionResult Login()
        {
            return View();
        }

        //[HttpPost]
        public ActionResult Denied()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserAccount user)
        {
            using (Context.SIAHContext db = new Context.SIAHContext())
            {
                //var usr = db.UserAccounts.Where(u => u.email == user.email && u.password == user.password).Include(p => p.rol).Include(h => h.hospital).FirstOrDefault();
                var usr = db.UserAccounts.Where(u => u.email == user.email).Include(p => p.rol).Include(h => h.hospital).FirstOrDefault();
                
                if (usr != null)
                {
                    String hash = usr.password;
                    if (Hashing.ValidatePassword(user.password, hash))
                    {
                        Session["userid"] = usr.id.ToString();
                        Session["email"] = usr.email.ToString();
                        Session["nombre"] = usr.nombre.ToString();
                        Session["rol"] = usr.rol.nombre.ToString();
                        if (usr.hospitalID != null)
                        {
                            Session["hospitalId"] = usr.hospitalID.ToString();
                            Session["hospital"] = usr.hospital.nombre.ToString();
                        }
                        //Intento de redirigir el login pero no funciona 
                        switch (usr.rol.nombre.ToString())
                        {
                            case "RespFarmacia":
                                return RedirectToAction("../Home/RespFarmaciaDashboard");
                            case "RespAutorizacion":
                                return RedirectToAction("../Home/RespAutorizacion");
                            case "DirectorArea":
                                return RedirectToAction("../Home/DirectorArea");
                            case "Compras":
                                return RedirectToAction("../Compras/CargarCompra");
                            default:
                                return RedirectToAction("LoggedIn");
                        }
                    }
                    else
                    {
                        TempData["PassMessage"] = "Contraseña incorrecta";
                        return RedirectToAction("Index", "Home");

                    }
                }
                else
                {
                    //ModelState.AddModelError("", "Usuario y/o contraseña incorrecto");
                    TempData["UserMessage"] = "Usuario  incorrecto";
                    return RedirectToAction("Index", "Home");

                }
            }
        }

        public ActionResult LoggedIn()
        {
            if (Session["userid"] != null)
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