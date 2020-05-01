USE [veraplus_ldms]
GO
/****** Object:  StoredProcedure [dbo].[usp_CompetenceAnalyticScore_READ_BY_AnalyticId]    Script Date: 5/2/2020 12:04:03 AM ******/
DROP PROCEDURE [dbo].[usp_CompetenceAnalyticScore_READ_BY_AnalyticId]
GO
/****** Object:  StoredProcedure [dbo].[usp_CompetenceAnalyticKnowledgeTopic_READ_BY_AnalyticId]    Script Date: 5/2/2020 12:04:03 AM ******/
DROP PROCEDURE [dbo].[usp_CompetenceAnalyticKnowledgeTopic_READ_BY_AnalyticId]
GO
/****** Object:  StoredProcedure [dbo].[usp_CompetenceAnalyticExpectatoin_READ_BY_AnalyticId]    Script Date: 5/2/2020 12:04:03 AM ******/
DROP PROCEDURE [dbo].[usp_CompetenceAnalyticExpectatoin_READ_BY_AnalyticId]
GO
/****** Object:  StoredProcedure [dbo].[usp_CompetenceAnalyticEmployee_READ_BY_AnalyticId]    Script Date: 5/2/2020 12:04:03 AM ******/
DROP PROCEDURE [dbo].[usp_CompetenceAnalyticEmployee_READ_BY_AnalyticId]
GO
/****** Object:  StoredProcedure [dbo].[usp_CompetenceAnalytic_Update]    Script Date: 5/2/2020 12:04:03 AM ******/
DROP PROCEDURE [dbo].[usp_CompetenceAnalytic_Update]
GO
/****** Object:  StoredProcedure [dbo].[usp_CompetenceAnalytic_Score]    Script Date: 5/2/2020 12:04:03 AM ******/
DROP PROCEDURE [dbo].[usp_CompetenceAnalytic_Score]
GO
/****** Object:  StoredProcedure [dbo].[usp_CompetenceAnalytic_READ_BY_AnalyticId]    Script Date: 5/2/2020 12:04:03 AM ******/
DROP PROCEDURE [dbo].[usp_CompetenceAnalytic_READ_BY_AnalyticId]
GO
/****** Object:  StoredProcedure [dbo].[usp_CompetenceAnalytic_READ_ALL]    Script Date: 5/2/2020 12:04:03 AM ******/
DROP PROCEDURE [dbo].[usp_CompetenceAnalytic_READ_ALL]
GO
/****** Object:  StoredProcedure [dbo].[usp_CompetenceAnalytic_Create]    Script Date: 5/2/2020 12:04:03 AM ******/
DROP PROCEDURE [dbo].[usp_CompetenceAnalytic_Create]
GO
/****** Object:  Table [dbo].[LDMS_T_CompetenceAnalytic_Score]    Script Date: 5/2/2020 12:04:03 AM ******/
DROP TABLE [dbo].[LDMS_T_CompetenceAnalytic_Score]
GO
/****** Object:  Table [dbo].[LDMS_T_CompetenceAnalytic_KnwldTopic]    Script Date: 5/2/2020 12:04:04 AM ******/
DROP TABLE [dbo].[LDMS_T_CompetenceAnalytic_KnwldTopic]
GO
/****** Object:  Table [dbo].[LDMS_T_CompetenceAnalytic_Employee]    Script Date: 5/2/2020 12:04:04 AM ******/
DROP TABLE [dbo].[LDMS_T_CompetenceAnalytic_Employee]
GO
/****** Object:  Table [dbo].[LDMS_T_CompetenceAnalytic]    Script Date: 5/2/2020 12:04:04 AM ******/
DROP TABLE [dbo].[LDMS_T_CompetenceAnalytic]
GO
/****** Object:  UserDefinedTableType [dbo].[TopicList]    Script Date: 5/2/2020 12:04:04 AM ******/
DROP TYPE [dbo].[TopicList]
GO
/****** Object:  UserDefinedTableType [dbo].[EmployeeIdList]    Script Date: 5/2/2020 12:04:04 AM ******/
DROP TYPE [dbo].[EmployeeIdList]
GO
/****** Object:  UserDefinedTableType [dbo].[CompetenceScore]    Script Date: 5/2/2020 12:04:04 AM ******/
DROP TYPE [dbo].[CompetenceScore]
GO
/****** Object:  UserDefinedTableType [dbo].[CompetenceExpectatoin]    Script Date: 5/2/2020 12:04:04 AM ******/
DROP TYPE [dbo].[CompetenceExpectatoin]
GO
