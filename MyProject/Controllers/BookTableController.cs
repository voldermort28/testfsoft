using System;
using System.Data;
using System.Web.Mvc;
using DayPilot.Web.Mvc.Json;
using MyProject.Functions;

namespace MyProject.Controllers
{
    public class BookTableController : Controller
    {
        public ActionResult Edit(int id)
        {
            DataRow dr = DbHelper.GetReservation(id);

            if (dr == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new
            {
                Id = id,
                Text = dr["ReservationName"],
                Start = Convert.ToDateTime(dr["ReservationStart"]),
                End = Convert.ToDateTime(dr["ReservationEnd"]),
                Status = new SelectList(new SelectListItem[]
                {
                    new SelectListItem { Text = "New", Value = "0"},
                    new SelectListItem { Text = "Confirmed", Value = "1"},
                    new SelectListItem { Text = "Arrived", Value = "2"},
                    new SelectListItem { Text = "Checked out", Value = "3"}
                }, "Value", "Text", dr["ReservationStatus"]),
                Paid = new SelectList(new SelectListItem[]
                {
                    new SelectListItem { Text = "0%", Value = "0"},
                    new SelectListItem { Text = "50%", Value = "50"},
                    new SelectListItem { Text = "100%", Value = "100"},
                }, "Value", "Text", dr["ReservationPaid"]),
                Resource = new SelectList(DbHelper.GetRoomSelectList(), "Value", "Text", dr["BanID"])
            });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Edit(FormCollection form)
        {
            int id = Convert.ToInt32( form["Id"]);
            string name = form["Text"];
            DateTime start = Convert.ToDateTime(form["Start"]);
            DateTime end = Convert.ToDateTime(form["End"]);
            int resource = Convert.ToInt32( form["Resource"]);
            int paid = Convert.ToInt32(form["Paid"]);
            int status = Convert.ToInt32(form["Status"]);

            DataRow dr = DbHelper.GetReservation(id);

            if (dr == null)
            {
                throw new Exception("The task was not found");
            }

            DbHelper.UpdateReservation(id, name, start, end, resource, status, paid);

            return JavaScript(SimpleJsonSerializer.Serialize("OK"));
        }

        public ActionResult Create()
        {
            return View(new 
            {
                //Start = Convert.ToDateTime(Request.QueryString["start"]).ToString("dd/MM/yyyy HH:mm"),
                //End = Convert.ToDateTime(Request.QueryString["end"]).ToString("dd/MM/yyyy HH:mm"),
                Start = Convert.ToDateTime(Request.QueryString["start"]),
                End = Convert.ToDateTime(Request.QueryString["end"]),
                Resource = new SelectList(DbHelper.GetRoomSelectList(), "Value", "Text", Request.QueryString["resource"])
            });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Create(FormCollection form)
        {
            DateTime start = Convert.ToDateTime(form["Start"]);
            DateTime end = Convert.ToDateTime(form["End"]);
            string text = form["Text"];
            int resource = Convert.ToInt32(form["Resource"]);

            DbHelper.CreateReservation(start, end, resource, text);
            return JavaScript(SimpleJsonSerializer.Serialize("OK"));
        }

        [HttpPost]
        public ActionResult Delete(int  id)
        {
            DbHelper.Delete(id);
            return Json(new { result = "OK", msg = "" });
        }


    }
}
