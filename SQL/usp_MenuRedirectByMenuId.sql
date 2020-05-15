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
-- [usp_MenuRedirectByMenuId] @submenuId ='MD01-02'
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[usp_MenuRedirectByMenuId]
	-- Add the parameters for the stored procedure here
	@submenuId nvarchar(50) 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	select 
	[ID] AS ID_SubModule,
	[SubModuleID] AS SubModuleID,
	[ID_Module] AS ID_Module,
	[SubModuleName_EN] AS SubModuleName_EN,
	[SubModuleName_TH] AS SubModuleName_TH,
	[Sequence] AS "Sequence",
	[Parent] AS "Parent",
	[URL] AS SubModule_URL,
    [Icon],
    [Description] ,
	[IsActive]
	from LDMS_M_SubModule where SubModuleID = @submenuId and [IsActive] =1

END
GO
