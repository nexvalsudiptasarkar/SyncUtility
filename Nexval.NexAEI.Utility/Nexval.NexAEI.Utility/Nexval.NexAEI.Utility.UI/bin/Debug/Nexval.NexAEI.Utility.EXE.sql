SELECT EntryDate, EntryTime, EmployeeID ,EmployeeName,AttentType,DeviceName,UserEmail,ClientIP, TimeZone,TimeZoneDiff,Purpose,Location,MaxDeviceLogID
FROM
  (
    SELECT
      FORMAT(LogDate, 'dd-MMM-yyyy') as EntryDate,
      MIN(LogDate) as EntryTime,                
      UserId as EmployeeID ,
      "" As EmployeeName,
      "CHECKIN" as AttentType,
      "FINESSE BIOMETRIC DEVICE" as DeviceName,
      "" as UserEmail,
      "none" as ClientIP,
      "Asia/Kolkata" as TimeZone,
      "19800" as TimeZoneDiff,
      "Checkin" as Purpose,
     "FineSSE Kolkata" as Location,
     Max( DeviceLogID) as MaxDeviceLogID      

    FROM [@#@Table@#@]
     WHERE  FORMAT(LogDate, 'dd-MMM-yyyy')  BETWEEN FORMAT([@#@DateFrom@#@], 'dd-MMM-yyyy') AND FORMAT([@#@DateTo@#@], 'dd-MMM-yyyy')
    
    GROUP BY
      UserId,  FORMAT(LogDate, 'dd-MMM-yyyy')
  ) AS CheckInDetails 
 
UNION SELECT EntryDate, EntryTime, EmployeeID ,EmployeeName,AttentType,DeviceName,UserEmail,ClientIP,TimeZone,TimeZoneDiff,Purpose,Location,MaxDeviceLogID
FROM
  (
    SELECT      
      FORMAT(LogDate, 'dd-MMM-yyyy') as EntryDate,
      MAX(LogDate) as EntryTime,
      UserId as EmployeeID,
      "" As EmployeeName,
      "CHECKOUT" as AttentType,
      "FINESSE BIOMETRIC DEVICE" as DeviceName,
      "" as UserEmail,
      "None" as ClientIP,
      "Asia/Kolkata" As TimeZone,
      "19800" as TimeZoneDiff,
       "CheckOut" As Purpose,
       "FineSSE Kolkata" as Location,
     Max(DeviceLogID) as MaxDeviceLogID

    FROM [@#@Table@#@]
    WHERE  FORMAT(LogDate, 'dd-MMM-yyyy')  BETWEEN FORMAT([@#@DateFrom@#@], 'dd-MMM-yyyy') AND FORMAT([@#@DateTo@#@], 'dd-MMM-yyyy')
    
    GROUP BY
      UserId,  FORMAT(LogDate, 'dd-MMM-yyyy')
  ) AS CheckOutDetails;
