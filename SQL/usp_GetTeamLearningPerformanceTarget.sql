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
-- [dbo].[usp_GetTeamLearningPerformanceTarget] @plantId = 0, @centerId = 0, @divisionId = 0 , @departmentId = 0, @sectionId = 0,	 @ficialYear =2020,	@IsSelectQ1=0,@IsSelectQ2=0,@IsSelectQ3=0,@IsSelectQ4 =0
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_GetTeamLearningPerformanceTarget]
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
IF OBJECT_ID('tempdb..#PerformanceResultTarget') IS NOT NULL
    DROP TABLE #PerformanceResultTarget

DECLARE @startDate DATETIME;
DECLARE @endDate DATETIME;
SET @startDate =DATEFROMPARTS(@ficialYear,4,1);
SET @endDate = DATEFROMPARTS(@ficialYear+1,3,31);

WITH    targetPlant AS
        (
			SELECT  
			usr.EmployeeId,
			usr.ID_plant 
			FROM LDMS_M_User usr  
			where usr.isActive = 1
				and (1 = case when @plantId >0 then (case when usr.ID_plant = @plantId then 1 else 0 end) else 1 end)
				and (1 = case when @centerId >0 then (case when usr.ID_center = @centerId then 1 else 0 end) else 1 end)
				and (1 = case when @divisionId >0 then (case when usr.ID_division = @divisionId then 1 else 0 end) else 1 end)
				and (1 = case when @departmentId >0 then (case when usr.ID_Department = @departmentId then 1 else 0 end) else 1 end)
				and (1 = case when @sectionId >0 then (case when usr.ID_Section = @sectionId then 1 else 0 end) else 1 end) 
        ),
		targetCenter AS (
			SELECT  
			usr.EmployeeId,
			usr.ID_center 
			FROM LDMS_M_User usr  
			where usr.isActive = 1
			and (1 = case when @plantId >0 then (case when usr.ID_plant = @plantId then 1 else 0 end) else 1 end)
			and (1 = case when @centerId >0 then (case when usr.ID_center = @centerId then 1 else 0 end) else 1 end)
			and (1 = case when @divisionId >0 then (case when usr.ID_division = @divisionId then 1 else 0 end) else 1 end)
			and (1 = case when @departmentId >0 then (case when usr.ID_Department = @departmentId then 1 else 0 end) else 1 end)
			and (1 = case when @sectionId >0 then (case when usr.ID_Section = @sectionId then 1 else 0 end) else 1 end) 
		),
		targetDivision AS(
			SELECT  
			usr.EmployeeId,
			usr.ID_division 
			FROM LDMS_M_User usr  
			where usr.isActive = 1 
			and (1 = case when @plantId >0 then (case when usr.ID_plant = @plantId then 1 else 0 end) else 1 end)
			and (1 = case when @centerId >0 then (case when usr.ID_center = @centerId then 1 else 0 end) else 1 end)
			and (1 = case when @divisionId >0 then (case when usr.ID_division = @divisionId then 1 else 0 end) else 1 end)
			and (1 = case when @departmentId >0 then (case when usr.ID_Department = @departmentId then 1 else 0 end) else 1 end)
			and (1 = case when @sectionId >0 then (case when usr.ID_Section = @sectionId then 1 else 0 end) else 1 end) 
		),
		targetDepartment as (
			SELECT  
				usr.EmployeeId,
				usr.ID_Department 
				FROM LDMS_M_User usr  
				where usr.isActive = 1
				and (1 = case when @plantId >0 then (case when usr.ID_plant = @plantId then 1 else 0 end) else 1 end)
				and (1 = case when @centerId >0 then (case when usr.ID_center = @centerId then 1 else 0 end) else 1 end)
				and (1 = case when @divisionId >0 then (case when usr.ID_division = @divisionId then 1 else 0 end) else 1 end)
				and (1 = case when @departmentId >0 then (case when usr.ID_Department = @departmentId then 1 else 0 end) else 1 end)
				and (1 = case when @sectionId >0 then (case when usr.ID_Section = @sectionId then 1 else 0 end) else 1 end) 
		),
		targetSection as 
		(
			SELECT  
			usr.EmployeeId,
			usr.ID_Section 
			FROM LDMS_M_User usr  
			where usr.isActive = 1
			and (1 = case when @plantId >0 then (case when usr.ID_plant = @plantId then 1 else 0 end) else 1 end)
			and (1 = case when @centerId >0 then (case when usr.ID_center = @centerId then 1 else 0 end) else 1 end)
			and (1 = case when @divisionId >0 then (case when usr.ID_division = @divisionId then 1 else 0 end) else 1 end)
			and (1 = case when @departmentId >0 then (case when usr.ID_Department = @departmentId then 1 else 0 end) else 1 end)
			and (1 = case when @sectionId >0 then (case when usr.ID_Section = @sectionId then 1 else 0 end) else 1 end) 
		),
		targetLevel as 
		(
			SELECT  
			usr.EmployeeId,
			JobGrade.ID as ID_JobGrade,
			JobGrade.JobGradeID,
			targetLevel.ID_Course
			FROM LDMS_M_User usr 
			JOIN LDMS_M_JobGrade JobGrade on JobGrade.ID = Usr.ID_JobGrade
			INNER JOIN LDMS_T_CourseJobGrade targetLevel on JobGrade.JobGradeID= targetLevel.ID_JobGrade
			where targetLevel.ID_Course>0 
			and usr.isActive = 1
			and (1 = case when @plantId >0 then (case when usr.ID_plant = @plantId then 1 else 0 end) else 1 end)
			and (1 = case when @centerId >0 then (case when usr.ID_center = @centerId then 1 else 0 end) else 1 end)
			and (1 = case when @divisionId >0 then (case when usr.ID_division = @divisionId then 1 else 0 end) else 1 end)
			and (1 = case when @departmentId >0 then (case when usr.ID_Department = @departmentId then 1 else 0 end) else 1 end)
			and (1 = case when @sectionId >0 then (case when usr.ID_Section = @sectionId then 1 else 0 end) else 1 end) 
		),
		targetPos as (
			SELECT  
			usr.EmployeeId,
			JobTitle.ID as ID_JobTitle,
			JobTitle.JobTitleID,
			targetPos.ID_Course
			FROM LDMS_M_User usr 
			JOIN LDMS_M_JobTitle JobTitle on JobTitle.ID = Usr.ID_JobTitle
			INNER JOIN LDMS_T_CourseJobTitle targetPos on JobTitle.JobTitleID= targetPos.ID_JobTitle
			where targetPos.ID_Course>0
			and usr.isActive = 1
			and (1 = case when @plantId >0 then (case when usr.ID_plant = @plantId then 1 else 0 end) else 1 end)
			and (1 = case when @centerId >0 then (case when usr.ID_center = @centerId then 1 else 0 end) else 1 end)
			and (1 = case when @divisionId >0 then (case when usr.ID_division = @divisionId then 1 else 0 end) else 1 end)
			and (1 = case when @departmentId >0 then (case when usr.ID_Department = @departmentId then 1 else 0 end) else 1 end)
			and (1 = case when @sectionId >0 then (case when usr.ID_Section = @sectionId then 1 else 0 end) else 1 end) 
		),
		targetEmp as 
		(
			SELECT  
			usr.EmployeeId, 
			targetEmp.ID_Course
			FROM LDMS_M_User usr  
			INNER JOIN LDMS_T_CourseEmployee targetEmp on usr.EmployeeId = targetEmp.EmployeeId
			where usr.isActive = 1 
			and (1 = case when @plantId >0 then (case when usr.ID_plant = @plantId then 1 else 0 end) else 1 end)
			and (1 = case when @centerId >0 then (case when usr.ID_center = @centerId then 1 else 0 end) else 1 end)
			and (1 = case when @divisionId >0 then (case when usr.ID_division = @divisionId then 1 else 0 end) else 1 end)
			and (1 = case when @departmentId >0 then (case when usr.ID_Department = @departmentId then 1 else 0 end) else 1 end)
			and (1 = case when @sectionId >0 then (case when usr.ID_Section = @sectionId then 1 else 0 end) else 1 end) 
		)  
