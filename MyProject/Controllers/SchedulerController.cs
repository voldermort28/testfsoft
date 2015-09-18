using System;
using System.Data;
using System.Drawing;
using System.Web.Mvc;
using DayPilot.Web.Mvc;
using DayPilot.Web.Mvc.Data;
using DayPilot.Web.Mvc.Enums;
using DayPilot.Web.Mvc.Events.Scheduler;
using MyProject.Functions;

namespace MyProject.Controllers
{


    public class SchedulerController : Controller
    {
        private static DateTime dateChange;

        public ActionResult Backend()
        {
            Console.WriteLine("start");
            return new Scheduler().CallBack(this);

        }

        [HttpPost]
        public ActionResult ChangeDate(DateTime date)
        {
            dateChange = date;
            return Json(new { result = "OK", msg = "" });
        }



        class Scheduler : DayPilotScheduler
        {
            protected override void OnInit(InitArgs e)
            {
                LoadInit();
            }

            private void LoadInit(int changeDate= 0)
            {
                DateTime start = DateTime.Now.AddDays(-3);
                DateTime end = DateTime.Now.AddDays(3);

                Timeline = new TimeCellCollection();
                for (DateTime cell = start; cell < end; cell = cell.AddDays(1))
                {
                   // Timeline.Add(cell, cell.AddDays(1));
                }

                Timeline.Add(DateTime.Now, DateTime.Now.AddDays(1));

                LoadRoomsAndReservations();
                ScrollTo(DateTime.Today.AddDays(-1));
                Separators.Add(changeDate == 0 ? DateTime.Now.AddDays(1) : dateChange, Color.Red);

                UpdateWithMessage("Welcome!", CallBackUpdateType.Full);
            }

            private void LoadRoomsAndReservations()
            {
                LoadRooms();
                LoadReservations();
            }

            private void LoadReservations()
            {
                Events = Db.GetReservations().Rows;

                DataStartField = "ReservationStart";
                DataEndField = "ReservationEnd";
                DataIdField = "ReservationId";
                DataTextField = "ReservationName";
                DataResourceField = "RoomId";

                DataTagFields = "ReservationStatus";

            }

            private void LoadRooms()
            {
                Resources.Clear();
                DataTable parent = Db.getParent();
                foreach (DataRow r in parent.Rows)
                {
                    Resource res = new Resource((string)r["RoomName"],null , false);
                    res.Expanded = true;
                    Resources.Add(res);

                    // get childer
                    DataTable dtChilder = Db.GetRoomsByChilder(Convert.ToInt32(r["RoomId"]));
                    foreach (DataRow rows in dtChilder.Rows)
                    {
                        //Resource resChilder = new Resource((string) r["RoomName"], Convert.ToString(r["RoomId"]), true);
                        //string status = (string)rows["RoomStatus"];
                        //int beds = Convert.ToInt32(rows["RoomSize"]);
                        //string bedsFormatted = (beds == 1) ? "1 bed" : String.Format("{0} beds", beds);
                        //resChilder.DataItem = rows;
                        //resChilder.Columns.Add(new ResourceColumn(bedsFormatted));
                        //resChilder.Columns.Add(new ResourceColumn(status));

                        res.Children.Add((string)rows["RoomName"], Convert.ToString(rows["RoomId"]));                        
                    }

                }
                //string roomFilter = "0";
                //if (ClientState["filter"] != null)
                //{
                //    roomFilter = (string)ClientState["filter"]["room"];
                //}

                //DataTable dt = Db.GetRoomsFiltered(roomFilter);

                //foreach (DataRow r in dt.Rows)
                //{
                //    string name = (string)r["RoomName"];
                //    string id = Convert.ToString(r["RoomId"]);
                //    string status = (string)r["RoomStatus"];
                //    int beds = Convert.ToInt32(r["RoomSize"]);
                //    string bedsFormatted = (beds == 1) ? "1 bed" : String.Format("{0} beds", beds);

                //    Resource res = new Resource(name, id);
                //    res.DataItem = r;
                //    res.Columns.Add(new ResourceColumn(bedsFormatted));
                //    res.Columns.Add(new ResourceColumn(status));

                //    Resources.Add(res);


                //}
            }

