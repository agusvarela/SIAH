using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SIAH.Models;
using SIAH.Context;
using System.Data.Entity;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.IO;

namespace SIAH.Controllers
{
    public class AccountController : Controller
    {
        private SIAHContext db = new SIAHContext();

        // GET: Account
        [AuthorizeUserAccessLevel(UserRole = "DirectorArea")]
        public ActionResult Index(string param)
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
            }
            using (SIAHContext db = new SIAHContext())
            {
                return View(db.UserAccounts.Include(u => u.rol).Where(x => x.active).ToList());
            }
        }

        //GET: Account/NewPassword
        [AuthorizeUserAccessLevel(Users = new string[] { "DirectorArea", "RespAutorizacion", "RespFarmacia", "Compras" })]
        public ActionResult NewPassword()
        {
            return View();
        }

        //POST: Account/NewPassword
        [HttpPost]
        [AuthorizeUserAccessLevel(Users = new string[] { "DirectorArea", "RespAutorizacion", "RespFarmacia", "Compras" })]
        public ActionResult NewPassword([Bind(Include = "password, confirmPassword")]UserAccount userProfile)
        { 
            string email = Session["email"].ToString();
            string password = userProfile.password;

            userProfile = db.UserAccounts.Where(user => user.email == email).First();
            String hashedPass = Hashing.HashPassword(password);
            userProfile.password = hashedPass;
            userProfile.confirmPassword = hashedPass;

            db.Entry(userProfile).State = EntityState.Modified;
            db.SaveChanges();

            Session.Clear();
            return RedirectToAction("Index", "Home", new { param = "Success" });
        }

        //GET: Account/Profile
        [AuthorizeUserAccessLevel(Users = new string[] { "DirectorArea", "RespAutorizacion", "RespFarmacia", "Compras" })]
        public ActionResult Profile()
        {
            string email = Session["email"].ToString();
            UserAccount userProfile = db.UserAccounts.Where(user => user.email == email).First();

            ViewBag.HospitalRequired = "";
            ViewBag.rolID = new SelectList(db.Roles.OrderBy(x => x.nombre), "id", "nombre");
            ViewBag.hospitalID = new SelectList(db.Hospitales.OrderBy(x => x.nombre), "id", "nombre");
            return View(userProfile);
        }

        //POST: Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeUserAccessLevel(Users = new string[] { "DirectorArea", "RespAutorizacion", "RespFarmacia", "Compras" })]
        public ActionResult Profile([Bind(Include = "nombre, apellido, email, rolID, password, confirmPassword, hospitalID")]UserAccount account)
        {
            string emailAPisar = account.email;
            string emailABuscar = Session["email"].ToString();
            account = db.UserAccounts.Where(user => user.email == emailABuscar).First();
            account.email = emailAPisar;
            ViewBag.HospitalRequired = "";
                
            if (string.Compare(db.Roles.Find(account.rolID.Value).nombre, "RespFarmacia") == 0 && account.hospitalID == null)
            {
                ViewBag.HospitalRequired = "El hospital es obligatorio para los responsables de farmacia";
            }
            else
            {
                using (SIAH.Context.SIAHContext db = new Context.SIAHContext())
                {
                    try
                    {
                        db.Entry(account).State = EntityState.Modified;
                        if (db.SaveChanges() > 0)
                        {
                            Session.Clear();
                            return RedirectToAction("Index", "Home", new { param = "Success" });
                        }
                    }
                    catch (Exception e)
                    {
                        return RedirectToAction("Index", new { param = e.Message });
                    }
                }
                ModelState.Clear();
            }

            UserAccount userProfile = db.UserAccounts.Where(user => user.email == emailABuscar).First();
            ViewBag.rolID = new SelectList(db.Roles.OrderBy(x => x.nombre), "id", "nombre");
            ViewBag.hospitalID = new SelectList(db.Hospitales.OrderBy(x => x.nombre), "id", "nombre");
            return View(userProfile);
        }

        //GET: Account/Register
        [AuthorizeUserAccessLevel(UserRole = "DirectorArea")]
        public ActionResult Register()
        {
            ViewBag.HospitalRequired = "";
            ViewBag.rolID = new SelectList(db.Roles.OrderBy(x => x.nombre), "id", "nombre");
            ViewBag.hospitalID = new SelectList(db.Hospitales.OrderBy(x => x.nombre), "id", "nombre");
            return View();
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
                    var usr = db.UserAccounts.Where(u => u.email == account.email && u.active).FirstOrDefault();
                    if (usr != null)
                    {
                        return RedirectToAction("Index", new { param = "Existe un usuario activo con esa dirección de correo" });
                    }
                    String hash = Hashing.HashPassword(account.password);
                    account.password = hash;
                    account.confirmPassword = hash;
                    account.active = true;
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

        //Edit

        [AuthorizeUserAccessLevel(UserRole = "DirectorArea")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.hospitalID = new SelectList(db.Hospitales.OrderBy(x => x.nombre), "id", "nombre");
            UserAccount userAccount = db.UserAccounts.Include(x => x.rol).Where(x => x.id == id).First();
            return View(userAccount);
        }

        [AuthorizeUserAccessLevel(UserRole = "DirectorArea")]
        [HttpPost]
        public ActionResult Edit([Bind(Include = "id, nombre, apellido, email, hospitalID")] UserAccount account)
        {
            UserAccount accountToModify = db.UserAccounts.Find(account.id);
            accountToModify.hospitalID = account.hospitalID;
            accountToModify.nombre = account.nombre;
            accountToModify.apellido = account.apellido;
            accountToModify.email = account.email;
            db.Entry(accountToModify).State = EntityState.Modified;

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
                var usr = db.UserAccounts.Where(u => u.email == user.email && u.active).Include(p => p.rol).Include(h => h.hospital).FirstOrDefault();
                
                if (usr != null)
                {
                    String hash = usr.password;
                    if (Hashing.ValidatePassword(user.password, hash))
                    {
                        Session["userid"] = usr.id.ToString();
                        Session["email"] = usr.email.ToString();
                        Session["nombre"] = usr.nombre.ToString();
                        Session["apellido"] = usr.apellido.ToString();
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

        // GET: Accounts/Delete/5
        [AuthorizeUserAccessLevel(UserRole = "DirectorArea")]
        [ActionName("Delete")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = db.UserAccounts.Find(id);
            if (!user.active)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user.active = false; //Borrado logico del usuario
            db.Entry(user).State = EntityState.Modified;

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

            return RedirectToAction("Index");
        }

        // GET: Accounts/ForgotPassword
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Accounts/ForgotPassword
        [HttpPost]
        public ActionResult ForgotPassword([Bind(Include = "email")] string email)
        {
            var account = db.UserAccounts.Where(x => x.email == email).FirstOrDefault();
            if (account == null)
            {
                return RedirectToAction("RecoveryResult", new { success = false });
            }
            string newPassword = GenerateRandomPassword();
            string hash = Hashing.HashPassword(newPassword);
            account.password = hash;
            account.confirmPassword = hash;
            db.Entry(account).State = EntityState.Modified;
            db.SaveChanges();
            Task.Run(() => SendEmail(email, newPassword));
            return RedirectToAction("RecoveryResult", new { success = true });
        }

        private string GenerateRandomPassword()
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            var length = 8;
            char letter;

            for (int i = 0; i < length; i++)
            {
                double flt = random.NextDouble();
                int shift = Convert.ToInt32(Math.Floor(25 * flt));
                letter = Convert.ToChar(shift + 65);
                builder.Append(letter);
            }

            return builder.ToString();
        }

        private async Task SendEmail(string email, string newPassword)
        {
            var message = new MailMessage();
            message.To.Add(new MailAddress(email));
            message.From = new MailAddress("siah.reclamos@gmail.com");
            message.Subject = string.Format("[SIAH] Su contraseña fue actualizada");
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("../Views/Shared/EmailPassword.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{newPassword}", newPassword);

            message.Body = body;

            message.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                await smtp.SendMailAsync(message);
            }
        }

        // GET: Accounts/RecoverySuccess
        public ActionResult RecoveryResult(bool success)
        {
            ViewBag.resultMessage = success ? 
                "Su nueva contraseña fue enviada correctamente a su dirección de correo" 
                : "El usuario no existe, inténtelo nuevamente";
            return View();
        }
    }
}