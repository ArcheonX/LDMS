-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- [dbo].[usp_GetPlanAndProgress_By_Employee] @employeeId ='STT00001' ,@ficialYear =2020,@IsSelectQ1 =1,@IsSelectQ2=0,@IsSelectQ3 =1,@IsSelectQ4= 0
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_GetPlanAndProgress_By_Employee]
	-- Add the parameters for the stored procedure here
	@employeeId nvarchar(50),
	@ficialYear INT,
	@IsSelectQ1 BIT =0,
	@IsSelectQ2 BIT =0,
	@IsSelectQ3 BIT =0,
	@IsSelectQ4 BIT =0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
IF OBJECT_ID('tempdb..#ProgressResult') IS NOT NULL
    DROP TABLE #ProgressResult

DECLARE @startDate DATETIME;
DECLARE @endDate DATETIME;
SET @startDate =DATEFROMPARTS(@ficialYear,4,1);
SET @endDate = DATEFROMPARTS(@ficialYear+1,3,31);

select 
		plat.ID AS ID_Platform,
		course.ID AS ID_Course,
        plat.PlatformID, plat.PlatformName_EN, plat.PlatformName_TH,
		subplat.SubPlatformName_EN,subplat.SubPlatformName_TH,
		course.CourseID,course.CourseName,
		course.Objective as CourseObjective,
		course.[Description] as CourseDescription,
		course.OutLine as CourseOutLine,
		method.Course_LearnMethodName_EN,method.Course_LearnMethodName_TH,
		class.LearnDateStart,
		class.LearnTimeStart,
		class.LearnDateEnd,
		class.LearnTimeEnd,
		class.LearnDateEnd As TargetDate, 
		class.RegisterDateStart,
		class.RegisterDateEnd,

		VenueRoom.VenueRoomID,
		VenueRoom.RoomName_EN,

		CASE WHEN class.LearnDateEnd > getdate() THEN datediff(day,getdate(),class.LearnDateEnd) ELSE 0 END AS RemainDay,

		CASE WHEN result.EmployeeID IS NOT NULL AND result.LearningResult = 70 AND result.ClassState = 70 THEN 'COMPLETED' 
			 WHEN class.ClassStatus = 70 AND (result.EmployeeID IS NULL OR result.LearningResult = 30  OR result.LearningResult = 99) THEN 'OVER DUE' 
		     WHEN result.EmployeeID IS NOT NULL AND result.LearningResult = 30 THEN 'ON PROGRESS'			
			 WHEN result.EmployeeID IS NOT NULL AND result.LearningResult = 30 AND  getdate()> class.LearnDateEnd  THEN 'OVER DUE'
			ELSE 'NOT START' END AS CourseStatus
		INTO #ProgressResult
		FROM LDMS_M_Platform plat
		INNER JOIN LDMS_M_SubPlatform subplat on subplat.ID_Platform = plat.ID
		INNER JOIN LDMS_M_SubPlatformCourse subplatcourse on subplatcourse.ID_SubPlatform = subplat.ID
		INNER JOIN  LDMS_M_Course course on  course.ID = subplatcourse.ID_Course
		INNER JOIN LDMS_M_CourseLearnMethod method on course.ID_LearnMethod = method.ID
		INNER JOIN LDMS_T_Class class on course.ID = class.ID_Course		
		INNER JOIN LDMS_M_VenueRoom VenueRoom on class.ID_VenueRoom = VenueRoom.ID
		
		INNER JOIN LDMS_T_CourseEmployee targetEmp on course.ID = targetEmp.ID_Course
		LEFT OUTER JOIN LDMS_T_ClassRegistration register on class.ID = register.ID_Class
		LEFT OUTER JOIN LDMS_T_ClassAttendAndResult result on class.ID = result.ID_Class and targetEmp.EmployeeID = result.EmployeeID
		
		WHERE class.RegisterDateStart>= @startDate AND class.RegisterDateEnd <= @endDate
		and course.IsActive = 1
		and targetEmp.EmployeeID = @employeeId; 

