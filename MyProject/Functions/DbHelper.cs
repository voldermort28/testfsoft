using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using DocumentFormat.OpenXml.Math;
using FastMember;
using Kendo.Mvc.Extensions;
using MyProject.Models;

namespace MyProject.Functions
{
    /// <summary>
    /// Summary description for DbHelper
    /// </summary>
    public static class DbHelper
    {

        public static DataTable GetRooms()
        {
            var table = new DataTable();
            using (var db = new QLNHEntities())
            {
                var data = db.DMBans.Where(x => x.TrangThai).Select(x => new
                {
                    TenBan = x.TenBan,
                    BanID = x.BanID,                   
                }).ToList();
                using (var reader = ObjectReader.Create(data))
                {
                    table.Load(reader);
                }

            }
            return table;

            //SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM [Room] where ParrentId is not null order by [RoomName]", ConfigurationManager.ConnectionStrings["daypilot"].ConnectionString);
            //DataTable dt = new DataTable();
            //da.Fill(dt);

            //return dt;
        }

        public static IEnumerable<SelectListItem> GetRoomSelectList()
        {
            return
                GetRooms().AsEnumerable().Select(u => new SelectListItem
                {
                    Value = Convert.ToString(u.Field<int>("BanID")),
                    Text = u.Field<string>("TenBan")
                });
        }

        public static DataRow GetReservation(int id)
        {
            var table = new DataTable();
            using (var db = new QLNHEntities())
            {
                var data = db.Reservations.Where(x => x.Status && x.ReservationId == id).Select(x => new
                {
                    ReservationStart = x.ReservationStart,
                    ReservationEnd = x.ReservationEnd,
                    ReservationId = x.ReservationId,
                    ReservationName = x.ReservationName,
                    BanID = x.BanID,
                    TrangThai = x.Status ? 1 : 0
                }).ToList();
                using (var reader = ObjectReader.Create(data))
                {
                    table.Load(reader);
                }

            }
            if (table.Rows.Count > 0)
            {
                return table.Rows[0];
            }
            return null;

            //SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM [Reservation] WHERE [ReservationId] = @id", ConfigurationManager.ConnectionStrings["daypilot"].ConnectionString);
            //da.SelectCommand.Parameters.AddWithValue("id", id);

            //DataTable dt = new DataTable();
            //da.Fill(dt);

            //if (dt.Rows.Count > 0)
            //{
            //    return dt.Rows[0];
            //}
            //return null;
        }

        public static DataTable GetReservations()
        {
            var table = new DataTable();
            using (var db = new QLNHEntities())
            {
                var data = db.Reservations.Where(x => x.Status).Select(x => new
                {
                    ReservationStart = x.ReservationStart,
                    ReservationEnd = x.ReservationEnd,
                    ReservationId = x.ReservationId,
                    ReservationName = x.ReservationName,
                    ReservationPaid  = x.ReservationPaid,
                    BanID = x.BanID,
                    TrangThai = x.Status ? 1 : 0
                }).ToList();
                using (var reader = ObjectReader.Create(data))
                {
                    table.Load(reader);
                }

            }
            return table;

            //SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM [Reservation]", ConfigurationManager.ConnectionStrings["daypilot"].ConnectionString);

            //DataTable dt = new DataTable();
            //da.Fill(dt);

            //return dt;
        }




        public static void MoveReservation(int id, DateTime start, DateTime end, int resource)
        {

            var table = new DataTable();
            using (var db = new QLNHEntities())
            {
                var objexist = db.Reservations.FirstOrDefault(x => x.ReservationId == id);
                if (objexist != null)
                {
                    objexist.ReservationStart = start;
                    objexist.ReservationEnd = end;
                    objexist.BanID = resource;
                    objexist.ReservationStart = start;
                    db.SaveChanges();
                }
            }             
        }

        public static void CreateReservation(DateTime start, DateTime end, int resource, string name)
        {
            var table = new DataTable();
            using (var db = new QLNHEntities())
            {
                var obj = new Reservation();
                obj.ReservationStart = start;
                obj.ReservationEnd = end;
                obj.BanID = resource;
                obj.ReservationName = name;
                obj.ReservationStatus = 0;
                obj.Status = true;             
                db.Reservations.Add(obj);
                db.SaveChanges();
            }
            //return table;


            //using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["daypilot"].ConnectionString))
            //{
            //    con.Open();
            //    SqlCommand cmd = new SqlCommand("INSERT INTO [Reservation] ([ReservationStart], [ReservationEnd], [RoomId], [ReservationName], [ReservationStatus]) VALUES (@start, @end, @resource, @name, 0)", con);
            //    cmd.Parameters.AddWithValue("start", start);
            //    cmd.Parameters.AddWithValue("end", end);
            //    cmd.Parameters.AddWithValue("resource", resource);
            //    cmd.Parameters.AddWithValue("name", name);
            //    cmd.ExecuteNonQuery();
            //}
        }

