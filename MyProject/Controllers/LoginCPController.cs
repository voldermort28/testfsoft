using System.Globalization;
using System.Web.Security;
using MyProject.Functions;
using MyProject.Mailers;
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
                            if (!String.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
                            else return Redirect("/HeThong");
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

        [HttpGet]
        public ActionResult ForgetPassAdmin()
        {
            return View();
        }
        
        [HttpGet]
        public ActionResult RenewPassAdmin(string token)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RenewPassAdmin(string email, string password, string longtime)
        {
            string deccryptEmail = Common.Decrypt(Common.Base64Decode(email)); 
            // validate
            if (!Common.ValidatePassAdmin(password))
            {
                return Json(new { result = "ERROR", msg = "Mật khẩu không hợp lệ" });
            }
            if ((deccryptEmail.ToLower() == password.ToLower())
                || (deccryptEmail.Substring(0, deccryptEmail.IndexOf('@')).ToLower() == password.ToLower()))
            {
                return Json(new { result = "ERROR", msg = "Mật khẩu trùng email" });
            }

            using (var db = new MyDatabaseEntities())
            {
                var user = db.admins.FirstOrDefault(x => x.email == deccryptEmail);
                if (user == null) ViewBag.msg = "Tài khoản không tồn tại";
                if (user != null)
                {
                    
                    return Json(new { result = "OK", msg = "Tạo mật khẩu mới thành công." });
                }               
                return View();
            }
        }


    }
}