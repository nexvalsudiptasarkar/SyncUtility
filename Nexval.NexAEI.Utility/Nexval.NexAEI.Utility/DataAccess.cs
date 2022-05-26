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
                //Utility.LogMessage(configInfo, "GetRecordDataSetCommon started");

                DataSet ds = GetRecordDataSetCommon(configInfo);

                //Utility.LogMessage(configInfo, "GetRecordDataSetCommon completed");

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
                            attent.DeviceLogId= dr.Field<int>("DeviceLogId");
                            attent.IndexNo = dr.Field<int>("IndexNo");
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
                Utility.LogMessage(configInfo, "Method name GetCheckinOutInfo()" + " " + exp.Message);
            }
            //Utility.LogMessage(configInfo, "GetCheckinOutInfo list filling completed");
            return allentlist;
        }

        public static void UpdateLog(List<AttendenceInfo> attendenceList, ConfigurationInfo configInfo)
        {
            try
            {
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
                    foreach (var atten in attendenceList)
                    {

                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "UPDATE NexAEIPush SET DeviceLogId=0," + "IndexNo =" + atten.IndexNo;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch(Exception ex)
            {
                Utility.LogMessage( configInfo,ex.Message);
            }
        }
        public static void UpdateLogPrev(List<AttendenceInfo> attendenceList, ConfigurationInfo configInfo)
        {
            try
            {
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
                    foreach (var atten in attendenceList)
                    {

                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "UPDATE NexAEIPushPrevious SET DeviceLogId=0," + "IndexNo =" + atten.IndexNo;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.LogMessage(configInfo, ex.Message);
            }
        }

        public static List<LogData> ReadData(ConfigurationInfo configInfo)
        {
            try
            {

                List<LogData> iLogData = new List<LogData>();
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
                    cmd.CommandText = $"SELECT DeviceLogId, IndexNo,TimeStamp FROM NexAEIPush";
                    DbDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            LogData lgData = new LogData();

                            lgData.DeviceLogId = (reader["DeviceLogId"]!=null? Convert.ToInt32(reader["DeviceLogId"]):default(int));
                            lgData.IndexNo = (reader["IndexNo"] != null ? Convert.ToInt32(reader["IndexNo"]) : default(int));
                            lgData.timeStamp = (reader["timeStamp"] != null ? Convert.ToDateTime(reader["timeStamp"]):DateTime.Now);

                            iLogData.Add(lgData);
                        }
                    }
                    else
                    {
                        Console.WriteLine("No rows found.");
                    }
                        reader.Close();
                }
                return iLogData;
            }
            catch (Exception ex)
            {
                Utility.LogMessage(configInfo, ex.Message);
            }
            return null;
        }

        private static DateTime? ParseDate(DataRow dr)
        {

            DateTime dt = DateTime.MinValue;
            DateTime.TryParse(Convert.ToString(dr["EntryTime"]), out dt);
            return dt;
        }
        private static DataSet GetRecordDataSetCommon(ConfigurationInfo configInfo)
        {
            DataSet ds = new DataSet("Attendence");
            try
            {
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

                //Utility.LogMessage(configInfo, "Db Connection established");

                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.CommandText = "NexaeiAttendanceDataSync"; //configInfo.SqlScript;
                    DateTime dateForAttn = DateTime.Now;
                    dateForAttn = dateForAttn.AddDays(-10);

                    cmd.Parameters.Add(dateForAttn);
                    cmd.CommandTimeout = 30;

                    using (DbDataAdapter da = factory.CreateDataAdapter())
                    {
                        DbCommandBuilder cb = factory.CreateCommandBuilder();
                        da.SelectCommand = cmd;
                        da.Fill(ds);
                        cb.DataAdapter = da;
                    }
                }
                //Utility.LogMessage(configInfo, "Data filling completed");
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                Utility.LogMessage(configInfo, "Method name: GetRecordDataSetCommon()" + " " + ex.Message);
            }
            return ds;
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
