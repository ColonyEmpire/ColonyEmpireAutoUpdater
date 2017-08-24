using System;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Mvc;
using UpdateWebService.Models;

namespace UpdateWebService.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(AdminLogin login)
        {
            if (login.Username == ConfigurationManager.AppSettings["adminUsername"] && login.Password == ConfigurationManager.AppSettings["adminPassword"])
            {
                UpdateCEZip(login.Files[0]);
                ViewBag.Message = "Update complete";
            }
            else
            {
                ViewBag.Message = "Incorrect Username/Password";
            }


            
            return View();
        }

        private void UpdateCEZip(HttpPostedFileBase httpPostedFileBase)
        {
            System.IO.File.Delete(System.Web.HttpContext.Current.Server.MapPath("~/Update/ColonyEmpire.zip"));
            httpPostedFileBase.SaveAs(System.Web.HttpContext.Current.Server.MapPath("~/Update/ColonyEmpire.zip"));


            UpdateVersion();
        }

        private void UpdateVersion()
        {
            var version = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Server.MapPath("~/Update/version.txt"));
            System.IO.File.WriteAllText(System.Web.HttpContext.Current.Server.MapPath("~/Update/version.txt"), (int.Parse(version)+1).ToString());
        }
    }
}