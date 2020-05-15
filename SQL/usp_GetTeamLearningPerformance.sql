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
-- [dbo].[usp_GetTeamLearningPerformance] @plantId = 0, @centerId = 0, @divisionId = 0 , @departmentId = 0, @sectionId = 0,	 @ficialYear =2020,	@IsSelectQ1=0,@IsSelectQ2=0,@IsSelectQ3=0,@IsSelectQ4 =0
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_GetTeamLearningPerformance]
	-- Add the parameters for the stored procedure here
	@plantId INT = 0,
	@centerId INT = 0,
	@divisionId INT = 0,
	@departmentId INT = 0,		
	@sectionId INT = 0,	
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
IF OBJECT_ID('tempdb..#PerformanceResult') IS NOT NULL
    DROP TABLE #PerformanceResult

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
		month(	class.LearnDateStart ) as TargetMonth,
		year(	class.LearnDateStart ) as TargetYear,	
		class.RegisterDateStart,
		class.RegisterDateEnd,
		class.ClassCapacity,
		class.ClassFee,
		(class.ClassFee / class.ClassCapacity) AS ClassFeePerPerson,
		VenueRoom.VenueRoomID,
		VenueRoom.RoomName_EN, 


		CASE WHEN class.LearnDateEnd > getdate() THEN datediff(day,getdate(),class.LearnDateEnd) ELSE 0 END AS RemainDay,

		CASE WHEN result.EmployeeID IS NOT NULL AND result.LearningResult = 70 AND result.ClassState = 70 THEN 'COMPLETED' 
			 WHEN class.ClassStatus = 70 AND (result.EmployeeID IS NULL OR result.LearningResult = 30  OR result.LearningResult = 99) THEN 'OVER DUE' 
		     WHEN result.EmployeeID IS NOT NULL AND result.LearningResult = 30 THEN 'ON PROGRESS'			
			 WHEN result.EmployeeID IS NOT NULL AND result.LearningResult = 30 AND  getdate()> class.LearnDateEnd  THEN 'OVER DUE'
			ELSE 'NOT START' END AS CourseStatus,
        JobGrade.ID as ID_JobGrade,
		JobGrade.JobGradeID,
		JobGrade.JobGradeName_EN,
		JobGrade.JobGradeName_TH,

		JobTitle.ID AS ID_JobTitle,
		JobTitle.JobTitleID,
		JobTitle.JobTitleName_EN,
		JobTitle.JobTitleName_TH,

		Plant.ID as ID_Plant,
		Plant.PlantID,
		Plant.PlantName_EN,
		Plant.PlantName_TH,

		center.ID as ID_Center,
		center.CenterID,
		center.CenterName_EN,
		center.CenterName_TH,

		division.ID as ID_Division,
		division.DivisionID,
		division.DivisionName_EN,
		division.DivisionName_TH,

		department.ID AS ID_Department,
		department.DepartmentID,
		department.DepartmentName_EN,
		department.DepartmentName_TH,

		isnull(section.ID,0) as ID_Section,
		isnull(section.SectionID,'') as SectionID,
		isnull(section.SectionName_EN,'') as SectionName_EN,
		isnull(section.SectionName_TH,'') as SectionName_TH,
	    
		targetLevel.ID_JobGrade as TargetJobGrade ,
		targetPos.ID_JobTitle as TargetJobTitle,		
		targetEmp.EmployeeID as TargetEmployeeID

		INTO #PerformanceResult
		FROM LDMS_M_Platform plat
		INNER JOIN LDMS_M_SubPlatform subplat on subplat.ID_Platform = plat.ID
		INNER JOIN LDMS_M_SubPlatformCourse subplatcourse on subplatcourse.ID_SubPlatform = subplat.ID
		INNER JOIN LDMS_M_Course course on  course.ID = subplatcourse.ID_Course
		INNER JOIN LDMS_M_CourseLearnMethod method on course.ID_LearnMethod = method.ID
		INNER JOIN LDMS_T_Class class on course.ID = class.ID_Course		
		INNER JOIN LDMS_T_CourseEmployee targetEmp on course.ID = targetEmp.ID_Course	
		INNER JOIN LDMS_T_CourseJobGrade targetLevel on course.ID = targetLevel.ID_Course	
		INNER JOIN LDMS_T_CourseJobTitle targetPos on course.ID = targetPos.ID_Course	

		INNER JOIN LDMS_M_User Usr on targetEmp.EmployeeID = Usr.EmployeeID

		LEFT OUTER JOIN LDMS_M_VenueRoom VenueRoom on class.ID_VenueRoom = VenueRoom.ID			
		LEFT OUTER JOIN LDMS_T_ClassRegistration register on class.ID = register.ID_Class
		LEFT OUTER JOIN LDMS_T_ClassAttendAndResult result on class.ID = result.ID_Class and targetEmp.EmployeeID = result.EmployeeID 
		
		LEFT OUTER JOIN LDMS_M_JobGrade JobGrade on JobGrade.ID = Usr.ID_JobGrade
		LEFT OUTER JOIN LDMS_M_JobTitle JobTitle on Usr.ID_JobTitle = JobTitle.ID

		LEFT OUTER JOIN LDMS_M_Plant Plant on Usr.ID_Plant = Plant.ID
		LEFT OUTER JOIN LDMS_M_Center center on Usr.ID_Center = center.ID
		LEFT OUTER JOIN LDMS_M_Division division on Usr.ID_Division = division.ID
		LEFT OUTER JOIN LDMS_M_Department department on Usr.ID_Department = department.ID
		LEFT OUTER JOIN LDMS_M_Section section on Usr.ID_Section = section.ID		
		WHERE class.RegisterDateStart>= @startDate AND class.RegisterDateEnd <= @endDate
		and course.IsActive = 1
		and (1 = case when @plantId >0 then (case when Plant.ID=@plantId then 1 else 0 end) else 1 end)
		and (1 = case when @centerId >0 then (case when center.ID=@centerId then 1 else 0 end) else 1 end)
		and (1 = case when @divisionId >0 then (case when division.ID=@divisionId then 1 else 0 end) else 1 end)
		and (1 = case when @departmentId >0 then (case when department.ID=@departmentId then 1 else 0 end) else 1 end)
		and (1 = case when @sectionId >0 then (case when section.ID=@sectionId then 1 else 0 end) else 1 end) 

