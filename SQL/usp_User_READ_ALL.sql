--USE [veraplus_ldms]
--GO

/****** Object:  StoredProcedure [dbo].[usp_User_READ_ALL]    Script Date: 4/6/2020 11:09:19 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- EXEC [dbo].[usp_User_READ_ALL] @paramEmployeeId='',@paramEmployeeName=null,@paramdepartments='',@paramsections='',@paramjobgrades='',@paramjobtitles=''
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_User_READ_ALL]
	 @paramEmployeeId nvarchar(50) = null,
	 @paramEmployeeName nvarchar(50) = null,
	 @paramdepartments nvarchar(max) = null,
	 @paramsections nvarchar(max) = null,
	 @paramjobgrades nvarchar(max) = null,
	 @paramjobtitles nvarchar(max) = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	 
    Declare @departments table (ID INT);
	Declare @sections table (ID INT);
	Declare @jobgrades table (ID INT);
	Declare @jobtitles table (ID INT);

	INSERT INTO @departments SELECT Item FROM dbo.SplitInts(isnuLL(@paramdepartments,''), ','); 
	INSERT INTO @sections SELECT Item FROM dbo.SplitInts(isnull(@paramsections,''), ','); 
	INSERT INTO @jobgrades SELECT Item FROM dbo.SplitInts(isnull(@paramjobgrades,''), ','); 
	INSERT INTO @jobtitles SELECT Item FROM dbo.SplitInts(isnull(@paramjobtitles,''), ','); 

    -- Insert statements for procedure here
	SELECT  
	ROW_NUMBER() OVER(ORDER BY muser.ID_User ASC)  as RowIndex,
	muser.ID_User,
	muser.DateOfBirth,muser.DriverLicenseID,muser.email,muser.EmployeeID,muser.Gender,muser.IDCardNumber,
	isnull(muser.IsAD,0) AS IsAD  , 
	muser.JoinDate ,muser.Name,
	muser.ID_Role,muser.IsInstructor,muser.Password,muser.Remark,muser.passwordSalt,muser.IsSectionHead,muser.IsAllowGPP as IsAllowGPP,muser.IsLocked, 
	muser.Nationality,muser.OutDate,muser.PhoneNumber,muser.Surname, muser.Is_FirstLogin as IsFirstLogin, 	muser.IsActive,
	--muser.ID_Plant,
	--muser.ID_Center,
	--muser.ID_Division,
	--muser.ID_Department,
	--muser.ID_Section,
	ISNULL(muser.ID_JobGrade,0) AS ID_JobGrade,
	ISNULL(muser.ID_JobTitle,0) AS ID_JobTitle, 
	ISNULL(muser.ID_Role,1) as ID_Role,
	--rol.ID as RoleId,rol.RoleDescription,rol.RoleName_EN,rol.RoleName_TH,
	Plant.ID as ID_Plant,Plant.PlantID,Plant.PlantName_EN,Plant.PlantName_TH,Plant.PlantDescription AS PlantDescription,
	Center.ID as ID_Center,Center.CenterID,Center.CenterName_EN,Center.CenterName_TH,Center.Description AS CenterDescription,
	Division.ID as ID_Division,Division.DivisionID,Division.DivisionName_EN,Division.DivisionName_TH,Division.Description AS DivisionDescription,
	dep.ID as ID_Department,dep.DepartmentID,dep.DepartmentName_EN,dep.DepartmentName_TH,dep.Description AS DepartmentDescription,
	section.ID as ID_Section,section.SectionID,section.SectionName_EN,section.SectionName_TH,section.Description AS DepartmentDescription

	FROM LDMS_M_User  muser WITH (NOLOCK)   
	LEFT OUTER JOIN LDMS_M_Plant plant  WITH (NOLOCK) on muser.ID_Plant = plant.ID
	LEFT OUTER JOIN LDMS_M_Center Center  WITH (NOLOCK) on muser.ID_Center = Center.ID
	LEFT OUTER JOIN LDMS_M_Division Division  WITH (NOLOCK) on muser.ID_Division = Division.ID 
	LEFT OUTER JOIN LDMS_M_Department dep  WITH (NOLOCK) on muser.ID_Department = dep.ID
	LEFT OUTER JOIN LDMS_M_Section section  WITH (NOLOCK) on muser.ID_Section = section.ID
	
	WHERE muser.IsDeleted = 0
	AND (1 =  CASE WHEN @paramEmployeeId IS NOT NULL AND @paramEmployeeId <> '' AND @paramEmployeeId <> ' ' THEN
				CASE WHEN UPPER(muser.EmployeeID) like ''+ UPPER(@paramEmployeeId) +'%' THEN 1 ELSE 0 END
			  ELSE 1 END)
    AND (1 =  CASE WHEN @paramEmployeeName IS NOT NULL AND @paramEmployeeName <> '' AND @paramEmployeeName <> ' ' THEN
				CASE WHEN UPPER(muser.Name) like ''+ UPPER(@paramEmployeeName) +'%' THEN 1 ELSE 0 END
			  ELSE 1 END)
	AND (1 =  CASE WHEN @paramEmployeeName IS NOT NULL AND @paramEmployeeName <> '' AND @paramEmployeeName <> ' ' THEN
				CASE WHEN UPPER(muser.Surname) like ''+ UPPER(@paramEmployeeName) +'%' THEN 1 ELSE 0 END
			  ELSE 1 END)
	AND (1 =  CASE WHEN exists (select * from @departments dp)THEN
				CASE WHEN exists (select * from @departments dp where dp.ID= muser.ID_Department)  THEN 1 ELSE 0 END
			  ELSE 1 END)
	AND (1 =  CASE WHEN exists (select * from @sections sc)THEN
				CASE WHEN exists (select * from @sections sc where sc.ID= muser.ID_Section)  THEN 1 ELSE 0 END
			  ELSE 1 END)
	AND (1 =  CASE WHEN exists (select * from @jobgrades jg)THEN
				CASE WHEN exists (select * from @jobgrades jg where jg.ID= muser.ID_JobGrade)  THEN 1 ELSE 0 END
			  ELSE 1 END)
	AND (1 =  CASE WHEN exists (select * from @jobtitles jt)THEN
				CASE WHEN exists (select * from @jobtitles jt where jt.ID= muser.ID_JobTitle)  THEN 1 ELSE 0 END
			  ELSE 1 END)

END
GO


