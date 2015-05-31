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
   // [OutputCache(Duration = 60 * 60, VaryByParam = "none")]
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
                            Session["SuperAdmin"] = obj.admin_type;
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgetPassAdmin(string email, String recaptcha)
        {
            if (!Common.VerifyCaptcha(recaptcha, Request.UserHostAddress))
            {
                return Json(new { result = "Lỗi ", msg = "Mã bảo vệ không hợp lệ" });
            }
            using (var db = new MyDatabaseEntities())
            {            
                // validate email
                if (!Common.ValidateEmail(email)) return Json(new { result = "Lỗi ", msg = "Email không hợp lệ" });
                var user = db.admins.FirstOrDefault(x => x.email == email);
                if (user == null)
                {
                    return Json(new { result = "OK", msg = "Hệ thống đã gửi thông tin thay đổi mật khẩu. </br>Vui lòng kiểm tra email để xem hướng dẫn tạo mật khẩu mới." });
                }
                if (String.IsNullOrEmpty(user.admincodereg))
                {
                    user.admincodereg = Guid.NewGuid().ToString();
                }
                var pa = db.resethistories.Where(a => a.customercodereg == user.admincodereg && a.status_id == Constant.StatusActive).OrderByDescending(a => a.senttime).FirstOrDefault();
                if (pa != null)
                {
                    pa.status_id = 2;
                    db.SaveChanges();
                }

                // write reset history
                string t = DateTime.Now.Ticks.ToString();
                var para = new resethistory
                {
                    customercodereg = user.admincodereg
                };
                var now = DateTime.Now;
                para.senttime = now;
                para.expiredtime = DateTime.Now.AddHours(2);
                para.longsenttime = t;
                para.status_id = 1;
                db.resethistories.Add(para);
                db.SaveChanges();

                if (Request.Url != null)
                {
                    string url = Request.Url.GetLeftPart(UriPartial.Authority);
                    string code = user.admincodereg+ "!" + t;
                    string link = url + "/LoginCP/RenewPassAdmin?" + "token=" + code;

                    // send mail
                    IUserMailer mailer = new UserMailer();
                    mailer.PasswordReset(email, link).Send();
                    return Json(new { result = "OK", msg = "Hệ thống đã gửi thông tin thay đổi mật khẩu. </br>Vui lòng kiểm tra email để xem hướng dẫn tạo mật khẩu mới." });
                }
                return Json(new { result = "Error", msg = "Có lỗi." });
            }
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
                   

                    user.password = Common.MaHoa(password);
                    var pa =
                        db.resethistories.FirstOrDefault(
                            a => a.customercodereg == user.admincodereg && a.longsenttime == longtime);
                    pa.status_id = 2;

                    // add history change password
                    var hisChangePass = new PassChange
                    {
                        customer_id = user.admin_id,
                        password = Common.MaHoa(password),
                        datechange = DateTime.Now
                    };
                    db.PassChanges.Add(hisChangePass);

                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();
                    }   
                     
                    return Json(new { result = "OK", msg = "Tạo mật khẩu mới thành công." });
                }               
                return View();
            }
        }


    }
}