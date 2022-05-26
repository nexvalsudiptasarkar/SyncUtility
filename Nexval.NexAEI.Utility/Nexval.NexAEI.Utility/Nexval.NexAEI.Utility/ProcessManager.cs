using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nexval.NexAEI.Utility
{
    public class ProcessManager
    {


        public string ExportDataToNexAei(ConfigurationInfo configInfo)
        {
            var respMessage = new StringBuilder();
            var returnMessage = "";
            
            try
            {
                configInfo.SqlScript = GetSQlString(configInfo);
                DataSet ds = DataAccess.GetRecordDataSetCommon(configInfo);
                List<AttendenceInfo> totalAttendanceList = DataAccess.GetCheckinOutInfo(configInfo, ds);



                if (totalAttendanceList.Count > 0)
                {

                   var batchList = SplitList<AttendenceInfo>(totalAttendanceList, 150);
                    int i = 0;
                   
                    foreach (List<AttendenceInfo> attendanceList in batchList)
                    {
                        i++;
                        Thread.Sleep(1000); //ms

                        try
                        {
                            BulkAttendancesInfo bulkAttendances = new BulkAttendancesInfo
                            {
                                OrgID = configInfo.OrganizationId,
                                AttendanceList = attendanceList

                            };

                            var record = new
                            {
                                BulkAttendances = bulkAttendances
                            };


                            CancellationTokenSource tokenSource = new CancellationTokenSource();
                            CancellationToken cancellationToken = tokenSource.Token;

                            Task<string> responseHttpMessage = PostStreamAsync(record, cancellationToken, configInfo.NexAEIApiUrl);
                            string responseMessage = responseHttpMessage.Result;

                            Utility.LogMessage(configInfo, $"Batch {i} Export Completed between  {configInfo.StartDateTime} and {configInfo.LastDateTime},Message:{responseMessage}", "ExportDataToAei");

                            respMessage.Append($"{responseMessage}");
                            returnMessage = $"Batch {i} Export Completed with response :{responseMessage} ";
                        }
                        catch (Exception ex)
                        {
                            Utility.LogMessage(configInfo, ex.ToString(), "ExportDataToNexAei");
                        }
                    }

                    Utility.LogMessage(configInfo, $"Export Completed between  {configInfo.StartDateTime} and {configInfo.LastDateTime},Message:{respMessage.ToString()}", "ExportDataToAei");

                    return respMessage.ToString();
                }
                {
                    string msg = $"No Records Found between  {configInfo.StartDateTime} and {configInfo.LastDateTime}";
                    Utility.LogMessage(configInfo, msg, "ExportDataToAei");
                    return msg;
                }
            }
            catch (Exception ex)
            {

                Utility.LogMessage(configInfo, ex.ToString(), "ExportDataToAei");

                return " Error: Not able to connect NexAEI";
            }

          
        }

        public ConfigurationInfo GetTrackerValues(ConfigurationInfo configInfo)
        {
            DateTime? startDate = DateTime.Now.Date.AddDays(-1);
            DateTime? endDate = DateTime.Now.Date;
            int? startIndexId = 0;
            int? lastIndexId = 0;
            try
            {
                DataSet dsTracker = DataAccess.GetLastLastSetDataSet(configInfo);


                if (dsTracker.Tables.Count > 0)
                {
                    if (dsTracker.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = dsTracker.Tables[0].Rows[0];
                        startDate = dr.Field<DateTime?>("StartDateTime");
                        endDate = dr.Field<DateTime?>("LastDateTime");
                        startIndexId = dr.Field<int?>("StartIndexId");
                        lastIndexId = dr.Field<int?>("LastIndexId");
                    }
                }
            }
            catch (Exception ex)
            {

                Utility.LogMessage(configInfo, ex.Message, "GetTrackerValues");

            }


            configInfo.StartDateTime = startDate.HasValue ? startDate.Value : DateTime.Now.Date.AddDays(-1);
            configInfo.LastDateTime = endDate.HasValue ? endDate.Value : DateTime.Now.Date;
            configInfo.LastIndexId = lastIndexId.HasValue ? lastIndexId.Value : 0;
            configInfo.StartIndexId = lastIndexId.HasValue ? startIndexId.Value : 0;

            return configInfo;
        }

        public static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }

        #region PrivateMethods
        private static string GetSQlString(ConfigurationInfo configInfo)
        {
            string sql = configInfo.SqlScript;

            string tableName = $"DeviceLogs_{configInfo.StartDateTime.Month}_{configInfo.StartDateTime.Year}";
            configInfo.AttendanceTableName = tableName;

            sql = sql.Replace("[@#@Table@#@]", tableName);

            sql = sql.Replace("[@#@DateFrom@#@]", $"'{configInfo.StartDateTime}'");

            sql = sql.Replace("[@#@DateTo@#@]", $"'{configInfo.LastDateTime}'");

            sql = sql.Replace("[@#@StartIndexId@#@]", $"{configInfo.StartIndexId}");
            sql = sql.Replace("[@#@LastIndexId@#@]", $"{configInfo.LastIndexId}");
            return sql;
        }

        private static async Task<string> PostStreamAsync(object content, CancellationToken cancellationToken, string url)
        {
            using (HttpClient client = new HttpClient())
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url))
            using (HttpContent httpContent = CreateHttpContent(content))
            {
                request.Content = httpContent;

                using (HttpResponseMessage responseHttpMessage = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
                {
                    responseHttpMessage.EnsureSuccessStatusCode();

                    if (responseHttpMessage.IsSuccessStatusCode)
                    {


                        if (responseHttpMessage.StatusCode == HttpStatusCode.NoContent)
                        {
                            return string.Empty;
                        }
                    }

                    string responseContentString = await responseHttpMessage.Content.ReadAsStringAsync();
                    return responseContentString;
                }
            }
        }
        private static async Task PostJsonStreamAsync(object content, CancellationToken cancellationToken, string url)
        {
            using (HttpClient client = new HttpClient())
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url))
            using (HttpContent httpContent = CreateHttpContent(content))
            {
                request.Content = httpContent;

                using (HttpResponseMessage response = await client
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                    .ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                }
            }
        }
        private static void SerializeJsonIntoStream(object value, Stream stream)
        {
            using (StreamWriter sw = new StreamWriter(stream, new UTF8Encoding(false), 1024, true))
            using (JsonTextWriter jtw = new JsonTextWriter(sw) { Formatting = Formatting.None })
            {
                JsonSerializer js = new JsonSerializer();
                js.Serialize(jtw, value);
                jtw.Flush();
            }
        }
        private static HttpContent CreateHttpContent(object content)
        {
            HttpContent httpContent = null;

            if (content != null)
            {
                MemoryStream ms = new MemoryStream();
                SerializeJsonIntoStream(content, ms);
                ms.Seek(0, SeekOrigin.Begin);
                httpContent = new StreamContent(ms);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            return httpContent;
        }
        #endregion

        #region NotUsed
        /*
          public string ExportDataToAei(ConfigurationInfo configInfo)
        {
            try
            {
                var attendanceList = DataAccess.GetCheckinOutInfo(configInfo);

                if (attendanceList.Count > 0)
                {

                    var bulkAttendances = new BulkAttendancesInfo
                    {
                        OrgID = configInfo.OrganizationId,
                        AttendanceList = attendanceList

                    };

                    var record = new
                    {
                        BulkAttendances = bulkAttendances
                    };


                    // var responseMessage = client.PostAsync(configInfo.NexAEIApiUrl, content).Result;

                    CancellationTokenSource tokenSource = new CancellationTokenSource();
                    var cancellationToken = tokenSource.Token;

                    var responseHttpMessage = PostStreamAsync(record, cancellationToken, configInfo.NexAEIApiUrl);
                    var responseMessage = responseHttpMessage.Result;

                    if (responseMessage.StatusCode == HttpStatusCode.NoContent)
                    {
                        return string.Empty;
                    }


                    if (responseMessage.IsSuccessStatusCode)
                    {
                        //DataAccess.UpdateTracker(configInfo);
                        Utility.LogMessage(configInfo, $"Export Completed between  {configInfo.StartDateTime} and {configInfo.LastDateTime}", "ExportDataToAei");
                        return responseMessage.ReasonPhrase;
                    }


                }
                {
                    Utility.LogMessage(configInfo, $"No Records Found between  {configInfo.StartDateTime} and {configInfo.LastDateTime}", "ExportDataToAei");
                }
            }
            catch (Exception ex)
            {

                Utility.LogMessage(configInfo, ex.Message, "ExportDataToAei");

            }

            return string.Empty;
        }
         */
        /*
          public async Task<string> ExportData(ConfigurationInfo configInfo)
       {

           var attendenceList = DataAccess.GetCheckinOutInfo(configInfo);
           var bulkAttendancesInfo = new BulkAttendancesInfo
           {
               AttendanceList = attendenceList
           };

           var bulkAttendancesInfoString = JsonConvert.SerializeObject(bulkAttendancesInfo, Newtonsoft.Json.Formatting.Indented);
           var record = new CheckinOutInfo
           {
               BulkAttendances = bulkAttendancesInfoString,
               OrgID = configInfo.OrganizationId
           };

           using (var client = new HttpClient())
           {
               var json = JsonConvert.SerializeObject(record, Newtonsoft.Json.Formatting.Indented);
               var content = new StringContent(json, Encoding.UTF8, "application/json");

               var responseMessage = client.PostAsync(configInfo.NexAEIApiUrl, content).Result;

               if (responseMessage.StatusCode == HttpStatusCode.NoContent)
               {
                   return string.Empty;
               }

               var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
               if (responseMessage.IsSuccessStatusCode)
               {
                   return response;
               }

               return string.Empty;
           }
       }
         */

        /*
             public string ManualExportDataToAei(ConfigurationInfo configInfo)
        {
            try
            {
                configInfo.SqlScript = GetSQlString(configInfo);
                DataSet ds = DataAccess.GetRecordDataSetCommon(configInfo);
                var attendanceList = DataAccess.GetCheckinOutInfo(configInfo, ds);

                if (attendanceList.Count > 0)
                {

                    var bulkAttendances = new BulkAttendancesInfo
                    {
                        OrgID = configInfo.OrganizationId,
                        AttendanceList = attendanceList

                    };

                    var record = new
                    {
                        BulkAttendances = bulkAttendances
                    };


                    // var responseMessage = client.PostAsync(configInfo.NexAEIApiUrl, content).Result;

                    CancellationTokenSource tokenSource = new CancellationTokenSource();
                    var cancellationToken = tokenSource.Token;

                    var responseHttpMessage = PostStreamAsync(record, cancellationToken, configInfo.NexAEIApiUrl);
                    var responseMessage = responseHttpMessage.Result;

                    if (responseMessage.StatusCode == HttpStatusCode.NoContent)
                    {
                        return string.Empty;
                    }


                    if (responseMessage.IsSuccessStatusCode)
                    {
                        //DataAccess.UpdateTracker(configInfo);
                        Utility.LogMessage(configInfo, $"Export Completed between  {configInfo.StartDateTime} and {configInfo.LastDateTime}", "ExportDataToAei");
                        return responseMessage.ReasonPhrase;
                    }


                }
                {
                    Utility.LogMessage(configInfo, $"No Records Found between  {configInfo.StartDateTime} and {configInfo.LastDateTime}", "ExportDataToAei");
                }
            }
            catch (Exception ex)
            {

                Utility.LogMessage(configInfo, ex.Message, "ExportDataToAei");

            }

            return string.Empty;
        }
         */

        /*
          public string ManualExportDataToAEI(ConfigurationInfo configInfo)
        {
            try
            {
                configInfo.SqlScript = GetSQlString(configInfo);
                DataSet ds = DataAccess.GetRecordDataSetCommon(configInfo);
                var attendanceList = DataAccess.GetCheckinOutInfo(configInfo, ds);

                if (attendanceList.Count > 0)
                {

                    var bulkAttendances = new BulkAttendancesInfo
                    {
                        OrgID = configInfo.OrganizationId,
                        AttendanceList = attendanceList

                    };

                    var record = new
                    {
                        BulkAttendances = bulkAttendances
                    };


                    // var responseMessage = client.PostAsync(configInfo.NexAEIApiUrl, content).Result;

                    CancellationTokenSource tokenSource = new CancellationTokenSource();
                    var cancellationToken = tokenSource.Token;

                    var responseHttpMessage = PostStreamAsync(record, cancellationToken, configInfo.NexAEIApiUrl);
                    var responseMessage = responseHttpMessage.Result;

                    if (responseMessage.StatusCode == HttpStatusCode.NoContent)
                    {
                        return string.Empty;
                    }


                    if (responseMessage.IsSuccessStatusCode)
                    {
                        //DataAccess.UpdateTracker(configInfo);
                        Utility.LogMessage(configInfo, $"Export Completed between  {configInfo.StartDateTime} and {configInfo.LastDateTime}", "ExportDataToAei");
                        return responseMessage.ReasonPhrase;
                    }


                }
                {
                    Utility.LogMessage(configInfo, $"No Records Found between  {configInfo.StartDateTime} and {configInfo.LastDateTime}", "ExportDataToAei");
                }
            }
            catch (Exception ex)
            {

                Utility.LogMessage(configInfo, ex.Message, "ExportDataToAei");

            }

            return string.Empty;
        }
         */
        #endregion
    }
}
