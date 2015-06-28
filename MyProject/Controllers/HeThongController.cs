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


        public ActionResult Customer()
        {
            return View();
        }


        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult Customer_Read([DataSourceRequest]DataSourceRequest request, string itemSearch = "")
        {
            if (string.IsNullOrEmpty(itemSearch))
            {
                var objs = db.DMKhachHangs.Select(x => new
                {
                    KhachHangID = x.KhachHangID,
                    MaKhachHang = x.MaKhachHang,
                    TenKhachHang = x.TenKhachHang,
                    NgaySinh = x.NgaySinh,
                    SDT = x.SDT,
                    Email = x.Email,
                    LoaiKhachHangID = x.LoaiKhachHangID,
                    DiemTich = x.DiemTich,
                    MaSoThue = x.MaSoThue,
                    DiaChi = x.DiaChi,
                    CardID = x.CardID,
                    NgayCap = x.NgayCap,
                    DinhMucCongNo = x.DinhMucCongNo
                }).ToList();
                return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
            else
            {
                var objs = db.DMKhachHangs.Where(x =>   x.MaKhachHang.Contains(itemSearch) || x.TenKhachHang.Contains(itemSearch)).Select(x => new
                {
                    KhachHangID = x.KhachHangID,
                    MaKhachHang = x.MaKhachHang,
                    TenKhachHang = x.TenKhachHang,
                    NgaySinh = x.NgaySinh,
                    SDT = x.SDT,
                    Email = x.Email,
                    LoaiKhachHangID = x.LoaiKhachHangID,
                    DiemTich = x.DiemTich,
                    MaSoThue = x.MaSoThue,
                    DiaChi = x.DiaChi,
                    CardID = x.CardID,
                    NgayCap = x.NgayCap,
                    DinhMucCongNo = x.DinhMucCongNo
                }).ToList();
                return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult CustomerEditor(string ID = "0")
        {
            try
            {
                ViewData["loaiKhachHang"] = db.DMLoaiKhachHangs
                    .Select(x => new { LoaiKhachHangID = x.LoaiKhachHangID, TenLoaiKhachHang = x.TenLoaiKhachHang })
                    .OrderByDescending(x => x.LoaiKhachHangID).ThenBy(x => x.LoaiKhachHangID).ToList();

                if (ID.Equals("0"))
                {
                    return View();
                }
                else
                {
                    var IDCus = int.Parse(ID);
                    var obj = db.DMKhachHangs.FirstOrDefault(x => x.KhachHangID == IDCus);
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

        public ActionResult SaveDMKhachHang(DMKhachHang dmKhachHang )
        {
            try
            {
                // add a new item
                if (dmKhachHang.KhachHangID == 0)
                {
                    if (db.DMKhachHangs.FirstOrDefault(x => x.KhachHangID == dmKhachHang.KhachHangID) == null)
                    {
                        dmKhachHang.NgayTao = DateTime.Now;
                        dmKhachHang.NguoiTao = Session["admss"].ToString();
                        dmKhachHang.NgaySua = DateTime.Now;
                        dmKhachHang.NguoiSua = Session["admss"].ToString(); 
                        db.DMKhachHangs.Add(dmKhachHang);
                        db.SaveChanges();
                        // ghi log
                        Common.NhatKiHeThong("Thêm khách hàng", "Thêm khách hàng", "khách hàng", "Thêm khách hàng  " + dmKhachHang.MaKhachHang + ":"+ dmKhachHang.TenKhachHang);
                    }
                    else
                    {
                        TempData["Notice"] = "Tầng " + dmKhachHang.MaKhachHang + " dùng này đã được đăng ký rồi";
                        TempData["ShowPopup"] = true;
                    }
                }
                else // modify item
                {
                    var existObj = db.DMKhachHangs.FirstOrDefault(x => x.KhachHangID == dmKhachHang.KhachHangID);
                    if (existObj != null)
                    {
                         
                        existObj.TenKhachHang = dmKhachHang.TenKhachHang;
                        existObj.NgaySinh = dmKhachHang.NgaySinh;
                        existObj.SDT = dmKhachHang.SDT;
                        existObj.Email = dmKhachHang.Email;
                        existObj.LoaiKhachHangID = dmKhachHang.LoaiKhachHangID;
                        existObj.DiemTich = dmKhachHang.DiemTich;
                        existObj.MaSoThue = dmKhachHang.MaSoThue;
                        existObj.DiaChi = dmKhachHang.DiaChi;
                        existObj.CardID = dmKhachHang.CardID;
                        existObj.NgayCap = dmKhachHang.NgayCap;
                        existObj.DinhMucCongNo = dmKhachHang.DinhMucCongNo;
                        existObj.NgaySua = DateTime.Now;
                        existObj.NguoiSua = Session["admss"].ToString();
                        db.SaveChanges();
                        // ghi log
                        Common.NhatKiHeThong("Sửa tầng", "Sửa tầng", "Tầng", "Sửa tầng  " + existObj.MaKhachHang + ":" + existObj.TenKhachHang);

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
            return RedirectToAction("DMKhachHang");
        }


        [HttpPost]
        public ActionResult SearchCustomer(string maKhachHang)
        {
           // var threadList = db.DMKhachHangs.Where(s => s.MaKhachHang == maKhachHang);
            var threadList = db.DMKhachHangs.Where(s => s.MaKhachHang == maKhachHang).Select(x => new
            {
                KhachHangID = x.KhachHangID,
                MaKhachHang = x.MaKhachHang,
                TenKhachHang = x.TenKhachHang,
                NgaySinh  = x.NgaySinh ,
                Email = x.Email,
                MaSoThue = x.MaSoThue,
                DinhMucCongNo = x.DinhMucCongNo,
                DiemTich = x.DiemTich,
                CardID = x.CardID,
                NgayCap = x.NgayCap,
                DiaChi = x.DiaChi,
                SDT = x.SDT
            }).ToList();

            return Json(threadList, JsonRequestBehavior.AllowGet);
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
        public ActionResult CustomerType_Read([DataSourceRequest]DataSourceRequest request, string itemSearch = "")
        {
            if (string.IsNullOrEmpty(itemSearch))
            {
                var objs = db.DMLoaiKhachHangs.Select(x => new
                {
                    LoaiKhachHangID = x.LoaiKhachHangID,
                    MaLoaiKhachHang = x.MaLoaiKhachHang,
                    TenLoaiKhachHang = x.TenLoaiKhachHang
                }).ToList();
                return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
            else
            {
                var objs = db.DMLoaiKhachHangs.Where(x => x.MaLoaiKhachHang.Contains(itemSearch) || x.TenLoaiKhachHang.Contains(itemSearch)).Select(x => new
                {
                    LoaiKhachHangID = x.LoaiKhachHangID,
                    MaLoaiKhachHang = x.MaLoaiKhachHang,
                    TenLoaiKhachHang = x.TenLoaiKhachHang
                }).ToList();
                return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            } 
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult CustomerType_Create([DataSourceRequest] DataSourceRequest request, DMLoaiKhachHang item)
        {
            //customertype obj = new customertype();
            if (item != null && ModelState.IsValid)
            {
                try
                {
                   
                    db.DMLoaiKhachHangs.Add(item);
                    db.SaveChanges();
                    // ghi log
                    Common.NhatKiHeThong("Thêm loại khách hàng", "Thêm loại khách hàng", "loại khách hàng", "Thêm loại khách hàng  " + item.MaLoaiKhachHang + ":" + item.TenLoaiKhachHang);
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
        public ActionResult CustomerType_Update([DataSourceRequest] DataSourceRequest request, DMLoaiKhachHang item)
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
                    // ghi log
                    Common.NhatKiHeThong("Sửa loại khách hàng", "sửa loại khách hàng", "loại khách hàng", "Sửa loại khách hàng  " + item.MaLoaiKhachHang + ":" + item.TenLoaiKhachHang);
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
        public ActionResult CustomerType_Destroy([DataSourceRequest] DataSourceRequest request, DMLoaiKhachHang item)
        {
            var obj = db.DMLoaiKhachHangs.FirstOrDefault(x => x.LoaiKhachHangID == item.LoaiKhachHangID);
            if (obj == null) return HttpNotFound();
            try
            {
                db.DMLoaiKhachHangs.Remove(obj);
                db.SaveChanges();
                // ghi log
                Common.NhatKiHeThong("Xóa loại khách hàng", "Xóa loại khách hàng", "loại khách hàng", "Xóa loại khách hàng  " + item.MaLoaiKhachHang + ":" + item.TenLoaiKhachHang);
            }
            catch (Exception ex)
            {
                Common.WriteLog(ex.Message + "\n" + ex.StackTrace);
            }
            return Json(new[] { item }.ToDataSourceResult(request, ModelState));
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
            var test = new ANhomMenu();
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


        #region "DMTang"
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult DMTang()
        {
            ViewData["status"] = db.status.Select(x => new { status_id = x.status_id, name = x.name }).ToList();
            return View();
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DMTang_Read([DataSourceRequest]DataSourceRequest request, string itemSearch = "")
        {

            if (string.IsNullOrEmpty(itemSearch))
            {
                var objs = db.DMTangs.Select(x => new
                {
                    TangID = x.TangID,
                    MaTang = x.MaTang,
                    TenTang = x.TenTang,
                    TrangThai = x.TrangThai ? 1 : 0
                }).ToList();
                return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
            else
            {
                var objs = db.DMTangs.Where(x => x.MaTang.Contains(itemSearch) || x.TenTang.Contains(itemSearch)).Select(x => new
                {
                    TangID = x.TangID,
                    MaTang = x.MaTang,
                    TenTang = x.TenTang,
                    TrangThai = x.TrangThai ? 1 : 0
                }).ToList();
                return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }

        }
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult DMTangEditor(string ID = "0")
        {
            try
            {
                ViewData["status"] = db.status.Select(x => new { ID = x.status_id, Name = x.name }).ToList();

                if (ID.Equals("0"))
                {
                    return View();
                }
                else
                {
                    var IDTang = int.Parse(ID);
                    var obj = db.DMTangs.FirstOrDefault(x => x.TangID == IDTang);
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
        public ActionResult SaveDMTang(DMTang dmTang, string isDelete = "")
        {
            try
            {
                // add a new item
                if (dmTang.TangID == 0)
                {
                    if (db.DMTangs.FirstOrDefault(x => x.TangID == dmTang.TangID) == null)
                    {
                        dmTang.NgayTao = DateTime.Now;
                        dmTang.NguoiTao = Session["admss"].ToString();
                        dmTang.NgaySua = DateTime.Now;
                        dmTang.NguoiSua = Session["admss"].ToString();
                        dmTang.TrangThai = true;

                        db.DMTangs.Add(dmTang);
                        db.SaveChanges();
                        // ghi log
                        Common.NhatKiHeThong("Thêm tầng", "Thêm tầng", "Tầng", "Thêm tầng  " + dmTang.MaTang);
                    }
                    else
                    {
                        TempData["Notice"] = "Tầng " + dmTang.MaTang + " dùng này đã được đăng ký rồi";
                        TempData["ShowPopup"] = true;
                    }
                }
                else // modify item
                {
                    var existObj = db.DMTangs.FirstOrDefault(x => x.TangID == dmTang.TangID);
                    if (existObj != null)
                    {
                        //existObj.email = admin.email;
                        //existObj.MaTang = dmTang.MaTang;
                        existObj.TenTang = dmTang.TenTang;
                        existObj.TrangThai = true;
                        existObj.NgaySua = DateTime.Now;
                        existObj.NguoiSua = Session["admss"].ToString();
                        db.SaveChanges();
                        // ghi log
                        Common.NhatKiHeThong("Sửa tầng", "Sửa tầng", "Tầng", "Sửa tầng  " + dmTang.MaTang + ":" + dmTang.MaTang);

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
            return RedirectToAction("DMTang");
        }


        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteDMTang(int tangID, string maTang)
        {
            try
            {
                var existObj = db.DMTangs.FirstOrDefault(x => x.TangID == tangID);
                if (existObj != null)
                {
                    db.DMTangs.Remove(existObj);
                    db.SaveChanges();
                    Common.NhatKiHeThong("Xóa tầng", "Xóa tầng", "Tầng", "Xóa tầng  " + tangID.ToString() + ":" + maTang);
                    // Show alert modal
                    TempData["ShowPopup"] = true;
                    TempData["Notice"] = "Đã xóa thành công";
                }
            }
            catch (Exception ex)
            {
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Có lỗi xảy ra, vui lòng kiểm tra log";
                Common.WriteLog(ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace);
            }
            return RedirectToAction("DMTang");
        }


        #endregion
         
        #region "DMPhong"
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult DMPhong()
        {
            ViewData["status"] = db.status.Select(x => new { status_id = x.status_id, name = x.name }).ToList();
            return View();
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DMPhong_Read([DataSourceRequest]DataSourceRequest request, string itemSearch = "")
        {

            if (string.IsNullOrEmpty(itemSearch))
            {
                var objs = db.DMPhongs.Select(x => new
                {
                    PhongID = x.PhongID,
                    MaTang = x.MaTang,
                    MaPhong  =x.MaPhong  , 
                    TenPhong = x.TenPhong,
                    TrangThai = x.TrangThai ? 1 : 0
                }).ToList();
                return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
            else
            {
                var objs = db.DMPhongs.Where(x => x.MaTang.Contains(itemSearch) || x.MaPhong.Contains(itemSearch) || x.TenPhong.Contains(itemSearch)).Select(x => new
                {
                    PhongID = x.PhongID,
                    MaTang = x.MaTang,
                    TenPhong = x.TenPhong,
                    MaPhong = x.MaPhong, 
                    TrangThai = x.TrangThai ? 1 : 0
                }).ToList();
                return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }

        }
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult DMPhongEditor(string ID = "0")
        {
            try
            {
                ViewData["maTang"] = db.DMTangs.Select(x => new { TangID = x.TangID, TenTang = x.TenTang }).ToList();

                if (ID.Equals("0") || string.IsNullOrEmpty(ID))
                {
                    return View();
                }
                else
                {
                    var IDPhong = int.Parse(ID);
                    var obj = db.DMPhongs.FirstOrDefault(x => x.PhongID == IDPhong);
                    if (obj == null)
                    {
                        return HttpNotFound();
                    }
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
        public ActionResult SaveDMPhong(DMPhong dmPhong, string isDelete = "")
        {
            try
            {
                // add a new item
                if (dmPhong.PhongID == 0)
                {
                    if (db.DMPhongs.FirstOrDefault(x => x.PhongID == dmPhong.PhongID) == null)
                    {
                        dmPhong.NgayTao = DateTime.Now;
                        dmPhong.NguoiTao = Session["admss"].ToString();
                        dmPhong.NgaySua = DateTime.Now;
                        dmPhong.NguoiSua = Session["admss"].ToString();
                        dmPhong.TrangThai = true;

                        db.DMPhongs.Add(dmPhong);
                        db.SaveChanges();
                        // ghi log
                        Common.NhatKiHeThong("Thêm phòng", "Thêm phòng", "Phòng", "Thêm phòng  " + dmPhong.MaPhong  +":"+ dmPhong.TenPhong);
                    }
                    else
                    {
                        TempData["Notice"] = "Tầng " + dmPhong.MaPhong + " dùng này đã được đăng ký rồi";
                        TempData["ShowPopup"] = true;
                    }
                }
                else // modify item
                {
                    var existObj = db.DMPhongs.FirstOrDefault(x => x.PhongID == dmPhong.PhongID);
                    if (existObj != null)
                    {
                        //existObj.email = admin.email;
                        //existObj.MaTang = dmTang.MaTang;
                        existObj.MaTang = dmPhong.MaTang;
                        existObj.TenPhong = dmPhong.TenPhong;
                        existObj.TrangThai = true;
                        existObj.NgaySua = DateTime.Now;
                        existObj.NguoiSua = Session["admss"].ToString();
                        db.SaveChanges();
                        // ghi log
                        Common.NhatKiHeThong("Sửa phòng", "Sửa phòng", "phòng", "Sửa phòng  " + dmPhong.MaPhong + ":" + dmPhong.TenPhong);

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
            return RedirectToAction("DMPhong");
        }


        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteDMPhong(int phongID)
        {
            try
            {
                var existObj = db.DMPhongs.FirstOrDefault(x => x.PhongID == phongID);
                if (existObj != null)
                {
                    db.DMPhongs.Remove(existObj);
                    db.SaveChanges();
                    Common.NhatKiHeThong("Xóa phòng", "Xóa phòng", "phòng", "Xóa phòng  " + existObj.PhongID.ToString() + ":" + existObj.TenPhong);
                    // Show alert modal
                    TempData["ShowPopup"] = true;
                    TempData["Notice"] = "Đã xóa thành công";
                }
            }
            catch (Exception ex)
            {
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Có lỗi xảy ra, vui lòng kiểm tra log";
                Common.WriteLog(ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace);
            }
            return RedirectToAction("DMTang");
        }


        #endregion

        #region "DMBan"
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult DMBan()
        {
          
            return View();
        }

        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DMBan_Read([DataSourceRequest]DataSourceRequest request, string itemSearch = "")
        {

            if (string.IsNullOrEmpty(itemSearch))
            {
                var objs = db.DMBans.Select(x => new
                {
                    PhongID = x.PhongID,
                    BanID = x.BanID,
                    MaBan = x.MaBan,
                    TenBan = x.TenBan,
                    SoLuong = x.SoLuong,
                    LoaiBan = x.LoaiBan,
                    MoTa  = x.MoTa
                }).ToList();
                return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }
            else
            {
                var objs = db.DMBans.Where(x => x.MaBan.Contains(itemSearch) || x.TenBan.Contains(itemSearch) || x.MoTa.Contains(itemSearch)).Select(x => new
                {
                    PhongID = x.PhongID,
                    BanID = x.BanID,
                    MaBan = x.MaBan,
                    TenBan = x.TenBan,
                    SoLuong = x.SoLuong,
                    LoaiBan = x.LoaiBan,
                    MoTa = x.MoTa
                }).ToList();
                return Json(objs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
            }

        }
        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult DMBanEditor(string ID = "0")
        {
            try
            {
                ViewData["maPhong"] = db.DMPhongs.Select(x => new { PhongID = x.PhongID, TenPhong = x.TenPhong }).ToList();

                if (ID.Equals("0") || string.IsNullOrEmpty(ID))
                {
                    return View();
                }
                else
                {
                    var IDBan = int.Parse(ID);
                    var obj = db.DMBans.FirstOrDefault(x => x.BanID == IDBan);
                    if (obj == null)
                    {
                        return HttpNotFound();
                    }
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
        public ActionResult SaveDMBan(DMBan dmBan, string isDelete = "")
        {
            try
            {
                // add a new item
                if (dmBan.BanID == 0)
                {
                    if (db.DMBans.FirstOrDefault(x => x.BanID == dmBan.BanID) == null)
                    {
                        dmBan.NgayTao = DateTime.Now;
                        dmBan.NguoiTao = Session["admss"].ToString();
                        dmBan.NgaySua = DateTime.Now;
                        dmBan.NguoiSua = Session["admss"].ToString();

                        db.DMBans.Add(dmBan);
                        db.SaveChanges();
                        // ghi log
                        Common.NhatKiHeThong("Thêm bàn", "Thêm bàn", "Bàn", "Thêm bàn  " + dmBan.MaBan + ":" + dmBan.TenBan);
                    }
                    else
                    {
                        TempData["Notice"] = "Tầng " + dmBan.MaBan + " dùng này đã được đăng ký rồi";
                        TempData["ShowPopup"] = true;
                    }
                }
                else // modify item
                {
                    var existObj = db.DMBans.FirstOrDefault(x => x.BanID == dmBan.BanID );
                    if (existObj != null)
                    {
                        existObj.PhongID = dmBan.PhongID;
                        existObj.TenBan = dmBan.TenBan;
                        existObj.SoLuong = dmBan.SoLuong;
                        existObj.LoaiBan = dmBan.LoaiBan;
                        existObj.MoTa = dmBan.MoTa;
                        existObj.NgaySua = DateTime.Now;
                        existObj.NguoiSua = Session["admss"].ToString();
                        db.SaveChanges();
                        // ghi log
                        Common.NhatKiHeThong("Sửa bàn", "Sửa bàn", "bàn", "Sửa bàn  " + dmBan.MaBan + ":" + dmBan.TenBan);

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
            return RedirectToAction("DMBan");
        }


        [SuperAdminAttributes.SuperAdmin]
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteDMBan(int banID)
        {
            try
            {
                var existObj = db.DMBans.FirstOrDefault(x => x.BanID == banID);
                if (existObj != null)
                {
                    db.DMBans.Remove(existObj);
                    db.SaveChanges();
                    Common.NhatKiHeThong("Xóa bàn", "Xóa bàn", "bàn", "Xóa bàn  " + existObj.MaBan.ToString() + ":" + existObj.TenBan);
                    // Show alert modal
                    TempData["ShowPopup"] = true;
                    TempData["Notice"] = "Đã xóa thành công";
                }
            }
            catch (Exception ex)
            {
                // Show alert modal
                TempData["ShowPopup"] = true;
                TempData["Notice"] = "Có lỗi xảy ra, vui lòng kiểm tra log";
                Common.WriteLog(ex.Message + "\n" + ex.InnerException + "\n" + ex.StackTrace);
            }
            return RedirectToAction("DMTang");
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