SELECT * INTO #PerformanceResultTarget
FROM
(
			select 
			plat.ID AS ID_Platform,
			course.ID AS ID_Course,
			plat.PlatformID, 
			course.CourseID, 
			targetPlant.EmployeeId, 
			class.RegisterDateStart,
			class.RegisterDateEnd  
			FROM LDMS_M_Platform plat
			INNER JOIN LDMS_M_SubPlatform subplat on subplat.ID_Platform = plat.ID
			INNER JOIN LDMS_M_SubPlatformCourse subplatcourse on subplatcourse.ID_SubPlatform = subplat.ID
			INNER JOIN LDMS_M_Course course on  course.ID = subplatcourse.ID_Course 
			INNER JOIN LDMS_T_Class class on course.ID = class.ID_Course	  
			LEFT OUTER JOIN targetPlant on targetPlant.ID_plant = course.ID_PlantTarget 
			WHERE class.RegisterDateStart>= @startDate AND class.RegisterDateEnd <= @endDate and course.IsActive = 1
			and (1 = case when @plantId >0 then (case when course.ID_PlantTarget = @plantId then 1 else 0 end) else 1 end)
			and (1 = case when @centerId >0 then (case when course.ID_CenterTarget = @centerId then 1 else 0 end) else 1 end)
			and (1 = case when @divisionId >0 then (case when course.ID_DivisionTarget = @divisionId then 1 else 0 end) else 1 end)
			and (1 = case when @departmentId >0 then (case when course.ID_DepartmentTarget = @departmentId then 1 else 0 end) else 1 end)
			and (1 = case when @sectionId >0 then (case when course.ID_SectionTarget = @sectionId then 1 else 0 end) else 1 end) 

			UNION ALL

			select 
			plat.ID AS ID_Platform,
			course.ID AS ID_Course,
			plat.PlatformID, 
			course.CourseID, 
			targetCenter.EmployeeId, 
			class.RegisterDateStart,
			class.RegisterDateEnd  
			FROM LDMS_M_Platform plat
			INNER JOIN LDMS_M_SubPlatform subplat on subplat.ID_Platform = plat.ID
			INNER JOIN LDMS_M_SubPlatformCourse subplatcourse on subplatcourse.ID_SubPlatform = subplat.ID
			INNER JOIN LDMS_M_Course course on  course.ID = subplatcourse.ID_Course 
			INNER JOIN LDMS_T_Class class on course.ID = class.ID_Course	  
			LEFT OUTER JOIN targetCenter on targetCenter.ID_center = course.ID_CenterTarget
			WHERE class.RegisterDateStart>= @startDate AND class.RegisterDateEnd <= @endDate and course.IsActive = 1
			and (1 = case when @plantId >0 then (case when course.ID_PlantTarget = @plantId then 1 else 0 end) else 1 end)
			and (1 = case when @centerId >0 then (case when course.ID_CenterTarget = @centerId then 1 else 0 end) else 1 end)
			and (1 = case when @divisionId >0 then (case when course.ID_DivisionTarget = @divisionId then 1 else 0 end) else 1 end)
			and (1 = case when @departmentId >0 then (case when course.ID_DepartmentTarget = @departmentId then 1 else 0 end) else 1 end)
			and (1 = case when @sectionId >0 then (case when course.ID_SectionTarget = @sectionId then 1 else 0 end) else 1 end) 

			UNION ALL

			select 
			plat.ID AS ID_Platform,
			course.ID AS ID_Course,
			plat.PlatformID, 
			course.CourseID, 
			targetDivision.EmployeeId, 
			class.RegisterDateStart,
			class.RegisterDateEnd  
			FROM LDMS_M_Platform plat
			INNER JOIN LDMS_M_SubPlatform subplat on subplat.ID_Platform = plat.ID
			INNER JOIN LDMS_M_SubPlatformCourse subplatcourse on subplatcourse.ID_SubPlatform = subplat.ID
			INNER JOIN LDMS_M_Course course on  course.ID = subplatcourse.ID_Course 
			INNER JOIN LDMS_T_Class class on course.ID = class.ID_Course	  
			LEFT OUTER JOIN targetDivision  on targetDivision.ID_division = course.ID_DivisionTarget 
			WHERE class.RegisterDateStart>= @startDate AND class.RegisterDateEnd <= @endDate and course.IsActive = 1
			and (1 = case when @plantId >0 then (case when course.ID_PlantTarget = @plantId then 1 else 0 end) else 1 end)
			and (1 = case when @centerId >0 then (case when course.ID_CenterTarget = @centerId then 1 else 0 end) else 1 end)
			and (1 = case when @divisionId >0 then (case when course.ID_DivisionTarget = @divisionId then 1 else 0 end) else 1 end)
			and (1 = case when @departmentId >0 then (case when course.ID_DepartmentTarget = @departmentId then 1 else 0 end) else 1 end)
			and (1 = case when @sectionId >0 then (case when course.ID_SectionTarget = @sectionId then 1 else 0 end) else 1 end) 

			UNION ALL

			select 
			plat.ID AS ID_Platform,
			course.ID AS ID_Course,
			plat.PlatformID, 
			course.CourseID, 
			targetDepartment.EmployeeId, 
			class.RegisterDateStart,
			class.RegisterDateEnd  
			FROM LDMS_M_Platform plat
			INNER JOIN LDMS_M_SubPlatform subplat on subplat.ID_Platform = plat.ID
			INNER JOIN LDMS_M_SubPlatformCourse subplatcourse on subplatcourse.ID_SubPlatform = subplat.ID
			INNER JOIN LDMS_M_Course course on  course.ID = subplatcourse.ID_Course 
			INNER JOIN LDMS_T_Class class on course.ID = class.ID_Course	  
			LEFT OUTER JOIN targetDepartment on targetDepartment.ID_Department = course.ID_DepartmentTarget 
			WHERE class.RegisterDateStart>= @startDate AND class.RegisterDateEnd <= @endDate and course.IsActive = 1
			and (1 = case when @plantId >0 then (case when course.ID_PlantTarget = @plantId then 1 else 0 end) else 1 end)
			and (1 = case when @centerId >0 then (case when course.ID_CenterTarget = @centerId then 1 else 0 end) else 1 end)
			and (1 = case when @divisionId >0 then (case when course.ID_DivisionTarget = @divisionId then 1 else 0 end) else 1 end)
			and (1 = case when @departmentId >0 then (case when course.ID_DepartmentTarget = @departmentId then 1 else 0 end) else 1 end)
			and (1 = case when @sectionId >0 then (case when course.ID_SectionTarget = @sectionId then 1 else 0 end) else 1 end) 

			UNION ALL

			select 
			plat.ID AS ID_Platform,
			course.ID AS ID_Course,
			plat.PlatformID, 
			course.CourseID, 
			targetSection.EmployeeId, 
			class.RegisterDateStart,
			class.RegisterDateEnd  
			FROM LDMS_M_Platform plat
			INNER JOIN LDMS_M_SubPlatform subplat on subplat.ID_Platform = plat.ID
			INNER JOIN LDMS_M_SubPlatformCourse subplatcourse on subplatcourse.ID_SubPlatform = subplat.ID
			INNER JOIN LDMS_M_Course course on  course.ID = subplatcourse.ID_Course 
			INNER JOIN LDMS_T_Class class on course.ID = class.ID_Course	  
			LEFT OUTER JOIN targetSection on targetSection.ID_Section = course.ID_SectionTarget
			WHERE class.RegisterDateStart>= @startDate AND class.RegisterDateEnd <= @endDate and course.IsActive = 1
			and (1 = case when @plantId >0 then (case when course.ID_PlantTarget = @plantId then 1 else 0 end) else 1 end)
			and (1 = case when @centerId >0 then (case when course.ID_CenterTarget = @centerId then 1 else 0 end) else 1 end)
			and (1 = case when @divisionId >0 then (case when course.ID_DivisionTarget = @divisionId then 1 else 0 end) else 1 end)
			and (1 = case when @departmentId >0 then (case when course.ID_DepartmentTarget = @departmentId then 1 else 0 end) else 1 end)
			and (1 = case when @sectionId >0 then (case when course.ID_SectionTarget = @sectionId then 1 else 0 end) else 1 end) 

			UNION ALL

			select 
			plat.ID AS ID_Platform,
			course.ID AS ID_Course,
			plat.PlatformID, 
			course.CourseID, 
			targetLevel.EmployeeId, 
			class.RegisterDateStart,
			class.RegisterDateEnd  
			FROM LDMS_M_Platform plat
			INNER JOIN LDMS_M_SubPlatform subplat on subplat.ID_Platform = plat.ID
			INNER JOIN LDMS_M_SubPlatformCourse subplatcourse on subplatcourse.ID_SubPlatform = subplat.ID
			INNER JOIN LDMS_M_Course course on  course.ID = subplatcourse.ID_Course 
			INNER JOIN LDMS_T_Class class on course.ID = class.ID_Course	  
			LEFT OUTER JOIN targetLevel on course.ID = targetLevel.ID_Course 
			WHERE class.RegisterDateStart>= @startDate AND class.RegisterDateEnd <= @endDate and course.IsActive = 1
			and (1 = case when @plantId >0 then (case when course.ID_PlantTarget = @plantId then 1 else 0 end) else 1 end)
			and (1 = case when @centerId >0 then (case when course.ID_CenterTarget = @centerId then 1 else 0 end) else 1 end)
			and (1 = case when @divisionId >0 then (case when course.ID_DivisionTarget = @divisionId then 1 else 0 end) else 1 end)
			and (1 = case when @departmentId >0 then (case when course.ID_DepartmentTarget = @departmentId then 1 else 0 end) else 1 end)
			and (1 = case when @sectionId >0 then (case when course.ID_SectionTarget = @sectionId then 1 else 0 end) else 1 end) 

			UNION ALL

			select 
			plat.ID AS ID_Platform,
			course.ID AS ID_Course,
			plat.PlatformID, 
			course.CourseID, 
			targetEmp.EmployeeId, 
			class.RegisterDateStart,
			class.RegisterDateEnd  
			FROM LDMS_M_Platform plat
			INNER JOIN LDMS_M_SubPlatform subplat on subplat.ID_Platform = plat.ID
			INNER JOIN LDMS_M_SubPlatformCourse subplatcourse on subplatcourse.ID_SubPlatform = subplat.ID
			INNER JOIN LDMS_M_Course course on  course.ID = subplatcourse.ID_Course 
			INNER JOIN LDMS_T_Class class on course.ID = class.ID_Course	  
			LEFT OUTER JOIN targetEmp on targetEmp.ID_Course = course.ID 
			WHERE class.RegisterDateStart>= @startDate AND class.RegisterDateEnd <= @endDate and course.IsActive = 1
			and (1 = case when @plantId >0 then (case when course.ID_PlantTarget = @plantId then 1 else 0 end) else 1 end)
			and (1 = case when @centerId >0 then (case when course.ID_CenterTarget = @centerId then 1 else 0 end) else 1 end)
			and (1 = case when @divisionId >0 then (case when course.ID_DivisionTarget = @divisionId then 1 else 0 end) else 1 end)
			and (1 = case when @departmentId >0 then (case when course.ID_DepartmentTarget = @departmentId then 1 else 0 end) else 1 end)
			and (1 = case when @sectionId >0 then (case when course.ID_SectionTarget = @sectionId then 1 else 0 end) else 1 end) 

			UNION ALL

			select 
			plat.ID AS ID_Platform,
			course.ID AS ID_Course,
			plat.PlatformID, 
			course.CourseID, 
			targetPos.EmployeeId, 
			class.RegisterDateStart,
			class.RegisterDateEnd  
			FROM LDMS_M_Platform plat
			INNER JOIN LDMS_M_SubPlatform subplat on subplat.ID_Platform = plat.ID
			INNER JOIN LDMS_M_SubPlatformCourse subplatcourse on subplatcourse.ID_SubPlatform = subplat.ID
			INNER JOIN LDMS_M_Course course on  course.ID = subplatcourse.ID_Course 
			INNER JOIN LDMS_T_Class class on course.ID = class.ID_Course	  
			LEFT OUTER JOIN targetPos on course.ID = targetPos.ID_Course 
			WHERE class.RegisterDateStart>= @startDate AND class.RegisterDateEnd <= @endDate and course.IsActive = 1
			and (1 = case when @plantId >0 then (case when course.ID_PlantTarget = @plantId then 1 else 0 end) else 1 end)
			and (1 = case when @centerId >0 then (case when course.ID_CenterTarget = @centerId then 1 else 0 end) else 1 end)
			and (1 = case when @divisionId >0 then (case when course.ID_DivisionTarget = @divisionId then 1 else 0 end) else 1 end)
			and (1 = case when @departmentId >0 then (case when course.ID_DepartmentTarget = @departmentId then 1 else 0 end) else 1 end)
			and (1 = case when @sectionId >0 then (case when course.ID_SectionTarget = @sectionId then 1 else 0 end) else 1 end) 
 )TBG


