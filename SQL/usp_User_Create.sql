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
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[usp_User_Create]
	-- Add the parameters for the stored procedure here 
	@EmployeeId nvarchar(50),
	@EmployeeName nvarchar(50),
	@EmployeeSurName nvarchar(50),
	@JobGradeId INT,
	@JobTitleId INT,
	@CenterId INT,
	@DivisionId INT,
	@DepartmentId INT,
	@SectionId INT,
	@RoleId INT,
	@IsInstructer INT,
	@IsSectionHead INT,
	@Nationality nvarchar(50)  =null,
	@Gender nvarchar(10)  =null,
	@Password  nvarchar(1024) =null,
	@PasswordSalt  nvarchar(1024) =null,
	@Remark nvarchar(250)  =null,
	@PhoneNumber  nvarchar(50)  =null,
	@Email  nvarchar(50)  =null,
	@CreateBy  nvarchar(50)  =null,
	@IDCardNumber nvarchar(50)  =null,
	@JoinDate datetime =null,
	@OutDate datetime =null,
	@DateOfBirth datetime =null,
	@ProfilePath  nvarchar(250)  =null  
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	DECLARE @PlantId INT; 
	SELECT @PlantId = ID_Plant FROM LDMS_M_Center  WHERE ID = @CenterId;
	IF EXISTS (SELECT * FROM LDMS_M_User WHERE EmployeeID = @EmployeeId)
		BEGIN
			 SELECT -- As many or few of these as you care to return
				100 AS ErrorNumber
			   ,1 AS ErrorSeverity
			   ,1 AS ErrorState
			   ,1 AS ErrorProcedure
			   ,1 AS ErrorLine
			   ,'An Employee ID already exists' AS ErrorMessage;
			   return;
		END
		--RAISERROR (50001 ,-1,-1, 'An Employee ID already exists');  

BEGIN TRANSACTION;
BEGIN TRY
INSERT INTO [dbo].[LDMS_M_User] ([EmployeeID] ,[Name] ,[Surname] ,[Gender] ,[Nationality] ,[Email] ,[PhoneNumber]
           ,[IsAD] ,[DriverLicenseID] ,[IDCardNumber] ,[ID_JobGrade] ,[ID_JobTitle] 
		   ,[ID_Plant] ,[ID_Center] ,[ID_Division] ,[ID_Department],[ID_Section] ,[ID_Role]
           ,[JoinDate] ,[OutDate] ,[DateOfBirth] ,[ProfilePath]  ,[Password] ,[PasswordSalt]
           ,[Remark] ,[IsInstructor] ,[IsSectionHead]  ,[IsForceChangePass] ,[IsAllowGPP] ,[Is_FirstLogin]
           ,[IsActive] ,[IsDeleted] ,[IsLocked] ,[CreateBy] ,[CreateDate])
     VALUES
           (@EmployeeId ,@EmployeeName ,@EmployeeSurName ,@Gender ,@Nationality ,@Email ,@PhoneNumber 
		   ,case when @Email is null or @Email ='' then 0 else 1 end ,null ,@IDCardNumber ,@JobGradeId ,@JobTitleId 
		   ,@PlantId ,@CenterId ,@DivisionId ,@DepartmentId ,@SectionId  ,@RoleId
           ,@JoinDate ,@OutDate  ,@DateOfBirth ,@ProfilePath  ,@Password ,@PasswordSalt
           ,@Remark ,@IsInstructer  ,@IsSectionHead ,case when @Email is null or @Email ='' then 1 else 0 end ,0 ,1
           ,1 ,0 ,0 ,@CreateBy ,GETDATE());
		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
	  ROLLBACK TRANSACTION;
	  SELECT -- As many or few of these as you care to return
		ERROR_NUMBER() AS ErrorNumber
	   ,ERROR_SEVERITY() AS ErrorSeverity
	   ,ERROR_STATE() AS ErrorState
	   ,ERROR_PROCEDURE() AS ErrorProcedure
	   ,ERROR_LINE() AS ErrorLine
	   ,ERROR_MESSAGE() AS ErrorMessage;
	END CATCH
END
