--USE [veraplus_ldms]
--GO

/****** Object:  StoredProcedure [dbo].[usp_User_READ_BY_EmployeeId]    Script Date: 4/6/2020 11:09:29 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- EXEC [dbo].[usp_User_READ_BY_DepartmentId] 22
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_User_READ_BY_SectionId]
	-- Add the parameters for the stored procedure here
	@param_SectionId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON; 

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
	--LEFT OUTER JOIN LDMS_M_Role rol  WITH (NOLOCK) on muser.ID_Role = rol.ID
	LEFT OUTER JOIN LDMS_M_Plant plant  WITH (NOLOCK) on muser.ID_Plant = plant.ID
	LEFT OUTER JOIN LDMS_M_Center Center  WITH (NOLOCK) on muser.ID_Center = Center.ID
	LEFT OUTER JOIN LDMS_M_Division Division  WITH (NOLOCK) on muser.ID_Division = Division.ID 
	LEFT OUTER JOIN LDMS_M_Department dep  WITH (NOLOCK) on muser.ID_Department = dep.ID
	LEFT OUTER JOIN LDMS_M_Section section  WITH (NOLOCK) on muser.ID_Section = section.ID
	WHERE muser.ID_Section = @param_SectionId AND muser.IsActive =1

END
GO


