-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
/*
declare @p12 dbo.EmployeeIdList
insert into @p12 values(N'ST00100019')
insert into @p12 values(N'ST00100020')
insert into @p12 values(N'ST00100021')

declare @p13 dbo.TopicList
insert into @p13 values(0,2,N'(HR-S002-02) Course Name2')
insert into @p13 values(0,15,N'(HR-S015-03) Course Name15')
insert into @p13 values(0,3,N'(HR-S003-03) Course Name3')
insert into @p13 values(0,7,N'(HR-S007-01) Course Name7')
insert into @p13 values(0,1,N'(HR-S001-01) Course Name1')
insert into @p13 values(0,11,N'(HR-S011-02) Course Name11')
insert into @p13 values(0,24,N'(LCO003) [ST-00001111kdfsdfsdfdsf]czxczcxzxc')
exec [dbo].[usp_CompetenceAnalytic_Update] @ID_CompetenceAnalytic=4,@CompetenceName=N'Competence  Update',@Criteria1=N'Limited Knowledge',@Criteria2=N'Have Knowledge but lack Skill',@Criteria3=N'Have Knowledge with skilled',@Criteria4=N'Have Knowledge and skillful',@Criteria5=N'Professional Skilled & Can train the others',@ID_Department=1,@ID_JobGrade=4,@ID_EmployeeManager=N'STT00001',@UpdateBy=N'STT00001',@EmployeeTable=@p12,@Topics=@p13
*/
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_CompetenceAnalytic_Update]
	-- Add the parameters for the stored procedure here
	@ID_CompetenceAnalytic INT,
	@CompetenceName nvarchar(50),
	@Criteria1 nvarchar(100),
	@Criteria2 nvarchar(100),
	@Criteria3 nvarchar(100),
	@Criteria4 nvarchar(100),
	@Criteria5 nvarchar(100),
	@ID_Department int = null,
	@ID_JobGrade int= null,
	@ID_EmployeeManager  nvarchar(50),
	@UpdateBy  nvarchar(50),
	@EmployeeTable EmployeeIdList readonly,
	@Topics TopicList readonly 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
		IF EXISTS (SELECT * FROM [LDMS_T_CompetenceAnalytic] WHERE [CompetenceAnalyticName] = @CompetenceName AND [ID] <> @ID_CompetenceAnalytic AND IS_ACTIVE = 1)
		BEGIN
			 SELECT -- As many or few of these as you care to return
				100 AS ErrorNumber
			   ,1 AS ErrorSeverity
			   ,1 AS ErrorState
			   ,1 AS ErrorProcedure
			   ,1 AS ErrorLine
			   ,'An CompetenceAnalytic Platform Name already exists' AS ErrorMessage;
			   return;
		END
		--RAISERROR (50001 ,-1,-1, 'An CompetenceAnalytic Platform Name already exists');  

	BEGIN TRANSACTION;
	BEGIN TRY  		
			UPDATE [dbo].[LDMS_T_CompetenceAnalytic_Employee]
			SET Is_Active = 0 
			WHERE [ID_CompetenceAnalytic] = @ID_CompetenceAnalytic and Is_Active =1
			AND NOT EXISTS (SELECT * FROM @EmployeeTable TT WHERE TT.EmployeeID = [LDMS_T_CompetenceAnalytic_Employee].EmployeeID AND [LDMS_T_CompetenceAnalytic_Employee].Is_Active =1)
			
			UPDATE [dbo].[LDMS_T_CompetenceAnalytic_KnwldTopic]
			SET Is_Active = 0
			WHERE [ID_CompetenceAnalytic] = @ID_CompetenceAnalytic and Is_Active =1
			AND NOT EXISTS (SELECT * FROM @Topics TT  WHERE TT.ID_Topic = [LDMS_T_CompetenceAnalytic_KnwldTopic].ID AND [LDMS_T_CompetenceAnalytic_KnwldTopic].Is_Active =1)
			
			UPDATE [dbo].[LDMS_T_CompetenceAnalytic]
			   SET [CompetenceAnalyticName] = @CompetenceName
				  ,[Criteria1] = @Criteria1
				  ,[Criteria2] = @Criteria2
				  ,[Criteria3] = @Criteria3
				  ,[Criteria4] = @Criteria4
				  ,[Criteria5] = @Criteria5 
				  ,[UpdateBy] = @UpdateBy
				  ,[UpdateDate] = GETDATE()
				  ,[ID_Department] = @ID_Department
				  ,[ID_JobGrade] = @ID_JobGrade
				  ,[Is_Active] = 1			 	
			 WHERE ID = @ID_CompetenceAnalytic and Is_Active =1;  


			UPDATE [LDMS_T_CompetenceAnalytic_KnwldTopic]
			SET [Expectatoin] =TB.Expectatoin
				 ,[UpdateBy] = @UpdateBy
				 ,[UpdateDate] = GETDATE()
			FROM [LDMS_T_CompetenceAnalytic_KnwldTopic] JOIN  @Topics  TB ON [LDMS_T_CompetenceAnalytic_KnwldTopic].ID = TB.ID_Topic AND [LDMS_T_CompetenceAnalytic_KnwldTopic].ID_CompetenceAnalytic = @ID_CompetenceAnalytic
			WHERE ISNULL(TB.ID_Topic,0) > 0;

			INSERT INTO [dbo].[LDMS_T_CompetenceAnalytic_Employee] ([ID_CompetenceAnalytic] ,[EmployeeID] ,[CreateBy] ,[CreateDate]  ,[IS_ACTIVE])
			SELECT @ID_CompetenceAnalytic ,TB.EmployeeId ,@UpdateBy ,GETDATE(), 1 FROM @EmployeeTable TB
			WHERE NOT EXISTS (SELECT * FROM [LDMS_T_CompetenceAnalytic_Employee] TT WHERE TT.[ID_CompetenceAnalytic]= @ID_CompetenceAnalytic AND TT.EmployeeID=TB.EmployeeId AND TT.Is_Active =1)

			INSERT INTO [dbo].[LDMS_T_CompetenceAnalytic_KnwldTopic]  ([ID_CompetenceAnalytic] ,[ID_Course] ,[KnowledgeTopicName] ,[CreateBy] ,[CreateDate],[IS_ACTIVE],[Expectatoin])
			SELECT @ID_CompetenceAnalytic ,TB.ID_Course,TB.TopicName,@UpdateBy ,GETDATE(), 1,Expectatoin FROM @Topics TB
			WHERE NOT EXISTS (SELECT * FROM [LDMS_T_CompetenceAnalytic_KnwldTopic] TT WHERE TT.[ID_CompetenceAnalytic]= @ID_CompetenceAnalytic AND TT.ID = TB.ID_Topic AND TT.Is_Active =1);

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
