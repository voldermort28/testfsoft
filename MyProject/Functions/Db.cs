﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace MyProject.Functions
{
    /// <summary>
    /// Summary description for Db
    /// </summary>
    public static class Db
    {

        public static DataTable GetRooms()
        {
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM [Room] where ParrentId is not null order by [RoomName]", ConfigurationManager.ConnectionStrings["daypilot"].ConnectionString);
            DataTable dt = new DataTable();
            da.Fill(dt);

            return dt;
        }

        public static IEnumerable<SelectListItem> GetRoomSelectList()
        {
            return
                GetRooms().AsEnumerable().Select(u => new SelectListItem
                {
                    Value = Convert.ToString(u.Field<int>("RoomId")),
                    Text = u.Field<string>("RoomName")
                });
        }

        public static DataRow GetReservation(string id)
        {
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM [Reservation] WHERE [ReservationId] = @id", ConfigurationManager.ConnectionStrings["daypilot"].ConnectionString);
            da.SelectCommand.Parameters.AddWithValue("id", id);

            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                return dt.Rows[0];
            }
            return null;
        }

        public static DataTable GetReservations()
        {
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM [Reservation]", ConfigurationManager.ConnectionStrings["daypilot"].ConnectionString);

            DataTable dt = new DataTable();
            da.Fill(dt);

            return dt;
        }




        public static void MoveReservation(string id, DateTime start, DateTime end, string resource)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["daypilot"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("UPDATE [Reservation] SET [ReservationStart] = @start, [ReservationEnd] = @end, [RoomId] = @resource WHERE [ReservationId] = @id", con);
                cmd.Parameters.AddWithValue("id", id);
                cmd.Parameters.AddWithValue("start", start);
                cmd.Parameters.AddWithValue("end", end);
                cmd.Parameters.AddWithValue("resource", resource);
                cmd.ExecuteNonQuery();
            }
        }

        public static void CreateReservation(DateTime start, DateTime end, string resource, string name)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["daypilot"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO [Reservation] ([ReservationStart], [ReservationEnd], [RoomId], [ReservationName], [ReservationStatus]) VALUES (@start, @end, @resource, @name, 0)", con);
                cmd.Parameters.AddWithValue("start", start);
                cmd.Parameters.AddWithValue("end", end);
                cmd.Parameters.AddWithValue("resource", resource);
                cmd.Parameters.AddWithValue("name", name);
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateReservation(string id, string name, DateTime start, DateTime end, string resource, int status, int paid)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["daypilot"].ConnectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("UPDATE [Reservation] SET [ReservationStart] = @start, [ReservationEnd] = @end, [RoomId] = @resource, [ReservationName] = @name, [ReservationStatus] = @status, [ReservationPaid] = @paid WHERE [ReservationId] = @id", con);
                cmd.Parameters.AddWithValue("id", id);
                cmd.Parameters.AddWithValue("start", start);
                cmd.Parameters.AddWithValue("end", end);
                cmd.Parameters.AddWithValue("name", name);
                cmd.Parameters.AddWithValue("resource", resource);
                cmd.Parameters.AddWithValue("status", status);
                cmd.Parameters.AddWithValue("paid", paid);
                cmd.ExecuteNonQuery();
            }

        }

        public static DataTable GetRoomsFiltered(string roomFilter)
        {
            SqlDataAdapter da = new SqlDataAdapter("SELECT [RoomId], [RoomName], [RoomStatus], [RoomSize] FROM [Room] WHERE RoomSize = @beds or @beds = '0'", ConfigurationManager.ConnectionStrings["daypilot"].ConnectionString);
            da.SelectCommand.Parameters.AddWithValue("beds", roomFilter);
            DataTable dt = new DataTable();
            da.Fill(dt);

            return dt;
        }

        public static bool IsFree(string id, DateTime start, DateTime end, string resource)
        {
            // event with the specified id will be ignored

            SqlDataAdapter da = new SqlDataAdapter("SELECT count(ReservationId) as count FROM [Reservation] WHERE NOT (([ReservationEnd] <= @start) OR ([ReservationStart] >= @end)) AND RoomId = @resource AND ReservationId <> @id", ConfigurationManager.ConnectionStrings["daypilot"].ConnectionString);
            da.SelectCommand.Parameters.AddWithValue("id", id);
            da.SelectCommand.Parameters.AddWithValue("start", start);
            da.SelectCommand.Parameters.AddWithValue("end", end);
            da.SelectCommand.Parameters.AddWithValue("resource", resource);
            DataTable dt = new DataTable();
            da.Fill(dt);

            int count = Convert.ToInt32(dt.Rows[0]["count"]);
            return count == 0;
        }

        public static DataTable getParent()
        {
            var da = new SqlDataAdapter("SELECT * FROM [Room] WHERE ParrentId is null order by ParrentId", ConfigurationManager.ConnectionStrings["daypilot"].ConnectionString);            
            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public static DataTable GetRoomsByChilder(int parentId)
        {
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM [Room] WHERE  ParrentId = @parentId ", ConfigurationManager.ConnectionStrings["daypilot"].ConnectionString);
            da.SelectCommand.Parameters.AddWithValue("parentId", parentId);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public static bool     Delete(int id)
        {
            SqlDataAdapter da = new SqlDataAdapter("DELETE [Reservation] WHERE [ReservationId] = @id", ConfigurationManager.ConnectionStrings["daypilot"].ConnectionString);
            da.SelectCommand.Parameters.AddWithValue("id", id);
            var dt = new DataTable();
            da.Fill(dt);
            return true;
        }

    }

}
