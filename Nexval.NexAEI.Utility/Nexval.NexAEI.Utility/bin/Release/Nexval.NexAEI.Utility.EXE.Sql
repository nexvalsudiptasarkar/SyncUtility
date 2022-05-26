IF OBJECT_ID('tempdb..#Results') IS NOT NULL DROP TABLE #Results

CREATE TABLE #Results
 (
	DeviceName varchar(100), 
	EmployeeID varchar(10),
	EmployeeName varchar(100), 
	EntryTime DateTime,
	AttentType varchar(10),
	UserEmail varchar(150),	
	ClientIP varchar(50),
	TimeZone varchar(50), 
	Location varchar(50), 
	TimeZoneDiff varchar(50), 
	Purpose varchar(50), 
	RowNum INT,
	RCount INT
)
DECLARE @DevicTeeNameText VARCHAR(100)= 'AKASH INS BIOMETRIC DEVICE'
DECLARE @CheckInText VARCHAR(10)= 'CHECKIN'
DECLARE @CheckOUTText VARCHAR(10)= 'CHECKOUT'
DECLARE @UserEmail VARCHAR(150)= ''
DECLARE @ClientIp VARCHAR(50)= 'none'
DECLARE @TimeZone VARCHAR(50)= 'Asia/Kolkata'
DECLARE @Location VARCHAR(50)= 'Kolkata'
DECLARE @TimeZoneDiff VARCHAR(50)= '19800'
DECLARE @FilterDate DateTime =getDate()-5

TRUNCATE TABLE #Results

;WITH CTECHECKIN AS
(
	SELECT
	    @DevicTeeNameText as DeviceName,		
		UserID as EmployeeID ,
		T1.UserName as EmployeeName,
		T1.IDateTime as EntryTime,		
		@CheckInText as AttentType,		
		@UserEmail as UserEmail,	
		@ClientIp as ClientIP,	
		@TimeZone as TimeZone,
		@Location as Location,
		@TimeZoneDiff as TimeZoneDiff,
		T1.device_name as  Purpose,
		ROW_NUMBER() over (PARTITION BY T1.UserId order by T1.IDateTime ASC) as RowNum,
		 (SELECT Count(*) FROM Transactions WHERE UserId = T1.UserId and   IDateTime >= CONVERT(DateTime, DATEDIFF(DAY, 0, @FilterDate)) and EntryExitType =0) as RCount
	FROM Transactions T1 where T1.IDateTime >= CONVERT(DateTime, DATEDIFF(DAY, 0, @FilterDate)) and T1.EntryExitType =0
)
INSERT INTO #Results 
SELECT * FROM  CTECHECKIN WHERE RowNum = 1

;WITH CTECHECKOUT AS
(
	SELECT 
		@DevicTeeNameText as DeviceName,
		T2.UserId as EmployeeID ,
		T2.UserName as EmployeeName,
		T2.IDateTime as EntryTime,
		@CheckOUTText as AttentType,		
		@UserEmail as UserEmail,	
		@ClientIp as ClientIP,	
		@TimeZone as TimeZone,
		@Location as Location,
		@TimeZoneDiff as TimeZoneDiff,
		T2.device_name as  Purpose,
	    ROW_NUMBER() over (PARTITION BY T2.UserId order by T2.IDateTime DESC) as RowNum,
	    (SELECT Count(*) FROM Transactions WHERE UserId = T2.UserId and   IDateTime >= CONVERT(DateTime, DATEDIFF(DAY, 0, @FilterDate)) and EntryExitType =0) as RCount
	  FROM Transactions T2 where T2.IDateTime >= CONVERT(DateTime, DATEDIFF(DAY, 0, @FilterDate)) and T2.EntryExitType =0
  )

INSERT INTO #Results
SELECT * FROM CTECHECKOUT	 WHERE RowNum = 1 and RCount>1

SELECT * FROM #Results WHERE RowNum = 1 ORDER BY EmployeeName



