using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MySony.Functions;
using MySony.Models;
using MySony.ViewModels;
using MySony.Filters;

namespace MySony.Controllers
{
    // [OutputCache(Duration = 60 * 60, VaryByParam = "none")]
    public class TrainingController : Controller
    {
        private MySonyEntities db = new MySonyEntities();
        // GET: Training         
        
        [LoginRequried]
        public ActionResult Index()
        {            
              
              return RedirectToAction("Review");
        }

        // GET: Training     
        [LoginRequried]
        public ActionResult Review(int ID = 0)
        {
            var tra = new TrainingVM();
            if (ID == 0)
            {
                var id = db.RS_Category.FirstOrDefault(x => x.IsActive && x.Tags.Contains(Constant.TagHuanLuyen));
                if (id != null)
                {
                    tra.lstImage = db.RS_Images.Where(x => x.CategoryID == id.ID && x.IsActive).ToList();
                    tra.Id = id.ID;  
                }                
            }
            else
            {
                tra.lstImage = db.RS_Images.Where(x => x.CategoryID == ID && x.IsActive).ToList();
                tra.Id = ID;                
            }
            return View(tra);
        }

        // GET GetTraining
        [LoginRequried]
          [HttpGet]
        public JsonResult GetTraining()
        {
            var res = (from a in db.RS_Category
                       where (a.Tags.Contains(Constant.TagHuanLuyen) && a.IsActive == true)
                         select new {
                           a.ID,
                           a.Name,                            
                                   }).ToList();
            return Json(res, JsonRequestBehavior.AllowGet);             
        }

        [LoginRequried]
          [AcceptVerbs(HttpVerbs.Get)]
          public ActionResult GetImageByRsCategoryId(int ID)
          {
              var objs = db.RS_Images.Where(x => x.CategoryID == ID && x.IsActive == true)
                          .Select(x => new
                          {
                              ID = x.ID, 
                              Description = x.Descript , 
                              LinkImage  = x.LinkImage,
                              Title   = x.Title
                          });
              return Json(objs, JsonRequestBehavior.AllowGet);
          }

    }
}