IF @IsSelectQ1 = 0 AND @IsSelectQ2 =0 AND @IsSelectQ3 =0 AND @IsSelectQ4 = 0
BEGIN
	SELECT  * FROM #PerformanceResult 
	GROUP BY ID_Platform,ID_Course,PlatformID,PlatformName_EN,PlatformName_TH,SubPlatformName_EN,SubPlatformName_TH,CourseID,CourseName,CourseObjective,CourseDescription,CourseOutLine,Course_LearnMethodName_EN,
	Course_LearnMethodName_TH,LearnDateStart,LearnTimeStart,LearnDateEnd,LearnTimeEnd,TargetDate,TargetMonth,TargetYear,RegisterDateStart,RegisterDateEnd,ClassCapacity,ClassFee,ClassFeePerPerson,VenueRoomID,RoomName_EN,
	RemainDay,CourseStatus,ID_JobGrade,JobGradeID,JobGradeName_EN,JobGradeName_TH,ID_JobTitle,JobTitleID,JobTitleName_EN,JobTitleName_TH,ID_Plant,PlantID,PlantName_EN,PlantName_TH,ID_Center,CenterID,CenterName_EN,
	CenterName_TH,ID_Division,DivisionID,DivisionName_EN,DivisionName_TH,ID_Department,DepartmentID,DepartmentName_EN,DepartmentName_TH,ID_Section,SectionID,SectionName_EN,SectionName_TH,TargetJobGrade,TargetJobTitle,TargetEmployeeID

