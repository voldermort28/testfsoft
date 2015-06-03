using DocumentFormat.OpenXml.Math;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using MyProject.Filters;
using MyProject.Functions;
using MyProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Text.RegularExpressions;
using System.Text;
using MyProject.ViewModels;
using System.IO;
using MyProject.Mailers;
using Kendo.Mvc;
using System.ComponentModel;
using System.Data.Entity.Core.Objects;

namespace MyProject.Controllers
{
    [AuthorizeIPAddressAttribute]
    [CheckPermissionActionFilter]
   // [OutputCache(Duration = 60 * 5, VaryByParam = "none", Location=System.Web.UI.OutputCacheLocation.Client)]
    [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
    public class HeThongController : Controller
    {
        MyDatabaseEntities db = new MyDatabaseEntities();
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
        
        
        
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [CheckPermissionActionFilter]
        public ActionResult Profile_Read([DataSourceRequest]DataSourceRequest request, int categoryId, string modelName = "", string serial = "", bool? isPending = null)
        {
            IQueryable<customer> lstCustomers;
            IQueryable<ProfileVM> objs;
             
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
        public ActionResult UpdateCustomerProduct(int Id, int profileId, int productId, string serialNumber, string timePucharsed, int shopId, int cityId)
        {
            try
            {
                 
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
           // return RedirectToAction("ProfileEditor", new { ID = ID });
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
            var objs = db.ANguoiDungs.Select(x => new
            {
                admin_id = x.NguoiDungID,
                admin_type = x.MaNhomNguoiDung,
                email = x.MaNguoiDung,
                password = x.MatKhau,
                status_id = x.TrangThai  ? 1:0
            }).ToList();
            return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
 

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult AdminEditor(int ID = 0)
        {
            try
            {
                ViewData["status"] = db.status.Select(x => new { ID = x.status_id, Name = x.name }).ToList();

                ViewData["type"] =
                    db.ANhomNguoiDungs.Select(
                        x => new {MaNhomNguoiDung = x.MaNhomNguoiDung, TenNhomNguoiDung = x.TenNhomNguoiDung}).ToList();
                 

                if (ID == 0) return View();
                else
                {
                    var obj = db.ANguoiDungs.FirstOrDefault(x => x.NguoiDungID == ID);
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
        public ActionResult SaveAdmin(ANguoiDung admin)
        {
            try
            {
                // add a new item
                if (admin.NguoiDungID == 0)
                {
                    if (!String.IsNullOrEmpty(admin.MatKhau))
                    {
                        if (db.ANguoiDungs.FirstOrDefault(x => x.MaNguoiDung == admin.MaNguoiDung) == null)
                        {                              
                            admin.MatKhau = Common.MaHoa(admin.MatKhau.Trim());
                            admin.NgayTao = DateTime.Now;
                            admin.NguoiTao = Session["admss"].ToString();
                            admin.NgaySua = DateTime.Now;
                            admin.NguoiSua = Session["admss"].ToString();
                            admin.Active = true;
                          
                            db.ANguoiDungs.Add(admin); 
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
                    var existObj = db.ANguoiDungs.FirstOrDefault(x => x.NguoiDungID == admin.NguoiDungID);
                    if (existObj != null)
                    {
                        if (!String.IsNullOrEmpty(admin.MatKhau))
                        {
                            string pass = Common.MaHoa(admin.MatKhau.Trim());
                            if (existObj.MatKhau.Trim() != pass) existObj.MatKhau = pass;
                        }
                        //existObj.email = admin.email;
                        existObj.MaNhomNguoiDung = admin.MaNhomNguoiDung;
                        existObj.TrangThai = admin.TrangThai;
                        existObj.MaNhomNguoiDung = admin.MaNhomNguoiDung;
                        existObj.TaiKhoan = admin.TaiKhoan;
                        existObj.TrangThai = admin.TrangThai;
                        existObj.MoTa = admin.MoTa;
                        existObj.MaNhanVien = admin.MaNhanVien;
                        existObj.NguoiDung = admin.NguoiDung;
                        existObj.ChucNang = admin.ChucNang;
                        existObj.Active = admin.Active;
                        existObj.NguoiSua = Session["admss"].ToString();
                        existObj.NgaySua = DateTime.Now;
                         
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
        #region AdminType
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult AdminType()
        {
            ViewData["status"] = db.status.Select(x => new { status_id = x.status_id, name = x.name }).ToList();
            return View();
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AdminType_Read([DataSourceRequest]DataSourceRequest request)
        {
            var objs = db.ANhomNguoiDungs.Select(x => new
            {
                NhomNguoiDungID = x.NhomNguoiDungID,
                MaNhomNguoiDung = x.MaNhomNguoiDung,
                TenNhomNguoiDung = x.TenNhomNguoiDung,
                MoTa = x.MoTa,
                status_id = x.TrangThai ? 1 : 0,
                NguoiDung = x.NguoiDung,
                ChucNang = x.ChucNang,
                Active = x.Active ==true ? 1 : 0,
             
            }).ToList();
            return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult AdminTypeEditor(int ID = 0)
        {
            try
            {
                ViewData["status"] = db.status.Select(x => new { ID = x.status_id, Name = x.name }).ToList();

                ViewData["type"] =
                    db.ANhomNguoiDungs.Select(
                        x => new { MaNhomNguoiDung = x.MaNhomNguoiDung, TenNhomNguoiDung = x.TenNhomNguoiDung }).ToList();


                if (ID == 0) return View();
                else
                {
                    var obj = db.ANhomNguoiDungs.FirstOrDefault(x => x.MaNhomNguoiDung == ID.ToString());
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
        public ActionResult SaveAdminType(ANhomNguoiDung adminType)
        {
            try
            {
                // add a new item
                if (adminType.NhomNguoiDungID == 0)
                {
                    if (db.ANhomNguoiDungs.FirstOrDefault(x => x.NhomNguoiDungID == adminType.NhomNguoiDungID) == null)
                    {  
                        adminType.NgayTao = DateTime.Now;
                        adminType.NguoiTao = Session["admss"].ToString();
                        adminType.NgaySua = DateTime.Now;
                        adminType.NguoiSua = Session["admss"].ToString();
                        adminType.Active = true;

                        db.ANhomNguoiDungs.Add(adminType);
                        db.SaveChanges();
                    }
                    else
                    {
                        TempData["Notice"] = "Nhóm người dùng này đã được đăng ký rồi";
                        TempData["ShowPopup"] = true;
                    }
                }
                else // modify item
                {
                    var existObj = db.ANhomNguoiDungs.FirstOrDefault(x => x.NhomNguoiDungID == adminType.NhomNguoiDungID);
                    if (existObj != null)
                    { 
                        //existObj.email = admin.email;
                        existObj.TenNhomNguoiDung = adminType.TenNhomNguoiDung;
                        existObj.TrangThai = adminType.TrangThai;
                        existObj.MoTa = adminType.MoTa;
                        existObj.NguoiDung = adminType.NguoiDung;
                        existObj.ChucNang = adminType.ChucNang;
                        existObj.NgaySua = DateTime.Now;
                        existObj.NguoiSua = Session["admss"].ToString();
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