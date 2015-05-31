using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MySony.Models;
using MySony.Functions;
using System.Web.Routing;

namespace MySony.Controllers
{
    public class LandingController : Controller
    {
        MySonyEntities db = new MySonyEntities();
        //
        // GET: /Landing/
        public ActionResult Index()
        {
            if (Common.IsLogedIn())
            {
                return RedirectToAction("Index","Home");
            }
            var obj = db.RS_Config.FirstOrDefault();
            if (obj != null) ViewBag.LandingContent = obj.LandingContent;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(String email, String pass, String recaptcha)
        {
            // check captcha
            string messsage;
            if (!Common.VerifyCaptcha(recaptcha, Request.UserHostAddress) && Common.NeedVerifyHuman())
            {
                return Json(new { result = "ERROR", msg = "Mã bảo vệ không hợp lệ" });
            }

            var res = db.customers.Where(x => x.email.ToLower() == email.ToLower() && x.status_id == Constant.StatusActive);
            if (res.Count() > 0) // co tai khoan
            {                
                customer u = res.FirstOrDefault();
                //MD5 md5hash = MD5.Create();
                // if (Common.VerifyMd5Hash(md5hash, pass, u.Pass))\
                try
                {
                    
                    if (u.status_active == Constant.Active)  // active  = 2
                    {
                        //String pass_md5 = mahoa
                        String pass_md5 = Common.MaHoa(pass);
                        if (pass_md5 == u.password)
                        {
                           
                            //prevent concurrent login > sign out cả 2
                            TimeSpan TimeOut = new TimeSpan(0, 0, System.Web.HttpContext.Current.Session.Timeout, 0, 0);
                            string key = u.customer_id.ToString();
                            string cacheKey = u.customer_id.ToString();
                            if ((string)HttpContext.Cache[cacheKey] != Session.SessionID && (string)HttpContext.Cache[cacheKey] != null)
                            {
                                
                               // string key = u.customer_id.ToString();
                                //System.Web.HttpContext.Current.Cache.Insert(key,
                                //    Session.SessionID,
                                //    null,
                                //    DateTime.MaxValue,
                                //    TimeOut,
                                //    System.Web.Caching.CacheItemPriority.NotRemovable,
                                //    null);
                                return Json(new { result = "ERROR", msg = "Tài khoản đang được đăng nhập bời người dùng khác" });
                            }
                           // TimeSpan TimeOut = new TimeSpan(0, 0, System.Web.HttpContext.Current.Session.Timeout, 0, 0);
                           
                            System.Web.HttpContext.Current.Cache.Insert(key,
                                Session.SessionID,
                                null,
                                DateTime.MaxValue,
                                TimeOut,
                                System.Web.Caching.CacheItemPriority.NotRemovable,
                                null);
                            Common.SetUserLogin(u.customer_id);
                            return RedirectToRoute(new RouteValueDictionary(new { Controller = "Home", Action = "Index" }));
                            //return Json(new { result = "OK" });
                        }
                        else // sai mat khau
                        {
                            if (!Common.NeedVerifyHuman())
                            {
                                 System.Web.HttpContext.Current.Cache.Insert(Request.UserHostAddress,
                                  email,
                                  null,
                                  DateTime.MaxValue,
                                  new TimeSpan(0, 0, 30, 0, 0),
                                  System.Web.Caching.CacheItemPriority.NotRemovable,
                                  null);
                            }
                            return Json(new { result = "ERROR", msg = "Mật khẩu hoặc tên tài khoản không đúng, vui lòng thử lại" });
                        }
                    }
                    else
                    {
                        messsage = Constant.MesssageNotActive;
                        return Json(new { result = "ERROR", msg = "Tài khoản "+email+" hiện chưa được kích hoạt. Hãy kiểm tra email để kích hoạt" });
                    }
                    
                }
                catch (Exception)
                {
                    return Json(new { result = "ERROR", msg = "Mật khẩu hoặc tên tài khoản không đúng, vui lòng thử lại" });
                }
            }
            else // th tai khoan ko ton tai
            {
                if (!Common.NeedVerifyHuman())
                {
                    System.Web.HttpContext.Current.Cache.Insert(Request.UserHostAddress,
                     email,
                     null,
                     DateTime.MaxValue,
                     new TimeSpan(0, 0, 30, 0, 0),
                     System.Web.Caching.CacheItemPriority.NotRemovable,
                     null);
                }
                return Json(new { result = "ERROR", msg = "Tài khoản " + email + " chưa được đăng ký" });
            }


        }

        public ActionResult Doublelogin()
        {
            string url = Request.Url.GetLeftPart(UriPartial.Authority);
            ViewBag.doublelogin = url;
            return View();
        }
	}
}