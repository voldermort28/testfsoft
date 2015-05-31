using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using MySony.Models;
using MySony.ViewModels;
using MySony.Functions;
using MySony.Mailers;
using System;
using System.Configuration;

namespace MySony.Controllers
{
    //[OutputCache(Duration = 60 * 60, VaryByParam = "none")]
    public class RegisterController : Controller
    {
        readonly MySonyEntities _db = new MySonyEntities();
        //[OutputCache(Duration = 60 * 60, VaryByParam = "none",Location=System.Web.UI.OutputCacheLocation.Client)]
        public ActionResult Index()
        {
            var registerVm = new RegisterVM
            {
                cartelogies = _db.categories.Where(x => x.status_id == Constant.StatusActive).ToList(),
                cities = _db.cities.Where(x => x.status_id == Constant.StatusActive).OrderByDescending(x => x.porder).ThenBy(x => x.name).ToList()
            };
            return View(registerVm);
        }

        [HttpGet]
        public ActionResult GetDistrictOfCity(int cityID)
        {
            var lstDistrict = _db.districts.Where(x => x.city_id == cityID && x.status_id ==Constant.StatusActive).Select(x => new { x.district_id, x.name }).OrderBy(x => x.name).ToList();
            return Json(lstDistrict, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetProductsOfCategory(int cartegoryID)
        {
              // Điện thoại XPERIA có ID là 11  --> check service để lấy Product
            //if (cartegoryID == Constant.IdProductMobileXp)
            //{
            //        var lstProducts   = RegisterCustomerProduct.ListXperia(db ,cartegoryID )    ;
            //        return Json(lstProducts, JsonRequestBehavior.AllowGet);
            //}
            //else
            //{
            //    var lstProducts = db.products.Where(x => x.category_id == cartegoryID && x.status_id == Constant.StatusActive).Select(x => new { x.product_id, x.name }).ToList();
            //    return Json(lstProducts, JsonRequestBehavior.AllowGet);
            //}

            var lstProducts = _db.products.Where(x => x.category_id == cartegoryID && x.status_id == Constant.StatusActive).Select(x => new { x.product_id, x.name }).OrderBy(x => x.name).ToList();
            return Json(lstProducts, JsonRequestBehavior.AllowGet);
           
        }


        [HttpGet]
        public ActionResult GetShopOfCity(int cityID)
        {
            var lstShops = _db.shops.Where(x => x.city_id == cityID && x.status_id == Constant.StatusActive).Select(x => new { x.shop_id, x.name, x.order_no }).OrderBy(x => x.order_no).ThenBy(x => x.name).ToList();
            return Json(lstShops, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterProcess(customer cus0, List<CustomerProductVM> listCusPro, String recaptcha)
        {

            string message = string.Empty;
            bool isProductActive = true;
            // check captcha
            if (!Common.VerifyCaptcha(recaptcha, Request.UserHostAddress))
            {
                return Json(new { result = "Lỗi ", msg = "Mã bảo vệ không hợp lệ" });
            }           
            // validate
            var objValidate = RegisterCustomerProduct.CheckValidate(cus0);
            if (objValidate)
            {
                return Json(new { result = "ERROR", msg = Constant.HaveExistSpecial });
            }
            var checkEmail = _db.customers.Where(x => x.email.ToLower().Trim() == cus0.email.ToLower().Trim() && x.status_id == Constant.StatusActive).FirstOrDefault();
            if (checkEmail != null)
            {
                return Json(new {result="ERROR", msg="Email đã tồn tại" });
            }
            if (!Common.ValidateEmail(cus0.email))
            {
                return Json(new {result="ERROR", msg="Email không hợp lệ" });
            }
            if ((cus0.email.ToLower() == cus0.password.ToLower())
                || (cus0.email.Substring(0, cus0.email.IndexOf('@')).ToLower() == cus0.password.ToLower()))
            {
                 return Json(new {result="ERROR", msg="Mật khẩu trùng email" });
            }
            if (!Common.ValidatePassUser(cus0.password))
            {
                 return Json(new {result="ERROR", msg="Mật khẩu không hợp lệ" });
            }

            foreach (var item in listCusPro)
	        {
                // Check chinh hang

                int status = RegisterCustomerProduct.CheckProdcutWebserive(item.CategoryID, item.Serial, item.ProductName);
	            if (status != 1)
	            {
	                // san pham khong chinh hang
	                switch (status)
	                {
                        case 4: // not active
                            // send email
                            //IUserMailer mailer1 = new UserMailer();
                           // mailer1.CustomerService(cus0, item).Send();
	                        isProductActive = false;
                            break;
                            //return Json(new { result = Constant.ErrorTitle, msg = Constant.MesssageNotActive });  
                        default:
                            return Json(new { result = Constant.ErrorTitle, msg = Constant.MessageRegisterError });  
	                }
	                  
	            }
              /* switch (status)
                {
                    case -1:
                        return Json(new { result = Constant.ErrorTitle, msg = "Có lỗi xẩy ra." });         
                    case 2:
                        return Json(new { result = Constant.ErrorTitle, msg = "Serial hoặc Product không hợp lệ. Hãy kiểm tra lại sản phẩm." });                        
                    case 3:
                        return Json(new { result = Constant.ErrorTitle, msg = "Serial hoặc Product không hợp lệ. Hãy kiểm tra lại sản phẩm." });
                    case 4:
                        return Json(new { result = Constant.ErrorTitle, msg = "Sản phẩm chưa được kích hoạt. Hãy kích hoạt sản phẩm." });
                    case 5:
                        return Json(new { result = Constant.ErrorTitle, msg = "Serial hoặc Product không hợp lệ. Hãy kiểm tra lại sản phẩm." });                    
                   
                } */
                // check exist shop 
                var shop = _db.shops.FirstOrDefault(x => x.shop_id == item.ShopID && x.status_id == Constant.StatusActive);
                if (shop == null)
                {
                    return Json(new { result = Constant.ErrorTitle, msg = Constant.ShopNotValidate });
                }
                
	        }

            // add new customer
            customer cus = new customer();
            cus.address = cus0.address;
            cus.birthday = cus0.birthday;
            //cus.city = cus0.city;
            cus.city_id = cus0.city_id;
            cus.customertype_id = Convert.ToInt32(ConfigurationManager.AppSettings["customertype_id"]);// 3;// mysony
            //cus.district = cus0;
            cus.district_id = cus0.district_id;
            cus.email = cus0.email;
            cus.firstname = cus0.firstname;
            cus.lastname = cus0.lastname;
            cus.mobilephone = cus0.mobilephone;
            cus.password = Common.MaHoa(cus0.password);
            cus.sex = cus0.sex;
            cus.status_active = 1; // inactive
            cus.status_id = 1;
            cus.datereg = DateTime.Now;
            cus.modifieddate = DateTime.Now;
            Guid code = Guid.NewGuid();
            cus.customercodereg = code.ToString();
            _db.customers.Add(cus);
            _db.SaveChanges();

            // add new order
            RegisterCustomerProduct.LstSerialRegister  = new ArrayList();
            foreach (CustomerProductVM item in listCusPro)
            {
                int numberStatus = 0;
                    numberStatus = RegisterCustomerProduct.RegisterProduct(_db, cus.customer_id, item.CategoryID, item.ProductID, item.Serial, item.BuyDate.ToString(), item.ShopID, item.CityID);
                if (numberStatus != Constant.SuccessRegiterProduct)
                {
                     // phat sinh loi
                    switch (numberStatus)
                    {
                        case 5:
                            message = "Thời gian mua không hợp lệ";
                            break;
                        default:
                            message = Constant.MessageRegisterError;
                            break;
                    }
                    /*switch (numberStatus)
                    {                        
                        case Constant.ExistCustProduct:    // serial not correct
                             message = "Sản phẩm đã được đăng kí";
                            break;
                        case   Constant.NotExistSerial:   // have exception
                               message = "Không tồn tại serial tương ứng. Hãy kiểm tra lại.";
                            break;
                        case    Constant.Excepction:
                            message = "Có lỗi khi đăng kí sản phẩm mới";
                            break;
                        case 4:
                            message = "Không tồn tại số serail tương ứng. Hãy kiểm tra lại";
                            break;
                        case 5:
                            message = "Thời gian mua không hợp lệ";
                            break;
                        default:
                            message = "Có lỗi khi đăng kí sản phẩm";
                            break;
                    }*/
                    // remove user vừa tạo khi có lỗi
                    var deleteOrderDetails = from details in _db.orders
                                             where details.customer_id == cus.customer_id
                                             select details;

                    foreach (var detail in deleteOrderDetails)
                    {
                        _db.orders.Remove(detail);
                    }

                    _db.customers.Remove(cus);
                    _db.SaveChanges();

                    return Json(new { result = "ERROR", msg = message });
                }
            }

            //// send mail
            try
            {

                String tokenActive = Common.Base64Encode(Common.Encrypt(cus.email + "|" + cus.customercodereg));
                String activeURL = "http://" + Request.Url.Host + "/Register/Active?tokenActive=" + tokenActive;
               int i =  Common.SendMailRegister(_db, cus, Convert.ToInt32(cus0.city_id),activeURL);
            }
            catch (Exception ex)
            {
                Common.WriteLog(ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace);
            }
            // done
            
            var dateGuarantee =  RegisterCustomerProduct.Message(_db,cus.customer_id);

            
            message = isProductActive ? String.Format(Constant.MessageSuccessfully, dateGuarantee) : Constant.MesssageNotActive;
         /*   message = Constant.MessageSuccessfully;*/

            return Json(new { result = "OK", msg = message });
        }
 

        [HttpGet]
        public ActionResult ForgetPass()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgetPass(string email, String recaptcha)
        {
            // check captcha
            if (!Common.VerifyCaptcha(recaptcha, Request.UserHostAddress))
            {
                return Json(new { result = "ERROR ", msg = "Mã bảo vệ không hợp lệ" });
            }
             
            // validate
            if (!Common.ValidateEmail(email)) return Json(new { result = "OK", msg = "Hệ thống đã gửi thông tin thay đổi mật khẩu. </br>Vui lòng kiểm tra email để xem hướng dẫn tạo mật khẩu mới." });// Json(new { result = "Lỗi ", msg = "Email không hợp lệ" });
            var user = _db.customers.FirstOrDefault(x => x.email == email && x.status_id == Constant.StatusActive);
            if (user == null) return Json(new { result = "OK", msg = "Hệ thống đã gửi thông tin thay đổi mật khẩu. </br>Vui lòng kiểm tra email để xem hướng dẫn tạo mật khẩu mới." });//Json(new { result = "Lỗi ", msg = "Email " + email + " chưa được đăng ký. </br> Hãy kiểm tra lại thông tin email ." });
            var pa = _db.resethistories.Where(a => a.customercodereg == user.customercodereg && a.status_id == Constant.StatusActive).OrderByDescending(a => a.senttime).FirstOrDefault();
            if (pa != null)
            {
                pa.status_id = 2;
                _db.SaveChanges();
            }

            string t = DateTime.Now.Ticks.ToString();
            resethistory para = new resethistory();
            para.customercodereg = user.customercodereg;
            DateTime now = DateTime.Now;
            para.senttime = now;
            para.expiredtime = DateTime.Now.AddHours(2);
            para.longsenttime = t;
            para.status_id = 1;

            _db.resethistories.Add(para);
            _db.SaveChanges();

            string url = Request.Url.GetLeftPart(UriPartial.Authority);
            string code = user.customercodereg.ToString() + "!" + t;
            string link = url + "/Register/RenewPass?" + "token=" + code;

            // send mail
            IUserMailer mailer = new UserMailer();
            mailer.PasswordReset(email, link).Send();
            return Json(new { result = "OK", msg = "Hệ thống đã gửi thông tin thay đổi mật khẩu. </br>Vui lòng kiểm tra email để xem hướng dẫn tạo mật khẩu mới." });
        }

        [HttpGet]
        public ActionResult RenewPass(string token)
        {
            ViewBag.result = "ERROR";
            string[] para = token.Split('!');
            string customercode = para[0].ToString();
            string timecode = para[1].ToString();
            long longtime = long.Parse(timecode);
            DateTime time = new DateTime(longtime);
            DateTime expiretime = time.AddHours(2);

            var pa = _db.resethistories.Where(a => a.customercodereg == customercode && a.longsenttime == timecode && a.status_id == Constant.StatusActive).OrderByDescending(a => a.senttime).FirstOrDefault();
            if (pa == null)
            {
                ViewBag.msg = "Link tạo mới mật khẩu không hợp lệ";
            }
            else
            {
                if (DateTime.Compare(DateTime.Parse(pa.expiredtime.ToString()), DateTime.Now) < 0)
                {
                    pa.status_id = 2;
                    _db.SaveChanges();
                    ViewBag.msg = "Link tạo mới mật khẩu hết hạn sử dụng";
                }
                else
                {
                    
                    var user = _db.customers.FirstOrDefault(x => x.customercodereg == customercode && x.status_id == Constant.StatusActive);
                    if (user == null) ViewBag.msg = "Tài khoản không tồn tại";
                    if (user.status_id == 2) ViewBag.msg = "Tài khoản này đã bị khóa, vui lòng liên hệ quản trị site";
                    else
                    {
                        ViewBag.result = "OK";
                        ViewBag.email =  Common.Base64Encode(Common.Encrypt(user.email));
                        ViewBag.longtime = timecode;
                        ViewBag.msg = "";
                    }
                }
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RenewPass(string email, string password, string longtime)
        {
            try 
            {
                // validate
                string deccryptEmail = Common.Decrypt(Common.Base64Decode(email)); 
                if (!Common.ValidatePassUser(password))
                {
                    return Json(new { result = "ERROR", msg = "Mật khẩu không hợp lệ" });
                }
                if ((deccryptEmail.ToLower() == password.ToLower())
                || (deccryptEmail.Substring(0, deccryptEmail.IndexOf('@')).ToLower() == password.ToLower()))
                {
                     return Json(new { result = "ERROR", msg = "Mật khẩu trùng email" });
                }

                var user = _db.customers.FirstOrDefault(x => x.email == deccryptEmail);
                if (user == null)
                {
                    return Json(new { result = "ERROR", msg = "Tài khoản không tồn tại." });
                }
                // check Historry Change Password
                var objHisPass = CheckPassword.CheckOldPass(_db, user.customer_id, Common.MaHoa(password));
                if (!objHisPass)
                {
                    return Json(new { result = "ERROR", msg = Constant.SameHistoryOldPass });
                }
                user.password = Common.MaHoa(password);
                user.status_active = 2;
                var pa = _db.resethistories.FirstOrDefault(a => a.customercodereg == user.customercodereg && a.longsenttime == longtime);
                pa.status_id = 2;

                // add history change password
                var hisChangePass = new PassChange
                {
                    customer_id = user.customer_id,
                    password = Common.MaHoa(password),
                    datechange = DateTime.Now
                };
                _db.PassChanges.Add(hisChangePass);
                if (ModelState.IsValid)
                {
                    _db.SaveChanges();
                }                
                return Json(new { result = "OK", msg = "Tạo mật khẩu mới thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { result = "ERROR", msg = "Lỗi: " + ex.Message + ex.StackTrace });
            }
        }

        public ActionResult Active(String tokenActive)
        {
            String tokenDecrypt = "";
            String code = "";
            String email = "";
            String msg = "";
            try
            {
                tokenDecrypt = Common.Decrypt(Common.Base64Decode(tokenActive));
                email = tokenDecrypt.Split('|')[0];
                code = tokenDecrypt.Split('|')[1];
            }
            catch (Exception)
            {

                msg = "Mã kích hoạt không hợp lệ";
            }

            if (String.IsNullOrEmpty(code) || String.IsNullOrEmpty(email))
            {
                msg = "Mã kích hoạt không hợp lệ";
            }
            else
            {
                var user = _db.customers.Where(x => x.customercodereg == code && x.email == email).FirstOrDefault();
                if (user != null)
                {
                    user.status_active = 2; // active
                    _db.SaveChanges();
                    msg = "OK";
                }
                else
                {
                    msg = "Mã kích hoạt không hợp lệ";
                }
            }
            ViewBag.msg = msg;
            return View();
        }

    }
}