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
CREATE OR ALTER PROCEDURE [dbo].[usp_User_Update]
	-- Add the parameters for the stored procedure here 
	@ID_Employee int,
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
	@Remark nvarchar(250)  =null,
	@PhoneNumber  nvarchar(50)  =null,
	@Email  nvarchar(50)  =null,
	@UpdateBy  nvarchar(50)  =null,
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
	IF NOT EXISTS (SELECT * FROM LDMS_M_User WHERE   ID_User = @ID_Employee)
	BEGIN
			 SELECT -- As many or few of these as you care to return
				100 AS ErrorNumber
			   ,1 AS ErrorSeverity
			   ,1 AS ErrorState
			   ,1 AS ErrorProcedure
			   ,1 AS ErrorLine
			   ,'An Employee ID doen''t exists' AS ErrorMessage;
			   return;
		END
		--RAISERROR (50001 ,-1,-1, 'An Employee ID doen''t exists');  

BEGIN TRANSACTION;
BEGIN TRY

   UPDATE [dbo].[LDMS_M_User]
   SET [EmployeeID] = @EmployeeId
      ,[Name] = @EmployeeName
      ,[Surname] =@EmployeeSurName
      ,[Gender] = @Gender
      ,[Nationality] =@Nationality
      ,[Email] = @Email
      ,[PhoneNumber] = @PhoneNumber
      ,[IsAD] = case when @Email is null or @Email ='' then 0 else 1 end
      ,[DriverLicenseID] = ''
      ,[IDCardNumber] =@IDCardNumber
      ,[ID_JobGrade] = @JobGradeId
      ,[ID_JobTitle] = @JobTitleId
      ,[ID_Plant] = @PlantId
      ,[ID_Center] = @CenterId
      ,[ID_Division] =@DivisionId
      ,[ID_Department] =@DepartmentId
      ,[JoinDate] = @JoinDate
      ,[OutDate] = @OutDate
      ,[DateOfBirth] = @DateOfBirth
      ,[ProfilePath] =@ProfilePath
      ,[ID_Role] = @RoleId 
      ,[Remark] = @Remark
      ,[IsInstructor] = @IsInstructer
      ,[IsSectionHead] = @IsSectionHead
      ,[ID_Section] =@SectionId   
      ,[IsActive] = 1
      ,[IsDeleted] = 0
      ,[IsLocked] =0 
      ,[UpdateBy] =@UpdateBy
      ,[UpdateDate] = GETDATE()
	WHERE ID_User  = @ID_Employee; 
	 
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
GO
