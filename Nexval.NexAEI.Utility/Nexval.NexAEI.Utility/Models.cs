
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Nexval.NexAEI.Utility
{
    /// <summary>
    /// AttendenceInfo
    /// </summary>
    public class AttendenceInfo
    {
        public string Purpose { get; set; }
        public string AttndType { get; set; }
        public string ClientUserAgent { get; set; }
        public string ClientIP { get; set; }
        public string EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string LocalTime { get; set; }
        public string Location { get; set; }      
        public string TimeZone { get; set; }
        public string TimeZoneDiff { get; set; }
        public string UserEmail { get; set; }
        [JsonIgnore]
        public int DeviceLogId { get; set;  }
        public int IndexNo { get; set; }
        public System.DateTime timeStamp { get; set; } 
    }

    /// <summary>
    /// CheckinOutInfo
    /// </summary>
    public class CheckinOutInfo
    {
        public string OrgID { get; set; }
        public string BulkAttendances { get; set; }
    }

    public class LogData
    {
        public int DeviceLogId { get; set; }
        public int IndexNo { get; set; }
        
        public System.DateTime timeStamp { get; set; } 
    }

    /// <summary>
    /// BulkAttendancesInfo
    /// </summary>
    [JsonObject("BulkAttendances")]
    public class BulkAttendancesInfo
    {
        [JsonProperty("OrgID")]
        public string OrgID { get; set; }

        [JsonProperty("AttendanceList")]
        public List<AttendenceInfo> AttendanceList { get; set; }
       
    }
}