IF @IsSelectQ1 = 0 AND @IsSelectQ2 =0 AND @IsSelectQ3 =0 AND @IsSelectQ4 = 0
BEGIN
	SELECT  ltrim(rtrim(EmployeeId)) FROM #PerformanceResultTarget where EmployeeId is not null GROUP BY EmployeeId
END
ELSE
BEGIN
	SELECT ltrim(rtrim(EmployeeId)) FROM #PerformanceResultTarget 
	WHERE LearnDateStart >= @startDate AND RegisterDateEnd <= DATEFROMPARTS(@ficialYear,6,30) and @IsSelectQ1 = 1
	and EmployeeId is not null
	GROUP BY EmployeeId
	UNION ALL
	SELECT ltrim(rtrim(EmployeeId)) FROM #PerformanceResultTarget 
	WHERE LearnDateStart >= DATEFROMPARTS(@ficialYear,7,1) AND RegisterDateEnd <= DATEFROMPARTS(@ficialYear,9,30) and @IsSelectQ1 = 2
	and EmployeeId is not null
	GROUP BY EmployeeId	
	UNION ALL
	SELECT ltrim(rtrim(EmployeeId)) FROM #PerformanceResultTarget 
	WHERE LearnDateStart >= DATEFROMPARTS(@ficialYear,10,1) AND RegisterDateEnd <= DATEFROMPARTS(@ficialYear,12,31) and @IsSelectQ1 = 3
	and EmployeeId is not null
	GROUP BY EmployeeId
	UNION ALL
	SELECT ltrim(rtrim(EmployeeId)) FROM #PerformanceResultTarget 
	WHERE LearnDateStart >= DATEFROMPARTS(@ficialYear,1,1) AND RegisterDateEnd <= DATEFROMPARTS(@ficialYear+1,3,31) and @IsSelectQ1 = 4	
	and EmployeeId is not null
	GROUP BY EmployeeId
END    -- Insert statements for procedure here 
END
GO
