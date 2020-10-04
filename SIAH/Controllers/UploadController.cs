using System;  
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;  
using System.Linq;  
using System.Web;  
using System.Web.Mvc;  
namespace FileUpload.Controllers  
{  
    public class UploadController : Controller
{
    // GET: Upload  
    public ActionResult Index()
    {
        return View();
    }
    [HttpGet]

    public ActionResult UploadFile()
    {
        return View();
    }
        
    [HttpPost]

    public ActionResult UploadFile(HttpPostedFileBase file)
    {
        try
        {
            if (file.ContentLength > 0)
            {
                string _FileName = Path.GetFileName(file.FileName);
                string _path = Path.Combine(Server.MapPath("~/UploadedFiles"), _FileName);
                file.SaveAs(_path);
            }

            ViewBag.Message = "Archivo Subido";
            return View();
        }
        catch (Exception e)
        {
            ViewBag.Message = "Epic Fail!";
                Console.WriteLine(e.Message);
            return View();
        }
    }
}  
}  