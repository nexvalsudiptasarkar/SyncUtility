using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Nexval.NexAEI.Utility
{
    public class ProcessManager
    {
        public async Task<string> ExportData(ConfigurationInfo configInfo)
        {
            try
            {
                //Utility.LogMessage(configInfo, "ExportData:: GetCheckinOutInfo() Started");

                var attendenceList = DataAccess.GetCheckinOutInfo(configInfo);

                //Utility.LogMessage(configInfo, "ExportData:: GetCheckinOutInfo() Completed");

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

                //Utility.LogMessage(configInfo, "ExportData:: Rest call object creation started");


                using (var client = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(record, Newtonsoft.Json.Formatting.Indented);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var responseMessage = client.PostAsync(configInfo.NexAEIApiUrl, content).Result;

                    //Utility.LogMessage(configInfo, "ExportData:: Rest call object response code : " + responseMessage.StatusCode.ToString());



                    if (responseMessage.StatusCode == HttpStatusCode.NoContent)
                    {
                        return string.Empty;
                    }

                    var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                    //Utility.LogMessage(configInfo, "ExportData:: Rest call object responseMessage : " + response.ToString());


                    if (responseMessage.IsSuccessStatusCode)
                    {
                        return response;
                    }

                }

                //Utility.LogMessage(configInfo, "ExportData:: Rest call object creation completed");

            }
            catch (Exception ex)
            {
                Utility.LogMessage(configInfo, "Method name ExportData()" + " " + ex.Message);
            }
            return string.Empty;
        }

        public async Task<string> ExportDataToAei(ConfigurationInfo configInfo)
        {
            try
            {
                //Utility.LogMessage(configInfo, "Method name GetCheckinOutInfo() started");

                var attendanceList = DataAccess.GetCheckinOutInfo(configInfo);

                //Utility.LogMessage(configInfo, "Method name GetCheckinOutInfo() completed");


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

                    //Utility.LogMessage(configInfo, "Rest call initiation started");
                    using (var client = new HttpClient())
                    {
                        var json = JsonConvert.SerializeObject(record, Newtonsoft.Json.Formatting.Indented);

                        //Utility.LogMessage(configInfo, "Rest call initiation started json data is : " + json);

                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        //Utility.LogMessage(configInfo, "Rest call initiation started content data is : " + content.ToString());

                        //Utility.LogMessage(configInfo, "Rest call initiation started endpoint data is : " + configInfo.NexAEIApiUrl);

                        var responseMessage = client.PostAsync(configInfo.NexAEIApiUrl, content).Result;

                        //Utility.LogMessage(configInfo, "Rest call initiation started content responseMessage is : " + responseMessage.ToString());

                        //Utility.LogMessage(configInfo, "Rest call initiation started content responseMessage.StatusCode is : " + responseMessage.StatusCode.ToString());

                        if (responseMessage.StatusCode == HttpStatusCode.NoContent)
                        {
                            return string.Empty;
                        }

                        var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);


                        //Utility.LogMessage(configInfo, "Rest call initiation started content response is : " + response.ToString());

                        if (responseMessage.IsSuccessStatusCode)
                        {
                            DataAccess.UpdateLog(attendanceList, configInfo);
                            return response;
                        }
                    }

                }
                else
                {
                    Utility.LogMessage(configInfo, "No attendance data to post");
                }
            }
            catch(Exception ex)
            {
                Utility.LogMessage(configInfo, "Method name ExportDataToAei()" + " " + ex.Message);
            }
           
            return string.Empty;
        }

       
    }
}
