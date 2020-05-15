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
-- EXEC [dbo].[usp_CompetenceAnalytic_READ_BY_AnalyticId]  1
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_CompetenceAnalytic_READ_BY_AnalyticId] 
@AnalyticId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT ROW_NUMBER() OVER(ORDER BY al.ID ASC)  as RowIndex,
	al.ID as ID_Analytic,
	al.ID_EmployeeManager,
	al.CompetenceAnalyticName,
	al.Criteria1,
	al.Criteria2,
	al.Criteria3,
	al.Criteria4,
	al.Criteria5,
	al.ID_Department,
	al.ID_JobGrade,
	JobGrade.JobGradeName_EN as JobGrade
	FROM LDMS_T_CompetenceAnalytic al WITH (NOLOCK) 
	LEFT OUTER JOIN LDMS_M_JobGrade JobGrade on JobGrade.ID = al.ID_JobGrade
	where al.ID = @AnalyticId and al.Is_Active=1;

END
GO


