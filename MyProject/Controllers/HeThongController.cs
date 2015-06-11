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
using Kendo.Mvc;
using System.ComponentModel;
using System.Data.Entity.Core.Objects;

namespace MyProject.Controllers
{ 
    [CheckPermissionActionFilter] 
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

     
         #endregion

        #region CustomerType
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult CustomerType()
        {
            ViewData["status"] = db.status.Select(x => new { status_id = x.status_id, name = x.name }).ToList();
            return View();
        }
  
        #endregion        
         
         
        #region NguoiDung
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
                NguoiDungID = x.NguoiDungID,
                MaNhomNguoiDung = x.MaNhomNguoiDung,
         MaNguoiDung   = x.MaNguoiDung , 
                TrangThai = x.TrangThai ? 1 : 0
            }).ToList();
            return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
 

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult AdminEditor(int ID = 0)
        {
            try
            {  
                List<SelectListItem> lstTrangThai = new List<SelectListItem>();
                lstTrangThai.Add(new SelectListItem { Value = "False", Text = "Chưa kích hoạt" });
                lstTrangThai.Add(new SelectListItem { Value = "True", Text = "Đã kích hoạt" });
                ViewData["TrangThai"] = lstTrangThai;
                ViewData["MaNhomNguoiDung"] =
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
        public ActionResult DeleteNguoiDung(int Id)
        {
            try
            {
                var admin = db.ANguoiDungs.FirstOrDefault(x => x.NguoiDungID == Id);
                if (admin == null) return HttpNotFound();
                admin.TrangThai = false ; // set status delete
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
                Active = x.Active == true ? 1 : 0,

            }).ToList();
            return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AdminTypeUserMenu_Read([DataSourceRequest]DataSourceRequest request ,  int admintypeId = 0)
        {
            var  lstObj = (from profile in db.ANhomNguoiDungs
                        join products in db.ANhomMenus on profile.MaNhomNguoiDung equals products.MaNhomNguoiDung into temp
                        from x in temp.DefaultIfEmpty()
                        where profile.NhomNguoiDungID ==  admintypeId
                              orderby profile.NhomNguoiDungID ascending 
                        select new ANhomMenu()
                        {
                            NhomMenuID = x.NhomMenuID,
                            MaMenu = x.MaMenu,
                            MaNhomNguoiDung = x.MaNhomNguoiDung,
                            VIEW = x.VIEW,
                            ADD = x.ADD,
                            EDIT = x.EDIT,
                            DELETE = x.DELETE,
                            IMPORT = x.IMPORT,
                            EXPORT = x.EXPORT,
                            PRINT = x.PRINT,
                            CONTROL = x.CONTROL
                        }).ToList();

            return Json(lstObj.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult AdminTypeEditor(string ID = "0")
        {
            try
            {
                ViewData["status"] = db.status.Select(x => new { ID = x.status_id, Name = x.name }).ToList();

                ViewData["type"] =
                    db.ANhomNguoiDungs.Select(
                        x => new { MaNhomNguoiDung = x.MaNhomNguoiDung, TenNhomNguoiDung = x.TenNhomNguoiDung }).ToList();
                 
                if (ID.Equals("0")) return View();
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
         
        #region Menu
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult AMenu()
        {
            ViewData["status"] = db.status.Select(x => new { status_id = x.status_id, name = x.name }).ToList();
            return View();
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AMenu_Read([DataSourceRequest]DataSourceRequest request)
        {
            var objs = db.AMenus.Select(x => new
            {
                MenuID = x.MenuID,
                MaMenu = x.MaMenu,
                TenMenu = x.TenMenu,
                TrangThai = x.TrangThai,
                MoTa = x.MoTa ,
                NguoiDung = x.NguoiDung ,
                ChucNang = x.ChucNang ,
                Active = x.Active
            }).ToList();
            return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }


        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult AMenuEditor(int ID = 0)
        {
            try
            {
                ViewData["status"] = db.status.Select(x => new { ID = x.status_id, Name = x.name }).ToList();
                List<SelectListItem> lstType = new List<SelectListItem>();
                lstType.Add(new SelectListItem { Value = "0", Text = "Đang hoạt động" });
                lstType.Add(new SelectListItem { Value = "1", Text = "Ngừng hoạt động" }); 
                ViewData["type"] = lstType;

                List<SelectListItem> lstActive = new List<SelectListItem>();
                lstActive.Add(new SelectListItem { Value = "0", Text = "Xóa" });
                lstActive.Add(new SelectListItem { Value = "1", Text = "Hoạt động" });
                ViewData["lstActive"] = lstActive;

                if (ID == 0) return View();
                else
                {
                    var obj = db.AMenus.FirstOrDefault(x => x.MenuID == ID);
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
        public ActionResult SaveAMenu(AMenu menu)
        {
            try
            {
              
                if (menu.MenuID == 0)
                {
                    if (db.AMenus.FirstOrDefault(x => x.MenuID == menu.MenuID) == null)
                    {
                        
                        // add a new item
                        menu.NguoiTao = Session["admss"].ToString();
                        menu.NgayTao = DateTime.Now;
                        menu.NguoiSua = Session["admss"].ToString();
                        menu.NgaySua = DateTime.Now;

                        db.AMenus.Add(menu);
                        db.SaveChanges();
                        //GhiLog
                        Common.NhatKiHeThong("Save Menu", "Thêm mới", "Menu", "Thêm mới menu " + menu.MaMenu);
                        TempData["Notice"] = "Thêm thành công";
                        TempData["ShowPopup"] = true;
                    }
                    else
                    {
                        TempData["Notice"] = "Mã menu đã được đăng kí";
                        TempData["ShowPopup"] = true;
                    }
                }
                else // modify item
                {
                    var existObj = db.AMenus.FirstOrDefault(x => x.MenuID == menu.MenuID);
                    if (existObj != null)
                    {
                       
                        existObj.TenMenu = menu.TenMenu;
                        existObj.TrangThai = menu.TrangThai;
                        existObj.MoTa = menu.MoTa;
                        existObj.NguoiDung = menu.NguoiDung;
                        existObj.ChucNang = menu.ChucNang;
                        existObj.NgaySua = DateTime.Now;
                        existObj.NguoiSua = Session["admss"].ToString();  
                        db.SaveChanges();
                        //GhiLog
                        Common.NhatKiHeThong("Save Menu", "Sửa menu", "Menu", "Sửa menu " + menu.MaMenu);
                        TempData["Notice"] = "Cập nhật dữ liệu thành công";
                        TempData["ShowPopup"] = true;
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
            return RedirectToAction("AMenu");
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

        public ActionResult ForeignKeyColumn()
        {
            // PopulateCategories();
            return View();
        }
        #endregion

        
         
    }
}