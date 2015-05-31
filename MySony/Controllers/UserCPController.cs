 
using System.Collections;
using MySony.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MySony.Models;
using MySony.ViewModels;
using System.Data.Entity;
using MySony.SonyServcies;
using MySony.Filters;

namespace MySony.Controllers
{
    // [OutputCache(Duration = 60*5, VaryByParam = "none", Location = System.Web.UI.OutputCacheLocation.Client)]
    [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore=true)]
    public class UserCPController : Controller
    {
        private MySonyEntities db = new MySonyEntities();
        [LoginRequried]
        public ActionResult Index()
        {
            int idCustomer =  (int)Session["UserID"];
            var profile = db.customers.FirstOrDefault(x => x.customer_id == idCustomer );
            if (profile != null)
            {
                ManageCustomerViewModel obj = new ManageCustomerViewModel();
                obj.email = profile.email;
                obj.firstname = profile.firstname;
                obj.lastname = profile.lastname;
                obj.sex = profile.sex;                    
                obj.birthday = profile.birthday;
                obj.mobilephone = profile.mobilephone;
                obj.homephone = Common.Cheknull(profile.homephone);
                obj.address = Common.Cheknull(profile.address);
                obj.DistrictName = profile.district != null ? Common.Cheknull(profile.district.name) : string.Empty;
                obj.CityName = profile.city != null ? Common.Cheknull(profile.city.name) : string.Empty;                
                obj.JobName = profile.job != null ? Common.Cheknull(profile.job.name) : string.Empty;
                obj.EducationName = profile.education != null ? Common.Cheknull(profile.education.name) : string.Empty;
                obj.identitycard =Common.Cheknull(profile.identitycard);                
                return View(obj);
            }
            return View();
        }

        [LoginRequried]
        public ActionResult MyProduct()
        {
            int idCustomer =  (int)Session["UserID"];
            CustomerVM registerVm = new CustomerVM
            {
                LstCategories = db.categories.Where(x => x.status_id == Constant.StatusActive).ToList(),
                LstProducts = null,
                LstCity = db.cities.Where(x=>x.status_id == Constant.StatusActive).OrderByDescending(x => x.porder).ThenBy(x => x.name).ToList(),                
                LstShops = null,
                LstCusPro = RegisterCustomerProduct.ListImage(db ,idCustomer)
            };
            return View(registerVm);
        }

        [LoginRequried]
        public ActionResult Logout()
        {
            var userid = Convert.ToString(Session["UserID"]);
            Common.SetUserLoggout(userid);
            if (!String.IsNullOrEmpty(Request.QueryString["next"]))
            {
                String next = Convert.ToString(Request.QueryString["next"]);
                return RedirectToLocal(next);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: /UserCP/ChangePassword
        [LoginRequried]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public  ActionResult ChangePassword(string currentPass, string newPass, string confirmPass)        
        {
            int hasPassword = CheckPermission(currentPass, newPass, confirmPass);

            switch (hasPassword)
            {
                case -4:    // not match validate 
                    return Json(new { result = -4, msg = Constant.PasswordNotMatch });
                case -3:    //  not not invalid
                    return Json(new { result = -3, msg = Constant.PasswordNotValid });
                case -2:    // not confirm password
                    return Json(new { result = -2, msg = Constant.PasswordNull });
                case -1:    // not confirm password
                    return Json(new { result = -1, msg = Constant.ConfirmPassword });   
                case 0:
                    return Json(new { result = 0, msg = Constant.MessageSuccfully });                    
                case 1:
                    return Json(new { result = 1, msg = Constant.NotPassUser });     
                case 3:
                    return Json(new { result = 3, msg = Constant.SameOldPass });
                case 4:
                    return Json(new { result = 4, msg = Constant.SameHistoryOldPass });    
                default:
                    return Json(new { result = 2, msg = Constant.HasEx });                    

            }
        }
          
         /// <summary>
         /// Kiem tra thong tin doi password
         /// </summary>
         /// <param name="currentpassword"> Password hien tai</param>
         /// <param name="newpassword">Password moi</param>
         /// <param name="confirmPassword">Confirm password</param>
         /// <returns>
         /// -1: Password mới và confirm password khong dung
         /// -2: Password mới hoac confirm password rong
         /// -3 :Mat khau khong hop le
         /// -4 :Mat khau khong hop le
         ///  0: Doi mat khau thanh cong
         ///  1: currentpassword khong dung 
         ///  2: Co Exception
         ///  3: Mat khau moi trung voi mat khau cu
         ///  4: Mat khau da tuong duoc su dung
         /// </returns>
         
         [LoginRequried]
        private int CheckPermission(string currentpassword ,string newpassword , string confirmPassword)
        {
            int dRtn = 0; 
            try
            {
                if (!newpassword.Equals(confirmPassword))
                {
                    return -1;
                }
                if (string.IsNullOrEmpty(newpassword) || string.IsNullOrEmpty(confirmPassword) )
                {
                      return -2;  
                }
                if (!Common.ValidatePassUser(newpassword))
                {
                    return -3;  // mat khau moi khong dung dinh dang
                }


                if (CheckPassword.CheckValidateHisPassword(currentpassword,newpassword))
                {
                    return -4;  // mat khau gan giong voi mat khau hien tai
                }

                string encCurrentrPass = Common.MaHoa(currentpassword);
                string encNewPass = Common.MaHoa(newpassword);
                using (var _db = new MySonyEntities())
                {
                    var idCustomer =  (int)Session["UserID"];
                    var profile = _db.customers.FirstOrDefault(x => x.customer_id == idCustomer && x.password == encCurrentrPass);
                    if (profile != null)
                    {
                        // validate new pass
                        if (newpassword == currentpassword)
                        {
                            return 3; // mau khau cu trung mat khau moi
                        }

                        var test = CheckPassword.CheckOldPass(_db, idCustomer, encNewPass);
                        if (!test)
                        {
                            return 4;  // mat khau da duoc su dung
                        }
                        profile.password = encNewPass;
                        profile.modifieddate = DateTime.Now;
                        // add new history
                        var objPasswrod  = new PassChange
                        {
                            customer_id = idCustomer,
                            password = encNewPass,
                            datechange = DateTime.Now
                        };

                        if (ModelState.IsValid)
                        {
                            _db.Entry(profile).State = EntityState.Modified;
                            _db.PassChanges.Add(objPasswrod);
                            _db.SaveChanges();
                            dRtn = 0; // cuccessfull
                        }
                    }
                    else
                    {
                        dRtn = 1;  // Not correct password
                    }
                }
            }
            catch (Exception)
            {
                dRtn = 2;                 // have Exception
            }
            return dRtn;
        }


        // POST: /UserCP/RegisterCustProduct
        [LoginRequried]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterCustProduct(int categoryId, int productId, string serialNumber, string timePuCharsedId, int cityId, int shopId, string productName)
        {
            try
            {
                var idCustomer = (int)Session["UserID"];
                int numberStatus = 0;
                RegisterCustomerProduct.LstSerialRegister = new ArrayList();

                int status = RegisterCustomerProduct.CheckProdcutWebserive(categoryId, serialNumber, productName);
                if (status != 1)
                {
                    // san pham khong chinh hang
                    switch (status)
                    {
                        case 4: // not active
                            return Json(new { result = Constant.ErrorTitle, msg = Constant.MesssageNotActive });
                        default:
                            return Json(new { result = Constant.ErrorTitle, msg = Constant.MessageRegisterError });
                    }
                }
                //switch (status)
                //{
                //    case -1:
                //        return Json(new { result = Constant.ErrorTitle, msg = "Có lỗi xẩy ra." }, JsonRequestBehavior.AllowGet);
                //    case 2:
                //        return Json(new { result = Constant.ErrorTitle, msg = "Serial hoặc Product không hợp lệ." }, JsonRequestBehavior.AllowGet);
                //    case 3:
                //        return Json(new { result = Constant.ErrorTitle, msg = "Serial hoặc Product không hợp lệ." }, JsonRequestBehavior.AllowGet);
                //    case 4:
                //        return Json(new { result = Constant.ErrorTitle, msg = "Sản phẩm chưa được kích hoạt. Hãy kích hoạt sản phẩm." }, JsonRequestBehavior.AllowGet);
                //    case 5:
                //        return Json(new { result = Constant.ErrorTitle, msg = "Sản phẩm đăng kí không là sản phẩm chính hãng. Hãy kiểm tra lại sản phẩm." }, JsonRequestBehavior.AllowGet);

                //} 


                numberStatus = RegisterCustomerProduct.RegisterProduct(db, idCustomer, categoryId, productId, serialNumber, timePuCharsedId, shopId, cityId);                                                     
                switch (numberStatus)
                {
                    case 1:
                        //succcess
                        return Json(new { result = Constant.SuccessfullyTitle, msg = Constant.RegisterSuccessfully + " <br/>" 
                                        + RegisterCustomerProduct.Message(db, idCustomer) }, JsonRequestBehavior.AllowGet);
                    //case 2:
                    //    // not exist serial with category and product
                    //    return Json(new { result = Constant.ErrorTitle, msg = Constant.NotExistSerialMessage }, JsonRequestBehavior.AllowGet);
                    //case 3:
                    //    // not exist serial with category and product
                    //    return Json(new { result = Constant.ErrorTitle, msg = Constant.Existprodcut }, JsonRequestBehavior.AllowGet);
                    //case 5:
                    //    return Json(new { result = Constant.ErrorTitle, msg = Constant.TimeNotTrue }, JsonRequestBehavior.AllowGet);
                    default: // 0
                        // have exeption
                        //return Json(new { result = Constant.ErrorTitle, msg = Constant.HasExRegisterProduct }, JsonRequestBehavior.AllowGet);
                        return Json(new { result = Constant.ErrorTitle, msg = Constant.MessageRegisterError });
                }
            }
            catch (Exception)
            {
                return Redirect("/Landing");
            }            
        }

        [LoginRequried]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ProfileEditor(int ID = 0)
        {
            ViewData["categories"] = db.categories.Where(x => x.status_id == Constant.StatusActive).Select(x => new { category_id = x.category_id, name = x.name }).ToList();
            ViewData["products"] = db.products.Where(x => x.status_id == Constant.StatusActive).Select(x => new { product_id = x.product_id, name = x.name }).ToList();
            ViewData["shops"] = db.shops.Where(x => x.status_id == Constant.StatusActive).Select(x => new { shop_id = x.shop_id, name = x.name }).ToList();
            if (ID == 0) return View();
            else
            {
                var obj = db.customers.FirstOrDefault(x => x.customer_id == ID);
                if (obj == null) return HttpNotFound();
                return View(obj);
            }
        }

        [LoginRequried]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetProductByCat(int categoryId)
        {
            var objs = db.products.Where(x => x.category_id == categoryId && x.status_id == Constant.StatusActive)
                        .Select(x => new { product_id = x.product_id, name = x.name });
            return Json(objs, JsonRequestBehavior.AllowGet);
        }

        [LoginRequried]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetShopByCity(int cityId)
        {
            var objs =
                    db.shops.Where(x => x.city_id == cityId && x.status_id == Constant.StatusActive)
                        .Select(x => new { shop_id = x.shop_id, name = x.name });
            return Json(objs, JsonRequestBehavior.AllowGet);
        }

        [LoginRequried]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetFullName()
        {
            int idCustomer = (int)Session["UserID"];
            var profile = db.customers.FirstOrDefault(x => x.customer_id == idCustomer);
            ManageCustomerViewModel obj = new ManageCustomerViewModel();
            obj.firstname = profile.firstname;
            obj.lastname = profile.lastname;
            return Json(obj, JsonRequestBehavior.AllowGet);

        }

         

        [LoginRequried]
        [HttpGet]
        public ActionResult GetCustProduct()
        {
            try
            {
                var idCustomer = (int)Session["UserID"];
                var lstShops = RegisterCustomerProduct.ListImage(db, idCustomer);
                return Json(lstShops, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Redirect("/Landing");
            }
        }
        
    }
}