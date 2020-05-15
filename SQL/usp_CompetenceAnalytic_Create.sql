-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
/*
declare @p10 dbo.EmployeeIdList 
declare @p11 dbo.TopicList
insert into @p11 values(0,1,N'(HR-S001-01) Course Name1')
insert into @p11 values(0,11,N'(HR-S011-02) Course Name11')
insert into @p11 values(0,24,N'(LCO003) [ST-00001111kdfsdfsdfdsf]czxczcxzxc')
insert into @p11 values(0,0,N'TTTT')  
insert into @p10 values('5000001144')
insert into @p10 values('5010421485')
insert into @p10 values('5012032455') 
exec [dbo].[usp_CompetenceAnalytic_Create] @CompetenceName=N'Competence  Test',
@Criteria1=N'Limited Knowledge',@Criteria2=N'Have Knowledge but lack Skill',
@Criteria3=N'Have Knowledge with skilled',@Criteria4=N'Have Knowledge and skillful',@Criteria5=N'Professional Skilled & Can train the others',
@ID_Department=1,@ID_JobGrade=NULL,@ID_EmployeeManager=N'STT00001',@CreateBy=N'STT00001',@EmployeeTable=@p10,@Topics=@p11
*/
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_CompetenceAnalytic_Create]
	-- Add the parameters for the stored procedure here
	@CompetenceName nvarchar(50),
	@Criteria1 nvarchar(100),
	@Criteria2 nvarchar(100),
	@Criteria3 nvarchar(100),
	@Criteria4 nvarchar(100),
	@Criteria5 nvarchar(100),
	@ID_Department int = null,
	@ID_JobGrade int= null,
	@ID_EmployeeManager  nvarchar(50),
	@CreateBy  nvarchar(50),
	@EmployeeTable EmployeeIdList readonly,
	@Topics TopicList readonly 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	IF EXISTS (SELECT * FROM [LDMS_T_CompetenceAnalytic] WHERE [CompetenceAnalyticName] = @CompetenceName  AND IS_ACTIVE = 1)
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
			DECLARE @ID_CompetenceAnalytic INT;  

			INSERT INTO [dbo].[LDMS_T_CompetenceAnalytic](ID_EmployeeManager,[CompetenceAnalyticName],[Criteria1]
				   ,[Criteria2] ,[Criteria3],[Criteria4],[Criteria5],[CreateBy],[CreateDate] ,[ID_Department] ,[ID_JobGrade],[IS_ACTIVE])
			VALUES (@ID_EmployeeManager ,@CompetenceName ,@Criteria1  ,@Criteria2 ,@Criteria3 ,@Criteria4 ,@Criteria5 ,@CreateBy ,GETDATE()  ,@ID_Department ,@ID_JobGrade,1);
	
			SELECT @ID_CompetenceAnalytic =  SCOPE_IDENTITY();

			INSERT INTO [dbo].[LDMS_T_CompetenceAnalytic_Employee] ([ID_CompetenceAnalytic] ,[EmployeeID] ,[CreateBy] ,[CreateDate]  ,[IS_ACTIVE])
			SELECT @ID_CompetenceAnalytic ,EmployeeId ,@CreateBy ,GETDATE(), 1 FROM @EmployeeTable;

			INSERT INTO [dbo].[LDMS_T_CompetenceAnalytic_KnwldTopic] 
			([ID_CompetenceAnalytic] ,[ID_Course] ,[KnowledgeTopicName] ,[CreateBy] ,[CreateDate],[IS_ACTIVE],[Expectatoin])
			SELECT @ID_CompetenceAnalytic ,ID_Course,TopicName,@CreateBy ,GETDATE(), 1,Expectatoin  FROM @Topics;

			--INSERT INTO [dbo].[LDMS_T_CompetenceAnalytic_Expectatoin] ([ID_CompetenceAnalytic] ,[ID_CompetenceKnowledgeTopic] ,[Score] ,[CreateBy] ,[CreateDate],[UpdateDate])
			--SELECT @ID_CompetenceAnalytic ,sc.ID_CompetenceKnowledgeTopic,sc.Score,@CreateBy ,getdate() ,getdate()
			--FROM @Expectatoins sc 


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