IF @IsSelectQ1 = 0 AND @IsSelectQ2 =0 AND @IsSelectQ3 =0 AND @IsSelectQ4 = 0
BEGIN
	SELECT * FROM #ProgressResult
	GROUP BY ID_Platform,ID_Course,PlatformID,PlatformName_EN,PlatformName_TH,SubPlatformName_EN,SubPlatformName_TH,CourseID,CourseName,CourseObjective,CourseDescription,CourseOutLine,Course_LearnMethodName_EN,
	Course_LearnMethodName_TH,LearnDateStart,LearnTimeStart,LearnDateEnd,LearnTimeEnd,TargetDate,RegisterDateStart,RegisterDateEnd,VenueRoomID,RoomName_EN,RemainDay,CourseStatus 
END
ELSE
BEGIN
	SELECT * FROM #ProgressResult 
	WHERE LearnDateStart >= @startDate AND RegisterDateEnd <= DATEFROMPARTS(@ficialYear,6,30)
	and @IsSelectQ1 = 1
	GROUP BY ID_Platform,ID_Course,PlatformID,PlatformName_EN,PlatformName_TH,SubPlatformName_EN,SubPlatformName_TH,CourseID,CourseName,CourseObjective,CourseDescription,CourseOutLine,Course_LearnMethodName_EN,
	Course_LearnMethodName_TH,LearnDateStart,LearnTimeStart,LearnDateEnd,LearnTimeEnd,TargetDate,RegisterDateStart,RegisterDateEnd,VenueRoomID,RoomName_EN,RemainDay,CourseStatus 

	UNION ALL

	SELECT * FROM #ProgressResult 
	WHERE LearnDateStart >= DATEFROMPARTS(@ficialYear,7,1) AND RegisterDateEnd <= DATEFROMPARTS(@ficialYear,9,30)
	and @IsSelectQ1 = 2
	GROUP BY ID_Platform,ID_Course,PlatformID,PlatformName_EN,PlatformName_TH,SubPlatformName_EN,SubPlatformName_TH,CourseID,CourseName,CourseObjective,CourseDescription,CourseOutLine,Course_LearnMethodName_EN,
	Course_LearnMethodName_TH,LearnDateStart,LearnTimeStart,LearnDateEnd,LearnTimeEnd,TargetDate,RegisterDateStart,RegisterDateEnd,VenueRoomID,RoomName_EN,RemainDay,CourseStatus 

	UNION ALL

	SELECT * FROM #ProgressResult 
	WHERE LearnDateStart >= DATEFROMPARTS(@ficialYear,10,1) AND RegisterDateEnd <= DATEFROMPARTS(@ficialYear,12,31)
	and @IsSelectQ1 = 3
	GROUP BY ID_Platform,ID_Course,PlatformID,PlatformName_EN,PlatformName_TH,SubPlatformName_EN,SubPlatformName_TH,CourseID,CourseName,CourseObjective,CourseDescription,CourseOutLine,Course_LearnMethodName_EN,
	Course_LearnMethodName_TH,LearnDateStart,LearnTimeStart,LearnDateEnd,LearnTimeEnd,TargetDate,RegisterDateStart,RegisterDateEnd,VenueRoomID,RoomName_EN,RemainDay,CourseStatus 

	UNION ALL

	SELECT * FROM #ProgressResult 
	WHERE LearnDateStart >= DATEFROMPARTS(@ficialYear,1,1) AND RegisterDateEnd <= DATEFROMPARTS(@ficialYear+1,3,31)
	and @IsSelectQ1 = 4
	GROUP BY ID_Platform,ID_Course,PlatformID,PlatformName_EN,PlatformName_TH,SubPlatformName_EN,SubPlatformName_TH,CourseID,CourseName,CourseObjective,CourseDescription,CourseOutLine,Course_LearnMethodName_EN,
	Course_LearnMethodName_TH,LearnDateStart,LearnTimeStart,LearnDateEnd,LearnTimeEnd,TargetDate,RegisterDateStart,RegisterDateEnd,VenueRoomID,RoomName_EN,RemainDay,CourseStatus 
END    -- Insert statements for procedure here


END
GO
