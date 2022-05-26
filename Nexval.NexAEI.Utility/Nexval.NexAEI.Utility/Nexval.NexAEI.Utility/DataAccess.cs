using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace Nexval.NexAEI.Utility
{
    public static class DataAccess
    {
        /// <summary>
        /// GetCheckinOutInfoDemo
        /// </summary>
        /// <param name="configInfo"></param>
        /// <returns></returns>
        public static List<AttendenceInfo> GetCheckinOutInfoDemo(ConfigurationInfo configInfo)
        {
            var allentlist = new List<AttendenceInfo>();

            int epoch = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            long unixTime = Utility.ConvertToUnixTime(DateTime.Now);
            var a = new AttendenceInfo
            {
                ClientUserAgent = "android",
                AttndType = "CHECKIN",
                Purpose = "CheckIn",
                // UserID = "176",
                UserEmail = "soumit.roychowdhury@nexval.com",
                EmployeeID = "1303",
                EmployeeName = "Soumit Roy Chowdhury",
                ClientIP = "192.168.165.39",
                Location = "Kolkata,NewTown",
                // QRCodeInput = $"CHECKIN:176:soumit.roychowdhury@nexval.com:100.101.23.0:1303::Soumit Roy Chowdhury",
                TimeZone = "Asia/Calcutta",
                TimeZoneDiff = "19800",
                LocalTime = unixTime.ToString()
            };
            allentlist.Add(a);
            return allentlist;
        }

        /// <summary>
        /// GetCheckinOutInfo
        /// </summary>
        /// <param name="configInfo"></param>
        /// <returns></returns>
        public static List<AttendenceInfo> GetCheckinOutInfo(ConfigurationInfo configInfo)
        {

            var allentlist = new List<AttendenceInfo>();
            try
            {
                DataSet ds = GetRecordDataSetCommon(configInfo);

                if (ds.Tables.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        AttendenceInfo attent = new AttendenceInfo();

                        try
                        {
                            attent.ClientUserAgent = dr.Field<string>("DeviceName");
                            attent.AttndType = dr.Field<string>("AttentType");
                            attent.Purpose = dr.Field<string>("Purpose");
                            attent.UserEmail = dr.Field<string>("UserEmail");
                            attent.EmployeeID = dr.Field<string>("EmployeeID");
                            attent.EmployeeName = dr.Field<string>("EmployeeName");
                            attent.ClientIP = dr.Field<string>("ClientIP");
                            attent.Location = dr.Field<string>("Location");
                            attent.TimeZone = dr.Field<string>("TimeZone");
                            attent.TimeZoneDiff = dr.Field<string>("TimeZoneDiff");
                            //  DateTime? entryTime = dr.Field<DateTime?>("EntryTime");
                            DateTime? entryTime = ParseDate(dr);
                            if (entryTime.HasValue)
                            {
                                var dt = Convert.ToDateTime(entryTime);
                                var epoach = Utility.ConvertToUnixTimestamp(dt);
                                attent.LocalTime = epoach.ToString();

                            }
                            attent.DeviceLogId = dr.Field<int>("MaxDeviceLogId");

                            if (configInfo.StartIndexId <= 0)
                            {
                                configInfo.StartIndexId = attent.DeviceLogId;
                            }

                            if (configInfo.LastIndexId < attent.DeviceLogId)
                            {
                                configInfo.LastIndexId = attent.DeviceLogId;
                            }

                            if (configInfo.StartIndexId > attent.DeviceLogId)
                            {
                                configInfo.StartIndexId = attent.DeviceLogId;
                            }

                            //  var qrcode = $"{attent.AttndType}:{ attent.UserID}:{attent.UserEmail}:{ attent.ClientIP}::{attent.EmployeeID}:{attent.EmployeeName}";
                            // attent.QRCodeInput = qrcode;
                        }
                        catch (Exception ex)
                        {
                            // Console.Write(ex.Message);
                            Utility.LogMessage(configInfo, ex.Message);
                        }

                        allentlist.Add(attent);
                    }
                }
            }
            catch (Exception exp)
            {
                //Console.Write(exp.Message);
                Utility.LogMessage(configInfo, exp.Message);
            }

            return allentlist;
        }

        public static void UpdateTracker(ConfigurationInfo configInfo)
        {
            try
            {
                //get the information out of the configuration file.
                ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings["TrackerConnectionString"];
                //get the proper factory 
                DbProviderFactory factory = DbProviderFactories.GetFactory(connectionStringSettings.ProviderName);
                //create a command of the proper type.
                DbConnection conn = factory.CreateConnection();
                //set the connection string
                conn.ConnectionString = connectionStringSettings.ConnectionString;
                //open the connection
                conn.Open();

                var lastDateTime = configInfo.LastDateTime.AddDays(1);
                using (DbCommand cmd = conn.CreateCommand())
                {
                    var str = $"UPDATE TRACKER SET StartDateTime='{configInfo.LastDateTime}', LastDateTime='{lastDateTime}',StartIndexId={configInfo.StartIndexId},LastIndexId={configInfo.LastIndexId} ";

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = str;
                    cmd.ExecuteNonQuery();

                }
            }
            catch (Exception ex)
            {
                Utility.LogMessage(configInfo, ex.Message);
            }
        }

        public static void InsertLog(ConfigurationInfo configInfo, string methodName, string message)
        {
            try
            {
                //get the information out of the configuration file.
                ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings["TrackerConnectionString"];
                //get the proper factory 
                DbProviderFactory factory = DbProviderFactories.GetFactory(connectionStringSettings.ProviderName);
                //create a command of the proper type.
                DbConnection conn = factory.CreateConnection();
                //set the connection string
                conn.ConnectionString = connectionStringSettings.ConnectionString;
                //open the connection
                conn.Open();

                message = message.Replace("'", "");

                using (DbCommand cmd = conn.CreateCommand())
                {
                    var str = $"INSERT INTO PROCESSLoGS (MethodName,Message) VALUES('{ methodName}','{message}')";

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = str;
                    cmd.ExecuteNonQuery();

                }
            }
            catch (Exception ex)
            {
                Utility.LogMessage(configInfo, ex.Message);
            }
        }
        public static void ClearLog(ConfigurationInfo configInfo)
        {
            try
            {
                //get the information out of the configuration file.
                ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings["TrackerConnectionString"];
                //get the proper factory 
                DbProviderFactory factory = DbProviderFactories.GetFactory(connectionStringSettings.ProviderName);
                //create a command of the proper type.
                DbConnection conn = factory.CreateConnection();
                //set the connection string
                conn.ConnectionString = connectionStringSettings.ConnectionString;
                //open the connection
                conn.Open();

               

                using (DbCommand cmd = conn.CreateCommand())
                {
                    var str = $"Delete * from PROCESSlogs WHERE DateAdd('m',2,LogDate) < Date()";

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = str;
                    cmd.ExecuteNonQuery();

                }
            }
            catch (Exception ex)
            {
                Utility.LogMessage(configInfo, ex.Message);
            }
        }
        private static DateTime? ParseDate(DataRow dr)
        {

            DateTime dt = DateTime.MinValue;
            DateTime.TryParse(Convert.ToString(dr["EntryTime"]), out dt);
            return dt;
        }
        public static DataSet GetRecordDataSetCommon(ConfigurationInfo configInfo)
        {

            DataSet ds = new DataSet("Attendence");
            try
            {
               // var sql = GetSQlString(configInfo);
                //get the information out of the configuration file.
                ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings["DeviceConnectionString"];
                //get the proper factory 
                DbProviderFactory factory = DbProviderFactories.GetFactory(connectionStringSettings.ProviderName);
                //create a command of the proper type.
                DbConnection conn = factory.CreateConnection();
                //set the connection string
                conn.ConnectionString = connectionStringSettings.ConnectionString;
                //open the connection
                conn.Open();

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = configInfo.SqlScript;
                    cmd.CommandTimeout = 30;
                    using (DbDataAdapter da = factory.CreateDataAdapter())
                    {
                        DbCommandBuilder cb = factory.CreateCommandBuilder();
                        da.SelectCommand = cmd;
                        da.Fill(ds);
                        cb.DataAdapter = da;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                Utility.LogMessage(configInfo, ex.Message);
            }
            return ds;
        }

       
        public static DataSet GetLastLastSetDataSet(ConfigurationInfo configInfo)
        {
            DataSet ds = new DataSet("tracker");
            try
            {
                //get the information out of the configuration file.
                ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings["TrackerConnectionString"];
                //get the proper factory 
                DbProviderFactory factory = DbProviderFactories.GetFactory(connectionStringSettings.ProviderName);
                //create a command of the proper type.
                DbConnection conn = factory.CreateConnection();
                //set the connection string
                conn.ConnectionString = connectionStringSettings.ConnectionString;
                //open the connection
                conn.Open();

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT  TOP 1 Tracker.* FROM Tracker; ";
                    cmd.CommandTimeout = 30;
                    using (DbDataAdapter da = factory.CreateDataAdapter())
                    {
                        DbCommandBuilder cb = factory.CreateCommandBuilder();
                        da.SelectCommand = cmd;
                        da.Fill(ds);
                        cb.DataAdapter = da;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                Utility.LogMessage(configInfo, ex.Message);
            }
            return ds;
        }

        public static List<AttendenceInfo> GetCheckinOutInfo(ConfigurationInfo configInfo, DataSet ds)
        {

            var allentlist = new List<AttendenceInfo>();
            try
            {               

                if (ds.Tables.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        AttendenceInfo attent = new AttendenceInfo();

                        try
                        {
                            attent.ClientUserAgent = dr.Field<string>("DeviceName");
                            attent.AttndType = dr.Field<string>("AttentType");
                            attent.Purpose = dr.Field<string>("Purpose");
                            attent.UserEmail = dr.Field<string>("UserEmail");
                            attent.EmployeeID = dr.Field<string>("EmployeeID");
                            attent.EmployeeName = dr.Field<string>("EmployeeName");
                            attent.ClientIP = dr.Field<string>("ClientIP");
                            attent.Location = dr.Field<string>("Location");
                            attent.TimeZone = dr.Field<string>("TimeZone");
                            attent.TimeZoneDiff = dr.Field<string>("TimeZoneDiff");
                            //  DateTime? entryTime = dr.Field<DateTime?>("EntryTime");
                            DateTime? entryTime = ParseDate(dr);
                            if (entryTime.HasValue)
                            {
                                var dt = Convert.ToDateTime(entryTime);
                                var epoach = Utility.ConvertToUnixTimestamp(dt);
                                attent.LocalTime = epoach.ToString();

                            }
                            attent.DeviceLogId = dr.Field<int>("MaxDeviceLogId");

                            if (configInfo.StartIndexId <= 0)
                            {
                                configInfo.StartIndexId = attent.DeviceLogId;
                            }

                            if (configInfo.LastIndexId < attent.DeviceLogId)
                            {
                                configInfo.LastIndexId = attent.DeviceLogId;
                            }

                            if (configInfo.StartIndexId > attent.DeviceLogId)
                            {
                                configInfo.StartIndexId = attent.DeviceLogId;
                            }

                            //  var qrcode = $"{attent.AttndType}:{ attent.UserID}:{attent.UserEmail}:{ attent.ClientIP}::{attent.EmployeeID}:{attent.EmployeeName}";
                            // attent.QRCodeInput = qrcode;
                        }
                        catch (Exception ex)
                        {
                            // Console.Write(ex.Message);
                            Utility.LogMessage(configInfo, ex.Message);
                        }

                        allentlist.Add(attent);
                    }
                }
            }
            catch (Exception exp)
            {
                //Console.Write(exp.Message);
                Utility.LogMessage(configInfo, exp.Message);
            }

            return allentlist;
        }

        /*
          private static DataSet GetRecordDataSet(ConfigurationInfo configInfo)
        {

            DataSet ds = new DataSet("Attendence");
            try
            {
                using (SqlConnection conn = new SqlConnection(configInfo.ConnectionString))
                {
                    SqlCommand sqlComm = new SqlCommand(configInfo.SqlScript, conn);
                    sqlComm.CommandType = CommandType.Text;

                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = sqlComm;
                    da.Fill(ds);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return ds;
        } 
         */

    }
}
