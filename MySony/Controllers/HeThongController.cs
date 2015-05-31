using DocumentFormat.OpenXml.Math;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using MySony.Filters;
using MySony.Functions;
using MySony.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Text.RegularExpressions;
using System.Text;
using MySony.ViewModels;
using System.IO;
using MySony.Mailers;
using Kendo.Mvc;
using System.ComponentModel;
using System.Data.Entity.Core.Objects;

namespace MySony.Controllers
{
    [AuthorizeIPAddressAttribute]
    [CheckPermissionActionFilter]
   // [OutputCache(Duration = 60 * 5, VaryByParam = "none", Location=System.Web.UI.OutputCacheLocation.Client)]
    [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
    public class HeThongController : Controller
    {
        MySonyEntities db = new MySonyEntities();
        private static readonly string[] validImageTypes = new string[] { "image/png", "image/jpg", "image/jpeg", "image/gif" };
        private static readonly string[] valideImageExt = new string[] { ".jpg", ".jpeg", ".png", ".gif" };
        [CheckPermissionActionFilter]
        public ActionResult Index()
        {

            TempData["ShowFullMenu"] = Common.ChekSuperAdmin();
            ViewBag.ShowFullMenu = Common.ChekSuperAdmin() ? "showMenu" : "NotshowMenu";
            return View();
        }

        #region Customer

        private List<FilterDescriptor> GetFilters(IEnumerable<IFilterDescriptor> filters)
        {
            List<FilterDescriptor> lstFilter = new List<FilterDescriptor>();
            if (filters.Any())
            {                
                foreach (var filter in filters)
                {
                    var descriptor = filter as FilterDescriptor;
                    if (descriptor != null)
                    {
                        lstFilter.Add(descriptor);
                    }
                    else if (filter is CompositeFilterDescriptor)
                    {
                        lstFilter.AddRange(GetFilters(((CompositeFilterDescriptor)filter).FilterDescriptors));
                    }
                }                
            }
            return lstFilter;
        }
        
        [AcceptVerbs(HttpVerbs.Get)]
        [CheckPermissionActionFilter]
        public ActionResult Profiles([DataSourceRequest]DataSourceRequest request)
        {
            ViewData["status"] = db.status.Select(x => new { status_id = x.status_id, name = x.name }).ToList();
            var lst = new SerialVm
            {
                lstCategories = db.categories.Where(x => x.status_id == Constant.StatusActive).ToDictionary(x=>x.category_id, x=> x.name)
            };
            return View(lst);
        }
        
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [CheckPermissionActionFilter]
        public ActionResult Profile_Read([DataSourceRequest]DataSourceRequest request, int categoryId, string modelName = "", string serial = "", bool? isPending = null)
        {
            IQueryable<customer> lstCustomers;
            IQueryable<ProfileVM> objs;


            Object lst;
            if (Common.ChekSuperAdmin())
            {
                // Load by isPending
                lstCustomers = isPending == true ? db.customers.Where(x => x.IsPending == true) : db.customers.Where(x => (x.IsPending != true || x.IsPending == null));
            }
            else
            {
                // Load by isPending
                lstCustomers = isPending == true ? db.customers.Where(x => x.IsPending == true && x.status_id == Constant.StatusActive) : db.customers.Where(x => (x.IsPending != true || x.IsPending == null) && x.status_id == Constant.StatusActive);
            }

            // Filtering
            List<FilterDescriptor> lstFilter = GetFilters(request.Filters);
            if (lstFilter.Any())
            {
                foreach (FilterDescriptor filter in lstFilter)
                {
                    switch (filter.Member)
                    {
                        case "Firstname":
                            lstCustomers = lstCustomers.Where(x => x.firstname.Contains(filter.ConvertedValue.ToString()));
                            break;
                        case "Lastname":
                            lstCustomers = lstCustomers.Where(x => x.lastname.Contains(filter.ConvertedValue.ToString()));
                            break;
                        case "Birthday":
                            DateTime Birthdt = Convert.ToDateTime(filter.ConvertedValue);
                            switch (filter.Operator)
                            {
                                case FilterOperator.IsEqualTo:
                                    lstCustomers = lstCustomers.Where(x => DbFunctions.TruncateTime(x.birthday) == Birthdt);
                                    break;
                                case FilterOperator.IsGreaterThan:
                                    lstCustomers = lstCustomers.Where(x => DbFunctions.TruncateTime(x.birthday) > Birthdt);
                                    break;
                                case FilterOperator.IsGreaterThanOrEqualTo:
                                    lstCustomers = lstCustomers.Where(x => DbFunctions.TruncateTime(x.birthday) >= Birthdt);
                                    break;
                                case FilterOperator.IsLessThan:
                                    lstCustomers = lstCustomers.Where(x => DbFunctions.TruncateTime(x.birthday) < Birthdt);
                                    break;
                                case FilterOperator.IsLessThanOrEqualTo:
                                    lstCustomers = lstCustomers.Where(x => DbFunctions.TruncateTime(x.birthday) <= Birthdt);
                                    break;
                            }
                            break;
                        case "Email":
                            lstCustomers = lstCustomers.Where(x => x.email.Contains(filter.ConvertedValue.ToString()));
                            break;
                        case "Mobile":
                            lstCustomers = lstCustomers.Where(x => x.mobilephone.Contains(filter.ConvertedValue.ToString()));
                            break;
                        case "RegDate":
                            DateTime Regdt = Convert.ToDateTime(filter.ConvertedValue).Date;
                            switch (filter.Operator)
                            {
                                case FilterOperator.IsEqualTo:
                                    lstCustomers = lstCustomers.Where(x => DbFunctions.TruncateTime(x.datereg) == Regdt);
                                    break;
                                case FilterOperator.IsGreaterThan:
                                    lstCustomers = lstCustomers.Where(x => DbFunctions.TruncateTime(x.datereg) > Regdt);
                                    break;
                                case FilterOperator.IsGreaterThanOrEqualTo:
                                    lstCustomers = lstCustomers.Where(x => DbFunctions.TruncateTime(x.datereg) >= Regdt);
                                    break;
                                case FilterOperator.IsLessThan:
                                    lstCustomers = lstCustomers.Where(x => DbFunctions.TruncateTime(x.datereg) < Regdt);
                                    break;
                                case FilterOperator.IsLessThanOrEqualTo:
                                    lstCustomers = lstCustomers.Where(x => DbFunctions.TruncateTime(x.datereg) <= Regdt);
                                    break;
                            }
                            break;
                    }
                }
            }

            // create ViewModel for customers
            objs = lstCustomers.Select(x => new ProfileVM
            {
                ID = x.customer_id,
                Firstname = x.firstname,
                Lastname = x.lastname,
                Birthday = x.birthday,
                Sex = x.sex.HasValue ? (x.sex.Value ? "Nam" : "Nữ") : String.Empty,
                Email = x.email,
                Mobile = x.mobilephone,
                City = x.city != null ? x.city.name : String.Empty,
                District = x.district != null ? x.district.name : String.Empty,
                Address = x.address,
                RegDate = x.datereg,
                status_id = x.status_id,
                status_active = x.status_active,
                IsActive = x.status_active == 1 ? "Chưa kích hoạt" : "Đã kích hoạt",
                Status = x.status_id == 1 ? "Đang hoạt động" : "Xóa",
            });

            // Search by category
            if (categoryId > 0)
            {
                var lstCustomerId = db.orders.Where(x => x.catagory_id == categoryId).Select(x => x.customer_id.Value); ;
                objs = objs.Where(x => lstCustomerId.Contains(x.ID));
            }
            // Search by product
            if (String.IsNullOrEmpty(modelName))
            {
                if (!String.IsNullOrEmpty(serial))
                {
                    var lstCustomerId = db.orders.Where(x => x.serial.Trim() == serial).Select(x => x.customer_id.Value); ;
                    objs = objs.Where(x => lstCustomerId.Contains(x.ID));
                }
            }
            else
            {
                IQueryable<Int32> lstCustomerId;
                if (!String.IsNullOrEmpty(serial)) lstCustomerId = db.orders.Where(x => x.product.name == modelName && x.serial.Trim() == serial).Select(x => x.customer_id.Value);
                else lstCustomerId = db.orders.Where(x => x.product.name == modelName).Select(x => x.customer_id.Value);
                objs = objs.Where(x => lstCustomerId.Contains(x.ID));
            }

            // Sum total records
            int total = objs.AsQueryable().Count();

            // sorting
            if (request.Sorts.Any())
            {
                foreach (SortDescriptor sortDescriptor in request.Sorts)
                {
                    if (sortDescriptor.SortDirection == ListSortDirection.Ascending)
                    {
                        switch (sortDescriptor.Member)
                        {
                            case "ID":
                                objs = objs.OrderBy(order => order.ID);
                                break;
                            case "Firstname":
                                objs = objs.OrderBy(order => order.Firstname);
                                break;
                            case "Lastname":
                                objs = objs.OrderBy(order => order.Lastname);
                                break;
                            case "Birthday":
                                objs = objs.OrderBy(order => order.Birthday);
                                break;
                            case "Email":
                                objs = objs.OrderBy(order => order.Email);
                                break;
                            case "Mobile":
                                objs = objs.OrderBy(order => order.Mobile);
                                break;
                            case "RegDate":
                                objs = objs.OrderBy(order => order.RegDate);
                                break;
                        }
                    }
                    else
                    {
                        switch (sortDescriptor.Member)
                        {
                            case "ID":
                                objs = objs.OrderByDescending(order => order.ID);
                                break;
                            case "Firstname":
                                objs = objs.OrderByDescending(order => order.Firstname);
                                break;
                            case "Lastname":
                                objs = objs.OrderByDescending(order => order.Lastname);
                                break;
                            case "Birthday":
                                objs = objs.OrderByDescending(order => order.Birthday);
                                break;
                            case "Email":
                                objs = objs.OrderByDescending(order => order.Email);
                                break;
                            case "Mobile":
                                objs = objs.OrderByDescending(order => order.Mobile);
                                break;
                            case "RegDate":
                                objs = objs.OrderByDescending(order => order.RegDate);
                                break;
                        }
                    }
                }
            }
            else objs = objs.OrderByDescending(x => x.RegDate); // EF can't page unsorted data

            // Apply paging
            if (request.PageSize == 0) request.PageSize = 20;
            if (request.Page > 0) objs = objs.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize);

            // Initialize  the DataSourceResult
            var result = new DataSourceResult()
            {
                Data = objs.ToList(), // Process data (paging and sorting applied)
                Total = total // Total number of records
            };
            return Json(result);
        }
        
        /* for delete
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Profile_Destroy([DataSourceRequest] DataSourceRequest request, ProfileVM item)
        {
            var obj = db.customers.FirstOrDefault(x=>x.customer_id == item.ID);
            if (obj == null) return HttpNotFound();
            try
            {
                obj.status_id = 2;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }
        */
        [AcceptVerbs(HttpVerbs.Get)]
        [CheckPermissionActionFilter]
        public ActionResult ProfileEditor(int ID = 0)
        {
            try
            {
                ViewData["cities"] = db.cities.Where(x => x.status_id == 1).Select(x => new { city_id = x.city_id, name = x.name, x.porder }).OrderByDescending(x => x.porder).ThenBy(x => x.name).ToList();
                ViewData["districts"] = db.districts.Where(x => x.status_id == 1).Select(x => new { district_id = x.district_id, name = x.name }).ToList();
                ViewData["types"] = db.customertypes.Where(x => x.status_id == 1).Select(x => new { ID = x.customertype_id, Name = x.name }).OrderByDescending(x=>x.ID).ToList();
                ViewData["status"] = db.status.Select(x => new { ID = x.status_id, Name = x.name }).ToList();
                ViewData["categories"] = db.categories.Where(x => x.status_id == 1).Select(x => new { category_id = x.category_id, name = x.name }).ToList();
                ViewData["products"] = db.products.Where(x => x.status_id == 1).Select(x => new { product_id = x.product_id, name = x.name }).ToList();
                ViewData["shops"] = db.shops.Where(x => x.status_id == 1).Select(x => new { shop_id = x.shop_id, name = x.name }).ToList();
                List<SelectListItem> lstActive = new List<SelectListItem>();
                lstActive.Add(new SelectListItem { Value = "1", Text = "Chưa kích hoạt" });
                lstActive.Add(new SelectListItem { Value = "2", Text = "Đã kích hoạt" });
                ViewData["isActive"] = lstActive;
                if (ID == 0) return View();
                else
                {
                    var obj = db.customers.FirstOrDefault(x=>x.customer_id == ID);
                    if (obj == null) return HttpNotFound();
                    obj.IsPending = false; // set default
                    return View(obj);
                }
            }
            catch (Exception ex)
            {
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Có lỗi xảy ra, vui lòng kiểm tra log";
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                return null;
            }
        }
        
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [CheckPermissionActionFilter]
        public ActionResult CustomerProduct_Read([DataSourceRequest]DataSourceRequest request, int customer_id = 0)
        {
            try
            {
                IEnumerable<CustomerProductVM> objs = null;
                if (customer_id > 0)
                {
                    objs = db.orders.Where(x => x.customer_id == customer_id).Select(x => new CustomerProductVM
                    {
                        ID = x.order_id,
                        CustomerID = x.customer_id,
                        Product = x.product.name,
                        Serial = x.serial,
                        Shop = x.shop.name,
                        BuyDate = x.date,
                        WarrantyEnd = x.WarrantyEnd,
                    });
                }
                return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                                
            }
            return null;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [CheckPermissionActionFilter]
        public ActionResult CustomerProduct_Destroy([DataSourceRequest] DataSourceRequest request, CustomerProductVM item)
        {
            var obj = db.orders.FirstOrDefault(x => x.order_id == item.ID);
            if (obj == null) return HttpNotFound();
            try
            {
                db.orders.Remove(obj);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }
        
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [CheckPermissionActionFilter]
        //[ValidateInput(false)]
        public ActionResult SaveProfile(customer profile, bool hidAddProduct = false)
        {
            try
            {
                if (Common.GetAdminType() == 1 || Common.GetAdminType() == 2)
                {
                    Guid code = Guid.NewGuid();
                // add a new item
                if (profile.customer_id == 0)
                {
                    // check permission add customer
                   
                        if (db.customers.FirstOrDefault(x => x.email.ToLower().Trim() == profile.email.ToLower().Trim()) == null)
                        {
                            //profile.status_active = 1; // default
                            profile.status_id = 1; // default
                            profile.datereg = DateTime.Now;
                            profile.modifieddate = DateTime.Now;                  
                            profile.password = Common.MaHoa(profile.password.Trim());                            
                            profile.customercodereg = code.ToString();
                            db.customers.Add(profile);
                            db.SaveChanges();
                            TempData["Notice"] = "Tạo mới khách hàng thành công";
                            TempData["ShowPopup"] = true;
                        }
                        else
                        {
                            TempData["Notice"] = "Email này đã được đăng ký rồi";
                            TempData["ShowPopup"] = true;
                        }                                       
                }
                else // modify item
                {
                    var existObj = db.customers.FirstOrDefault(x => x.customer_id == profile.customer_id);
                    if (existObj != null)
                    {
                        if (existObj.password.Trim() != profile.password.Trim()) existObj.password = Common.MaHoa(profile.password.Trim());
                        if (!existObj.datereg.HasValue) existObj.datereg = DateTime.Now;
                        existObj.address = profile.address;
                        existObj.birthday = profile.birthday;
                        existObj.city_id = profile.city_id;
                        existObj.customertype_id = profile.customertype_id;
                        existObj.district_id = profile.district_id;
                        existObj.email = profile.email;
                        existObj.firstname = profile.firstname;
                        existObj.lastname = profile.lastname;
                        existObj.sex = profile.sex;
                        existObj.status_active = profile.status_active;
                        existObj.IsPending = profile.IsPending;
                        existObj.status_id = 1; // default
                        existObj.modifieddate = DateTime.Now;
                        
                        if (string.IsNullOrEmpty(existObj.customercodereg))
                        {
                            existObj.customercodereg = code.ToString();
                        }
                        db.SaveChanges();
                        TempData["Notice"] = "Cập nhật khách hàng thành công";
                        TempData["ShowPopup"] = true;
                    }
                }
                }
                else
                {
                    TempData["Notice"] = "Bạn không có quyền tạo mới khách hàng. Hãy liên hệ với administrator";
                    TempData["ShowPopup"] = true;
                }
            }
            catch (Exception ex)
            {
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Có lỗi xảy ra, vui lòng kiểm tra log";
                Common.WriteLog(ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace);
            }
            if (hidAddProduct) return RedirectToAction("ProfileEditor", new { ID = profile.customer_id });
            else return RedirectToAction("Profiles");
        }
        
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [CheckPermissionActionFilter]
        public ActionResult DeleteProfile(int Id, bool isDeletePhysical = false)
        {
            string message;
            try
            {
                if (Common.GetAdminType() == 1 || Common.GetAdminType() == 2)
                {
                    var profile = db.customers.FirstOrDefault(x => x.customer_id == Id);
                    if (profile == null)
                    {
                        return HttpNotFound();
                    }
                    DeleteCustomer(isDeletePhysical, profile);
                    // Show alert modal
                     message= "Xóa thành công";
                }
                else
                {
                    message = "Bạn không có quyền tạo mới khách hàng. Hãy liên hệ với administrator";
                }
                TempData["Notice"] = message;
                TempData["ShowPopup"] = true;

            }
            catch (Exception ex)
            {
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Có lỗi xảy ra, vui lòng kiểm tra log";
                Common.WriteLog(ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace);
            }
            return RedirectToAction("Profiles");
        }
        /// <summary>
        /// Delete customer
        /// </summary>
        /// <param name="isDeletePhysical">isDeletePhysical  = true; Delete physical
        /// isDeletePhysical  = fale; Delete logic</param>
        /// <param name="profile"> customer </param>
        private void DeleteCustomer(bool isDeletePhysical, customer profile)
        {
        // delete physical
            if (isDeletePhysical)
            {
                if (Common.GetAdminType() == 1)
                {
                    var deleteOrderDetails = from details in db.orders
                                             where details.customer_id == profile.customer_id
                                             select details;

                    foreach (var detail in deleteOrderDetails)
                    {
                        db.orders.Remove(detail);
                    }
                    db.customers.Remove(profile);
                    WirteLogDelete(profile);
                }
            }
            else
            {
                profile.status_id = 2; // set status delete
                profile.modifieddate = DateTime.Now;
                WirteLogDelete(profile);
            }
            db.SaveChanges();
        }

        private void WirteLogDelete(customer profile)
        {
            if (GetCurrentAdminId() > 0)
            {
                // write log
                db.LogErrors.Add(new LogError()
                {
                    Content = "Admin: " + GetCurrentAdminName() + " delete customer " + profile.email,
                    Created = DateTime.Now
                });
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [CheckPermissionActionFilter]
        public ActionResult GetCustomerProduct(int ID)
        {
            try
            {
                var obj = db.orders.FirstOrDefault(x => x.order_id == ID);
                if (obj != null) return Json(new { res = "OK", categoryId = obj.catagory_id, productId = obj.product_id, serialNumber = obj.serial.Trim(), timePucharsed = obj.date.HasValue? obj.date.Value.ToString("yyyy/MM/dd") :"", shopId = obj.shop_id, cityBuyId = obj.city_id });
                else return Json(new { res = "ERROR" }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Có lỗi xảy ra, vui lòng xem log";
                Common.WriteLog(ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace);
                return null;
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [CheckPermissionActionFilter]
        public ActionResult UpdateCustomerProduct(int Id, int profileId, int productId, string serialNumber, string timePucharsed, int shopId, int cityId)
        {
            try
            {
                if (Common.GetAdminType() == 1 || Common.GetAdminType() == 2)
                {               
                var obj = db.orders.FirstOrDefault(x => x.order_id == Id);
                if (obj != null)
                {
                    // Calculate warranty date end
                    var dSerial = RegisterCustomerProduct.GetSerial(db, obj.catagory_id.Value, productId, serialNumber);
                    string warrantyEnd = RegisterCustomerProduct.GetWarrantyTime(db, serialNumber, obj.catagory_id.Value, Convert.ToDateTime(timePucharsed), dSerial);
                    
                    obj.product_id = productId;
                    obj.serial = serialNumber;
                    obj.date = Convert.ToDateTime(timePucharsed);
                    obj.shop_id = shopId;
                    obj.city_id = cityId;
                    obj.modifieddate = DateTime.Now;                                        
                    if(!String.IsNullOrEmpty(warrantyEnd)) obj.WarrantyEnd = Convert.ToDateTime(warrantyEnd);
                    db.SaveChanges();
                    TempData["Notice"] = "Cập nhật dữ liệu thành công ";
                    TempData["ShowPopup"] = true;
                }
                }
                else
                {
                    TempData["Notice"] = "Bạn không có quyền tạo mới khách hàng. Hãy liên hệ với administrator";
                    TempData["ShowPopup"] = true;
                }
            }
            catch (Exception ex)
            {
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Có lỗi xảy ra, vui lòng xem log";
                Common.WriteLog(ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace);
            }
            return RedirectToAction("ProfileEditor", new { ID = profileId });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [CheckPermissionActionFilter]
        public ActionResult SaveCustomerProduct(int profileId, int categoryId, int productId, string serialNumber, string timePucharsed, int shopId, int cityId)
        {
            try
            {
                if (Common.GetAdminType() == 1 || Common.GetAdminType() == 2)
                {
                    int numberStatus = RegisterCustomerProduct.RegisterProduct(db, profileId, categoryId, productId, serialNumber, timePucharsed, shopId, cityId);
                    switch (numberStatus)
                    {
                        case Constant.ExistCustProduct:    // serial not correct                            
                            TempData["Notice"] = "Sản phẩm đã được đăng kí. Hãy khiểm tra lại";
                            break;
                        case Constant.NotExistSerial:   // have exception
                            TempData["Notice"] = "Không tồn tại serial tương ứng. Hãy kiểm tra lại.";
                            break;
                        case Constant.Excepction:
                            TempData["Notice"] = "Có lỗi khi đăng kí sản phẩm mới";
                            break;
                        default:
                            TempData["Notice"] = "Thêm sản phẩm thành công";
                            break;
                    }
                    TempData["ShowPopup"] = true;
                }
                else
                {
                    TempData["Notice"] = "Bạn không có quyền tạo mới khách hàng. Hãy liên hệ với administrator";
                    TempData["ShowPopup"] = true;
                }
            }
            catch (Exception ex)
            {
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Có lỗi xảy ra, vui lòng xem log";
                Common.WriteLog(ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace);
            }            
            return RedirectToAction("ProfileEditor", new { ID = profileId });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassByAdmin(int ID)
        {
            try
            {
                var profile = db.customers.FirstOrDefault(x => x.customer_id == ID);
                if (profile != null)
                {
                    string newPass = Common.GenRandomString(8);
                    profile.password = Common.MaHoa(newPass);
                    db.SaveChanges();
                    // send mail
                    IUserMailer mailer = new UserMailer();
                    mailer.ResetPassByAdmin(profile.email, profile.firstname + " " + profile.lastname, newPass).Send();
                    return Json(new { res = "OK", result = "Reset mật khẩu thành công" });
                }
                return Json(new {res="ERROR", result = "Thông tin khách hàng không tồn tại"});
            }
            catch (Exception ex)
            {
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Có lỗi xảy ra, vui lòng xem log";
                
                Common.WriteLog(ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace);
                return Json(new { res = "ERROR", result = "Có lỗi xảy ra, vui lòng xem log" });
            }
            return RedirectToAction("ProfileEditor", new { ID = ID });
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        [CheckPermissionActionFilter]
        public void ExportProfile(string fromDate, string toDate , int category , string modelName , string serial)
        {
            int categoryFrom = category !=-1? category : 0;
            int categoryTo = category !=-1 ? category : 999;
            int productIFrom = 0;
            int productIdTo = 999999999;
            DateTime fromDtFrom = Convert.ToDateTime("1900/01/01");
            DateTime fromDtTo = Convert.ToDateTime("9999/01/01");
            var existObj = db.products.FirstOrDefault(x => x.name == modelName);
            if (existObj != null)
            {
                productIFrom = existObj.product_id;
                productIdTo = existObj.product_id;
            }

            if (!string.IsNullOrEmpty(fromDate))
            {
                fromDtFrom = Convert.ToDateTime(fromDate);
            }


            if (!string.IsNullOrEmpty(toDate))
            {
                fromDtTo = Convert.ToDateTime(toDate);
            }

            
            List<ProfileVM> lstObj = new List<ProfileVM>();

            try
            {
                if (string.IsNullOrEmpty(serial))
                {
                    lstObj = (from profile in db.customers
                        join products in db.orders on profile.customer_id equals products.customer_id into temp
                        from j in temp.DefaultIfEmpty()
                        where profile.datereg >= fromDtFrom && profile.datereg <= fromDtTo
                              && profile.status_id == 1 && j.catagory_id >= categoryFrom && j.catagory_id <= categoryTo
                              && j.product_id >= productIFrom && j.product_id <= productIdTo
                              orderby profile.datereg descending
                        select new ProfileVM
                        {
                            ID = profile.customer_id,
                            Firstname = profile.firstname.ToUpper(),
                            Lastname = profile.lastname.ToUpper(),
                            Birthday = profile.birthday,
                            Sex = profile.sex.Value ? "Nam" : "Nữ",
                            Email = profile.email,
                            Mobile = profile.mobilephone,
                            City = profile.city.name,
                            District = profile.district.name,
                            Address = profile.address,
                            RegDate = profile.datereg,
                            Status = profile.status_id == 1 ? "Kích hoạt" : "Xóa",
                            Shop = j.shop.name,
                            BuyDate = j.date,
                            WarrantyEnd = j.WarrantyEnd,
                            Serial  = j.serial  
                        }).ToList();
                }
                else
                {
                    lstObj = (from profile in db.customers
                              join products in db.orders on profile.customer_id equals products.customer_id into temp
                              from j in temp.DefaultIfEmpty()
                              where profile.datereg >= fromDtFrom && profile.datereg <= fromDtTo
                              && profile.status_id == 1 && j.catagory_id >= categoryFrom && j.catagory_id <= categoryTo
                              && j.product_id >= productIFrom && j.product_id <= productIdTo
                              && j.serial == serial
                              orderby profile.datereg descending
                              select new ProfileVM
                              {
                                  ID = profile.customer_id,
                                  Firstname = profile.firstname.ToUpper(),
                                  Lastname = profile.lastname.ToUpper(),
                                  Birthday = profile.birthday,
                                  Sex = profile.sex.Value ? "Nam" : "Nữ",
                                  Email = profile.email,
                                  Mobile = profile.mobilephone,
                                  City = profile.city.name,
                                  District = profile.district.name,
                                  Address = profile.address,
                                  RegDate = profile.datereg,
                                  Status = profile.status_id == 1 ? "Kích hoạt" : "Xóa",
                                  Shop = j.shop.name,
                                  BuyDate = j.date,
                                  WarrantyEnd = j.WarrantyEnd,
                                  Serial = j.serial 
                              }).ToList();
                }
                int cnnt = lstObj.Count;
                const string fileName = "Profiles.xlsx";
                if (cnnt > 63000)
                {
                    CreateExcelFile.CreateExcelDocumentList(lstObj, fileName, HttpContext.ApplicationInstance.Response);
                }
                else
                {
                    CreateExcelFile.CreateExcelDocument(lstObj, fileName, HttpContext.ApplicationInstance.Response);
                }
                
            }
            catch(Exception ex)
            {
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Có lỗi xảy ra khi export, vui lòng xem log";

                Common.WriteLog(ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace);
            }
                                
        }
        #endregion

        #region CustomerType
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult CustomerType()
        {
            ViewData["status"] = db.status.Select(x => new { status_id = x.status_id, name = x.name }).ToList();
            return View();
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult CustomerType_Read([DataSourceRequest]DataSourceRequest request)
        {
            var objs = db.customertypes.Select(x => new
            {
                customertype_id = x.customertype_id,
                name = x.name,
                status_id = x.status_id
            }).ToList();
            return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult CustomerType_Create([DataSourceRequest] DataSourceRequest request, customertype item)
        {
            //customertype obj = new customertype();
            if (item != null && ModelState.IsValid)
            {
                try
                {
                    //obj.customertype_id = (db.customertypes.Count() > 0)?db.customertypes.Max(x => x.customertype_id) + 1 : 1;
                    //obj.name = item.name;
                    //if (item.status != null) obj.status_id = item.status.status_id;
                    db.customertypes.Add(item);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult CustomerType_Update([DataSourceRequest] DataSourceRequest request, customertype item)
        {
            //var obj = db.customertypes.FirstOrDefault(x => x.customertype_id == item.customertype_id);
            //if (obj == null) return HttpNotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    //obj.name = item.name;
                    //if (item.status != null) obj.status_id = item.status.status_id;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult CustomerType_Destroy([DataSourceRequest] DataSourceRequest request, customertype item)
        {
            var obj = db.customertypes.FirstOrDefault(x => x.customertype_id == item.customertype_id);
            if (obj == null) return HttpNotFound();
            try
            {
                db.customertypes.Remove(obj);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }
        #endregion        

        #region ProductCategory
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ProductCategory()
        {
            return View();
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult ProductCategory_Read([DataSourceRequest]DataSourceRequest request)
        {
            var objs = db.categories.Select(x => new
            {
                category_id = x.category_id,
                name = x.name,
                modelImage = x.modelImage,
                serialImage = x.serialImage,
                viewModelImage = x.viewModelImage,
                viewSerialImage = x.viewSerialImage,
                status_id = x.status_id,
                RS_base_warranty_months = x.RS_base_warranty_months,
            }).ToList();
            return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult ProductCategory_Destroy([DataSourceRequest] DataSourceRequest request, category item)
        {
            category obj = db.categories.Find(item.category_id);
            if (obj == null) return HttpNotFound();
            try
            {
                db.categories.Remove(obj);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
            }
            return Json(new[] { obj }.ToDataSourceResult(request, ModelState));
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ProductCategoryEditor(int Id)
        {
            ViewData["status"] = db.status.Select(x => new { ID = x.status_id, Name = x.name }).ToList();
            if (Id == 0) return View();
            else
            {
                var obj = db.categories.Find(Id);
                if (obj == null) return HttpNotFound();
                return View(obj);
            }
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult SaveProductCategory(category item)
        {
            try
            {
                if (item.category_id == 0)
                {
                    db.categories.Add(item);
                }
                else db.Entry(item).State = EntityState.Modified;
                if (item.modelImage != null)
                {
                    item.modelImage = HttpUtility.HtmlDecode(item.modelImage);
                    if (HttpUtility.HtmlDecode(item.modelImage).Contains("src")) item.modelImage = Regex.Match(HttpUtility.HtmlDecode(item.modelImage), "<img.+?src=[\"'](.+?)[\"'].+?>", RegexOptions.IgnoreCase).Groups[1].Value;
                }
                if (item.serialImage != null)
                {
                    item.serialImage = HttpUtility.HtmlDecode(item.serialImage);
                    if (HttpUtility.HtmlDecode(item.serialImage).Contains("src")) item.serialImage = Regex.Match(HttpUtility.HtmlDecode(item.serialImage), "<img.+?src=[\"'](.+?)[\"'].+?>", RegexOptions.IgnoreCase).Groups[1].Value;
                }
                if (item.viewModelImage != null)
                {
                    item.viewModelImage = HttpUtility.HtmlDecode(item.viewModelImage);
                    if (HttpUtility.HtmlDecode(item.viewModelImage).Contains("src")) item.viewModelImage = Regex.Match(HttpUtility.HtmlDecode(item.viewModelImage), "<img.+?src=[\"'](.+?)[\"'].+?>", RegexOptions.IgnoreCase).Groups[1].Value;
                }
                if (item.viewSerialImage != null)
                {
                    item.viewSerialImage = HttpUtility.HtmlDecode(item.viewSerialImage);
                    if (HttpUtility.HtmlDecode(item.viewSerialImage).Contains("src")) item.viewSerialImage = Regex.Match(HttpUtility.HtmlDecode(item.viewSerialImage), "<img.+?src=[\"'](.+?)[\"'].+?>", RegexOptions.IgnoreCase).Groups[1].Value;
                }
                                
                db.SaveChanges();
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Lưu thông tin thành công";
            }
            catch (Exception ex)
            {
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Có lỗi xảy ra, vui lòng kiểm tra log";
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
            }
            //return RedirectToAction("ProductCategoryEditor", new { Id = item.category_id });
            return RedirectToAction("ProductCategory");
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteProductCategory(int Id)
        {
            try
            {
                var item = db.categories.FirstOrDefault(x=>x.category_id == Id);
                if (item == null) return HttpNotFound();
                item.status_id = 2; // delete
                //db.categories.Remove(item);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
            }
            return RedirectToAction("ProductCategory");
        }
        #endregion

        #region Product
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Product()
        {
            ViewData["categories"] = db.categories.Where(x => x.status_id == 1).Select(x => new { category_id = x.category_id, name = x.name }).ToList();
            ViewData["status"] = db.status.Select(x => new { status_id = x.status_id, name = x.name }).ToList();
            return View();
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Product_Read([DataSourceRequest]DataSourceRequest request)
        {
            var objs = db.products.Select(x => new
            {
                product_id = x.product_id,
                name = x.name,
                productcode = x.productcode,
                category_id = x.category_id,
                status_id = x.status_id
            }).ToList();
            return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Product_Create([DataSourceRequest] DataSourceRequest request, product item)
        {
            if (item != null && ModelState.IsValid)
            {
                try
                {
                    //product obj = new product();
                    //obj.product_id = (db.products.Count() > 0) ? db.products.Max(x => x.product_id) + 1 : 1;
                    //obj.name = item.name;
                    //obj.productcode = item.productcode;
                    //obj.category_id = item.category.category_id;
                    //if (item.status != null) obj.status_id = item.status.status_id;
                    db.products.Add(item);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Product_Update([DataSourceRequest] DataSourceRequest request, product item)
        {
            //var obj = db.products.FirstOrDefault(x => x.product_id == item.product_id);
            //if (obj == null) return HttpNotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    //obj.name = item.name;
                    //obj.productcode = item.productcode;
                    //obj.category_id = item.category.category_id;
                    //if (item.status != null) obj.status_id = item.status.status_id;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            //return RedirectToAction("Product", "HeThong");
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Product_Destroy([DataSourceRequest] DataSourceRequest request, product item)
        {
            var obj = db.products.FirstOrDefault(x => x.product_id == item.product_id);
            if (obj == null) return HttpNotFound();
            try
            {
                db.products.Remove(obj);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public void ExportProduct()
        {
            var lstObj = (from p in db.products
                          join c in db.categories on p.category_id equals c.category_id
                          select new
                          {
                            id = p.product_id,
                            modelname = c.name,
                            productcode = p.productcode,
                            name = p.name,
                            status = p.status.name
                          }).ToList();
            int cnnt = lstObj.Count;
            const string fileName = "Products.xlsx";
            if (cnnt > 63000)
            {
                CreateExcelFile.CreateExcelDocumentList(lstObj, fileName, HttpContext.ApplicationInstance.Response);
            }
            else
            {
                CreateExcelFile.CreateExcelDocument(lstObj, fileName, HttpContext.ApplicationInstance.Response);
            }
                

           // CreateExcelFile.CreateExcelDocument(lstObj, "Products.xlsx", HttpContext.ApplicationInstance.Response);
        }
        #endregion

        #region Serial
        
        public ActionResult Serial()
        {
            ViewData["status"] = db.status.Select(x => new { status_id = x.status_id, name = x.name }).ToList();
            var lst = new SerialVm
            {
                lstCategories = db.categories.Where(x => x.status_id == Constant.StatusActive).ToDictionary(x => x.category_id, x => x.name)
            };
            return View(lst);
        }
        
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Serial_Read([DataSourceRequest]DataSourceRequest request , int categoryId , string productName , string serialNumber)
        {
            List<Int32> lstSerialId;
            if (categoryId == -1 && String.IsNullOrEmpty(productName) && String.IsNullOrEmpty(serialNumber)) lstSerialId = db.serials.Select(x => x.serial_id).ToList();
            else if (categoryId > 0 && String.IsNullOrEmpty(productName) && String.IsNullOrEmpty(serialNumber))
            {
                lstSerialId = (from s in db.serials
                               join p in db.products on s.productcode equals p.productcode
                               where p.category_id == categoryId
                               select s.serial_id).ToList();
            }
            else lstSerialId = db.SearchSerial(serialNumber, null, productName, null, null, null).Select(x => x.serial_id).ToList();

            // Sum total records
            int total = lstSerialId.Count;

            // Apply paging
            if (request.PageSize == 0) request.PageSize = 20;
            if (request.Page > 0) lstSerialId = lstSerialId.OrderBy(x=>x.AsNullable()).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();

            IQueryable<SerialDTO> objs = (from s in db.serials
                                          join o in db.orders on s.serialnumber equals o.serial into temp
                                          from j in temp.DefaultIfEmpty()
                                          where lstSerialId.Contains(s.serial_id)
                                          select new SerialDTO
                                          {
                                              serial_id = s.serial_id,
                                              serialnumber = s.serialnumber,
                                              modelname = s.modelname,
                                              productcode = s.productcode,
                                              manufacturingdate = s.manufacturingdate,
                                              batterynumber = s.batterynumber,
                                              adapternumber = s.adapternumber,
                                              alphalenumber = s.alphalenumber,
                                              period = s.period,
                                              expireddate = s.expireddate,
                                              dateimport = s.dateimport,
                                              status_id = s.status_id,
                                              fullname = j.customer != null ? j.customer.firstname + " " + j.customer.lastname : String.Empty,
                                          });

            // Filtering
            List<FilterDescriptor> lstFilter = GetFilters(request.Filters);
            if (lstFilter.Any())
            {
                foreach (FilterDescriptor filter in lstFilter)
                {
                    switch (filter.Member)
                    {
                        case "modelname":
                            objs = objs.Where(x => x.modelname.Contains(filter.ConvertedValue.ToString()));
                            break;
                        case "serialnumber":
                            objs = objs.Where(x => x.serialnumber.Contains(filter.ConvertedValue.ToString()));
                            break;
                        case "productcode":
                            objs = objs.Where(x => x.serialnumber.Contains(filter.ConvertedValue.ToString()));
                            break;
                        case "manufacturingdate":
                            DateTime manufacturingdate = Convert.ToDateTime(filter.ConvertedValue);
                            switch (filter.Operator)
                            {
                                case FilterOperator.IsEqualTo:
                                    objs = objs.Where(x => DbFunctions.TruncateTime(x.manufacturingdate) == manufacturingdate);
                                    break;
                                case FilterOperator.IsGreaterThan:
                                    objs = objs.Where(x => DbFunctions.TruncateTime(x.manufacturingdate) > manufacturingdate);
                                    break;
                                case FilterOperator.IsGreaterThanOrEqualTo:
                                    objs = objs.Where(x => DbFunctions.TruncateTime(x.manufacturingdate) >= manufacturingdate);
                                    break;
                                case FilterOperator.IsLessThan:
                                    objs = objs.Where(x => DbFunctions.TruncateTime(x.manufacturingdate) < manufacturingdate);
                                    break;
                                case FilterOperator.IsLessThanOrEqualTo:
                                    objs = objs.Where(x => DbFunctions.TruncateTime(x.manufacturingdate) <= manufacturingdate);
                                    break;
                            }
                            
                            break;
                        case "batterynumber":
                            objs = objs.Where(x => x.batterynumber.Contains(filter.ConvertedValue.ToString()));
                            break;
                        case "adapternumber":
                            objs = objs.Where(x => x.adapternumber.Contains(filter.ConvertedValue.ToString()));
                            break;
                        case "alphalenumber":
                            objs = objs.Where(x => x.alphalenumber.Contains(filter.ConvertedValue.ToString()));
                            break;
                        case "period":
                            objs = objs.Where(x => x.period == (decimal)filter.ConvertedValue);
                            break;
                        case "fullname":
                            objs = objs.Where(x => x.fullname.Contains(filter.ConvertedValue.ToString()));
                            break;
                        case "expireddate":
                            DateTime expireddate = Convert.ToDateTime(filter.ConvertedValue);
                            switch (filter.Operator)
                            {
                                case FilterOperator.IsEqualTo:
                                    objs = objs.Where(x => DbFunctions.TruncateTime(x.expireddate) == expireddate);
                                    break;
                                case FilterOperator.IsGreaterThan:
                                    objs = objs.Where(x => DbFunctions.TruncateTime(x.expireddate) > expireddate);
                                    break;
                                case FilterOperator.IsGreaterThanOrEqualTo:
                                    objs = objs.Where(x => DbFunctions.TruncateTime(x.expireddate) >= expireddate);
                                    break;
                                case FilterOperator.IsLessThan:
                                    objs = objs.Where(x => DbFunctions.TruncateTime(x.expireddate) < expireddate);
                                    break;
                                case FilterOperator.IsLessThanOrEqualTo:
                                    objs = objs.Where(x => DbFunctions.TruncateTime(x.expireddate) <= expireddate);
                                    break;
                            }
                            break;
                        case "dateimport":
                            DateTime dateimport = Convert.ToDateTime(filter.ConvertedValue).Date;
                            switch (filter.Operator)
                            {
                                case FilterOperator.IsEqualTo:
                                    objs = objs.Where(x => DbFunctions.TruncateTime(x.dateimport) == dateimport);
                                    break;
                                case FilterOperator.IsGreaterThan:
                                    objs = objs.Where(x => DbFunctions.TruncateTime(x.dateimport) > dateimport);
                                    break;
                                case FilterOperator.IsGreaterThanOrEqualTo:
                                    objs = objs.Where(x => DbFunctions.TruncateTime(x.dateimport) >= dateimport);
                                    break;
                                case FilterOperator.IsLessThan:
                                    objs = objs.Where(x => DbFunctions.TruncateTime(x.dateimport) < dateimport);
                                    break;
                                case FilterOperator.IsLessThanOrEqualTo:
                                    objs = objs.Where(x => DbFunctions.TruncateTime(x.dateimport) <= dateimport);
                                    break;
                            }
                            break;
                    }
                }
            }

            // sorting
            if (request.Sorts.Any())
            {
                foreach (SortDescriptor sortDescriptor in request.Sorts)
                {
                    if (sortDescriptor.SortDirection == ListSortDirection.Ascending)
                    {
                        switch (sortDescriptor.Member)
                        {
                            case "modelname":
                                objs = objs.OrderBy(order => order.modelname);
                                break;
                            case "manufacturingdate":
                                objs = objs.OrderBy(order => order.manufacturingdate);
                                break;
                            case "expireddate":
                                objs = objs.OrderBy(order => order.expireddate);
                                break;
                            case "dateimport":
                                objs = objs.OrderBy(order => order.dateimport);
                                break;
                            case "fullname":
                                objs = objs.OrderBy(order => order.fullname);
                                break;
                        }
                    }
                    else
                    {
                        switch (sortDescriptor.Member)
                        {
                            case "modelname":
                                objs = objs.OrderByDescending(order => order.modelname);
                                break;
                            case "manufacturingdate":
                                objs = objs.OrderByDescending(order => order.manufacturingdate);
                                break;
                            case "expireddate":
                                objs = objs.OrderByDescending(order => order.expireddate);
                                break;
                            case "dateimport":
                                objs = objs.OrderByDescending(order => order.dateimport);
                                break;
                            case "fullname":
                                objs = objs.OrderByDescending(order => order.fullname);
                                break;
                        }
                    }
                }
            }
            else objs = objs.OrderBy(x => x.modelname); // EF can't page unsorted data

            // Initialize  the DataSourceResult
            var result = new DataSourceResult()
            {
                Data = objs.ToList(), // Process data (paging and sorting applied)
                Total = total // Total number of records
            };
            return Json(result);
        }
               
        [HttpPost]
        public ActionResult GetProductsOfCategory(int categoryId)
        {
            var threadList = db.products.Where(s => s.category_id == categoryId && s.status_id == Constant.StatusActive).Select(s => s.name).Distinct();
            return Json(threadList, JsonRequestBehavior.AllowGet);
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public void ExportSerial()
        {
            var lstObj = db.serials.Where(x => x.status_id == 1).Select(x => new
            {
                id = x.serial_id,
                modelname = x.modelname,
                serialnumber = x.serialnumber,
                productcode = x.productcode,
                warrantycardnumber = x.warrantycardnumber,
                partcode = x.partcode,
                manufacturingdate = x.manufacturingdate,
                batterynumber = x.batterynumber,
                adapternumber = x.adapternumber,
                alphalenumber = x.alphalenumber,
                period = x.period,
                expireddate = x.expireddate,
                RM = x.RM,
                RM1 = x.RM1,
                RM2 = x.RM2,
                dateimport = x.dateimport
            }).ToList();

            int cnnt = lstObj.Count;
            const string fileName = "Serial.xlsx";
            if (cnnt > 63000)
            {
                CreateExcelFile.CreateExcelDocumentList(lstObj, fileName, HttpContext.ApplicationInstance.Response);
            }
            else
            {
                CreateExcelFile.CreateExcelDocument(lstObj, fileName, HttpContext.ApplicationInstance.Response);
            }

            //CreateExcelFile.CreateExcelDocument(lstObj, "Serial.xlsx", HttpContext.ApplicationInstance.Response);
        }
        #endregion

        #region Category
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Category()
        {
            return View();
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Category_Read([DataSourceRequest]DataSourceRequest request)
        {
            var objs = db.RS_Category.Select(x => new
            {
                Name = x.Name,
                IsActive = x.IsActive,
                Created = x.Created ,
                ID       = x.ID ,
                Tags = x.Tags
            }).ToList();
            return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Category_Create([DataSourceRequest] DataSourceRequest request, RS_Category item)
        {
            if (item != null && ModelState.IsValid)
            {
                try
                {
                    if (GetCurrentAdminId() > 0) item.CreatedBy = item.ModifiedBy = GetCurrentAdminId();
                    item.ID = (db.RS_Category.Count() > 0) ? db.RS_Category.Max(x => x.ID) + 1 : 1;
                    item.Created = item.Modified = DateTime.Now;                    
                    db.RS_Category.Add(item);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Category_Update([DataSourceRequest] DataSourceRequest request, RS_Category item)
        {
            var obj = db.RS_Category.FirstOrDefault(x => x.ID == item.ID);
            if (obj == null) return HttpNotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    obj.Name = item.Name;
                    obj.IsActive = item.IsActive;
                    obj.Tags = item.Tags;
                    obj.Modified = DateTime.Now;
                    db.Entry(obj).State = EntityState.Modified;
                    if (GetCurrentAdminId() > 0) obj.ModifiedBy = GetCurrentAdminId();                                        
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Category_Destroy([DataSourceRequest] DataSourceRequest request, RS_Category item)
        {
            var obj = db.RS_Category.FirstOrDefault(x => x.ID == item.ID);
            if (obj == null) return HttpNotFound();
            try
            {
                db.RS_Category.Remove(obj);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetCategory()
        {
            return Json(db.RS_Category.ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Article
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Article()
        {
            ViewData["categories"] = db.RS_Category.Select(x => new { ID = x.ID, Name = x.Name }).ToList();
            return View();
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Article_Read([DataSourceRequest]DataSourceRequest request, int CategoryId = 0)
        {
            if (CategoryId > 0)
            {
                var objs = db.RS_Article.Where(x => x.CategoryID == CategoryId).OrderByDescending(x => x.IsTop).ThenByDescending(x=>x.Modified).Select(x => new
                {
                    ID = x.ID,
                    Brief = x.Brief,
                    CategoryID = x.CategoryID,
                    Created = x.Created,
                    IsActive = x.IsActive,
                    IsTop = x.IsTop,
                    Slug = x.Slug,
                    Title = x.Title,
                    OrderNo = x.OrderNo,
                    PublishDate = x.PublishDate,
                }).ToList();
                return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
            else
            {
                var objs = db.RS_Article.OrderByDescending(x => x.IsTop).ThenByDescending(x => x.Created).Select(x => new
                {
                    ID = x.ID,
                    Brief = x.Brief,
                    CategoryID = x.CategoryID,
                    Created = x.Created,
                    IsActive = x.IsActive,
                    IsTop = x.IsTop,
                    Slug = x.Slug,
                    Title = x.Title
                }).ToList();
                return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Article_Destroy([DataSourceRequest] DataSourceRequest request, RS_Article item)
        {
            var obj = db.RS_Article.FirstOrDefault(x => x.ID == item.ID);
            if (obj == null) return HttpNotFound();
            try
            {
                db.RS_Article.Remove(obj);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ArticleEditor(int ID = 0)
        {
            try
            {
                ViewData["categories"] = db.RS_Category.Select(x => new { ID = x.ID, Name = x.Name }).ToList();
                if (ID == 0) return View();
                else
                {
                    var obj = db.RS_Article.Find(ID);
                    if (obj == null) return HttpNotFound();
                    return View(obj);
                }
            }
            catch (Exception ex)
            {
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Có lỗi xảy ra, vui lòng kiểm tra log";
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                return null;
            }
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult SaveArticle(RS_Article article)
        {
            try
            {
                if (article.ID == 0)
                {
                    if (GetCurrentAdminId() > 0) article.CreatedBy = GetCurrentAdminId();
                    article.Created = DateTime.Now;
                    db.RS_Article.Add(article);
                }
                else
                {
                    db.Entry(article).State = EntityState.Modified;
                    article.ArticleContent = HttpUtility.HtmlDecode(article.ArticleContent);
                    article.Modified = DateTime.Now;
                    if (GetCurrentAdminId() > 0)
                    {
                        article.ModifiedBy = GetCurrentAdminId();
                    }                    
                }
                db.SaveChanges();
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Lưu thông tin thành công";

            }
            catch (Exception ex)
            {
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Có lỗi xảy ra, vui lòng kiểm tra log";
                Common.WriteLog(ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace);
            }
            //return RedirectToAction("ArticleEditor", new { ID = article.ID, cat = article.CategoryID });
            return RedirectToAction("Article");
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteArticle(int Id)
        {
            try
            {
                var article = db.RS_Article.Find(Id);
                if (article == null) return HttpNotFound();
                db.RS_Article.Remove(article);
                if (GetCurrentAdminId() > 0)
                {
                    // write log
                    db.LogErrors.Add(new LogError()
                    {
                        Content = "Admin: " + GetCurrentAdminName() + " remove article " + article.Title,
                        Created = DateTime.Now
                    });
                }
                db.SaveChanges();
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Xóa thành công";
            }
            catch (Exception ex)
            {
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Có lỗi xảy ra, vui lòng kiểm tra log";
                Common.WriteLog(ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace);
            }
            return RedirectToAction("Article");
        }
        #endregion
            
        #region Video
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Video()
        {
            List<String> status = new List<string>();
            status.Add("Đang diễn ra");
            status.Add("Sắp diễn ra");
            ViewData["status"] = status;// db.status.Select(x => new { status_id = x.status_id, name = x.name }).ToList();
            return View();
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Video_Read([DataSourceRequest]DataSourceRequest request)
        {
            var objs = db.RS_Video.Select(x => new
            {
                ID = x.ID,
                Title = x.Title,
                Brief = x.Brief,
                Thumb = x.Thumb,
                Source = x.Source,
                IsActive = x.IsActive,
                Status = x.Status,
                IsHighlight = x.IsHighlight,
                PublishDate = x.PublishDate,
            }).ToList();
            return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Video_Create([DataSourceRequest] DataSourceRequest request, RS_Video item)
        {
            if (item != null && ModelState.IsValid)
            {
                try
                {
                    if (GetCurrentAdminId() > 0) item.CreatedBy = item.ModifiedBy = GetCurrentAdminId();
                    item.ID = (db.RS_Video.Count() > 0) ? db.RS_Video.Max(x => x.ID) + 1 : 1;
                    item.Created = item.Modified = DateTime.Now;
                    item.Status = 1; // 1 = Đang diễn ra, 2 = Sắp diễn ra                    
                    db.RS_Video.Add(item);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Video_Update([DataSourceRequest] DataSourceRequest request, RS_Video item)
        {
            var obj = db.RS_Video.FirstOrDefault(x => x.ID == item.ID);
            if (obj == null) return HttpNotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    obj.Title = item.Title;
                    obj.Brief = item.Brief;
                    obj.Source = item.Source;
                    obj.IsActive = item.IsActive;
                    obj.Thumb = item.Thumb;
                    obj.Status = item.Status;
                    obj.IsHighlight = item.IsHighlight;
                    obj.PublishDate = item.PublishDate;
                    if (GetCurrentAdminId() > 0) obj.ModifiedBy = GetCurrentAdminId();
                    obj.Modified = DateTime.Now;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Video_Destroy([DataSourceRequest] DataSourceRequest request, RS_Video item)
        {
            var obj = db.RS_Video.FirstOrDefault(x => x.ID == item.ID);
            if (obj == null) return HttpNotFound();
            try
            {
                db.RS_Video.Remove(obj);
                if (GetCurrentAdminId() > 0)
                {
                    // write log
                    db.LogErrors.Add(new LogError()
                    {
                        Content = "Admin: " + GetCurrentAdminName() + " remove video " + obj.Title,
                        Created = DateTime.Now
                    });
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }


        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult RSVideoEditor(int id)
        {
            try
            {
                RS_Video obj = null;
               
                // add new video
                if (id != 0)
                {                    
                     obj = db.RS_Video.FirstOrDefault(x =>x.ID == id);
                    if (obj == null) return HttpNotFound();
                    return View(obj);
                }
                return View(obj);

            }
            catch (Exception ex)
            {
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Không thể tải dữ liệu, vui lòng xem log";
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                return null;
            }
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult SaveRSVideo(RS_Video video)
        {
            try
            {
                if (video.Title == null)
                {
                    video.Title = string.Empty;
                }

                if (string.IsNullOrEmpty(video.PublishDate.Trim()))
                {
                    video.PublishDate = DateTime.Now.ToString("dd/MM/yyyy");
                }

                if (video.Thumb == null)
                {
                    video.Thumb = string.Empty;
                }
                else
                {
                    if (HttpUtility.HtmlDecode(video.Thumb).Contains("src"))
                    {
                        video.Thumb = Regex.Match(HttpUtility.HtmlDecode(video.Thumb), "<img.+?src=[\"'](.+?)[\"'].+?>", RegexOptions.IgnoreCase).Groups[1].Value;
                    }
                }

                if (video.ID == 0)
                {
                    if (GetCurrentAdminId() > 0)
                    {
                        video.CreatedBy = GetCurrentAdminId();
                        video.ModifiedBy = GetCurrentAdminId();
                    }
                    video.Thumb = HttpUtility.HtmlDecode(video.Thumb);
                    video.Created = DateTime.Now;
                    video.Modified = DateTime.Now;
                    db.RS_Video.Add(video);
                    db.SaveChanges();
                }
                else
                {
                    db.Entry(video).State = EntityState.Modified;
                    if (string.IsNullOrEmpty(video.Created.ToString()))
                    {
                        video.Created = DateTime.Now;
                    }

                    video.Modified = DateTime.Now;
                    if (GetCurrentAdminId() > 0)
                    {
                        video.ModifiedBy = GetCurrentAdminId();
                    }
                    db.SaveChanges();
                }
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Lưu thông tin thành công";

            }
            catch (Exception ex)
            {
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Có lỗi xảy ra, vui lòng xem log";
                Common.WriteLog(ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace);
            }
            return RedirectToAction("Video");
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRSVideo(int Id)
        {
            try
            {
                var video = db.RS_Video.Find(Id);
                if (video == null) return HttpNotFound();
                db.RS_Video.Remove(video);
                db.SaveChanges();
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Xóa thành công";
            }
            catch (Exception ex)
            {
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Có lỗi xảy ra, vui lòng xem log";
                Common.WriteLog(ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace);
            }
            return RedirectToAction("Video");
        }
       

        #endregion

        #region FAQ
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult FAQ()
        {
            return View();
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult FAQ_Read([DataSourceRequest]DataSourceRequest request)
        {
            var objs = db.RS_FAQ.Select(x => new
            {
                OrderNo = x.OrderNo,
                Question = x.Question,
                Answer = x.Answer
            }).ToList();
            return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult FAQ_Create([DataSourceRequest] DataSourceRequest request, RS_FAQ item)
        {
            if (item != null && ModelState.IsValid)
            {
                try
                {
                    if (GetCurrentAdminId() > 0) item.CreatedBy = item.ModifiedBy = GetCurrentAdminId();
                    item.ID = (db.RS_FAQ.Count() > 0) ? db.RS_FAQ.Max(x => x.ID) + 1 : 1;
                    item.Created = item.Modified = DateTime.Now;
                    db.RS_FAQ.Add(item);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult FAQ_Update([DataSourceRequest] DataSourceRequest request, RS_FAQ item)
        {
            //var obj = db.RS_FAQ.FirstOrDefault(x => x.ID == item.ID);
            //if (obj == null) return HttpNotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    //obj.Question = item.Question;
                    //obj.Answer = item.Answer;
                    //obj.OrderNo = item.OrderNo;
                    db.Entry(item).State = EntityState.Modified;
                    if (GetCurrentAdminId() > 0) item.ModifiedBy = GetCurrentAdminId();
                    item.Modified = DateTime.Now;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult FAQ_Destroy([DataSourceRequest] DataSourceRequest request, RS_FAQ item)
        {
            var obj = db.RS_FAQ.FirstOrDefault(x => x.ID == item.ID);
            if (obj == null) return HttpNotFound();
            try
            {
                db.RS_FAQ.Remove(obj);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }
        #endregion

        #region RSConfig

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult RSConfig()
        {
            RS_Config obj;
            obj = db.RS_Config.FirstOrDefault();
            if (obj == null)
            {
                db.RS_Config.Add(new RS_Config());
                db.SaveChanges();
            }
            return View(obj);
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult SaveBannerHome(HttpPostedFileBase Banner)
        {
            if (Banner.ContentLength > 0 && validImageTypes.Contains(Banner.ContentType.ToLower()) && valideImageExt.Contains(Path.GetExtension(Banner.FileName.ToLower())))
            {
                var fileName = Path.GetFileName(Banner.FileName);
                var destinationPath = Path.Combine(Server.MapPath("~/Galleries"), fileName);
                Banner.SaveAs(destinationPath);
                var obj = db.RS_Config.FirstOrDefault();
                obj.BannerHome = destinationPath;
                db.SaveChanges();
            }
            return Content(""); // Return an empty string to signify success
        }

        //public ActionResult RemoveBannerHome(string Banner)
        //{
        //    var fileName = Path.GetFileName(Banner);
        //    var physicalPath = Path.Combine(Server.MapPath("~/Galleries"), fileName);
        //    if (System.IO.File.Exists(physicalPath)) System.IO.File.Delete(physicalPath);
        //    return Content(""); // Return an empty string to signify success
        //}

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult SaveBackgroundLanding(HttpPostedFileBase Background)
        {
            if (Background.ContentLength > 0 && validImageTypes.Contains(Background.ContentType.ToLower()) && valideImageExt.Contains(Path.GetExtension(Background.FileName.ToLower())))
            {
                var fileName = Path.GetFileName(Background.FileName);
                var destinationPath = Path.Combine(Server.MapPath("~/Galleries"), fileName);
                Background.SaveAs(destinationPath);
                var obj = db.RS_Config.FirstOrDefault();
                obj.BackgroundLanding = destinationPath;
                db.SaveChanges();
            }
            return Content(""); // Return an empty string to signify success
        }

        //public ActionResult RemoveBackgroundLanding(string Background)
        //{
        //    var fileName = Path.GetFileName(Background);
        //    var physicalPath = Path.Combine(Server.MapPath("~/Galleries"), fileName);
        //    if (System.IO.File.Exists(physicalPath)) System.IO.File.Delete(physicalPath);
        //    return Content(""); // Return an empty string to signify success
        //}

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult SaveRSConfig(RS_Config obj)
        {
            try
            {
                var exist = db.RS_Config.FirstOrDefault();
                if (exist == null) return HttpNotFound();
                exist.LandingContent = obj.LandingContent;
                db.SaveChanges();
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Lưu thành công";

            }
            catch (Exception ex)
            {
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Có lỗi xảy ra, vui lòng xem log";
                Common.WriteLog(ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace);
            }
            return RedirectToAction("RSConfig");
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRSConfig(int Id)
        {
            try
            {
                var obj = db.RS_Config.Find(Id);
                if (obj == null) return HttpNotFound();
                db.RS_Config.Remove(obj);
                db.SaveChanges();
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Xóa thành công";
            }
            catch (Exception ex)
            {
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Có lỗi xảy ra, vui lòng xem log";
                Common.WriteLog(ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace);
            }
            return RedirectToAction("RSConfig");
        }
        #endregion

        #region RSImages
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult RSImages()
        {
            ViewData["categories"] = db.RS_Category.Select(x => new { ID = x.ID, Name = x.Name }).ToList();
            return View();
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult RSImages_Read([DataSourceRequest]DataSourceRequest request, int CategoryId = 0)
        {
            TempData["ShowPopup"] = false;
            var obj = db.RS_Images.Where(x => x.CategoryID == CategoryId).Select(x => new
            {
                ID = x.ID,
                Title = x.Title,
                Descript = x.Descript,
                Created = x.Created,
                Url = x.Url,
                IsActive = x.IsActive,
                LinkImage = x.LinkImage,
                CategoryID = x.CategoryID
            }).ToList();

            return Json(obj.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult RSImages_Destroy([DataSourceRequest] DataSourceRequest request, RS_Images item)
        {
            var obj = db.RS_Images.FirstOrDefault(x => x.ID == item.ID);
            if (obj == null) return HttpNotFound();
            try
            {
                db.RS_Images.Remove(obj);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult RSImagesEditor(int id, int cat)
        {
            try
            {
                ViewData["categories"] = db.RS_Category.Select(x => new { ID = x.ID, Name = x.Name }).ToList();
                // add new image
                if (id == 0)
                {
                    var objs = new RS_Images();
                    objs.CategoryID = cat;
                    return View(objs);
                }
                else
                {
                    // update image
                    var obj = db.RS_Images.FirstOrDefault(x => x.CategoryID == cat && x.ID == id);
                    if (obj == null) return HttpNotFound();
                    return View(obj);
                }
                 
            }
            catch (Exception ex)
            {
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Không thể tải dữ liệu, vui lòng xem log";
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                return null;
            }
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult SaveRSImages(RS_Images image)
        {
            try
            {
                
                if (image.Url == null)
                {
                    image.Url = string.Empty;
                }
                if (image.LinkImage == null)
                {
                    image.LinkImage = string.Empty;
                }
                else
                {
                    if (HttpUtility.HtmlDecode(image.LinkImage).Contains("src"))
                    {
                        image.LinkImage = Regex.Match(HttpUtility.HtmlDecode(image.LinkImage), "<img.+?src=[\"'](.+?)[\"'].+?>", RegexOptions.IgnoreCase).Groups[1].Value;
                    }
                }

                if (image.ID == 0)
                {
                    if (GetCurrentAdminId() > 0)
                    {
                        image.CreatedBy = GetCurrentAdminId();
                        image.ModifiedBy = GetCurrentAdminId();
                    }
                    image.LinkImage = HttpUtility.HtmlDecode(image.LinkImage);                    
                    image.Created = DateTime.Now;
                    image.Modified = DateTime.Now;
                    db.RS_Images.Add(image);
                    db.SaveChanges();
                }
                else
                {
                    db.Entry(image).State = EntityState.Modified;
                    if (string.IsNullOrEmpty(image.Created.ToString()))
                    {
                        image.Created = DateTime.Now;
                    }
                    image.Modified = DateTime.Now;
                    if (GetCurrentAdminId() > 0)
                    {
                        image.ModifiedBy = GetCurrentAdminId();
                    }
                    db.SaveChanges();
                }
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Lưu thông tin thành công";

            }
            catch (Exception ex)
            {
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Có lỗi xảy ra, vui lòng xem log";
                Common.WriteLog(ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace);
            }
            return RedirectToAction("RSImages");
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRSImages(int Id)
        {
            try
            {
                var image = db.RS_Images.Find(Id);
                if (image == null) return HttpNotFound();
                db.RS_Images.Remove(image);
                db.SaveChanges();
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Xóa thành công";
            }
            catch (Exception ex)
            {
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Có lỗi xảy ra, vui lòng xem log";
                Common.WriteLog(ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace);
            }
            return RedirectToAction("RSImages");
        }
        #endregion

        #region Menu
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Menu()
        {
            return View();
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Menu_Read([DataSourceRequest]DataSourceRequest request)
        {
            var objs = db.RS_Menu.Select(x => new
            {
                ID = x.ID,
                Name = x.Name,
                CssClass = x.CssClass,
                Url = x.Url
            }).ToList();
            return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Menu_Create([DataSourceRequest] DataSourceRequest request, RS_Menu item)
        {
            if (item != null && ModelState.IsValid)
            {
                try
                {
                    if (GetCurrentAdminId() > 0) item.CreatedBy = item.ModifiedBy = GetCurrentAdminId();
                    //item.ID = (db.RS_Menu.Count() > 0) ? db.RS_Menu.Max(x => x.ID) + 1 : 1;
                    item.Created = item.Modified = DateTime.Now;
                    db.RS_Menu.Add(item);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Menu_Update([DataSourceRequest] DataSourceRequest request, RS_Menu item)
        {
            //var obj = db.RS_Menu.FirstOrDefault(x => x.ID == item.ID);
            //if (obj == null) return HttpNotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    //obj.Name = item.Name;
                    //obj.CssClass = item.CssClass;
                    //obj.Url = item.Url;
                    db.Entry(item).State = EntityState.Modified;
                    if (GetCurrentAdminId() > 0) item.ModifiedBy = GetCurrentAdminId();
                    item.Modified = DateTime.Now;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Menu_Destroy([DataSourceRequest] DataSourceRequest request, RS_Menu item)
        {
            var obj = db.RS_Menu.FirstOrDefault(x => x.ID == item.ID);
            if (obj == null) return HttpNotFound();
            try
            {
                db.RS_Menu.Remove(obj);            
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }
        #endregion

        #region Admin
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Admin()
        {
            ViewData["status"] = db.status.Select(x => new { status_id = x.status_id, name = x.name }).ToList();
            return View();
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Admin_Read([DataSourceRequest]DataSourceRequest request)
        {
            var objs = db.admins.Select(x => new
            {
                admin_id = x.admin_id,
                admin_type = x.admin_type,
                email = x.email,
                password = x.password,
                status_id = x.status_id
            }).ToList();
            return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        /* for grid
        
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Admin_Create([DataSourceRequest] DataSourceRequest request, admin item)
        {
            CustomValidateModel(item);
            if (item != null && ModelState.IsValid)
            {
                try
                {
                    item.admin_id = (db.admins.Count() > 0) ? db.admins.Max(x => x.admin_id) + 1 : 1;
                    item.password = Common.MaHoa(item.password.Trim());
                    item.status_id = item.status_id;
                    db.admins.Add(item);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Admin_Update([DataSourceRequest] DataSourceRequest request, admin item)
        {
            CustomValidateModel(item);
            if (item == null) return HttpNotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    var obj = db.admins.FirstOrDefault(x => x.admin_id == item.admin_id);
                    obj.email = item.email.Trim();
                    obj.status_id = item.status_id;
                    if (obj.password.Trim() != item.password.Trim() && !String.IsNullOrEmpty(item.password)) obj.password = Common.MaHoa(item.password.Trim());
                    // write log
                    if (GetCurrentAdminId() > 0)
                    {
                        db.LogErrors.Add(new LogError()
                        {
                            Content = "Admin: " + GetCurrentAdminName() + " change information of account " + item.email,
                            Created = DateTime.Now
                        });
                    }
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Admin_Destroy([DataSourceRequest] DataSourceRequest request, admin item)
        {
            try
            {
                db.admins.Attach(item);
                item.status_id = 2; // set delete
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        // validation for admin
        private void CustomValidateModel(admin model)
        {
            var existingEntity = db.admins.FirstOrDefault(x => x.email == model.email);
            if (existingEntity != null) ModelState.AddModelError("email", "Email này đã được đăng ký.");
            if (model.status_id == null) ModelState.AddModelError("status_id", "Chưa chọn trạng thái");
            if (!Common.ValidatePass(model.password)) ModelState.AddModelError("password", "Mật khẩu phải ít nhất 8 ký tự, chứa chữ hoa, chữ thường, số và ký tự đặc biệt.");
        }
        */

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult AdminEditor(int ID = 0)
        {
            try
            {
                ViewData["status"] = db.status.Select(x => new { ID = x.status_id, Name = x.name }).ToList();
                List<SelectListItem> lstType = new List<SelectListItem>();
                lstType.Add(new SelectListItem { Value = "1", Text = "Head account" });
                lstType.Add(new SelectListItem { Value = "2", Text = "CIC account" });
                lstType.Add(new SelectListItem { Value = "3", Text = "ASC account" });
                ViewData["type"] = lstType;

                if (ID == 0) return View();
                else
                {
                    var obj = db.admins.FirstOrDefault(x => x.admin_id == ID);
                    if (obj == null) return HttpNotFound();
                    return View(obj);
                }
            }
            catch (Exception ex)
            {
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Có lỗi xảy ra, vui lòng kiểm tra log";
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                return null;
            }
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]        
        [ValidateAntiForgeryToken]
        public ActionResult SaveAdmin(admin admin)
        {
            try
            {
                // add a new item
                if (admin.admin_id == 0)
                {
                    if (!String.IsNullOrEmpty(admin.password))
                    {
                        if (db.admins.FirstOrDefault(x => x.email == admin.email) == null)
                        {
                            admin.password = Common.MaHoa(admin.password.Trim());
                            admin.admincodereg = new Guid().ToString();  
                            db.admins.Add(admin);
                            db.SaveChanges();
                        }
                        else
                        {
                            TempData["Notice"] = "Tài khoản này đã được đăng ký rồi";
                            TempData["ShowPopup"] = true;
                        }
                    }
                    else
                    {
                        TempData["Notice"] = "Vui lòng nhập mật khẩu";
                        TempData["ShowPopup"] = true;
                    }
                }
                else // modify item
                {
                    var existObj = db.admins.FirstOrDefault(x => x.admin_id == admin.admin_id);
                    if (existObj != null)
                    {
                        if (!String.IsNullOrEmpty(admin.password))
                        {
                            string pass = Common.MaHoa(admin.password.Trim());
                            if (existObj.password.Trim() != pass) existObj.password = pass;
                        }
                        //existObj.email = admin.email;
                        existObj.admin_type = admin.admin_type;
                        existObj.status_id = admin.status_id;
                        db.SaveChanges();
                    }
                }                
            }
            catch (Exception ex)
            {
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Có lỗi xảy ra, vui lòng kiểm tra log";
                Common.WriteLog(ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace);
            }
            return RedirectToAction("Admin");
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]        
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAdmin(int Id)
        {
            try
            {
                var admin = db.admins.FirstOrDefault(x => x.admin_id == Id);
                if (admin == null) return HttpNotFound();
                admin.status_id = 2; // set status delete
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Có lỗi xảy ra, vui lòng kiểm tra log";
                Common.WriteLog(ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace);
            }
            return RedirectToAction("Admin");
        }
        #endregion

        #region Log
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ViewLog()
        {
            return View();
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Log_Read([DataSourceRequest]DataSourceRequest request)
        {
            var objs = db.LogErrors.ToList();
            return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post), ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Log_Destroy([DataSourceRequest] DataSourceRequest request, LogError item)
        {
            var obj = db.LogErrors.Find(item.ID);
            if (obj == null) return HttpNotFound();
            try
            {
                db.LogErrors.Remove(obj);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
            }
            return Json(new[] { obj }.ToDataSourceResult(request, ModelState));
        }
        #endregion

        #region City
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult City()
        {
            ViewData["status"] = db.status.Select(x => new { status_id = x.status_id, name = x.name }).ToList();
            return View();
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult City_Read([DataSourceRequest]DataSourceRequest request)
        {
            var objs = db.cities.Select(x => new {
                city_id = x.city_id,
                name = x.name,
                porder = x.porder,
                status_id = x.status_id
            }).OrderByDescending(x=>x.porder).ThenBy(x => x.name).ToList();
            return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult City_Create([DataSourceRequest] DataSourceRequest request, city item)
        {
            //city obj = new city();
            if (item != null && ModelState.IsValid)
            {
                try
                {
                    //obj.city_id = (db.cities.Count() > 0)? db.cities.Max(x => x.city_id) + 1 : 1 ;//item.city_id;
                    //obj.name = item.name;
                    //obj.porder = item.porder;
                    //if (item.status != null) obj.status_id = item.status.status_id;
                    db.cities.Add(item);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }


        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult City_Update([DataSourceRequest] DataSourceRequest request, city item)
        {
            //var obj = db.cities.FirstOrDefault(x => x.city_id == item.city_id);
            //if (obj == null) return HttpNotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    //obj.name = item.name;
                    //obj.porder = item.porder;
                    //if (item.status != null) obj.status_id = item.status.status_id;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            //return RedirectToAction("City", "HeThong");
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }
                
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult City_Destroy([DataSourceRequest] DataSourceRequest request, city item)
        {
            var obj = db.cities.FirstOrDefault(x => x.city_id == item.city_id);
            if (obj == null) return HttpNotFound();
            try
            {
                db.cities.Remove(obj);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }
        #endregion

        #region District
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult District()
        {
            //ViewData["cities"] = db.cities.Where(x => x.status_id == 1).Select(x => new { city_id = x.city_id, name = x.name }).ToList();
            ViewData["cities"] = db.cities.Where(x => x.status_id == 1).Select(x => new { city_id = x.city_id, name = x.name, x.porder }).OrderByDescending(x => x.porder).ThenBy(x => x.name).ToList();
            ViewData["status"] = db.status.Select(x => new { status_id = x.status_id, name = x.name }).ToList();
            return View();
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult District_Read([DataSourceRequest]DataSourceRequest request)
        {
            var objs = db.districts.Select(x => new
            {
                district_id = x.district_id,
                name = x.name,
                city_id = x.city_id,
                status_id = x.status_id
            }).OrderByDescending(x => x.city_id).ThenBy(x => x.name).ToList();
            return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult District_Create([DataSourceRequest] DataSourceRequest request, district item)
        {
            //district obj = new district();
            if (item != null && ModelState.IsValid)
            {
                try
                {
                    //obj.district_id = (db.districts.Count() > 0)?db.districts.Max(x => x.district_id) + 1:1 ;//item.district_id;
                    //obj.city_id = item.city.city_id;
                    //obj.name = item.name;
                    //if (item.status != null) obj.status_id = item.status.status_id;
                    db.districts.Add(item);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult District_Update([DataSourceRequest] DataSourceRequest request, district item)
        {
            //var obj = db.districts.FirstOrDefault(x => x.district_id == item.district_id);
            //if (obj == null) return HttpNotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    //obj.name = item.name;
                    //obj.city_id = item.city.city_id;
                    //if (item.status != null) obj.status_id = item.status.status_id;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult District_Destroy([DataSourceRequest] DataSourceRequest request, district item)
        {
            var obj = db.districts.FirstOrDefault(x => x.district_id == item.district_id);
            if (obj == null) return HttpNotFound();
            try
            {
                db.districts.Remove(obj);
                if (GetCurrentAdminId() > 0)
                {
                    // write log
                    db.LogErrors.Add(new LogError()
                    {
                        Content = "Admin: " + GetCurrentAdminName() + " remove district " + obj.name,
                        Created = DateTime.Now
                    });
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }
        #endregion

        #region Education

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Education()
        {
            ViewData["status"] = db.status.Select(x => new { status_id = x.status_id, name = x.name }).ToList();
            return View();
        }


        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Education_Read([DataSourceRequest]DataSourceRequest request)
        {
            var objs = db.educations.Select(x => new
            {
                education_id = x.education_id,
                name = x.name,
                status_id = x.status_id
            }).ToList();
            return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Education_Create([DataSourceRequest] DataSourceRequest request, education item)
        {
            education obj = new education();
            if (item != null && ModelState.IsValid)
            {
                try
                {
                    obj.name = item.name;
                    obj.education_id = (db.educations.Count() > 0) ? db.educations.Max(x => x.education_id) + 1 : 1;
                    if (item.status != null) obj.status_id = item.status.status_id;
                    db.educations.Add(obj);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return Json(new[] { obj }.ToDataSourceResult(request, ModelState));
        }


        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Education_Update([DataSourceRequest] DataSourceRequest request, education item)
        {
            var obj = db.educations.FirstOrDefault(x => x.education_id == item.education_id);
            if (obj == null) return HttpNotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    
                    obj.name = item.name;
                    if (item.status != null) obj.status_id = item.status.status_id;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }


        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Education_Destroy([DataSourceRequest] DataSourceRequest request, education item)
        {
            var obj = db.educations.FirstOrDefault(x => x.education_id == item.education_id);
            if (obj == null) return HttpNotFound();
            try
            {
                db.educations.Remove(obj);
                if (GetCurrentAdminId() > 0)
                {
                    // write log
                    db.LogErrors.Add(new LogError()
                    {
                        Content = "Admin: " + GetCurrentAdminName() + " remove education " + obj.name,
                        Created = DateTime.Now
                    });
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }
        #endregion

        #region Job

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Job()
        {
            ViewData["status"] = db.status.Select(x => new { status_id = x.status_id, name = x.name }).ToList();
            return View();
        }


        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Job_Read([DataSourceRequest]DataSourceRequest request)
        {
            var objs = db.jobs.Select(x => new
            {
                job_id = x.job_id,
                name = x.name,
                status_id = x.status_id
            }).ToList();
            return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Job_Create([DataSourceRequest] DataSourceRequest request, job item)
        {
            job obj = new job();
            if (item != null && ModelState.IsValid)
            {
                try
                {
                    obj.job_id = (db.jobs.Count() > 0) ? db.jobs.Max(x => x.job_id) + 1 : 1;
                    obj.name = item.name;
                    if (item.status != null) obj.status_id = item.status.status_id;
                    db.jobs.Add(obj);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return Json(new[] { obj }.ToDataSourceResult(request, ModelState));
        }


        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Job_Update([DataSourceRequest] DataSourceRequest request, job item)
        {
            var obj = db.jobs.FirstOrDefault(x => x.job_id == item.job_id);
            if (obj == null) return HttpNotFound();
            if (ModelState.IsValid)
            {
                try
                {

                    obj.name = item.name;
                    if (item.status != null) obj.status_id = item.status.status_id;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Job_Destroy([DataSourceRequest] DataSourceRequest request, job item)
        {
            var obj = db.jobs.FirstOrDefault(x => x.job_id == item.job_id);
            if (obj == null) return HttpNotFound();
            try
            {
                db.jobs.Remove(obj);
                if (GetCurrentAdminId() > 0)
                {
                    // write log
                    db.LogErrors.Add(new LogError()
                    {
                        Content = "Admin: " + GetCurrentAdminName() + " remove job " + obj.name,
                        Created = DateTime.Now
                    });
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }
        #endregion

        #region ShopType
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult ShopType()
        {
            ViewData["status"] = db.status.Select(x => new { status_id = x.status_id, name = x.name }).ToList();
            return View();
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult ShopType_Read([DataSourceRequest]DataSourceRequest request)
        {
            var objs = db.shoptypes.Select(x => new
            {
                shoptype_id = x.shoptype_id,
                name = x.name,
                status_id = x.status_id
            }).ToList();
            return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult ShopType_Create([DataSourceRequest] DataSourceRequest request, shoptype item)
        {
            //shoptype obj = new shoptype();
            if (item != null && ModelState.IsValid)
            {
                try
                {
                    //obj.shoptype_id = (db.shoptypes.Count() > 0) ? db.shoptypes.Max(x => x.shoptype_id) + 1 : 1;
                    //obj.name = item.name;
                    //if (item.status != null) obj.status_id = item.status.status_id;
                    db.shoptypes.Add(item);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult ShopType_Update([DataSourceRequest] DataSourceRequest request, shoptype item)
        {
            //var obj = db.shoptypes.FirstOrDefault(x => x.shoptype_id == item.shoptype_id);
            //if (obj == null) return HttpNotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    //obj.name = item.name;
                    //if (item.status != null) obj.status_id = item.status.status_id;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult ShopType_Destroy([DataSourceRequest] DataSourceRequest request, shoptype item)
        {
            var obj = db.shoptypes.FirstOrDefault(x => x.shoptype_id == item.shoptype_id);
            if (obj == null) return HttpNotFound();
            try
            {
                db.shoptypes.Remove(obj);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }
        #endregion

        #region Branch
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Branch()
        {
            //ViewData["cities"] = db.cities.Where(x => x.status_id == 1).Select(x => new { city_id = x.city_id, name = x.name }).ToList();
            ViewData["cities"] = db.cities.Where(x => x.status_id == 1).Select(x => new { city_id = x.city_id, name = x.name, x.porder }).OrderByDescending(x => x.porder).ThenBy(x => x.name).ToList();
            ViewData["status"] = db.status.Select(x => new { status_id = x.status_id, name = x.name }).ToList();
            return View();
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Branch_Read([DataSourceRequest]DataSourceRequest request)
        {
            var objs = db.RS_Branch.Select(x => new
            {
                ID = x.ID,
                name = x.name,
                city_id = x.city_id,
                status_id = x.status_id
            }).ToList();
            return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Branch_Create([DataSourceRequest] DataSourceRequest request, RS_Branch item)
        {
            //RS_Branch obj = new RS_Branch();
            if (item != null && ModelState.IsValid)
            {
                try
                {
                    //obj.ID = (db.RS_Branch.Count() > 0) ? db.RS_Branch.Max(x => x.ID) + 1 : 1;
                    //obj.name = item.name;
                    //if (item.city != null) obj.city_id = item.city.city_id;
                    //else obj.city_id = item.city_id;
                    //if (item.status != null) obj.status_id = item.status.status_id;
                    if (GetCurrentAdminId() > 0) item.CreatedBy = item.ModifiedBy = GetCurrentAdminId();
                    item.Created = item.Modified = DateTime.Now;
                    db.RS_Branch.Add(item);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }


        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Branch_Update([DataSourceRequest] DataSourceRequest request, RS_Branch item)
        {
            //var obj = db.RS_Branch.FirstOrDefault(x => x.ID == item.ID);
            //if (obj == null) return HttpNotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    //obj.name = item.name;
                    //if (item.city != null) obj.city_id = item.city.city_id;
                    //else obj.city_id = item.city_id;
                    //if (item.status != null) obj.status_id = item.status.status_id;
                    db.Entry(item).State = EntityState.Modified;
                    if (GetCurrentAdminId() > 0) item.ModifiedBy = GetCurrentAdminId();
                    item.Modified = DateTime.Now;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            //return RedirectToAction("Branch", "HeThong");
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Branch_Destroy([DataSourceRequest] DataSourceRequest request, RS_Branch item)
        {
            var obj = db.RS_Branch.FirstOrDefault(x => x.ID == item.ID);
            if (obj == null) return HttpNotFound();
            try
            {
                db.RS_Branch.Remove(obj);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }
        #endregion

        #region Shop
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Shop()
        {
            ViewData["branches"] = db.RS_Branch.Select(x => new { ID = x.ID, name = x.name, city_id = x.city_id }).ToList();
            ViewData["cities"] = db.cities.Select(x => new { ID = x.city_id, name = x.name }).ToList();
            ViewData["shoptypes"] = db.shoptypes.Select(x => new { shoptype_id = x.shoptype_id, name = x.name }).ToList();
            ViewData["status"] = db.status.Select(x => new { status_id = x.status_id, name = x.name }).ToList();
            return View();
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Shop_Read([DataSourceRequest]DataSourceRequest request)
        {
            var objs = db.shops.Select(x => new
            {
                order_no = x.order_no,
                branch_id = x.branch_id,
                shoptype_id = x.shoptype_id,
                shop_id = x.shop_id,
                name = x.name,
                city_id = x.city_id,
                status_id = x.status_id
            }).ToList();
            return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Shop_Create([DataSourceRequest] DataSourceRequest request, shop item)
        {
            //shop obj = new shop();
            if (item != null && ModelState.IsValid)
            {
                try
                {
                    //obj.shop_id = (db.shops.Count() > 0) ? db.shops.Max(x => x.shop_id) + 1 : 1;
                    //obj.name = item.name;
                    //if (item.status != null) obj.status_id = item.status.status_id;
                    //obj.shoptype_id = item.shoptype.shoptype_id;
                    //obj.branch_id = item.RS_Branch.ID;
                    //obj.city_id = item.RS_Branch.city_id;
                    var objCity = db.RS_Branch.FirstOrDefault(x => x.ID == item.branch_id);
                    if (objCity != null) item.city_id = objCity.city_id;
                    db.shops.Add(item);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Shop_Update([DataSourceRequest] DataSourceRequest request, shop item)
        {
            //var obj = db.shops.FirstOrDefault(x => x.shop_id == item.shop_id);
            //if (obj == null) return HttpNotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    //obj.name = item.name;
                    //obj.branch_id = item.RS_Branch.ID;
                    //obj.city_id = item.RS_Branch.city_id;
                    //obj.shoptype_id = item.shoptype.shoptype_id;
                    //if (item.status != null) obj.status_id = item.status.status_id;
                    var objCity = db.RS_Branch.FirstOrDefault(x => x.ID == item.branch_id);
                    if (objCity != null) item.city_id = objCity.city_id;

                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            //return RedirectToAction("Shop", "HeThong");
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Shop_Destroy([DataSourceRequest] DataSourceRequest request, shop item)
        {
            var obj = db.shops.FirstOrDefault(x => x.shop_id == item.shop_id);
            if (obj == null) return HttpNotFound();
            try
            {
                db.shops.Remove(obj);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }
        #endregion

        #region Status
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Status()
        {
            return View();
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Status_Read([DataSourceRequest]DataSourceRequest request)
        {
            var objs = db.status.Select(x => new
            {
                status_id = x.status_id,
                name = x.name
            }).ToList();
            return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
        
        [AcceptVerbs(HttpVerbs.Post)]
        [SuperAdminAttributes.SuperAdmin]
        [ValidateAntiForgeryToken]
        public ActionResult Status_Create([DataSourceRequest] DataSourceRequest request, status item)
        {
            if (item != null && ModelState.IsValid)
            {
                try
                {
                    //item.status_id = (db.status.Count() > 0)?db.status.Max(x => x.status_id) + 1 : 1;
                    db.status.Add(item);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Status_Update([DataSourceRequest] DataSourceRequest request, status item)
        {
            //var obj = db.status.FirstOrDefault(x => x.status_id == item.status_id);
            //if (obj == null) return HttpNotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    //obj.name = item.name;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return Json(new[] {item}.ToDataSourceResult(request, ModelState));
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Status_Destroy([DataSourceRequest] DataSourceRequest request, status item)
        {
            var obj = db.status.FirstOrDefault(x => x.status_id == item.status_id);
            if (obj == null) return HttpNotFound();
            try
            {
                db.status.Remove(obj);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
        }
        #endregion

        #region Mail Temaple
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult MailTemplate()
        {
            ViewData["cities"] = db.cities.Where(x => x.status_id == 1).Select(x => new { city_id = x.city_id, name = x.name, x.porder }).OrderByDescending(x => x.porder).ThenBy(x => x.name).ToList();
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [CheckPermissionActionFilter]
        public ActionResult MailTemplate_Read([DataSourceRequest]DataSourceRequest request)
        {
            var objs = db.RS_mail_template.Select(x => new
            {
                ID = x.ID,
                CityID = x.CityID,
                Subject = x.Subject,
                Content = x.Content,
                Created = x.Created,
                StatusID = x.StatusID,                
            }).ToList();
            
            return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult MailTemplateEditor(string idEmail)
        {
            if (idEmail != null)
            {
                int value = Convert.ToInt32(idEmail);
                var obj = db.RS_mail_template.Where(x => x.ID == value).FirstOrDefault();
                return View(obj);
            }
            else
            {
                return View();
            }
             
        }


        [SuperAdminAttributes.SuperAdmin]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult RegisterMailTem(int mailId, string title, string subjectName, int? cityId, string Content)
        {
            if (cityId == -1)
            {
                cityId = null;
            }
            try
            {
                switch (mailId)
                {
                    case 0 :          // add new mail
                           var mailtmp = new RS_mail_template
                         {
                            // Title = title,
                             Subject = subjectName,
                             CityID = null,
                             Content = Content,
                             StatusID =  Constant.StatusActive    ,
                             Created =  DateTime.Now ,

                         };
                         db.RS_mail_template.Add(mailtmp);
                         db.SaveChanges();
                        break;

                    default:         // Update mail
                        var mail = db.RS_mail_template.FirstOrDefault(e =>  e.ID ==  mailId);
                        if (mail != null)
                        {
                          //  mail.Title = title;
                            mail.Subject = subjectName;
                            mail.Content = Content;
                            mail.CityID = cityId;
                            db.SaveChanges();
                        }
                        break;

                }
               
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Lưu thành công";

            }
            catch (Exception ex)
            {
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Có lỗi xảy ra, vui lòng xem log";
                Common.WriteLog(ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace);
            }
            return RedirectToAction("MailTemplate");
        }

        [SuperAdminAttributes.SuperAdmin]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterMailTem_Delete(int mailId)
        {
            try
            {
                 
                var mail = db.RS_mail_template.FirstOrDefault(e => e.ID == mailId);
                if (mail != null)
                        {
                            db.RS_mail_template.Remove(mail);
                            db.SaveChanges();
                        }                         
                // Show alert modal 
                TempData["Notice"] = "Xóa thành công";

            }
            catch (Exception ex)
            {   
                TempData["Notice"] = "Có lỗi xảy ra, vui lòng xem log";
                Common.WriteLog(ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace);
            }
            TempData["ShowPopup"] = true;
            return RedirectToAction("MailTemplate");
        }


#endregion

        #region Helper
        public int GetCurrentAdminId()
        {
            return (int)Session["admss"];
        }

        public string GetCurrentAdminName()
        {
            int AdminId = GetCurrentAdminId();
            var admin = db.admins.FirstOrDefault(x => x.admin_id == AdminId);
            if (admin != null) return admin.email;
            else return String.Empty;
        }

        public JsonResult GetSlug(string Title)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp1 = Title.Normalize(NormalizationForm.FormD);
            string temp2 = regex.Replace(temp1, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D').Replace(' ', '-');
            string returnSlug = "/" + Regex.Replace(temp2, "[^0-9a-zA-Z -]", String.Empty) + ".html";
            return Json(returnSlug, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetListCity()
        {
            var listCity = db.cities.Where(x =>   x.status_id == Constant.StatusActive)
                        .Select(x => new { city_id = x.city_id, name = x.name }).ToList();
            return Json(listCity, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDistrictByCity(int cityId)
        {
            var districts = db.districts.Where(x => x.city_id == cityId && x.status_id == 1).Select(x => new { district_id = x.district_id, name = x.name }).ToList();
            return Json(districts, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetShopByCity(int cityId)
        {
            var shops = db.shops.Where(x => x.city_id == cityId && x.status_id == 1).Select(x => new { shop_id = x.shop_id, name = x.name }).ToList();
            return Json(shops, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult checkEmail(string email)
        {
            var obj = db.customers.FirstOrDefault(x => x.email == email);
            if (obj != null) return Json(false, JsonRequestBehavior.AllowGet);
            else return Json(true, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult CheckProduct(int category, string product, string serial)
        {
            // check product in service
            int status = RegisterCustomerProduct.CheckProdcutWebserive(category, serial, product);
            bool isExist;
            if (status == 1)
            {
                isExist = true;
            }
            else
            {
                isExist = false;
            }            
            return Json(isExist, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetProductByCat(int CategoryId)
        {
            var objs = db.products.Where(x => x.category_id == CategoryId && x.status_id == 1).Select(x => new { product_id = x.product_id, name = x.name });
            return Json(objs, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAllProducts()
        {
            var objs = db.products.Where(x => x.status_id == 1).Select(x => new { product_id = x.product_id, name = x.name });
            return Json(objs, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public ActionResult ForeignKeyColumn()
        {
           // PopulateCategories();
            return View();
        }

        public ActionResult ForeignKeyColumn_Read([DataSourceRequest] DataSourceRequest request)
        {
            var brand = db.RS_Branch.Where(x => x.status_id == 1).ToList();
            return Json(brand.ToDataSourceResult(request));
        }

    }
}