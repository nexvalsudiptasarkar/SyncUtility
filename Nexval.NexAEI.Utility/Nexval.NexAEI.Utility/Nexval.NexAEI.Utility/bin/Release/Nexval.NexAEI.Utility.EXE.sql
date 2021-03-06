SELECT EntryDate, EntryTime, EmployeeID ,AttentType,DeviceName,UserEmail,ClientIP, TimeZone,TimeZoneDiff,Purpose
FROM
  (
    SELECT
        FORMAT(LogDate, 'dd-MMM-yyyy') as EntryDate,
        MIN(LogDate) as EntryTime,                
        UserId as EmployeeID ,
        "CHECKIN  " as AttentType,
        "FINESSE BIOMETRIC DEVICE" as DeviceName,
       "" as UserEmail,
      "none" as ClientIP,
      "Asia/Kolkata" as TimeZone,
      "19800" as TimeZoneDiff,
      "" as Purpose
       

    FROM DeviceLogs_1_2019
    
    GROUP BY
      UserId,  FORMAT(LogDate, 'dd-MMM-yyyy')
  ) AS CheckInDetails 
 
UNION

SELECT EntryDate, EntryTime, EmployeeID ,AttentType,DeviceName,UserEmail,ClientIP,TimeZone,TimeZoneDiff,Purpose
FROM
  (
    SELECT
        FORMAT(LogDate, 'dd-MMM-yyyy') as EntryDate,
       MAX(LogDate) as EntryTime,
        UserId as EmployeeID,
        "CHECKOUT" as AttentType,
           "FINESSE BIOMETRIC DEVICE" as DeviceName,
         "" as UserEmail,
       "None" as ClientIP,
      "Asia/Kolkata" As TimeZone,
      "19800" as TimeZoneDiff,
        "" As Purpose

    FROM DeviceLogs_1_2019
    
    GROUP BY
      UserId,  FORMAT(LogDate, 'dd-MMM-yyyy')
  ) AS CheckOutDetails 
 