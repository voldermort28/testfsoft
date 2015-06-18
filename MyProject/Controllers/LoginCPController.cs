using System.Globalization;
using System.Web.Security;
using MyProject.Functions; 
using MyProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace MyProject.Controllers
{ 
    public class LoginCPController : Controller
    { 
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string txtUsername, string txtPassword, string returnUrl = "/HeThong")
        {
            using (var db = new MyDatabaseEntities())
            {
                string username = txtUsername.Trim().ToLower();
                var obj = db.admins.FirstOrDefault(x => x.email.ToLower() == username);
                if (obj != null)
                {
                    if (obj.status_id == 1)
                    {
                        if (Common.MaHoa(txtPassword.Trim()).Equals(obj.password))
                        {
                            Session["admss"] = obj.admin_id;                             
                            Session["admsstype"] = obj.admin_type;
                            Session["admssemail"] = obj.email;
                            if (!String.IsNullOrEmpty(returnUrl))
                            {
                                //Ghi log login
                                Common.NhatKiHeThong("Đăng nhập hệ thống", "Đăng nhập", "Login Form", "Đăng nhập hệ thống ");
                                return Redirect(returnUrl);
                            }
                            else
                            {
                                return Redirect("/HeThong");
                            }
                        }
                        else ViewBag.Alert = "Username or password is wrong";
                    }
                    else ViewBag.Alert = "This account is locked";
                }
                else ViewBag.Alert = "Username or password is wrong";
            }
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "LoginCP");
        } 
    }
}