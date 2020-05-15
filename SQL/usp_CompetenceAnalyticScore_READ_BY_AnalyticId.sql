--USE [veraplus_ldms]
--GO

/****** Object:  StoredProcedure [dbo].[usp_Center_READ_ALL]    Script Date: 4/6/2020 11:07:58 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- EXEC [dbo].[usp_CompetenceAnalyticScore_READ_BY_AnalyticId] 1
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_CompetenceAnalyticScore_READ_BY_AnalyticId] 
@AnalyticId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT ROW_NUMBER() OVER( ORDER BY TB.ID_CompetenceKnowledgeTopic ASC)  as RowIndex,TB.* FROM
	(	
		SELECT
		score.ID_CompetenceAnalytic, 
		score.ID_CompetenceKnowledgeTopic,
		score.ID_CompetenceEmployee,
		ISNULL(score.Score,0) as Score,
		CAST(0 AS INT) as MinScore,
		CAST(5 AS INT) as MaxScore
		FROM LDMS_T_CompetenceAnalytic_Employee emp WITH (NOLOCK) 
		INNER JOIN LDMS_T_CompetenceAnalytic_Score score WITH (NOLOCK)  on score.ID_CompetenceEmployee = emp.EmployeeID and emp.Is_Active =1 and emp.ID_CompetenceAnalytic = score.ID_CompetenceAnalytic
		INNER JOIN LDMS_T_CompetenceAnalytic_KnwldTopic topc WITH (NOLOCK)  on score.ID_CompetenceKnowledgeTopic = topc.ID and emp.Is_Active =1 and topc.ID_CompetenceAnalytic = score.ID_CompetenceAnalytic and topc.Is_Active =1 
		where score.ID_CompetenceAnalytic = @AnalyticId AND ISNULL(topc.ID_Course,0) = 0

		UNION ALL 

		SELECT
		emp.ID_CompetenceAnalytic, 
		tpc.ID as ID_CompetenceKnowledgeTopic,
		emp.EmployeeID AS ID_CompetenceEmployee,
		CAST(0 AS INT) as Score,
		CAST(0 AS INT) as MinScore,
		CAST(5 AS INT) as MaxScore
		FROM LDMS_T_CompetenceAnalytic_Employee emp WITH (NOLOCK) 
		INNER JOIN LDMS_T_CompetenceAnalytic_KnwldTopic tpc WITH (NOLOCK) on tpc.ID_CompetenceAnalytic = emp.ID_CompetenceAnalytic and tpc.Is_Active =1 
		where emp.ID_CompetenceAnalytic = @AnalyticId AND ISNULL(tpc.ID_Course,0) = 0 and emp.Is_Active =1  
		and not exists (select * from LDMS_T_CompetenceAnalytic_Score score WITH (NOLOCK) where score.ID_CompetenceAnalytic = @AnalyticId and score.ID_CompetenceEmployee = emp.EmployeeID and score.ID_CompetenceKnowledgeTopic=tpc.ID)

		UNION ALL

		SELECT
		emp.ID_CompetenceAnalytic, 
		topc.ID AS ID_CompetenceKnowledgeTopic,
		emp.EmployeeID AS ID_CompetenceEmployee,

		(CASE WHEN score.Score IS NOT NULL THEN score.Score
		      WHEN res.LearningResult = 99 OR res.LearningResult = 30 THEN 0
			 WHEN res.LearningResult = 70 THEN 3
			 ELSE 0 END) Score,

		(CASE WHEN res.LearningResult = 99 OR res.LearningResult = 30 THEN 0
			 WHEN res.LearningResult = 70 THEN 3
			 ELSE 0 END) AS MinScore,

		(CASE WHEN res.LearningResult = 99 OR res.LearningResult = 30 THEN 2
			 WHEN res.LearningResult = 70 THEN 5
			 ELSE 0 END) AS MaxScore 

		FROM LDMS_T_CompetenceAnalytic_Employee emp WITH (NOLOCK)
		INNER JOIN LDMS_T_CompetenceAnalytic_KnwldTopic topc WITH (NOLOCK) on  topc.ID_CompetenceAnalytic = emp.ID_CompetenceAnalytic and topc.Is_Active =1  
		INNER JOIN LDMS_T_CompetenceAnalytic_Score score WITH (NOLOCK) on score.ID_CompetenceEmployee = emp.EmployeeID and emp.Is_Active =1 and emp.ID_CompetenceAnalytic = score.ID_CompetenceAnalytic and score.ID_CompetenceKnowledgeTopic = topc.ID
		LEFT OUTER JOIN LDMS_T_ClassAttendAndResult res WITH (NOLOCK) on topc.ID_Course = res.ID_Course and emp.EmployeeID = res.EmployeeID
		where emp.ID_CompetenceAnalytic = @AnalyticId AND ISNULL(topc.ID_Course,0)<> 0 and emp.Is_Active =1 

		UNION ALL

		SELECT
		emp.ID_CompetenceAnalytic, 
		topc.ID AS ID_CompetenceKnowledgeTopic,
		emp.EmployeeID AS ID_CompetenceEmployee,

		(CASE WHEN res.LearningResult = 99 OR res.LearningResult = 30 THEN 0
			 WHEN res.LearningResult = 70 THEN 3
			 ELSE 0 END) Score,

		(CASE WHEN res.LearningResult = 99 OR res.LearningResult = 30 THEN 0
			 WHEN res.LearningResult = 70 THEN 3
			 ELSE 0 END) AS MinScore,

		(CASE WHEN res.LearningResult = 99 OR res.LearningResult = 30 THEN 2
			 WHEN res.LearningResult = 70 THEN 5
			 ELSE 0 END) AS MaxScore 

		FROM LDMS_T_CompetenceAnalytic_Employee emp WITH (NOLOCK)
		INNER JOIN LDMS_T_CompetenceAnalytic_KnwldTopic topc WITH (NOLOCK) on  topc.ID_CompetenceAnalytic = emp.ID_CompetenceAnalytic and topc.Is_Active =1  
		LEFT OUTER JOIN LDMS_T_ClassAttendAndResult res WITH (NOLOCK) on topc.ID_Course = res.ID_Course and emp.EmployeeID = res.EmployeeID
		where emp.ID_CompetenceAnalytic = @AnalyticId AND ISNULL(topc.ID_Course,0)<> 0 and emp.Is_Active =1 
		and not exists (select * from LDMS_T_CompetenceAnalytic_Score score WITH (NOLOCK) where score.ID_CompetenceAnalytic = @AnalyticId and score.ID_CompetenceEmployee = emp.EmployeeID and score.ID_CompetenceKnowledgeTopic=topc.ID)


	) TB ORDER BY TB.ID_CompetenceKnowledgeTopic,TB.ID_CompetenceEmployee
	
	--if pass default = 3 can changes 3,4,5
	--if not pass default = 0 can change 0,1,2
	--if no result fixed  = 0
END
GO