        public static void UpdateReservation(int id, string name, DateTime start, DateTime end, int resource, int status, int paid)
        {
            var table = new DataTable();
            using (var db = new QLNHEntities())
            {
                var objexist = db.Reservations.FirstOrDefault(x => x.ReservationId == id);
                if (objexist != null)
                {
                    objexist.ReservationStart = start;
                    objexist.ReservationEnd = end;
                    objexist.ReservationName = name;
                    objexist.BanID = resource;
                    objexist.ReservationStatus = status;
                    objexist.ReservationPaid = paid;
                    db.SaveChanges();
                }
            }

            //using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["daypilot"].ConnectionString))
            //{
            //    con.Open();
            //    SqlCommand cmd = new SqlCommand("UPDATE [Reservation] SET [ReservationStart] = @start, [ReservationEnd] = @end, [RoomId] = @resource, [ReservationName] = @name, [ReservationStatus] = @status, [ReservationPaid] = @paid WHERE [ReservationId] = @id", con);
            //    cmd.Parameters.AddWithValue("id", id);
            //    cmd.Parameters.AddWithValue("start", start);
            //    cmd.Parameters.AddWithValue("end", end);
            //    cmd.Parameters.AddWithValue("name", name);
            //    cmd.Parameters.AddWithValue("resource", resource);
            //    cmd.Parameters.AddWithValue("status", status);
            //    cmd.Parameters.AddWithValue("paid", paid);
            //    cmd.ExecuteNonQuery();
            //}

        }

        public static DataTable GetRoomsFiltered(string roomFilter)
        {
            SqlDataAdapter da = new SqlDataAdapter("SELECT [RoomId], [RoomName], [RoomStatus], [RoomSize] FROM [Room] WHERE RoomSize = @beds or @beds = '0'", ConfigurationManager.ConnectionStrings["daypilot"].ConnectionString);
            da.SelectCommand.Parameters.AddWithValue("beds", roomFilter);
            DataTable dt = new DataTable();
            da.Fill(dt);

            return dt;
        }

        public static bool IsFree(int id, DateTime start, DateTime end, int resource)
        {
            int count = 0;
            using (var db = new QLNHEntities())
            {
                count = db.Reservations.Count(x => !(x.ReservationEnd <= start || x.ReservationStart >= end) && x.BanID == resource && x.ReservationId == id);
               
            } 
            return count == 0;
        }

        public static DataTable getParent()
        {
            var table = new DataTable();
            using (var db = new QLNHEntities())
            {
                var objs = db.DMPhongs.Select(x => new
                {
                    PhongID = x.PhongID,
                    MaTang = x.MaTang,
                    MaPhong = x.MaPhong,
                    TenPhong = x.TenPhong 
                }).ToList();


                using (var reader = ObjectReader.Create(objs))
                {
                    table.Load(reader);
                }

            }
            return table;
 


            //var da = new SqlDataAdapter("SELECT * FROM [Room] WHERE ParrentId is null order by ParrentId", ConfigurationManager.ConnectionStrings["daypilot"].ConnectionString);            
            //var dt = new DataTable();
            //da.Fill(dt);
            //return dt;


        }

        public static DataTable GetRoomsByParrent(string parentId)
        {

            var table = new DataTable();
            using (var db = new QLNHEntities())
            {
                var data = db.DMBans.Where(x => x.MaPhong == parentId).Select(
                x => new
                {
                    MaPhong = x.MaPhong,
                    MaBan = x.MaBan,
                    TenBan = x.TenBan,
                    BanID   = x.BanID

                }).ToList();
                using (var reader = ObjectReader.Create(data))
                {
                    table.Load(reader);
                }

            }
            return table;

            //SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM [Room] WHERE  ParrentId = @parentId ", ConfigurationManager.ConnectionStrings["daypilot"].ConnectionString);
            //da.SelectCommand.Parameters.AddWithValue("parentId", parentId);
            //DataTable dt = new DataTable();
            //da.Fill(dt);
            //return dt;
        }

        public static bool     Delete(int id)
        {
              var table = new DataTable();
            using (var db = new QLNHEntities())
            {
                var objexist = db.Reservations.FirstOrDefault(x => x.ReservationId == id);
                if (objexist != null)
                {
                    objexist.Status = false;
                    db.SaveChanges();
                }
            }
            //SqlDataAdapter da = new SqlDataAdapter("DELETE [Reservation] WHERE [ReservationId] = @id", ConfigurationManager.ConnectionStrings["daypilot"].ConnectionString);
            //da.SelectCommand.Parameters.AddWithValue("id", id);
            //var dt = new DataTable();
            //da.Fill(dt);
            return true;
        }

        public static DataTable ToDataTable<T>(this IList<T> data)
        {
            PropertyDescriptorCollection props =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }



    }

}