END
ELSE
BEGIN
	SELECT * FROM #PerformanceResult 
	WHERE LearnDateStart >= @startDate AND RegisterDateEnd <= DATEFROMPARTS(@ficialYear,6,30)
	and @IsSelectQ1 = 1
	GROUP BY ID_Platform,ID_Course,PlatformID,PlatformName_EN,PlatformName_TH,SubPlatformName_EN,SubPlatformName_TH,CourseID,CourseName,CourseObjective,CourseDescription,CourseOutLine,Course_LearnMethodName_EN,
	Course_LearnMethodName_TH,LearnDateStart,LearnTimeStart,LearnDateEnd,LearnTimeEnd,TargetDate,TargetMonth,TargetYear,RegisterDateStart,RegisterDateEnd,ClassCapacity,ClassFee,ClassFeePerPerson,VenueRoomID,RoomName_EN,
	RemainDay,CourseStatus,ID_JobGrade,JobGradeID,JobGradeName_EN,JobGradeName_TH,ID_JobTitle,JobTitleID,JobTitleName_EN,JobTitleName_TH,ID_Plant,PlantID,PlantName_EN,PlantName_TH,ID_Center,CenterID,CenterName_EN,
	CenterName_TH,ID_Division,DivisionID,DivisionName_EN,DivisionName_TH,ID_Department,DepartmentID,DepartmentName_EN,DepartmentName_TH,ID_Section,SectionID,SectionName_EN,SectionName_TH,TargetJobGrade,TargetJobTitle,TargetEmployeeID

	UNION ALL

	SELECT * FROM #PerformanceResult 
	WHERE LearnDateStart >= DATEFROMPARTS(@ficialYear,7,1) AND RegisterDateEnd <= DATEFROMPARTS(@ficialYear,9,30)
	and @IsSelectQ1 = 2
	GROUP BY ID_Platform,ID_Course,PlatformID,PlatformName_EN,PlatformName_TH,SubPlatformName_EN,SubPlatformName_TH,CourseID,CourseName,CourseObjective,CourseDescription,CourseOutLine,Course_LearnMethodName_EN,
	Course_LearnMethodName_TH,LearnDateStart,LearnTimeStart,LearnDateEnd,LearnTimeEnd,TargetDate,TargetMonth,TargetYear,RegisterDateStart,RegisterDateEnd,ClassCapacity,ClassFee,ClassFeePerPerson,VenueRoomID,RoomName_EN,
	RemainDay,CourseStatus,ID_JobGrade,JobGradeID,JobGradeName_EN,JobGradeName_TH,ID_JobTitle,JobTitleID,JobTitleName_EN,JobTitleName_TH,ID_Plant,PlantID,PlantName_EN,PlantName_TH,ID_Center,CenterID,CenterName_EN,
	CenterName_TH,ID_Division,DivisionID,DivisionName_EN,DivisionName_TH,ID_Department,DepartmentID,DepartmentName_EN,DepartmentName_TH,ID_Section,SectionID,SectionName_EN,SectionName_TH,TargetJobGrade,TargetJobTitle,TargetEmployeeID

	UNION ALL

	SELECT * FROM #PerformanceResult 
	WHERE LearnDateStart >= DATEFROMPARTS(@ficialYear,10,1) AND RegisterDateEnd <= DATEFROMPARTS(@ficialYear,12,31)
	and @IsSelectQ1 = 3
	GROUP BY ID_Platform,ID_Course,PlatformID,PlatformName_EN,PlatformName_TH,SubPlatformName_EN,SubPlatformName_TH,CourseID,CourseName,CourseObjective,CourseDescription,CourseOutLine,Course_LearnMethodName_EN,
	Course_LearnMethodName_TH,LearnDateStart,LearnTimeStart,LearnDateEnd,LearnTimeEnd,TargetDate,TargetMonth,TargetYear,RegisterDateStart,RegisterDateEnd,ClassCapacity,ClassFee,ClassFeePerPerson,VenueRoomID,RoomName_EN,
	RemainDay,CourseStatus,ID_JobGrade,JobGradeID,JobGradeName_EN,JobGradeName_TH,ID_JobTitle,JobTitleID,JobTitleName_EN,JobTitleName_TH,ID_Plant,PlantID,PlantName_EN,PlantName_TH,ID_Center,CenterID,CenterName_EN,
	CenterName_TH,ID_Division,DivisionID,DivisionName_EN,DivisionName_TH,ID_Department,DepartmentID,DepartmentName_EN,DepartmentName_TH,ID_Section,SectionID,SectionName_EN,SectionName_TH,TargetJobGrade,TargetJobTitle,TargetEmployeeID

	UNION ALL

	SELECT * FROM #PerformanceResult 
	WHERE LearnDateStart >= DATEFROMPARTS(@ficialYear,1,1) AND RegisterDateEnd <= DATEFROMPARTS(@ficialYear+1,3,31)
	and @IsSelectQ1 = 4
	GROUP BY ID_Platform,ID_Course,PlatformID,PlatformName_EN,PlatformName_TH,SubPlatformName_EN,SubPlatformName_TH,CourseID,CourseName,CourseObjective,CourseDescription,CourseOutLine,Course_LearnMethodName_EN,
	Course_LearnMethodName_TH,LearnDateStart,LearnTimeStart,LearnDateEnd,LearnTimeEnd,TargetDate,TargetMonth,TargetYear,RegisterDateStart,RegisterDateEnd,ClassCapacity,ClassFee,ClassFeePerPerson,VenueRoomID,RoomName_EN,
	RemainDay,CourseStatus,ID_JobGrade,JobGradeID,JobGradeName_EN,JobGradeName_TH,ID_JobTitle,JobTitleID,JobTitleName_EN,JobTitleName_TH,ID_Plant,PlantID,PlantName_EN,PlantName_TH,ID_Center,CenterID,CenterName_EN,
	CenterName_TH,ID_Division,DivisionID,DivisionName_EN,DivisionName_TH,ID_Department,DepartmentID,DepartmentName_EN,DepartmentName_TH,ID_Section,SectionID,SectionName_EN,SectionName_TH,TargetJobGrade,TargetJobTitle,TargetEmployeeID
END    -- Insert statements for procedure here


END
GO