            protected override void OnEventMove(EventMoveArgs e)
            {
                string id = e.Id;
                DateTime start = e.NewStart;
                DateTime end = e.NewEnd;
                string resource = e.NewResource;

                string message = null;
                if (!Db.IsFree(id, start, end, resource))
                {
                    message = "The reservation cannot overlap with an existing reservation.";
                }
                else if (e.OldEnd <= DateTime.Today)
                {
                    message = "This reservation cannot be changed anymore.";
                }
                else if (e.NewStart < DateTime.Today)
                {
                    message = "The reservation cannot be moved to the past.";
                }
                else
                {
                    Db.MoveReservation(e.Id, e.NewStart, e.NewEnd, e.NewResource);
                }
                
                LoadReservations();
                UpdateWithMessage(message);
            }

            protected override void OnEventResize(EventResizeArgs e)
            {
                Db.MoveReservation(e.Id, e.NewStart, e.NewEnd, e.Resource);
                LoadReservations();
                Update();
            }

            protected override void OnBeforeEventRender(BeforeEventRenderArgs e)
            {
                e.Html = String.Format("{0} ({1:d} - {2:d})", e.Text, e.Start, e.End);
                int status = Convert.ToInt32(e.Tag["ReservationStatus"]);

                switch (status)
                {
                    case 0: // new
                        if (e.Start < DateTime.Today.AddDays(2)) // must be confirmed two day in advance
                        {
                            e.DurationBarColor = "red";
                            e.ToolTip = "Expired (not confirmed in time)";
                        }
                        else
                        {
                            e.DurationBarColor = "orange";
                            e.ToolTip = "New";
                        }
                        break;
                    case 1:  // confirmed
                        if (e.Start < DateTime.Today || (e.Start == DateTime.Today && DateTime.Now.TimeOfDay.Hours > 18))  // must arrive before 6 pm
                        {
                            e.DurationBarColor = "#f41616";  // red
                            e.ToolTip = "Late arrival";
                        }
                        else
                        {
                            e.DurationBarColor = "green";
                            e.ToolTip = "Confirmed";
                        }
                        break;
                    case 2: // arrived
                        if (e.End < DateTime.Today || (e.End == DateTime.Today && DateTime.Now.TimeOfDay.Hours > 11))  // must checkout before 10 am
                        {
                            e.DurationBarColor = "#f41616"; // red
                            e.ToolTip = "Late checkout";
                        }
                        else
                        {
                            e.DurationBarColor = "#1691f4";  // blue
                            e.ToolTip = "Arrived";
                        }
                        break;
                    case 3: // checked out
                        e.DurationBarColor = "gray";
                        e.ToolTip = "Checked out";
                        break;
                    default:
                        throw new ArgumentException("Unexpected status.");
                }

                e.Html = e.Html + String.Format("<br /><span style='color:gray'>{0}</span>", e.ToolTip);
                int paid = 0;
                if (e.DataItem["ReservationPaid"] != DBNull.Value)
                {
                    paid = Convert.ToInt32(e.DataItem["ReservationPaid"]);
                }
                
                string paidColor = "#aaaaaa";

                e.Areas.Add(new Area().Bottom(10).Right(4).Html("<div style='color:" + paidColor + "; font-size: 8pt;'>Paid: " + paid + "%</div>").Visible());
                e.Areas.Add(new Area().Left(4).Bottom(8).Right(4).Height(2).Html("<div style='background-color:" + paidColor + "; height: 100%; width:" + paid + "%'></div>").Visible());
            }

            protected override void OnBeforeResHeaderRender(BeforeResHeaderRenderArgs e)
            {
                string status = string.Empty;
                if (e.DataItem.Source != null)
                {
                    status = (string)e.DataItem["RoomStatus"];    
                }
                
                switch (status)
                {
                    case "Dirty":
                        e.CssClass = "status_dirty";
                        break;
                    case "Cleanup":
                        e.CssClass = "status_cleanup";
                        break;
                }
            }


            protected override void OnCommand(CommandArgs e)
            {
                switch (e.Command)
                {
                    case "refresh":
                        LoadReservations();
                        UpdateWithMessage("Refreshed");
                        break;
                    case "changeDate":
                        LoadInit(1);
                        LoadReservations();
                        UpdateWithMessage("Refreshed");
                        break;
                    case "filter":
                        LoadRoomsAndReservations();
                        UpdateWithMessage("Updated", CallBackUpdateType.Full);
                        break;
                }
            }


        }

    }

}
