USE [veraplus_ldms]
GO
/****** Object:  UserDefinedTableType [dbo].[EmployeeIdList]    Script Date: 4/24/2020 8:06:06 AM ******/
CREATE TYPE [dbo].[EmployeeIdList] AS TABLE(
	[EmployeeId] [nvarchar](50) NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[IDList]    Script Date: 4/24/2020 8:06:06 AM ******/
CREATE TYPE [dbo].[IDList] AS TABLE(
	[ID] [int] NULL
)
GO
/****** Object:  UserDefinedFunction [dbo].[genClassID]    Script Date: 4/24/2020 8:06:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[genClassID]
(
    @paramCourseID varchar(50)
	
)
RETURNS varchar(100) -- or whatever length you need
AS
BEGIN
    DECLARE @ClassID as varchar(50);
	Declare @running int;
	select @ClassID = isnull(max([ClassID]), @paramCourseID+ '-' + convert(varchar(4),YEAR(getdate())) + '-' + '00')
	from [dbo].[LDMS_T_Class]
	where [ClassID] like @paramCourseID + '-'  + convert(varchar(4),YEAR(getdate())) + '%'
	set @running =  cast(Substring(@ClassID, len(@ClassID) - 1, 2) as int) + 1;
	set @ClassID = @paramCourseID + '-'  + convert(varchar(4),YEAR(getdate())) + '-' + format(@running, '00')
    RETURN  @ClassID

END
GO
/****** Object:  UserDefinedFunction [dbo].[genCourseID]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[genCourseID]
(
    @paramIDCourseType int
	,@paramDeptID  varchar(10)
)
RETURNS varchar(100) -- or whatever length you need
AS
BEGIN
    Declare @prefix varchar(50);
	Declare @suffix varchar(50);
	Declare @CourseID varchar(50);
	Declare @running int;
    SELECT @prefix = CourseType.PrefixCode, @suffix = case when CourseType.SubPrefixCode is not null then  @paramDeptID else '' end
	from [LDMS_M_CourseType] As CourseType
    where CourseType.ID = @paramIDCourseType
	if (@prefix is null) return 'NO VALID COURSE ID';
	Select @CourseID = isnull(max(CourseID), @prefix + isnull(@suffix, '') + '000')
	from [dbo].[LDMS_M_Course]
	where CourseID like @prefix + isnull(@suffix, '') + '%'

	set @running =  cast(Substring(@CourseID, len(@CourseID) - 2, len(@CourseID)) as int) + 1;
	set @CourseID = @prefix + isnull(@suffix, '') + format(@running, '000');
    RETURN  @CourseID

END
GO
/****** Object:  UserDefinedFunction [dbo].[SplitInts]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[SplitInts]
(
   @List      VARCHAR(MAX),
   @Delimiter VARCHAR(255)
)
RETURNS TABLE
AS
  RETURN ( SELECT Item = CONVERT(INT, Item) FROM
      ( SELECT Item = x.i.value('(./text())[1]', 'varchar(max)')
        FROM ( SELECT [XML] = CONVERT(XML, '<i>'
        + REPLACE(@List, @Delimiter, '</i><i>') + '</i>').query('.')
          ) AS a CROSS APPLY [XML].nodes('i') AS x(i) ) AS y
      WHERE Item IS NOT NULL
  );
GO
/****** Object:  View [dbo].[VW_Flat_Course_Employee]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

Create VIEW [dbo].[VW_Flat_Course_Employee]
AS
select display.ID_COURSE, ISNULL(left(display.[EmployeeID], LEN(display.[EmployeeID]) -2), '') as [EmployeeID]
from (
select distinct mainTable.ID_COURSE, innerdisplay.[EmployeeID] 
from [dbo].[LDMS_T_CourseEmployee] mainTable
cross apply (select concattable.[EmployeeID] + ', '
			from [dbo].[LDMS_T_CourseEmployee] concattable
			where mainTable.ID_COURSE = concattable.ID_COURSE
			order by concattable.ID_COURSE
			for xml path ('')) as innerdisplay ([EmployeeID]))display
GO
/****** Object:  View [dbo].[VW_Flat_Course_JobGrade]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

Create VIEW [dbo].[VW_Flat_Course_JobGrade]
AS
select display.ID_COURSE, ISNULL(left(display.[ID_JobGrade], LEN(display.[ID_JobGrade]) -2), '') as [ID_JobGrade]
from (
select distinct mainTable.ID_COURSE, innerdisplay.[ID_JobGrade] 
from [dbo].[LDMS_T_CourseJobGrade] mainTable
cross apply (select concattable.[ID_JobGrade] + ', '
			from [dbo].[LDMS_T_CourseJobGrade] concattable
			where mainTable.ID_COURSE = concattable.ID_COURSE
			order by concattable.ID_COURSE
			for xml path ('')) as innerdisplay ([ID_JobGrade]))display
GO
/****** Object:  View [dbo].[VW_Flat_Course_JobTitle]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[VW_Flat_Course_JobTitle]
AS
select display.ID_COURSE, ISNULL(left(display.[ID_JobTitle], LEN(display.[ID_JobTitle]) -2), '') as [ID_JobTitle]
from (
select distinct mainTable.ID_COURSE, innerdisplay.[ID_JobTitle] 
from [dbo].[LDMS_T_CourseJobTitle] mainTable
cross apply (select concattable.[ID_JobTitle] + ', '
			from [dbo].[LDMS_T_CourseJobTitle] concattable
			where mainTable.ID_COURSE = concattable.ID_COURSE
			order by concattable.ID_COURSE
			for xml path ('')) as innerdisplay ([ID_JobTitle]))display
GO
/****** Object:  View [dbo].[VW_M_Course_Display]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE VIEW [dbo].[VW_M_Course_Display]
AS
SELECT       dbo.LDMS_M_Course.CourseID, dbo.LDMS_M_Course.ID, dbo.LDMS_M_Course.CourseName, dbo.LDMS_M_Course.ID_LearnMethod, dbo.LDMS_M_Course.ID_CourseType, dbo.LDMS_M_Course.Objective, 
                         dbo.LDMS_M_Course.Description, dbo.LDMS_M_Course.OutLine, dbo.LDMS_M_Course.IsRefreshment, dbo.LDMS_M_Course.RefreshmentPeriod, dbo.LDMS_M_Course.RefreshmentUnit, dbo.LDMS_M_Course.ID_PlantTarget, 
                         dbo.LDMS_M_Course.ID_CenterTarget, dbo.LDMS_M_Course.ID_DivisionTarget, dbo.LDMS_M_Course.ID_DepartmentTarget, dbo.LDMS_M_Course.ID_SectionTarget, dbo.LDMS_M_Course.ID_DepartmentCreate, 
                         dbo.LDMS_M_Course.CreateBy, dbo.LDMS_M_Course.CreateDate, dbo.LDMS_M_Course.UpdateBy, dbo.LDMS_M_Course.UpdateDate, dbo.LDMS_M_Course.IsActive, dbo.VW_Flat_Course_Employee.EmployeeID AS TargetEmployeeID, 
                         dbo.VW_Flat_Course_JobGrade.ID_JobGrade AS TargetJobgrade, dbo.VW_Flat_Course_JobTitle.ID_JobTitle AS TargetJobTitle
FROM            dbo.LDMS_M_Course LEFT JOIN
                         dbo.VW_Flat_Course_Employee ON dbo.LDMS_M_Course.ID = dbo.VW_Flat_Course_Employee.ID_COURSE LEFT JOIN
                         dbo.VW_Flat_Course_JobGrade ON dbo.LDMS_M_Course.ID = dbo.VW_Flat_Course_JobGrade.ID_COURSE LEFT JOIN
                         dbo.VW_Flat_Course_JobTitle ON dbo.LDMS_M_Course.ID = dbo.VW_Flat_Course_JobTitle.ID_COURSE
GO
/****** Object:  StoredProcedure [dbo].[sp_M_Amphur_Select]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_M_Amphur_Select]
	@PROVINCE_ID int
AS
BEGIN
	
	SET NOCOUNT ON;

    SELECT [AMPHUR_ID]
      ,[AMPHUR_CODE]
      ,[AMPHUR_NAME]
      ,[AMPHUR_NAME_ENG]
      ,[GEO_ID]
      ,[PROVINCE_ID]
	FROM [dbo].[LDMS_M_Amphurs]
	WHERE [PROVINCE_ID] = @PROVINCE_ID
	ORDER BY [AMPHUR_NAME]



END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_CodeLookUp_Select]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_M_CodeLookUp_Select]
	@TableName nvarchar(50),
	@FieldName nvarchar(50)
AS
BEGIN
	
	SET NOCOUNT ON;

    SELECT [ID]
      ,[TableName]
      ,[FieldName]
      ,[Code]
      ,[StateName]
      ,[Sequence]
      ,[Description]
      ,[IsDefault]
	FROM [dbo].[LDMS_M_CodeLookUp]
	WHERE [IsActive] = 1 AND [TableName] = @TableName AND [FieldName] = @FieldName
	ORDER BY [Sequence] ASC


END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_Course_Insert]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_M_Course_Insert]
			@paramCourseName nvarchar(50)
           ,@paramID_LearnMethod int
           ,@paramID_CourseType int
           ,@paramObjective nvarchar(200)
           ,@paramDescription nvarchar(200)
           ,@paramOutLine nvarchar(200)
           ,@paramIsRefreshment int
           ,@paramRefreshmentPeriod int
           ,@paramRefreshmentUnit nvarchar(10)
           ,@paramTargetEmployeeID EmployeeIdList REadonly
           ,@paramID_PlantTarget int
           ,@paramID_CenterTarget int
           ,@paramID_DivisionTarget int
           ,@paramID_DepartmentTarget int
           ,@paramID_SectionTarget int
           ,@paramJobGradeTargetID EmployeeIdList REadonly
           ,@paramJobTitleTargetID EmployeeIdList REadonly
           ,@paramID_DepartmentCreate int
           ,@paramCreateBy nvarchar(50)
           
           
           ,@paramIsActive tinyint
AS
BEGIN
	Declare @paramCourseID nvarchar(50)
	DEclare @IDCOurse int
	SET NOCOUNT ON;
	set @paramCourseID = null
	SET @paramCourseID =  dbo.[genCourseID](@paramID_CourseType,@paramID_DepartmentCreate);
	

    INSERT INTO [dbo].[LDMS_M_Course]
           ([CourseID]
           ,[CourseName]
           ,[ID_LearnMethod]
           ,[ID_CourseType]
           ,[Objective]
           ,[Description]
           ,[OutLine]
           ,[IsRefreshment]
           ,[RefreshmentPeriod]
           ,[RefreshmentUnit]
           --,[TargetEmployeeID]
           ,[ID_PlantTarget]
           ,[ID_CenterTarget]
           ,[ID_DivisionTarget]
           ,[ID_DepartmentTarget]
           ,[ID_SectionTarget]
           --,[JobGradeTargetID]
           --,[JobTitleTargetID]
           ,[ID_DepartmentCreate]
           ,[CreateBy]
           ,[CreateDate]
           ,[UpdateBy]
           ,[UpdateDate]
           ,[IsActive])
     VALUES
           (@paramCourseID			
			,@paramCourseName
			,@paramID_LearnMethod
			,@paramID_CourseType		
			,@paramObjective		
			,@paramDescription		
			,@paramOutLine			
			,@paramIsRefreshment		
			,@paramRefreshmentPeriod	
			,@paramRefreshmentUnit		
			--,@paramTargetEmployeeID		
			,@paramID_PlantTarget		
			,@paramID_CenterTarget		
			,@paramID_DivisionTarget	
			,@paramID_DepartmentTarget	
			,@paramID_SectionTarget		
			--,@paramJobGradeTargetID		
			--,@paramJobTitleTargetID		
			,@paramID_DepartmentCreate	
			,@paramCreateBy			
			,getdate()		
			,@paramCreateBy			
			,getdate()		
			,@paramIsActive)

	SELECT @IDCOurse = ID from [LDMS_M_Course] where CourseID =  @paramCourseID
	INSERT INTO [dbo].[LDMS_T_CourseEmployee]
           ([ID_Course]
           ,[EmployeeID]
           ,[CreateDateTime])
    Select @IDCOurse as [ID_Course] ,[EmployeeId] as [EmployeeID], getdate() as [CreateDateTime]
	from @paramTargetEmployeeID

	INSERT INTO [dbo].[LDMS_T_CourseJobGrade]
           ([ID_Course]
           ,[ID_JobGrade]
           ,[CreateDateTime])
	select @IDCOurse as [ID_Course],[EmployeeId] as [ID_JobGrade], getdate() as [CreateDateTime]
	from @paramJobGradeTargetID

	INSERT INTO [dbo].[LDMS_T_CourseJobTitle]
           ([ID_Course]
           ,[ID_JobTitle]
           ,[CreateDateTime])
	select @IDCOurse as [ID_Course],[EmployeeId] as [ID_JobTitle], getdate() as [CreateDateTime]
	from @paramJobTitleTargetID

	 SELECT [CourseID]
      ,[ID]
      ,[CourseName]
      ,[ID_LearnMethod]
      ,[ID_CourseType]
      ,[Objective]
      ,[Description]
      ,[OutLine]
      ,[IsRefreshment]
      ,[RefreshmentPeriod]
      ,[RefreshmentUnit]
      ,[ID_PlantTarget]
      ,[ID_CenterTarget]
      ,[ID_DivisionTarget]
      ,[ID_DepartmentTarget]
      ,[ID_SectionTarget]
      ,[ID_DepartmentCreate]
      ,[CreateBy]
      ,[CreateDate]
      ,[UpdateBy]
      ,[UpdateDate]
      ,[IsActive]
      ,[TargetEmployeeID]
      ,[TargetJobgrade]
      ,[TargetJobTitle]
  FROM [dbo].[VW_M_Course_Display] where id= @IDCOurse 
END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_Course_Select]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
CREATE  PROCEDURE [dbo].[sp_M_Course_Select]
	 @paramCourseId nvarchar(50) = null,
	 @paramCourseName nvarchar(50) = null,
	 @paramCourseStatus varchar(1) = null,
	 @paramLearnMethod varchar(1) = null
	 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	DECLARE @sql varchar(3000)
	
	DECLARE @CountColumn varchar(50)
	DECLARE @RowFilter varchar(1500)
	SET @RowFilter = ' WHERE 1 = 1 '

	IF @paramCourseId IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND c.[CourseID] LIKE ''%'+@paramCourseId+ '%'' '

	IF @paramCourseName IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND c.[CourseName] LIKE ''%'+@paramCourseName+ '%'' ' 
	
	IF @paramLearnMethod IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND c.[ID_LearnMethod] = ' + @paramLearnMethod + ' '

	IF @paramCourseStatus IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND c.IsActive = ' + @paramCourseStatus + ' '



	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SET @Sql = 'SELECT  
				ROW_NUMBER() OVER(ORDER BY c.CourseID ASC)  as RowIndex, 
				c.ID,
				c.CourseID,
				C.CourseName,
				c.ID_LearnMethod,
				cm.Course_LearnMethodName_EN,
				CASE WHEN c.IsActive = 1 THEN
						''Active''
					 WHEN c.IsActive = 0 THEN
						''In Active''
				END
				AS  CourseStatus

				FROM  LDMS_M_Course  c WITH (NOLOCK) 
				LEFT OUTER JOIN LDMS_M_CourseLearnMethod cm WITH (NOLOCK)  on c.ID_LearnMethod = cm.ID ' + @RowFilter

    print (@Sql)
	exec (@Sql) 

	--WHERE c.IsActive = @paramCourseStatus

	/*AND  c.ID_LearnMethod = @paramLearnMethod OR @paramLearnMethod IS NOT NULL

	AND (1 =  CASE WHEN @paramCourseId IS NOT NULL AND @paramCourseId <> '' AND @paramCourseId <> ' ' THEN
				CASE WHEN UPPER(c.CourseID) like '%'+ UPPER(@paramCourseId) +'%' THEN 1 ELSE 0 END
			  ELSE 1 END)
    AND (1 =  CASE WHEN @paramCourseName IS NOT NULL AND @paramCourseName <> '' AND @paramCourseName <> ' ' THEN
				CASE WHEN UPPER(c.CourseName) like '%'+ UPPER(@paramCourseName) +'%' THEN 1 ELSE 0 END
			  ELSE 1 END)

	--AND ( @paramLearnMethod IS NOT NULL AND c.ID_LearnMethod = @paramLearnMethod )*/
			  
	
END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_Course_Select_ByID]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
CREATE  PROCEDURE [dbo].[sp_M_Course_Select_ByID]
	 @paramIDCourse int 
	 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	DECLARE @sql varchar(3000)
	
	DECLARE @CountColumn varchar(50)
	DECLARE @RowFilter varchar(1500)
	SET @RowFilter = ' WHERE 1 = 1 '

	



	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT [ID]
      ,[CourseID]
      ,[CourseName]
      ,[ID_LearnMethod]
      ,[ID_CourseType]
      ,[Objective]
      ,[Description]
      ,[OutLine]
      ,[IsRefreshment]
      ,[RefreshmentPeriod]
      ,[RefreshmentUnit]
      ,[TargetEmployeeID]
      ,[ID_PlantTarget]
      ,[ID_CenterTarget]
      ,[ID_DivisionTarget]
      ,[ID_DepartmentTarget]
      ,[ID_SectionTarget]
      ,TargetJobgrade  as [JobGradeTargetID]
      ,TargetJobTitle as[JobTitleTargetID]
      ,[ID_DepartmentCreate]
      ,[CreateBy]
      ,[CreateDate]
      ,[UpdateBy]
      ,[UpdateDate]
      ,[IsActive]
	FROM [dbo].[VW_M_Course_Display]
	where id = @paramIDCourse

	--WHERE c.IsActive = @paramCourseStatus

	/*AND  c.ID_LearnMethod = @paramLearnMethod OR @paramLearnMethod IS NOT NULL

	AND (1 =  CASE WHEN @paramCourseId IS NOT NULL AND @paramCourseId <> '' AND @paramCourseId <> ' ' THEN
				CASE WHEN UPPER(c.CourseID) like '%'+ UPPER(@paramCourseId) +'%' THEN 1 ELSE 0 END
			  ELSE 1 END)
    AND (1 =  CASE WHEN @paramCourseName IS NOT NULL AND @paramCourseName <> '' AND @paramCourseName <> ' ' THEN
				CASE WHEN UPPER(c.CourseName) like '%'+ UPPER(@paramCourseName) +'%' THEN 1 ELSE 0 END
			  ELSE 1 END)

	--AND ( @paramLearnMethod IS NOT NULL AND c.ID_LearnMethod = @paramLearnMethod )*/
			  
	
END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_Course_SelectByInstructor]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_M_Course_SelectByInstructor]
	@ID_Instructor int
AS
BEGIN
	
	SET NOCOUNT ON;

	SELECT c.[ID] [ID_Class], cu.[ID] [ID_Course], ps.[ID_Platform], sc.[ID_SubPlatform],
	  cu.[CourseID], cu.[CourseName], pl.[PlatformName_TH], pl.[PlatformName_EN], cu.[IsActive],
	  cu.[UpdateDate],
	  (SELECT ISNULL(AVG(cs.[PostTestScore]),0)  FROM [dbo].[LDMS_T_ClassAttendAndResult] cs WHERE cs.[ID_Class] = c.[ID] AND cs.[ID_Course] = cu.[ID]) [avgScore]
	FROM [dbo].[LDMS_T_Class] c 
	 LEFT JOIN [dbo].[LDMS_M_Course] cu ON cu.[ID] = c.[ID_Course]
	 LEFT JOIN [dbo].[LDMS_M_SubPlatformCourse] sc ON sc.[ID_Course] = cu.[ID]
	 LEFT JOIN [dbo].[LDMS_M_SubPlatform] ps ON ps.ID = sc.[ID_SubPlatform]
	 LEFT JOIN [dbo].[LDMS_M_Platform] pl ON pl.[ID] = ps.[ID_Platform]
	WHERE c.[ID_Instructor] = @ID_Instructor

   
END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_Course_SelectPaging]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--EXEC [dbo].[sp_M_Instructor_SelectPaging] NULL, NULL, NULL, NULL, 'INT', NULL, NULL, 1, 10, '',''
CREATE PROCEDURE [dbo].[sp_M_Course_SelectPaging]
	@Course_ID varchar(100) = NULL,
	@IsActive varchar(10) = NULL,
	@Course_Name varchar(100) = NULL,
	@Learn_Method varchar(500) = NULL,
	@PageNum  int = null,
	@PageSize int = null,
	@SortField varchar(100) = null,
	@SortOrder varchar(4) = null
AS
BEGIN
	
	SET NOCOUNT ON;

	DECLARE @sql varchar(3000)
	
	DECLARE @CountColumn varchar(50)
	DECLARE @RowFilter varchar(1500)
	SET @RowFilter = ' WHERE 1 = 1'

	IF @Course_ID IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND i.[CourseID] LIKE ''%'+@Course_ID+ '%'' '

	IF @IsActive IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND i.[IsActive] = '+@IsActive+ ' '
	
	IF @Course_Name IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND i.[CourseName] LIKE = ''%'+@Course_Name+ '%'' '

	IF @Learn_Method IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND i.[ID_LearnMethod] = '+@Learn_Method+ ' '

	
	
	SET @CountColumn = '[CourseID]'
	
	if isnull(@SortOrder, '') = '' 
	   set @SortOrder = ''

    IF isnull(@SortField, '') = ''
		SET @SortField = '[CourseName] DESC'
	ELSE
		SET @SortField = @SortField+' '+ @SortOrder

	SET @sql = 'SELECT i.ID, i.CourseID, i.CourseName, o.Course_LearnMethodName_EN, 
	case i.isActive when 1 then ''active'' else ''inactive'' end as CourseStatus
  FROM [dbo].[LDMS_M_Course] i
  LEFT JOIN [dbo].[LDMS_M_CourseLearnMethod] o on i.ID_LearnMethod = o.ID '+ @RowFilter;

	 exec dbo.sp_PagingHelper @PageNum,@PageSize,@SortField,@sql,@CountColumn

	

END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_Course_Update]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE  PROCEDURE [dbo].[sp_M_Course_Update]
	-- Add the parameters for the stored procedure here 
	@ID_Course			int
	,@paramCourseID			nvarchar(50)
	,@paramCourseName		nvarchar(50)            
,@paramID_LearnMethod		int                 
,@paramID_CourseType		int                  
,@paramObjective		nvarchar(200)            
,@paramDescription		nvarchar(200)          
,@paramOutLine			nvarchar(200)              
,@paramIsRefreshment		int                  
,@paramRefreshmentPeriod	int              
,@paramRefreshmentUnit		nvarchar(10)       
,@paramTargetEmployeeID		EmployeeIdList Readonly
,@paramID_PlantTarget		int                 
,@paramID_CenterTarget		int                
,@paramID_DivisionTarget	int              
,@paramID_DepartmentTarget	int            
,@paramID_SectionTarget		int               
,@paramJobGradeTargetID		EmployeeIdList Readonly    
,@paramJobTitleTargetID		EmployeeIdList Readonly             
,@paramID_DepartmentCreate	int            
,@paramUpdateBy			nvarchar(50) = NULL
,@paramIsActive			tinyint	 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

BEGIN TRANSACTION;
BEGIN TRY
	UPDATE [dbo].[LDMS_M_Course]
       SET [CourseID]		= @paramCourseID
      ,[CourseName]		= @paramCourseName		
      ,[ID_LearnMethod]		= @paramID_LearnMethod		
      ,[ID_CourseType]		= @paramID_CourseType		
      ,[Objective]		= @paramObjective		
      ,[Description]		= @paramDescription		
      ,[OutLine]		= @paramOutLine			
      ,[IsRefreshment]		= @paramIsRefreshment		
      ,[RefreshmentPeriod]	= @paramRefreshmentPeriod	
      ,[RefreshmentUnit]	= @paramRefreshmentUnit		
     -- ,[TargetEmployeeID]	= @paramTargetEmployeeID	
      ,[ID_PlantTarget]		= @paramID_PlantTarget		
      ,[ID_CenterTarget]	= @paramID_CenterTarget		
      ,[ID_DivisionTarget]	= @paramID_DivisionTarget	
      ,[ID_DepartmentTarget]	= @paramID_DepartmentTarget	
      ,[ID_SectionTarget]	= @paramID_SectionTarget	
    --  ,[JobGradeTargetID]	= @paramJobGradeTargetID	
     -- ,[JobTitleTargetID]	= @paramJobTitleTargetID	
      ,[ID_DepartmentCreate]	= @paramID_DepartmentCreate	
      ,[UpdateBy]		= @paramUpdateBy		
      ,[UpdateDate]		= getdate()		
      ,[IsActive]		= @paramIsActive
    WHERE [ID] = @ID_Course
	
	Delete from [dbo].[LDMS_T_CourseJobGrade]
	where [ID_Course] = @ID_Course

	Delete from [dbo].[LDMS_T_CourseJobGrade]
	where [ID_Course] = @ID_Course

	Delete from [dbo].[LDMS_T_CourseJobTitle]
	where [ID_Course] = @ID_Course

	INSERT INTO [dbo].[LDMS_T_CourseEmployee]
           ([ID_Course]
           ,[EmployeeID]
           ,[CreateDateTime])
    Select @ID_Course as [ID_Course] ,[EmployeeId] as [EmployeeID], getdate() as [CreateDateTime]
	from @paramTargetEmployeeID
		
	INSERT INTO [dbo].[LDMS_T_CourseJobGrade]
           ([ID_Course]
           ,[ID_JobGrade]
           ,[CreateDateTime])
	select @ID_Course as [ID_Course],[EmployeeId] as [ID_JobGrade], getdate() as [CreateDateTime]
	from @paramJobGradeTargetID

	INSERT INTO [dbo].[LDMS_T_CourseJobTitle]
           ([ID_Course]
           ,[ID_JobTitle]
           ,[CreateDateTime])
	select @ID_Course as [ID_Course],[EmployeeId] as [ID_JobTitle], getdate() as [CreateDateTime]
	from @paramJobTitleTargetID

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

	SELECT [CourseID]
      ,[ID]
      ,[CourseName]
      ,[ID_LearnMethod]
      ,[ID_CourseType]
      ,[Objective]
      ,[Description]
      ,[OutLine]
      ,[IsRefreshment]
      ,[RefreshmentPeriod]
      ,[RefreshmentUnit]
      ,[ID_PlantTarget]
      ,[ID_CenterTarget]
      ,[ID_DivisionTarget]
      ,[ID_DepartmentTarget]
      ,[ID_SectionTarget]
      ,[ID_DepartmentCreate]
      ,[CreateBy]
      ,[CreateDate]
      ,[UpdateBy]
      ,[UpdateDate]
      ,[IsActive]
      ,[TargetEmployeeID]
      ,[TargetJobgrade]
      ,[TargetJobTitle]
  FROM [dbo].[VW_M_Course_Display] where id= @ID_Course 
END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_CourseLearnMethod_ALL]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE  PROCEDURE [dbo].[sp_M_CourseLearnMethod_ALL] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT ROW_NUMBER() OVER(ORDER BY ID ASC)  as RowIndex, ID AS ID, Course_LearnMethodName_TH, Course_LearnMethodName_EN FROM LDMS_M_CourseLearnMethod WITH (NOLOCK)  WHERE IsActive=1;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_CourseType_ALL]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE  PROCEDURE [dbo].[sp_M_CourseType_ALL] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT ROW_NUMBER() OVER(ORDER BY ID ASC)  as RowIndex, ID AS ID, 
			   [CourseTypeName_EN] 
			  ,[CourseTypeName_TH]
			  ,[ID_PlatformType]
			  ,[PrefixFormat]
			  ,[PrefixCode]
			  ,[SubPrefixCode]
			  ,[RunningLength]
			  ,[Description]
			  ,[CreateBy]
			  ,[CreateDate]
			  ,[UpdateBy]
			  ,[UpdateDate]

			FROM LDMS_M_CourseType WITH (NOLOCK)  WHERE IsActive=1;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_District_Select]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_M_District_Select]
	@PROVINCE_ID int,
	@AMPHUR_ID int
AS
BEGIN
	
	SET NOCOUNT ON;

    SELECT [DISTRICT_ID]
      ,[DISTRICT_CODE]
      ,[DISTRICT_NAME]
      ,[DISTRICT_NAME_ENG]
      ,[AMPHUR_ID]
      ,[PROVINCE_ID]
      ,[GEO_ID]
	FROM [dbo].[LDMS_M_Districts]
	WHERE [AMPHUR_ID] = @AMPHUR_ID AND [PROVINCE_ID] = @PROVINCE_ID
	ORDER BY [DISTRICT_NAME]

END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_Employee_SelectPaging]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--EXEC [dbo].[sp_M_Instructor_SelectPaging] NULL, NULL, NULL, NULL, 'INT', NULL, NULL, 1, 10, '',''
CREATE PROCEDURE [dbo].[sp_M_Employee_SelectPaging]
	@EmployeeID varchar(10) = NULL,
	@EmployeeName varchar(50) = NULL,
	@DepartmentID varchar(10) = NULL,
	@SectionID varchar(10) = NULL,
	@JobGradeID varchar(10) = NULL,
	@JobTitleID varchar(10) = NULL
	--@PageNum  int = null,
	--@PageSize int = null,
	--@SortField varchar(100) = null,
	--@SortOrder varchar(4) = null
AS
BEGIN
	
	SET NOCOUNT ON;

	DECLARE @sql varchar(3000)
	
	DECLARE @CountColumn varchar(50)
	DECLARE @RowFilter varchar(1500)
	SET @RowFilter = ' WHERE 1 = 1'

	IF @EmployeeID IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND u.[EmployeeID] LIKE ''%'+@EmployeeID+ '%'' '

	
	IF @EmployeeName IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND u.[Name] LIKE = ''%'+@EmployeeName+ '%'' OR  u.[Surname] LIKE = ''%'+@EmployeeName+ '%'' '

	IF @DepartmentID IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND u.[ID_Department] = '+@DepartmentID+ ' '

	IF @SectionID IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND r.[ID_Section] = '''+@SectionID+ ''' '

	IF @JobGradeID IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND u.[ID_JobGrade] = '''+@JobGradeID+ ''' '

	IF @JobTitleID IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND u.[ID_JobTitle]  = '''+@JobTitleID+ ''' '
	
	--SET @CountColumn = '[ID]'
	
	
    --IF @SortField = ''
	--	SET @SortField = '[UpdateDate] DESC'
	--ELSE
	--	SET @SortField = @SortField+' '+ @SortOrder

	SET @sql = 'SELECT 
				   u.[EmployeeID]
				  ,u.[Name] + '' '' + u.[Surname] As EmployeeName
				  ,g.[JobGradeName_EN]
				  ,u.[ID_JobGrade]
				  ,t.[JobTitleName_EN]
				  ,u.[ID_JobTitle]
				  ,t.[JobTitleName_EN]
				  ,u.[ID_Department]
				  ,d.[DepartmentName_EN]
				  ,r.[ID_Section]
				  ,se.[SectionName_EN]
				  ,u.[IsActive]
				  ,u.[UpdateDate]
				FROM LDMS_M_User u 
				INNER JOIN LDMS_M_Department d 
				ON u.[ID_Department] = d.[ID]
				INNER JOIN LDMS_M_JobGrade g
				ON u.[ID_JobGrade] = g.[ID]
				INNER JOIN LDMS_M_JobTitle t
				ON u.[ID_JobTitle] = t.[ID]
				INNER JOIN LDMS_M_UserRole r 
				ON u.[EmployeeID] = r.[EmployeeID] 
				INNER JOIN LDMS_M_Section se
				ON r.[ID_Section] = se.[ID]' + @RowFilter;

	 --exec dbo.sp_PagingHelper @PageNum,@PageSize,@SortField,@sql,@CountColumn
	 exec (@sql);
	

END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_Instructor_Insert]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_M_Instructor_Insert]
	@Instructor_ID nvarchar(10),
    @Instructor_Name nvarchar(100),
	@ID_Employee nvarchar(50) = null,
    @Type nvarchar(50),
    @Phone nvarchar(20),
    @Email nvarchar(50),
    @Organization nvarchar(250),
    @Address nvarchar(100),
    @District_Id int = NULL,
    @Aumphur_Id int = NULL,
    @Provice_Id int = null,
    @Post_Code nvarchar(5),
    @Path_Image nvarchar(200),
    @Path_Certificate nvarchar(200),
    @Education_Level nvarchar(500),
    @Major nvarchar(200),
    @Course_Experience nvarchar(500),
    @Skill_Professional nvarchar(500),
    @Customer_Reference nvarchar(500),
    @Manufacturing_Area nvarchar(500),
    @Create_By nvarchar(50),
    @IsActive tinyint
AS
BEGIN
	
	SET NOCOUNT ON;

    INSERT INTO [dbo].[LDMS_M_Instructor]
           ([Instructor_ID]
           ,[Instructor_Name]
		   ,[ID_Employee]
           ,[Type]
           ,[Phone]
           ,[Email]
           ,[Organization]
           ,[Address]
           ,[District_Id]
           ,[Aumphur_Id]
           ,[Provice_Id]
           ,[Post_Code]
           ,[Path_Image]
           ,[Path_Certificate]
           ,[Education_Level]
           ,[Major]
           ,[Course_Experience]
           ,[Skill_Professional]
           ,[Customer_Reference]
           ,[Manufacturing_Area]
           ,[Create_By]
           ,[Create_Date]
           ,[Update_By]
           ,[Update_Date]
           ,[IsActive])
     VALUES
           (@Instructor_ID
           ,@Instructor_Name
		   ,@ID_Employee
           ,@Type
           ,@Phone
           ,@Email
           ,@Organization
           ,@Address
           ,@District_Id
           ,@Aumphur_Id
           ,@Provice_Id
           ,@Post_Code
           ,@Path_Image
           ,@Path_Certificate
           ,@Education_Level
           ,@Major
           ,@Course_Experience
           ,@Skill_Professional
           ,@Customer_Reference
           ,@Manufacturing_Area
           ,@Create_By
           ,GETDATE()
           ,@Create_By
           ,GETDATE()
           ,@IsActive)

	SELECT @@IDENTITY [ID]

END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_Instructor_Select]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--EXEC [dbo].[sp_M_Instructor_Select] 2
CREATE PROCEDURE [dbo].[sp_M_Instructor_Select]
	@ID int
AS
BEGIN
	
	SET NOCOUNT ON;

	SELECT i.[ID]
      ,i.[Instructor_ID]
      ,i.[Instructor_Name]
      ,i.[Type]
	  ,i.[ID_Employee]
      ,i.[Phone]
      ,i.[Email]
      ,i.[Organization]
      ,i.[Address]
      ,i.[District_Id]
      ,i.[Aumphur_Id]
      ,i.[Provice_Id]
      ,i.[Post_Code]
      ,i.[Path_Image]
      ,i.[Path_Certificate]
      ,i.[Education_Level]
      ,i.[Major]
      ,i.[Course_Experience]
      ,i.[Skill_Professional]
      ,i.[Customer_Reference]
      ,i.[Manufacturing_Area]
      ,i.[Create_By]
      ,i.[Create_Date]
      ,i.[Update_By]
      ,i.[Update_Date]
      ,i.[IsActive]
	  ,u.[ID_Department]
	  ,dp.[DepartmentName_EN]
	  ,lk.[StateName] [TypeName]
	  ,p.[PROVINCE_NAME]
	  ,a.[AMPHUR_NAME]
	  ,d.[DISTRICT_NAME]
	FROM [dbo].[LDMS_M_Instructor] i
		LEFT JOIN [dbo].[LDMS_M_CodeLookUp] lk ON lk.Code = i.[Type] AND lk.[FieldName] = 'Type' AND lk.[TableName] = 'LDMS_M_Instructor'
		LEFT JOIN [dbo].[LDMS_M_User] u ON u.[EmployeeID] = i.[ID_Employee]
		LEFT JOIN [dbo].[LDMS_M_Department] dp ON dp.[ID] = u.[ID_Department]
		LEFT JOIN [dbo].[LDMS_M_Provinces] p ON p.[PROVINCE_ID] = i.[Provice_Id]
		LEFT JOIN [dbo].[LDMS_M_Amphurs] a ON a.[AMPHUR_ID] = i.[Aumphur_Id]
		LEFT JOIN [dbo].[LDMS_M_Districts] d ON d.[DISTRICT_ID] = i.[District_Id]
	WHERE i.[ID] = @ID
   
END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_Instructor_SelectByInstructorID]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_M_Instructor_SelectByInstructorID]
	@Instructor_ID nvarchar(10)
AS
BEGIN
	
	SET NOCOUNT ON;

    SELECT COUNT([ID]) FROM [dbo].[LDMS_M_Instructor]
	WHERE [Instructor_ID] = @Instructor_ID

END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_Instructor_SelectPaging]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--EXEC [dbo].[sp_M_Instructor_SelectPaging] NULL, NULL, NULL, NULL, 'INT', NULL, NULL, 1, 10, '',''
CREATE PROCEDURE [dbo].[sp_M_Instructor_SelectPaging]
	@Instructor_ID varchar(10) = NULL,
	@IsActive varchar(1) = NULL,
	@Instructor_Name varchar(100) = NULL,
	@Organization varchar(250) = NULL,
	@Type varchar(50) = NULL,
	@Course_Experience varchar(500) = NULL,
	@CourseName varchar(50) = NULL,
	@PageNum  int = null,
	@PageSize int = null,
	@SortField varchar(100) = null,
	@SortOrder varchar(4) = null
AS
BEGIN
	
	SET NOCOUNT ON;

	DECLARE @sql varchar(3000)
	
	DECLARE @CountColumn varchar(50)
	DECLARE @RowFilter varchar(1500)
	SET @RowFilter = ' WHERE 1 = 1'

	IF @Instructor_ID IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND i.[Instructor_ID] LIKE ''%'+@Instructor_ID+ '%'' '

	IF @IsActive IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND i.[IsActive] = '+@IsActive+ ' '
	
	IF @Instructor_Name IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND i.[Instructor_Name] LIKE = ''%'+@Instructor_Name+ '%'' '

	IF @Organization IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND i.[Organization] = '+@Organization+ ' '

	IF @Type IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND i.[Type] = '''+@Type+ ''' '

	IF @Course_Experience IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND i.[Course_Experience] LIKE ''%'+@Course_Experience+ '%'' '

	IF @CourseName IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND u.[CourseName]  LIKE ''%'+@CourseName+ '%'' '
	
	SET @CountColumn = '[ID]'
	
	
    IF @SortField = ''
		SET @SortField = '[update_date] DESC'
	ELSE
		SET @SortField = @SortField+' '+ @SortOrder

	SET @sql = 'SELECT i.[ID]
					  ,i.[Instructor_ID]
					  ,i.[Instructor_Name]
					  ,i.[Type]
					  ,l.[StateName] [TypeName]
					  ,i.[Phone]
					  ,i.[Email]
					  ,i.[Organization]
					  ,i.[Address]
					  ,i.[District_Id]
					  ,i.[Aumphur_Id]
					  ,i.[Provice_Id]
					  ,i.[Post_Code]
					  ,i.[Path_Image]
					  ,i.[Path_Certificate]
					  ,i.[Education_Level]
					  ,i.[Major]
					  ,i.[Course_Experience]
					  ,i.[Skill_Professional]
					  ,i.[Customer_Reference]
					  ,i.[Manufacturing_Area]
					  ,i.[Create_By]
					  ,i.[Create_Date]
					  ,i.[Update_By]
					  ,i.[Update_Date]
					  ,i.[IsActive]
					  ,c.[ID_Course]
					  ,u.[CourseName]
				FROM LDMS_M_Instructor i
					LEFT JOIN [dbo].[LDMS_M_CodeLookUp] l ON l.[Code] = i.[Type]
					LEFT JOIN [dbo].[LDMS_T_Class] c ON c.[ID_Instructor] = i.[ID]
					LEFT JOIN [dbo].[LDMS_M_Course] u ON u.[ID] = c.[ID_Course] '+@RowFilter;

	 exec dbo.sp_PagingHelper @PageNum,@PageSize,@SortField,@sql,@CountColumn

	

END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_Instructor_Update]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_M_Instructor_Update]
	@ID int,
	@Instructor_ID nvarchar(10),
    @Instructor_Name nvarchar(100),
	@ID_Employee nvarchar(50) = null,
    @Type nvarchar(50),
    @Phone nvarchar(20),
    @Email nvarchar(50),
    @Organization nvarchar(250) = NULL,
    @Address nvarchar(100),
    @District_Id int = NULL,
    @Aumphur_Id int = NULL,
    @Provice_Id int = NULL,
    @Post_Code nvarchar(5),
    @Path_Image nvarchar(200),
    @Path_Certificate nvarchar(200),
    @Education_Level nvarchar(500),
    @Major nvarchar(200),
    @Course_Experience nvarchar(500),
    @Skill_Professional nvarchar(500),
    @Customer_Reference nvarchar(500),
    @Manufacturing_Area nvarchar(500),
    @Update_By nvarchar(50),
    @IsActive tinyint
AS
BEGIN
	
	SET NOCOUNT ON;

    UPDATE [dbo].[LDMS_M_Instructor] SET
		[Instructor_ID] = @Instructor_ID, [Instructor_Name] = @Instructor_Name, [Type] = @Type, [Phone] = @Phone, [ID_Employee] = @ID_Employee,
		[Email] = @Email, [Organization] = @Organization, [Address] = @Address, [District_Id] = @District_Id, 
		[Aumphur_Id] = @Address, [Provice_Id] = @Provice_Id, [Post_Code] = @Post_Code, [Path_Image] = @Path_Image,
		[Path_Certificate] = @Path_Certificate, [Major] = @Major, [Course_Experience] = @Course_Experience,
		[Skill_Professional] = @Skill_Professional, [Customer_Reference] = @Customer_Reference, [Manufacturing_Area] = @Manufacturing_Area,
		[Update_By] = @Update_By, [Update_Date] = GETDATE(), [IsActive] = @IsActive
	WHERE [ID] = @ID


END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_Instructor_UpdateStatus]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_M_Instructor_UpdateStatus]
	@ID int,
	@Update_By nvarchar(50)  NULL
AS
BEGIN
	
	SET NOCOUNT ON;

	UPDATE [dbo].[LDMS_M_Instructor] SET
		[IsActive] = CASE [IsActive]  WHEN 1 THEN 0 ELSE 1 END,
		 [Update_By] = @Update_By, [Update_Date] = GETDATE()
	WHERE [ID] = @ID

END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_JobGrade_ALL]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE  PROCEDURE [dbo].[sp_M_JobGrade_ALL] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT ROW_NUMBER() OVER(ORDER BY ID ASC)  as RowIndex, 
			   [ID] AS ID 
			  ,[JobGradeID]
			  ,[JobGradeName_EN]
			  ,[JobGradeName_TH]
			  ,[Description]
			  ,[CreateBy]
			  ,[CreateDate]
			  ,[UpdateBy]
			  ,[UpdateDate]

			FROM LDMS_M_JobGrade WITH (NOLOCK)  WHERE IsActive=1;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_Platform_Insert]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE  PROCEDURE [dbo].[sp_M_Platform_Insert]
	-- Add the parameters for the stored procedure here 
	@PlatformID nvarchar(20),
	@PlatformName_EN nvarchar(50),
	@PlatformName_TH nvarchar(50),
	@ID_PlatformType int,
	@PlatformDescription nvarchar(500),
	@ID_Department_Create int,
	@PlatformStatus int,
	@CreateBy  nvarchar(50)  =null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

BEGIN TRANSACTION;
BEGIN TRY
	INSERT INTO [dbo].[LDMS_M_Platform]
           (     [PlatformID]
				,[PlatformName_EN]
				,[PlatformName_TH]
				,[ID_PlatformType]
				,[PlatformDescription]
				,[ID_Department_Create]
				,[CreateBy]
				,[CreateDate]
				,[UpdateBy]
				,[UpdateDate]
				,[IsActive])
     VALUES
           (
		     @PlatformID
			,@PlatformName_EN
			,@PlatformName_TH
			,@ID_PlatformType
			,@PlatformDescription
			,@ID_Department_Create
            ,@CreateBy
            ,GETDATE()
            ,null
            ,null
            ,@PlatformStatus);

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

	SELECT @@IDENTITY
END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_Platform_Select_byID]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
CREATE PROCEDURE [dbo].[sp_M_Platform_Select_byID]
	 @ID int 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT  
	   [ID]
	  ,[PlatformID]
      ,[PlatformName_EN]
      ,[PlatformName_TH]
      ,[ID_PlatformType]
      ,[PlatformDescription]
      ,[ID_Department_Create]
      ,[CreateBy]
      ,[CreateDate]
      ,[UpdateBy]
      ,[UpdateDate]
      ,[IsActive]
	FROM  LDMS_M_Platform  p 
	WHERE p.ID = @ID  --p.IsActive = 1
     
	
			  
	
END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_Platform_Update]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE  PROCEDURE [dbo].[sp_M_Platform_Update]
	-- Add the parameters for the stored procedure here 
	@ID_Platform nvarchar(10),
	@PlatformID nvarchar(50),
	@PlatformName_EN nvarchar(50),
	@PlatformName_TH nvarchar(50),
	@ID_PlatformType int,
	@PlatformDescription nvarchar(500),
	@ID_Department_Create int,
	@PlatformStatus int,
	@UpdateBy  nvarchar(50)  = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

BEGIN TRANSACTION;
BEGIN TRY
	UPDATE [dbo].[LDMS_M_Platform]
          SET [PlatformID] = @PlatformID
				,[PlatformName_EN] = @PlatformName_EN
				,[PlatformName_TH] = @PlatformName_TH
				,[ID_PlatformType] = @ID_PlatformType
				,[PlatformDescription] = @PlatformDescription
				,[ID_Department_Create] = @ID_Department_Create
				,[UpdateBy] = @UpdateBy
				,[UpdateDate] = GETDATE()
				,[IsActive] = @PlatformStatus
    WHERE [ID] = @ID_Platform

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

	SELECT 1
END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_PlatformType_ALL]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE  PROCEDURE [dbo].[sp_M_PlatformType_ALL] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT ROW_NUMBER() OVER(ORDER BY ID ASC)  as RowIndex, ID, PlatformTypeName_TH, PlatformTypeName_EN FROM LDMS_M_PlatformType WITH (NOLOCK)  WHERE IsActive=1;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_Platfrom_Select]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
CREATE   PROCEDURE [dbo].[sp_M_Platfrom_Select]
	 @paramPlatformId nvarchar(50) = null,
	 @paramPlatformName nvarchar(50) = null,
	 @paramPlatformType nvarchar(50) = null,
	 @paramPlatformStatus int = 1
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT  
	ROW_NUMBER() OVER(ORDER BY p.PlatformID ASC)  as RowIndex,
	p.ID,
	p.PlatformID,
	p.PlatformName_TH,
	p.PlatformName_EN,
	p.ID_PlatformType,
	pt.PlatformTypeName_EN as PlatformType,
	CASE WHEN p.IsActive = 1 THEN
			'Active'
		 WHEN p.IsActive = 0 THEN
			'In Active'
	END
	AS  PlatformStatus

	FROM  LDMS_M_Platform  p WITH (NOLOCK) 
	LEFT OUTER JOIN LDMS_M_PlatformType pt WITH (NOLOCK)  on p.ID_PlatformType = pt.ID
	WHERE p.IsActive = @paramPlatformStatus

	AND (1 =  CASE WHEN @paramPlatformId IS NOT NULL AND @paramPlatformId <> '' AND @paramPlatformId <> ' ' THEN
				CASE WHEN UPPER(p.PlatformId) like '%'+ UPPER(@paramPlatformId) +'%' THEN 1 ELSE 0 END
			  ELSE 1 END)
    AND (1 =  CASE WHEN @paramPlatformName IS NOT NULL AND @paramPlatformName <> '' AND @paramPlatformName <> ' ' THEN
				CASE WHEN UPPER(p.PlatformName_EN) like '%'+ UPPER(@paramPlatformName) +'%' THEN 1 ELSE 0 END
			  ELSE 1 END)
	AND (1 =  CASE WHEN @paramPlatformType IS NOT NULL AND @paramPlatformType <> '' AND @paramPlatformType <> ' ' THEN
				CASE WHEN UPPER(pt.PlatformTypeName_EN) like '%'+ UPPER(@paramPlatformType) +'%' THEN 1 ELSE 0 END
			  ELSE 1 END)

	ORDER BY p.CreateDate DESC;
	
END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_Platfrom_Select_Paging]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
CREATE   PROCEDURE [dbo].[sp_M_Platfrom_Select_Paging]
	 @PlatformId nvarchar(50) = null,
	 @PlatformName nvarchar(50) = null,
	 @PlatformType nvarchar(50) = null,
	 @PlatformStatus nvarchar(1) = null,
	 @PageNum  int = null,
	 @PageSize int = null,
	 @SortField varchar(100) = null,
	 @SortOrder varchar(4) = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @sql varchar(3000)
	
	DECLARE @CountColumn varchar(50)
	DECLARE @RowFilter varchar(1500)
	SET @RowFilter = ' WHERE 1 = 1 '

	IF @PlatformId IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND p.[PlatformID] LIKE ''%'+@PlatformId+ '%'' '

	IF 	 @PlatformName IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND p.[PlatformName_EN] LIKE ''%'+@PlatformName+ '%'' ' 
	
	IF @PlatformType IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND p.[ID_PlatformType] = '+@PlatformType+ ' '

	IF @PlatformStatus IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND p.[IsActive] = '+@PlatformStatus+ ' '

	
	
	SET @CountColumn = '[ID]'
	
	SET @SortField = '[UpdateDate] DESC'



	SET @sql = 'SELECT  
				-- ROW_NUMBER() OVER(ORDER BY p.PlatformID ASC)  as RowIndex,
				p.ID AS ID,
				p.PlatformID,
				p.PlatformName_TH,
				p.PlatformName_EN,
				p.ID_PlatformType,
				pt.PlatformTypeName_EN as PlatformType,
				CASE WHEN p.IsActive = 1 THEN
						''Active''
					 WHEN p.IsActive = 0 THEN
						''In Active''
				END
				AS  PlatformStatus,
				p.UpdateDate as UpdateDate

				FROM  LDMS_M_Platform  p WITH (NOLOCK) 
				LEFT OUTER JOIN LDMS_M_PlatformType pt WITH (NOLOCK)  on p.ID_PlatformType = pt.ID ' +  @RowFilter

	
	  exec dbo.sp_PagingHelper @PageNum,@PageSize,@SortField,@sql,@CountColumn
  
	
	
END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_Provinces_Select]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_M_Provinces_Select]

AS
BEGIN
	
	SET NOCOUNT ON;

    SELECT [PROVINCE_ID]
      ,[PROVINCE_CODE]
      ,[PROVINCE_NAME]
      ,[PROVINCE_NAME_ENG]
      ,[GEO_ID]
	FROM [dbo].[LDMS_M_Provinces]
	ORDER BY [PROVINCE_NAME]

END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_SubPlatform_Delete]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE  PROCEDURE [dbo].[sp_M_SubPlatform_Delete]
	-- Add the parameters for the stored procedure here 
	@ID_SubPlatform nvarchar(10)
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

BEGIN TRANSACTION;
BEGIN TRY
	UPDATE [dbo].[LDMS_M_SubPlatform]
         SET [IsActive] = 0
    WHERE [ID] = @ID_SubPlatform

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

	SELECT 1
END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_SubPlatform_Insert]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE  PROCEDURE [dbo].[sp_M_SubPlatform_Insert]
	-- Add the parameters for the stored procedure here 
	@ID_Platform int,
	@SubPlatformName_EN nvarchar(50),
	@SubPlatformName_TH nvarchar(50),
	@CreateBy  nvarchar(50)  =null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

BEGIN TRANSACTION;
BEGIN TRY
	INSERT INTO [dbo].[LDMS_M_SubPlatform]
           (   [ID_Platform]
			  ,[SubPlatformName_EN]
			  ,[SubPlatformName_TH]
			  ,[CreateBy]
			  ,[CreateDate]
			  ,[UpdateBy]
			  ,[UpdateDate]
			  ,[IsActive])
     VALUES
           (
		    @ID_Platform 
		   ,@SubPlatformName_EN
		   ,@SubPlatformName_TH
           ,@CreateBy
           ,GETDATE()
           ,null
           ,null
           ,1);
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

	SELECT @@IDENTITY
END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_SubPlatform_Select]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
CREATE PROCEDURE [dbo].[sp_M_SubPlatform_Select]
	 @ID_Platform nvarchar(50),
	 @SubPlatformNameEN nvarchar (50)
	
	 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT  
			p.ID,
			P.ID_Platform,
			p.SubPlatformName_EN,
			p.SubPlatformName_TH,
			CASE WHEN p.IsActive = 1 THEN
					'Active'
					WHEN p.IsActive = 0 THEN
					'In Active'
			END
			AS  SubPlatformStatus,
			p.UpdateDate

			FROM  LDMS_M_SubPlatform  p
			WHERE p.SubPlatformName_EN = @SubPlatformNameEN
			AND p.ID_Platform = @ID_Platform
	
			  
END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_SubPlatform_Select_Paging]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
CREATE PROCEDURE [dbo].[sp_M_SubPlatform_Select_Paging]
	 @ID_Platform nvarchar(10),
	 @Keyword nvarchar(50) = null,
	 @PageNum  int = null,
	 @PageSize int = null,
	 @SortField varchar(100) = null,
	 @SortOrder varchar(4) = null
	
	 
AS
BEGIN

	SET NOCOUNT ON;

	DECLARE @sql varchar(3000)
	
	DECLARE @CountColumn varchar(50)
	DECLARE @RowFilter varchar(1500)
	SET @RowFilter = ' WHERE 1 = 1 AND p.IsActive = 1'

	IF @ID_Platform IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND p.[ID_Platform] = '''+@ID_Platform+ ''' '

	IF 	 @Keyword IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND ( p.[ID_Platform] LIKE ''%'+@Keyword+ '%'' OR  p.[SubPlatformName_EN] LIKE ''%'+@Keyword+ '%'' )' 
	
	SET @CountColumn = '[ID]'
	
	SET @SortField = '[UpdateDate] DESC'

    -- Insert statements for procedure here
	SET @sql = 'SELECT  
					p.ID,
					P.ID_Platform,
					p.SubPlatformName_EN,
					p.SubPlatformName_TH,
					CASE WHEN p.IsActive = 1 THEN
							''Active''
						 WHEN p.IsActive = 0 THEN
							''In Active''
					END
					AS  SubPlatformStatus,
					p.UpdateDate

					FROM  LDMS_M_SubPlatform  p ' + @RowFilter
			
	
		 exec dbo.sp_PagingHelper @PageNum,@PageSize,@SortField,@sql,@CountColumn	  
	
END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_SubPlatform_Update]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE  PROCEDURE [dbo].[sp_M_SubPlatform_Update]
	-- Add the parameters for the stored procedure here 
	@ID_SubPlatform int,
	@SubPlatformName_EN nvarchar(50),
	@SubPlatformName_TH nvarchar(50),
	@UpdateBy  nvarchar(50)  =null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

BEGIN TRANSACTION;
BEGIN TRY
	
	UPDATE [dbo].LDMS_M_SubPlatform
	SET [SubPlatformName_EN] = @SubPlatformName_EN,
	    [SubPlatformName_TH] = @SubPlatformName_TH,
		[UpdateBy] = @UpdateBy,
		[UpdateDate] = GETDATE()
	WHERE [ID] = @ID_SubPlatform

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

	SELECT 1

END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_SubPlatformByID]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
CREATE PROCEDURE [dbo].[sp_M_SubPlatformByID]
	 @ID_SubPlatform int 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT  
	   s.[ID]
      ,s.[ID_Platform]
      ,s.[SubPlatformName_EN]
      ,s.[SubPlatformName_TH]
      ,s.[CreateBy]
      ,s.[CreateDate]
      ,s.[UpdateBy]
      ,s.[UpdateDate]
      ,s.[IsActive]
	  ,p.PlatformID as PlatformID
	  ,p.PlatformName_EN as PlatfromName
	FROM  LDMS_M_SubPlatform  s inner join
	LDMS_M_Platform p on s.ID_Platform = p.ID
	WHERE p.IsActive = 1
    AND S.ID = @ID_SubPlatform
	
			  
	
END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_SubPlatformCourse_Insert]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE  PROCEDURE [dbo].[sp_M_SubPlatformCourse_Insert]
	-- Add the parameters for the stored procedure here 
	@ID_SubPlatform int,
	@ID_Course int,
	@CreateBy nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

BEGIN TRANSACTION;
BEGIN TRY
	INSERT INTO [dbo].[LDMS_M_SubPlatformCourse]
				(  
				   [ID_SubPlatform]
				  ,[ID_Course]
				  ,[CreateBy]
				  ,[CreateDate])
     VALUES
           (
		    @ID_SubPlatform
		   ,@ID_Course
		   ,@CreateBy
           ,GETDATE()
		   );
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

	-- SELECT @@IDENTITY
END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_SubPlatformCourseBy_SubPlatformID]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
CREATE PROCEDURE [dbo].[sp_M_SubPlatformCourseBy_SubPlatformID]
	 @ID_SubPlatform int 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT  
		ROW_NUMBER() OVER(ORDER BY c.CourseID ASC)  as RowIndex,
		        C.ID,
				c.CourseID,
				C.CourseName,
				c.ID_LearnMethod,
				cm.Course_LearnMethodName_EN,
				CASE WHEN c.IsActive = 1 THEN
						'Active'
					 WHEN c.IsActive = 0 THEN
						'In Active'
				END
				AS  CourseStatus
				FROM [LDMS_M_SubPlatformCourse] s WITH (NOLOCK) 
				INNER JOIN  LDMS_M_Course  c WITH (NOLOCK) on s.ID_Course = c.ID
				INNER JOIN LDMS_M_CourseLearnMethod cm WITH (NOLOCK)  on c.ID_LearnMethod = cm.ID
				WHERE s.[ID_SubPlatform] = @ID_SubPlatform
    

END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_SubPlatformCourseJson_Delete]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[sp_M_SubPlatformCourseJson_Delete]
	-- Add the parameters for the stored procedure here 
	 @ID_SubPlatform varchar(10),
	 @ID_Course varchar(10)
AS
BEGIN

	DELETE [dbo].[LDMS_M_SubPlatformCourse] 
	WHERE ID_SubPlatform = @ID_SubPlatform
		AND ID_Course = @ID_Course

	SELECT  1
END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_SubPlatformCourseJson_Insert]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[sp_M_SubPlatformCourseJson_Insert]
	-- Add the parameters for the stored procedure here 
	@json NVARCHAR(max),
	@ID_SubPlatform nvarchar(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	--DELETE [dbo].[LDMS_M_SubPlatformCourse] 
	--WHERE ID_SubPlatform = @ID_SubPlatform

	SET NOCOUNT ON;
	INSERT INTO [dbo].[LDMS_M_SubPlatformCourse]
           (     [ID_SubPlatform]
				,[ID_Course]
				)
			 SELECT
				ID_SubPlatform
			   ,ID_Course
			   
			FROM OPENJSON(@json)
			WITH (
			  ID_SubPlatform      int 
			, ID_Course           int
			) AS jsonValues

	-- SELECT @@IDENTITY
	SELECT  1
END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_SubPlatformCourseSelect_Paging]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
CREATE PROCEDURE [dbo].[sp_M_SubPlatformCourseSelect_Paging]
	 @ID_SubPlatform nvarchar(10),
	 @Keyword nvarchar(50) = null,
	 @PageNum  int = null,
	 @PageSize int = null,
	 @SortField varchar(100) = null,
	 @SortOrder varchar(4) = null
	
	 
AS
BEGIN

	SET NOCOUNT ON;

	DECLARE @sql varchar(3000)
	
	DECLARE @CountColumn varchar(50)
	DECLARE @RowFilter varchar(1500)
	SET @RowFilter = ' WHERE 1 = 1 '

	IF @ID_SubPlatform IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND s.[ID_SubPlatform] = '''+@ID_SubPlatform+ ''' '

	IF @Keyword IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND ( c.[CourseID] LIKE ''%'+@Keyword+ '%'' OR  c.[CourseName] LIKE ''%'+@Keyword+ '%'' OR  c.[Course_LearnMethodName_EN]  LIKE ''%'+@Keyword+ '%'' )' 
	
	SET @CountColumn = '[ID]'
	
	SET @SortField = '[CourseID] DESC'

    -- Insert statements for procedure here
	SET @sql = ' SELECT  
					ROW_NUMBER() OVER(ORDER BY c.CourseID ASC)  as RowIndex,
		        C.ID,
				c.CourseID,
				C.CourseName,
				c.ID_LearnMethod,
				cm.Course_LearnMethodName_EN,
				CASE WHEN c.IsActive = 1 THEN
						''Active''
					 WHEN c.IsActive = 0 THEN
						''In Active''
				END
				AS  CourseStatus
				FROM [LDMS_M_SubPlatformCourse] s WITH (NOLOCK) 
				INNER JOIN  LDMS_M_Course  c WITH (NOLOCK) on s.ID_Course = c.ID
				INNER JOIN LDMS_M_CourseLearnMethod cm WITH (NOLOCK)  on c.ID_LearnMethod = cm.ID ' + @RowFilter
				-- WHERE s.[ID_SubPlatform] = @ID_SubPlatform
			
	
		 exec dbo.sp_PagingHelper @PageNum,@PageSize,@SortField,@sql,@CountColumn	  
	
END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_VanueRoom_Select_ByPlantID]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
CREATE  PROCEDURE [dbo].[sp_M_VanueRoom_Select_ByPlantID]
	 @paramIDPlant int 
	 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	--DECLARE @sql varchar(3000)
	
	--DECLARE @CountColumn varchar(50)
	--DECLARE @RowFilter varchar(1500)
	--SET @RowFilter = ' WHERE 1 = 1 '

	



	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT  [ID]
      ,[VenueRoomID]
      ,[ID_Plant]
      ,[RoomName_EN]
      ,[RoomName_TH]
      ,[Description]
      ,[CreateBy]
      ,[CreateDate]
      ,[UpdateBy]
      ,[UpdateDate]
      ,[IsActive]
  FROM [dbo].[LDMS_M_VenueRoom]
	where id = @paramIDPlant

	--WHERE c.IsActive = @paramCourseStatus

	/*AND  c.ID_LearnMethod = @paramLearnMethod OR @paramLearnMethod IS NOT NULL

	AND (1 =  CASE WHEN @paramCourseId IS NOT NULL AND @paramCourseId <> '' AND @paramCourseId <> ' ' THEN
				CASE WHEN UPPER(c.CourseID) like '%'+ UPPER(@paramCourseId) +'%' THEN 1 ELSE 0 END
			  ELSE 1 END)
    AND (1 =  CASE WHEN @paramCourseName IS NOT NULL AND @paramCourseName <> '' AND @paramCourseName <> ' ' THEN
				CASE WHEN UPPER(c.CourseName) like '%'+ UPPER(@paramCourseName) +'%' THEN 1 ELSE 0 END
			  ELSE 1 END)

	--AND ( @paramLearnMethod IS NOT NULL AND c.ID_LearnMethod = @paramLearnMethod )*/
			  
	
END
GO
/****** Object:  StoredProcedure [dbo].[sp_M_VanueRoom_SelectAll]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
CREATE  PROCEDURE [dbo].[sp_M_VanueRoom_SelectAll]
	
AS
BEGIN
	
	SET NOCOUNT ON;

	SELECT ROW_NUMBER() OVER(ORDER BY ID ASC)  as RowIndex,
	   [ID]
      ,[VenueRoomID]
      ,[ID_Plant]
      ,[RoomName_EN]
      ,[RoomName_TH]
      ,[Description]
      ,[CreateBy]
      ,[CreateDate]
      ,[UpdateBy]
      ,[UpdateDate]
      ,[IsActive]
  FROM [dbo].[LDMS_M_VenueRoom] WITH (NOLOCK) WHERE [IsActive] = 1



END
GO
/****** Object:  StoredProcedure [dbo].[sp_PagingHelper]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_PagingHelper]
                (
	                @pagenum  int,
	                @pagesize int,		
	                @orderby varchar(500),
	                @cmd varchar(max),
	                @countColumn varchar(100)
                )
                AS
                BEGIN

	                declare @rowNum1 as integer 
	                declare @rowNum2 as integer 
	                declare @max as integer
	                set @rowNum1 = (@pagenum - 1)* @pagesize + 1
	                set @rowNum2 = (@pagenum * @pagesize)
                    set @max = @pagenum * @pagesize

	                declare @cmdselect as varchar(8000)
	                declare @cmdcnt as varchar(8000)
	                declare @cmdselectEnd  as varchar(8000)
	                declare @cmdcntEnd  as varchar(8000)


	                set @cmdcnt = ' select count('+@countColumn+') as TotalRecords from  ( '
	                set @cmdselect =  ' select top(' + cast(@max as varchar(10)) + ') result.* from ( select row_number() over(order by ' + isnull(@orderby,'')  +  ') as rownumber ,c_all.* from ( '
	                set @cmdselectEnd = ' )c_all)result where result.rownumber between ' + cast(@rowNum1 as varchar(10)) + ' and ' + cast(@rowNum2 as varchar(10))
	                set @cmdcntEnd = ' ) as cnt'
	
	                print (@cmdselect + @cmd + @cmdselectEnd)
	                print (@cmdcnt + @cmd + @cmdcntEnd)

	                exec (@cmdselect + @cmd + @cmdselectEnd)
	                exec (@cmdcnt + @cmd + @cmdcntEnd)
                END





GO
/****** Object:  StoredProcedure [dbo].[sp_T_Class_Insert]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_T_Class_Insert]
			
           @paramID_Course int
           ,@paramID_Instructor int
           ,@paramClassCapacity int
           ,@paramClassFee decimal(8,2)
           ,@paramLearnDateStart date
           ,@paramLearnTimeStart nvarchar(10)
           ,@paramLearnDateEnd date
           ,@paramLearnTimeEnd  nvarchar(10)
           ,@paramRegisterDateStart date
           ,@paramRegisterDateEnd date
           ,@paramID_PlantVenue int
           ,@paramID_VenueRoom int
           ,@paramPlaceAndLocation nvarchar(200)
           ,@paramClassStatus int
           ,@paramIsAttend int
           ,@paramAttendNum int
           ,@paramIsTest int
           ,@paramTestFullScore int
           ,@paramTestPercentage int
           ,@paramIsSkill int
           ,@paramSkillFullScore int
           ,@paramSkillPercentage int
           ,@paramIsCoaching int
           ,@paramIsCertificate int
           ,@paramIsAttachCert int
           ,@paramCertificationRemark nvarchar(100)
           ,@paramReminderDurationDay int
           ,@paramCreateBy nvarchar(50)
           ,@paramIsActive int
           
           
           
AS
BEGIN
	
	SET NOCOUNT ON;
	declare @paramClassID nvarchar(50);
	declare @CourseID nvarchar(50);
	select top 1 @CourseID = CourseID
	from LDMS_M_COURSE
	where ID = @paramID_Course
	
	--if (@CourseID is null)  return null;
	
	set @paramClassID = [dbo].[genClassID](@CourseID);

   INSERT INTO [dbo].[LDMS_T_Class]
           ([ClassID]
           ,[ID_Course]
           ,[ID_Instructor]
           ,[ClassCapacity]
           ,[ClassFee]
           ,[LearnDateStart]
           ,[LearnTimeStart]
           ,[LearnDateEnd]
           ,[LearnTimeEnd]
           ,[RegisterDateStart]
           ,[RegisterDateEnd]
           ,[ID_PlantVenue]
           ,[ID_VenueRoom]
           ,[PlaceAndLocation]
           ,[ClassStatus]
           ,[IsAttend]
           ,[AttendNum]
           ,[IsTest]
           ,[TestFullScore]
           ,[TestPercentage]
           ,[IsSkill]
           ,[SkillFullScore]
           ,[SkillPercentage]
           ,[IsCoaching]
           ,[IsCertificate]
           ,[IsAttachCert]
           ,[CertificationRemark]
           ,[ReminderDurationDay]
           ,[CreateBy]
           ,[CreateDate]
           ,[UpdateBy]
           ,[UpdateDate]
           ,[IsActive])
     VALUES
           (@paramClassID		
           ,@paramID_Course		
           ,@paramID_Instructor		
           ,@paramClassCapacity		
           ,@paramClassFee		
           ,@paramLearnDateStart	
           ,@paramLearnTimeStart	
           ,@paramLearnDateEnd		
           ,@paramLearnTimeEnd		
           ,@paramRegisterDateStart	
           ,@paramRegisterDateEnd	
           ,@paramID_PlantVenue		
           ,@paramID_VenueRoom		
           ,@paramPlaceAndLocation	
           ,@paramClassStatus		
           ,@paramIsAttend		
           ,@paramAttendNum		
           ,@paramIsTest		
           ,@paramTestFullScore		
           ,@paramTestPercentage	
           ,@paramIsSkill		
           ,@paramSkillFullScore	
           ,@paramSkillPercentage	
           ,@paramIsCoaching		
           ,@paramIsCertificate		
           ,@paramIsAttachCert		
           ,@paramCertificationRemark	
           ,@paramReminderDurationDay	
           ,@paramCreateBy	
		   ,getdate()
		   ,@paramCreateBy
		   ,getdate()
           ,@paramIsActive)

	SELECT * from LDMS_T_Class where ClassID = @paramClassID

END
GO
/****** Object:  StoredProcedure [dbo].[sp_T_Class_Select]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
CREATE  PROCEDURE [dbo].[sp_T_Class_Select]
	 @paramCourseId nvarchar(50) = null,
	 @paramCourseName nvarchar(50) = null,
	 @paramCourseStatus varchar(1) = null,
	 @paramLearnMethod varchar(1) = null
	 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	DECLARE @sql varchar(3000)
	
	DECLARE @CountColumn varchar(50)
	DECLARE @RowFilter varchar(1500)
	SET @RowFilter = ' WHERE 1 = 1 '

	IF @paramCourseId IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND course.[ID] = '+@paramCourseId+ ' '

	IF @paramCourseName IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND course.[CourseName] LIKE ''%'+@paramCourseName+ '%'' ' 
	
	IF @paramLearnMethod IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND course.[ID_LearnMethod] = ' + @paramLearnMethod + ' '

	IF @paramCourseStatus IS NOT NULL
		SET @RowFilter = @RowFilter + ' AND course.IsActive = ' + @paramCourseStatus + ' '



	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SET @Sql = '  /****** Script for SelectTopNRows command from SSMS  ******/
SELECT 
       class.[ID]
      ,class.[ClassID]
      ,course.CourseName
      ,instructor.Instructor_Name
	  ,class.[LearnDateStart]
	  ,register.RegisterDate    
      ,class.[ClassFee]
      ,class.[ClassCapacity] as Capacity
      ,room.VenueRoomID
	  ,room.RoomName_EN
      ,class.[ID_PlantVenue]
      ,code.StateName as ClassStatus
	  ,code.Description as StatusName
      ,class.[IsActive]
  FROM [LDMS_T_Class] class
  left join [LDMS_M_Instructor] instructor on class.ID_Instructor = instructor.ID
  inner join [LDMS_M_Course] course on class.ID_Course = course.ID
  left join [dbo].[LDMS_T_ClassRegistration] register on Class.ID = register.ID_Class and register.ID_Course = course.ID
  left join [dbo].[LDMS_M_VenueRoom] room on class.ID_VenueRoom = room.ID
  inner join [dbo].[LDMS_M_CodeLookUp] code on code.TableName = ''LDMS_T_Class'' and code.FieldName = ''ClassStatus'' and class.ClassStatus = code.Code ' + @RowFilter

    print (@Sql)
	exec (@Sql) 

	--WHERE c.IsActive = @paramCourseStatus

	/*AND  c.ID_LearnMethod = @paramLearnMethod OR @paramLearnMethod IS NOT NULL

	AND (1 =  CASE WHEN @paramCourseId IS NOT NULL AND @paramCourseId <> '' AND @paramCourseId <> ' ' THEN
				CASE WHEN UPPER(c.CourseID) like '%'+ UPPER(@paramCourseId) +'%' THEN 1 ELSE 0 END
			  ELSE 1 END)
    AND (1 =  CASE WHEN @paramCourseName IS NOT NULL AND @paramCourseName <> '' AND @paramCourseName <> ' ' THEN
				CASE WHEN UPPER(c.CourseName) like '%'+ UPPER(@paramCourseName) +'%' THEN 1 ELSE 0 END
			  ELSE 1 END)

	--AND ( @paramLearnMethod IS NOT NULL AND c.ID_LearnMethod = @paramLearnMethod )*/
			  
	
END
GO
/****** Object:  StoredProcedure [dbo].[sp_T_Class_Select_ByID]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- =============================================
CREATE  PROCEDURE [dbo].[sp_T_Class_Select_ByID]
	 @paramIDClass int 
	 
AS
BEGIN
	SELECT class.[ID]
      ,class.[ClassID]
      ,class.[ID_Course]
      ,class.[ID_Instructor]
      ,class.[ClassCapacity]
      ,class.[ClassFee]
      ,class.[LearnDateStart]
      ,class.[LearnTimeStart]
      ,class.[LearnDateEnd]
      ,class.[LearnTimeEnd]
      ,class.[RegisterDateStart]
      ,class.[RegisterDateEnd]
      ,class.[ID_PlantVenue]
      ,class.[ID_VenueRoom]
      ,class.[PlaceAndLocation]
      ,class.[ClassStatus]
      ,class.[IsAttend]
      ,class.[AttendNum]
      ,class.[IsTest]
      ,class.[TestFullScore]
      ,class.[TestPercentage]
      ,class.[IsSkill]
      ,class.[SkillFullScore]
      ,class.[SkillPercentage]
      ,class.[IsCoaching]
      ,class.[IsCertificate]
      ,class.[IsAttachCert]
      ,class.[CertificationRemark]
      ,class.[ReminderDurationDay]
      ,class.[CreateBy]
      ,class.[CreateDate]
      ,class.[UpdateBy]
      ,class.[UpdateDate]
      ,class.[IsActive]
	  ,course.CourseName
	  ,instructor.[Instructor_Name] as InstructorName
  FROM [dbo].[LDMS_T_Class] class
  inner join [LDMS_M_Course] course on class.ID_Course = course.ID
  left join [LDMS_M_Instructor] instructor on class.ID_Instructor = instructor.ID
  where class.[ID] = @paramIDClass			  

end


GO
/****** Object:  StoredProcedure [dbo].[sp_T_Class_Update]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE  PROCEDURE [dbo].[sp_T_Class_Update]
	-- Add the parameters for the stored procedure here 
	@paramIDCLass int,
	-- @paramClassID			nvarchar(50),                               
	@paramID_Course		int,                                      
	@paramID_Instructor		int,                          
	@paramClassCapacity		int,                          
	@paramClassFee		decimal(8,2),                              
	@paramLearnDateStart		date,                        
	@paramLearnTimeStart		nvarchar(7),                     
	@paramLearnDateEnd		date,                                  
	@paramLearnTimeEnd		nvarchar(7),                               
	@paramRegisterDateStart		date,                     
	@paramRegisterDateEnd		date,                       
	@paramID_PlantVenue		int,                          
	@paramID_VenueRoom		int,                                   
	@paramPlaceAndLocation		nvarchar(200),             
	@paramClassStatus		int,                                    
	@paramIsAttend		int,                                   
	@paramAttendNum		int,                                      
	@paramIsTest		int,                                     
	@paramTestFullScore		int,                          
	@paramTestPercentage		int,                         
	@paramIsSkill		int,                                    
	@paramSkillFullScore		int,                         
	@paramSkillPercentage		int,                        
	@paramIsCoaching		int,                                 
	@paramIsCertificate		int,                      
	@paramIsAttachCert		int,                               
	@paramCertificationRemark		nvarchar(100),          
	@paramReminderDurationDay		int,   
	@paramUpdateBy		nvarchar(50),                              
	@paramIsActive		int	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

BEGIN TRANSACTION;
BEGIN TRY
	UPDATE [dbo].[LDMS_T_Class]
	SET 
       [ID_Course] =		@paramID_Course
      ,[ID_Instructor] =	@paramID_Instructor	
      ,[ClassCapacity] =	@paramClassCapacity	
      ,[ClassFee] =		@paramClassFee		
      ,[LearnDateStart] =	@paramLearnDateStart	
      ,[LearnTimeStart] =	@paramLearnTimeStart	
      ,[LearnDateEnd] =		@paramLearnDateEnd	
      ,[LearnTimeEnd] =		@paramLearnTimeEnd	
      ,[RegisterDateStart] =	@paramRegisterDateStart	
      ,[RegisterDateEnd] =	@paramRegisterDateEnd	
      ,[ID_PlantVenue] =	@paramID_PlantVenue	
      ,[ID_VenueRoom] =		@paramID_VenueRoom	
      ,[PlaceAndLocation] =	@paramPlaceAndLocation	
      ,[ClassStatus] =		@paramClassStatus	
      ,[IsAttend] =		@paramIsAttend		
      ,[AttendNum] =		@paramAttendNum		
      ,[IsTest] =		@paramIsTest		
      ,[TestFullScore] =	@paramTestFullScore	
      ,[TestPercentage] =	@paramTestPercentage	
      ,[IsSkill] =		@paramIsSkill		
      ,[SkillFullScore] =	@paramSkillFullScore	
      ,[SkillPercentage] =	@paramSkillPercentage	
      ,[IsCoaching] =		@paramIsCoaching	
      ,[IsCertificate] =	@paramIsCertificate	
      ,[IsAttachCert] =		@paramIsAttachCert	
      ,[CertificationRemark] =	@paramCertificationRemark
      ,[ReminderDurationDay] =	@paramReminderDurationDay
      ,[UpdateBy] =		@paramUpdateBy		
      ,[UpdateDate] =		getdate()
      ,[IsActive] =		@paramIsActive	
    WHERE [ID] = @paramIDCLass

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

	SELECT * from [LDMS_T_Class] where [ID] = @paramIDCLass
END
GO
/****** Object:  StoredProcedure [dbo].[sp_T_ClassAttendandResult_Insert]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_T_ClassAttendandResult_Insert]
			@paramID_Class		int
           ,@paramID_Course		int
           ,@paramID_Employee		int
           ,@paramClassState		int
           ,@paramLearningResult	int
           ,@paramAttendCountNum	tinyint
           ,@paramIsScanRFID		tinyint
           ,@paramPreTestScore		int
           ,@paramPostTestScore		int
           ,@paramSkillScore		int
           ,@paramCoachingStatus	tinyint
           ,@paramCertificateStatus	tinyint
           ,@paramCreateBy		nvarchar(50)          
           
AS
BEGIN
	
	SET NOCOUNT ON;

   INSERT INTO [dbo].[LDMS_T_ClassAttendAndResult]
           ([ID_Class]
           ,[ID_Course]
           ,[ID_Employee]
           ,[ClassState]
           ,[LearningResult]
           ,[AttendCountNum]
           ,[IsScanRFID]
           ,[PreTestScore]
           ,[PostTestScore]
           ,[SkillScore]
           ,[CoachingStatus]
           ,[CertificateStatus]
           ,[CreateBy]
           ,[CreateDate]
           ,[UpdateBy]
           ,[UpdateDate])
     VALUES
           (@paramID_Class		
           ,@paramID_Course		
           ,@paramID_Employee		
           ,@paramClassState		
           ,@paramLearningResult	
           ,@paramAttendCountNum	
           ,@paramIsScanRFID		
           ,@paramPreTestScore		
           ,@paramPostTestScore		
           ,@paramSkillScore		
           ,@paramCoachingStatus	
           ,@paramCertificateStatus	
           ,@paramCreateBy		
           ,getdate()		
           ,@paramCreateBy		
           ,getdate())

	SELECT @@IDENTITY [ID]

END
GO
/****** Object:  StoredProcedure [dbo].[sp_T_ClassCertificate_Insert]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_T_ClassCertificate_Insert]
			@paramID_Class		int
           ,@paramID_Course		int
           ,@paramID_Employee		int
           ,@paramCertificate_Path	nvarchar(100)
           ,@paramDescription		nvarchar(100)
           ,@paramCreateBy		nvarchar(50)
           
           
               
           
AS
BEGIN
	
	SET NOCOUNT ON;

   INSERT INTO [dbo].[LDMS_T_ClassCertificate]
           ([ID_Class]
           ,[ID_Course]
           ,[ID_Employee]
           ,[Certificate_Path]
           ,[Description]
           ,[CreateBy]
           ,[CreateDate]
           ,[UpdateBy]
           ,[UpdateDate])
     VALUES
           (@paramID_Class		
           ,@paramID_Course		
           ,@paramID_Employee		
           ,@paramCertificate_Path	
           ,@paramDescription		
           ,@paramCreateBy		
           ,getdate()		
           ,@paramCreateBy		
           ,getdate())

	SELECT @@IDENTITY [ID]

END
GO
/****** Object:  StoredProcedure [dbo].[sp_T_ClassRegistration_Insert]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_T_ClassRegistration_Insert]
			@paramID_Class			int
           ,@paramID_Course			int
           ,@paramID_Employee		int
           ,@paramRegisterStatus	int
           ,@paramRemarkManager		nvarchar(50)
           ,@paramRegisterDate		datetime
           ,@paramUpdateBy			nvarchar(50)
           ,@paramRemarkAdmin		nvarchar(50)	        
           
           
AS
BEGIN
	
	SET NOCOUNT ON;

   INSERT INTO [dbo].[LDMS_T_ClassRegistration]
           ([ID_Class]
           ,[ID_Course]
           ,[ID_Employee]
           ,[RegisterStatus]
           ,[RemarkManager]
           ,[RegisterDate]
           ,[UpdateBy]
           ,[UpdateDate]
           ,[RemarkAdmin])
     VALUES
           (@paramID_Class	
           ,@paramID_Course	
           ,@paramID_Employee	
           ,@paramRegisterStatus
           ,@paramRemarkManager	
           ,@paramRegisterDate	
           ,@paramUpdateBy	
           ,getdate()	
           ,@paramRemarkAdmin)

	SELECT @@IDENTITY [ID]

END
GO
/****** Object:  StoredProcedure [dbo].[sp_T_ClassRegistration_Update]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE  PROCEDURE [dbo].[sp_T_ClassRegistration_Update]
	-- Add the parameters for the stored procedure here 
	@paramIDCLassREgister int,
	@paramID_Class		int,
	@paramID_Course		int,
	@paramID_Employee	int,
	@paramRegisterStatus	int,
	@paramRemarkManager	nvarchar(50),
	@paramRegisterDate	datetime,
	@paramUpdateBy		nvarchar(50),
	@paramUpdateDate	datetime,
	@paramRemarkAdmin	nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

BEGIN TRANSACTION;
BEGIN TRY
	UPDATE [dbo].[LDMS_T_ClassRegistration]
   SET [ID_Class] =			@paramID_Class		
      ,[ID_Course] =		@paramID_Course			
      ,[ID_Employee] =		@paramID_Employee		
      ,[RegisterStatus] =	@paramRegisterStatus		
      ,[RemarkManager] =	@paramRemarkManager		
      ,[RegisterDate] =		@paramRegisterDate		
      ,[UpdateBy] =			@paramUpdateBy		
      ,[UpdateDate] =		@paramUpdateDate		
      ,[RemarkAdmin] =		@paramRemarkAdmin
    WHERE [ID] = @paramIDCLassREgister

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

	SELECT 1
END
GO
/****** Object:  StoredProcedure [dbo].[sp_T_CourseTargetAudient_Search]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
CREATE  PROCEDURE [dbo].[sp_T_CourseTargetAudient_Search]
	 @paramCourseId nvarchar(50) 
	 ,@paramPlantId nvarchar(50) 
	 ,@paramCenterId nvarchar(50) 
	 ,@paramDivisionId nvarchar(50) 
	 ,@paramDepartmentId nvarchar(50) 
	 ,@paramSectionId nvarchar(50) 

AS
BEGIN
	 
	 DECLARE @paramID_Course nvarchar(50) 

	------DEBUG : Start ------------
	 --DECLARE @paramCourseId nvarchar(50) 
	 --DECLARE @paramPlantId nvarchar(50) 
	 --DECLARE @paramCenterId nvarchar(50) 
	 --DECLARE @paramDivisionId nvarchar(50) 
	 --DECLARE @paramDepartmentId nvarchar(50) 
	 --DECLARE @paramSectionId nvarchar(50) 

	 --SET @paramCourseId = 'LCO001'
	 --SET @paramPlantId = '2'
	 --SET @paramCenterId = '1'
	 --SET @paramDivisionId = '1'
	 --SET @paramDepartmentId = '7'
	 --SET @paramSectionId = '221'
	------DEBUG : END ----------------

	SET @paramID_Course = (SELECT ID FROM LDMS_M_Course WHERE CourseID = @paramCourseId)


	SELECT mu.EmployeeID			as EmployeeID
			,mu.Name				as EmployeeName
			,mjg.JobGradeName_EN	as JobGrade
			,mjt.JobTitleName_EN	as JobTitle
			,mp.PlantName_EN		as Plant
			,mc.CenterName_EN		as Center
			,mdi.DivisionName_EN	as Division
			,mde.DepartmentName_EN	as Department
			,CASE
				WHEN (tcr.ID_Employee is not null) AND ((tca.ID_Employee is not null) or (tca.LearningResult = 99))
				THEN 'Registered' ---- Status: Register + On Progress + Qualified
				ELSE 'UnQualified' ---- Status: Not Register + UnQualified
				END as RegistStatus
	FROM LDMS_M_User mu
	INNER JOIN LDMS_M_UserRole mur on mu.EmployeeID = mur.EmployeeID
	INNER JOIN LDMS_M_JobGrade mjg on mjg.ID = mu.ID_JobGrade
	INNER JOIN LDMS_M_JobTitle mjt on mjt.ID = mu.ID_JobTitle
	INNER JOIN LDMS_M_Plant mp on mp.ID = mu.ID_Plant
	INNER JOIN LDMS_M_Center mc on mc.ID = mu.ID_Center
	INNER JOIN LDMS_M_Division mdi on mdi.ID = mu.ID_Division
	INNER JOIN LDMS_M_Department mde on mde.ID = mu.ID_Department
	LEFT JOIN LDMS_T_ClassAttendAndResult tca on tca.ID_Employee = mur.ID and tca.ID_Course = @paramID_Course
	LEFT JOIN LDMS_T_ClassRegistration tcr on tcr.ID_Employee = mur.ID and tcr.ID_Course = @paramID_Course
	WHERE mu.IsActive = 1
		AND ((mu.EmployeeID IN (SELECT EmployeeID FROM LDMS_T_CourseEmployee WHERE ID_Course = @paramID_Course )) 
				OR ((1 =  CASE WHEN @paramPlantId IS NOT NULL AND @paramPlantId <> '' AND @paramPlantId <> ' ' THEN
								CASE WHEN UPPER(mu.ID_Plant) like '%'+ UPPER(@paramPlantId) +'%' THEN 1 ELSE 0 END
								ELSE 1 END)
					AND (1 =  CASE WHEN @paramCenterId IS NOT NULL AND @paramCenterId <> '' AND @paramCenterId <> ' ' THEN
								CASE WHEN UPPER(mu.ID_Center) like '%'+ UPPER(@paramCenterId) +'%' THEN 1 ELSE 0 END
								ELSE 1 END)
					AND (1 =  CASE WHEN @paramDivisionId IS NOT NULL AND @paramDivisionId <> '' AND @paramDivisionId <> ' ' THEN
								CASE WHEN UPPER(mu.ID_Division) like '%'+ UPPER(@paramDivisionId) +'%' THEN 1 ELSE 0 END
								ELSE 1 END)
					AND (1 =  CASE WHEN @paramDepartmentId is NOT NULL AND @paramDepartmentId <> '' AND @paramDepartmentId<> ' ' THEN
								CASE WHEN UPPER(mu.ID_Department) like '%'+ UPPER(@paramDepartmentId) +'%' THEN 1 ELSE 0 END
								ELSE 1 END)
					AND (1 =  CASE WHEN @paramSectionId IS NOT NULL AND @paramSectionId <> '' AND @paramSectionId <> ' ' THEN
								CASE WHEN UPPER(mur.ID_Section) like '%'+ UPPER(@paramSectionId) +'%' THEN 1 ELSE 0 END
								ELSE 1 END))
					AND (mu.ID_JobGrade IN (SELECT jg.ID FROM LDMS_T_CourseJobGrade cj 
																INNER JOIN LDMS_M_JobGrade jg on cj.ID_JobGrade = jg.JobGradeID
																WHERE cj.ID_Course = @paramID_Course))
					AND (mu.ID_JobTitle IN (SELECT JT.ID FROM LDMS_T_CourseJobTitle ct
																INNER JOIN LDMS_M_JobTitle jt on ct.ID_JobTitle = jt.JobTitleID
																WHERE ID_Course = @paramID_Course))
					)

	GROUP BY mu.EmployeeID
			,mu.Name
			,mjg.JobGradeName_EN
			,mjt.JobTitleName_EN
			,mp.PlantName_EN
			,mc.CenterName_EN
			,mdi.DivisionName_EN
			,mde.DepartmentName_EN
			,tcr.ID_Employee
			,tca.ID_Employee
			,tca.LearningResult
			  
	
END
GO
/****** Object:  StoredProcedure [dbo].[usp_Center_READ_ALL]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE   PROCEDURE [dbo].[usp_Center_READ_ALL] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT ROW_NUMBER() OVER(PARTITION BY ID ORDER BY ID ASC)  as RowIndex,* FROM LDMS_M_Center WITH (NOLOCK) WHERE IsActive=1;
END
GO
/****** Object:  StoredProcedure [dbo].[usp_Course_Master_Report]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- EXEC usp_Course_Master_Report @paramTrainingDateFrm ='2020/04/01',@paramTrainingDateTo='2020/04/30'
-- =============================================
CREATE   PROCEDURE [dbo].[usp_Course_Master_Report]
	@paramTrainingDateFrm DATETIME,
	@paramTrainingDateTo DATETIME,
	@paramCourseId nvarchar(100) =null,
	@paramDepartmentId INT =null,
	@paramJobGradeId INT =null,
	@paramStatus INT =null,	
	@paramTrainingStatus INT =null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
	Platfor.PlatformName_EN AS "Platform",
	SubPlatform.SubPlatformName_EN AS "Sub Platform",
	Course.CourseID AS "Course Code",
	Course.CourseName AS "Course Name",
	CourseType.CourseTypeName_EN AS "Course Type",
	class.ClassID AS "Class ID",
	'' AS "Emp ID",
	'' AS "Employee Name",
	'' AS "Job Grade",
	'' AS "Job Title",
	'' AS "Department",
	'' AS "Division",
	'' AS "Section",
	'' AS "Status Register",
	'' AS "Status Approval",
	'' AS "Status Attend",
	1200 AS "Class Fee",
	100 AS "Course Amount Learner",
	80 AS "Course Amount Qualifield",
	20 AS "Course Amount Unqualifield",
	80 AS "Course Avg.Score"
	FROM  LDMS_M_Course  Course 
	INNER JOIN LDMS_M_CourseType CourseType ON Course.ID_CourseType = CourseType.ID
	INNER JOIN LDMS_M_SubPlatformCourse  SubPlatformCourse ON Course.ID = SubPlatformCourse.ID_Course
	INNER JOIN LDMS_M_SubPlatform SubPlatform ON SubPlatformCourse.ID_SubPlatform =  SubPlatform.ID
	INNER JOIN LDMS_M_Platform  Platfor ON SubPlatform.ID_Platform = Platfor.ID
	INNER JOIN LDMS_T_Class class on Course.ID = class.ID_Course

	WHERE 1 = (CASE WHEN @paramCourseId <> '' and @paramCourseId is not null THEN
				CASE WHEN Course.ID = @paramCourseId THEN 1 ELSE 0 END
				ELSE 1 END)


END
GO
/****** Object:  StoredProcedure [dbo].[usp_Course_READ_ALL]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE   PROCEDURE [dbo].[usp_Course_READ_ALL] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT ROW_NUMBER() OVER(PARTITION BY ID ORDER BY ID ASC)  as RowIndex,* FROM LDMS_M_Course WITH (NOLOCK) WHERE IsActive=1;
END
GO
/****** Object:  StoredProcedure [dbo].[usp_Department_READ_ALL]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE   PROCEDURE [dbo].[usp_Department_READ_ALL] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT ROW_NUMBER() OVER(PARTITION BY ID ORDER BY ID ASC)  as RowIndex,
	dep.ID as ID_Department,dep.DepartmentID,dep.DepartmentName_EN,dep.DepartmentName_TH,dep.Description ,dep.ID_Division
	FROM LDMS_M_Department dep WITH (NOLOCK) WHERE dep.IsActive=1;
END
GO
/****** Object:  StoredProcedure [dbo].[usp_Division_READ_ALL]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE   PROCEDURE [dbo].[usp_Division_READ_ALL] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT ROW_NUMBER() OVER(PARTITION BY ID ORDER BY ID ASC)  as RowIndex,* FROM LDMS_M_Division WITH (NOLOCK) WHERE IsActive=1;
END
GO
/****** Object:  StoredProcedure [dbo].[usp_Instructor_Master_Report]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- EXEC usp_Instructor_Master_Report @paramTrainingDateFrm ='2020/04/01',@paramTrainingDateTo='2020/04/30'
-- =============================================
CREATE   PROCEDURE [dbo].[usp_Instructor_Master_Report]
	@paramTrainingDateFrm DATETIME,
	@paramTrainingDateTo DATETIME,
	@paramInstructorId nvarchar(100) =null,
	@paramDepartmentId INT =null,
	@paramJobGradeId INT =null,
	@paramStatus INT =null,	
	@paramTrainingStatus INT =null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT 
	Instructor.Instructor_ID AS "ID",
	Instructor.Instructor_Name as "Name",
	CASE Instructor.IsActive WHEN 1 THEN 'Active' ELSE 'In Active' END AS   "Status",
	Instructor.[Type] AS "Type",
	Instructor.ID_Employee AS "EmpID",
	'' AS "Job Level",
	'' AS "Job Grade",
	'' AS "Job Title",
	'' AS "Department",
	'' AS "Division",
	'' AS "Section",
	Instructor.Organization AS "Organization",
	Instructor.[Address] AS "Adress",
	Instructor.Phone AS "Phone no.",
	Instructor.Email AS "email",
	Instructor.Course_Experience AS "Experience",
	Instructor.Education_Level AS "Education",
	Instructor.Customer_Reference AS "Customer Referrence", 
	'' AS "Industrusty",
	Course.CourseID AS "courseID",
	Course.CourseName AS "Coursename",
	50 AS "Amount Learner",
	20 AS "Amount Qualifield",
	20 AS "Amount Unqualifield",
	100 AS "Avg.Score"
	FROM  LDMS_M_Course  Course 
	LEFT OUTER JOIN LDMS_T_Class class on Course.ID = class.ID_Course
	LEFT OUTER JOIN LDMS_M_Instructor Instructor on class.ID_Instructor = Instructor.Id

	WHERE 1 = (CASE WHEN @paramInstructorId <> '' and @paramInstructorId is not null THEN
				CASE WHEN Instructor.Instructor_ID = @paramInstructorId THEN 1 ELSE 0 END
				ELSE 1 END)


END
GO
/****** Object:  StoredProcedure [dbo].[usp_JobGrade_READ_ALL]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE   PROCEDURE [dbo].[usp_JobGrade_READ_ALL] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT ROW_NUMBER() OVER(PARTITION BY ID ORDER BY ID ASC)  as RowIndex,* FROM LDMS_M_JobGrade WITH (NOLOCK)  WHERE IsActive=1;
END
GO
/****** Object:  StoredProcedure [dbo].[usp_JobTitle_READ_ALL]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE   PROCEDURE [dbo].[usp_JobTitle_READ_ALL] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT ROW_NUMBER() OVER(PARTITION BY ID ORDER BY ID ASC)  as RowIndex,* FROM LDMS_M_JobTitle WITH (NOLOCK)  WHERE IsActive=1;
END
GO
/****** Object:  StoredProcedure [dbo].[usp_LDMS_M_UserRole_UpdateSection]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE   PROCEDURE [dbo].[usp_LDMS_M_UserRole_UpdateSection]
	-- Add the parameters for the stored procedure here
	@employeeId nvarchar(50),
	@sectionId INT,
	@sectionHeader TINYINT,
	@updateBy nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	UPDATE	[dbo].[LDMS_M_UserRole]
	SET   
		[IsSectionHead] = @sectionHeader
       ,[ID_Section] = @sectionId 
       ,[IsActive] = 1
	   ,[UpdateBy] = @updateBy
	   ,[UpdateDate] = getdate()
	WHERE [EmployeeID]  = @employeeId; 
END
GO
/****** Object:  StoredProcedure [dbo].[usp_LDMS_T_DataLog_Create]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE   PROCEDURE [dbo].[usp_LDMS_T_DataLog_Create]
	-- Add the parameters for the stored procedure here
	@LogTypeId INT,
	@EmployeeID nvarchar(50)=null,
	@ip_address nvarchar(50)=null,
	@LogDetail text =null,
	@CreateBy  nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	INSERT INTO [dbo].[LDMS_T_DataLog]
           (LogDateTime
		   ,[LogTypeId]
           ,[EmployeeID]
           ,[ip_address]
           ,[LogDetail]
		   ,[CreateBy]
		   ,[IsActive])
     VALUES
           (getdate()
		   ,@LogTypeId
           ,@EmployeeID
           ,@ip_address
           ,@LogDetail
		   ,@CreateBy
		   ,1)
END
GO
/****** Object:  StoredProcedure [dbo].[usp_Plaform_Master_Report]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- EXEC usp_Plaform_Master_Report @paramTrainingDateFrm ='2020/04/01',@paramTrainingDateTo='2020/04/30'
-- =============================================
CREATE   PROCEDURE [dbo].[usp_Plaform_Master_Report]
	@paramTrainingDateFrm DATETIME,
	@paramTrainingDateTo DATETIME,
	@paramPlaformId nvarchar(100) =null,
	@paramDepartmentId INT =null,
	@paramJobGradeId INT =null,
	@paramStatus INT =null,	
	@paramTrainingStatus INT =null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	 SELECT
	Platfor.PlatformName_EN AS "Platform",
	SubPlatform.SubPlatformName_EN AS "Sub Platform",
	Course.CourseID AS "Course Code",
	Course.CourseName AS "Course Name",
	CourseType.CourseTypeName_EN AS "Course Type",
	class.ClassID AS "Class ID",
	'' AS "Emp ID",
	'' AS "Employee Name",
	'' AS "Job Level",
	'' AS "Job Grade",
	'' AS "Job Title",
	'' AS "Department",
	'' AS "Division",
	'' AS "Section",
	'' AS "Status Register",
	'' AS "Status Approval",
	'' AS "Status Attend",
	1200 AS "Class Fee",
	100 AS "Course Amount Learner",
	80 AS "Course Amount Qualifield",
	20 AS "Course Amount Unqualifield",
	80 AS "Course Avg.Score"
	FROM  LDMS_M_Course  Course 
	INNER JOIN LDMS_M_CourseType CourseType ON Course.ID_CourseType = CourseType.ID
	INNER JOIN LDMS_M_SubPlatformCourse  SubPlatformCourse ON Course.ID = SubPlatformCourse.ID_Course
	INNER JOIN LDMS_M_SubPlatform SubPlatform ON SubPlatformCourse.ID_SubPlatform =  SubPlatform.ID
	INNER JOIN LDMS_M_Platform  Platfor ON SubPlatform.ID_Platform = Platfor.ID
	INNER JOIN LDMS_T_Class class on Course.ID = class.ID_Course

	WHERE 1 = (CASE WHEN @paramPlaformId <> '' and @paramPlaformId is not null THEN
				CASE WHEN Platfor.ID = @paramPlaformId THEN 1 ELSE 0 END
				ELSE 1 END)
END
GO
/****** Object:  StoredProcedure [dbo].[usp_Plant_READ_ALL]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE   PROCEDURE [dbo].[usp_Plant_READ_ALL] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT ROW_NUMBER() OVER(PARTITION BY ID ORDER BY ID ASC)  as RowIndex,* FROM LDMS_M_Plant WITH (NOLOCK) WHERE IsActive=1;
END
GO
/****** Object:  StoredProcedure [dbo].[usp_Platform_READ_ALL]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE   PROCEDURE [dbo].[usp_Platform_READ_ALL] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT ROW_NUMBER() OVER(PARTITION BY ID ORDER BY ID ASC)  as RowIndex,* FROM LDMS_M_Platform WITH (NOLOCK) WHERE IsActive=1;
END
GO
/****** Object:  StoredProcedure [dbo].[usp_Role_READ_ALL]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE   PROCEDURE [dbo].[usp_Role_READ_ALL] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT ROW_NUMBER() OVER(ORDER BY ID ASC)  as RowIndex,ID AS RoleId,ISNULL(RoleName_EN,'') AS RoleName_EN,ISNULL(RoleName_TH,'') AS RoleName_TH,ISNULL(RoleDescription,'') AS RoleDescription  FROM LDMS_M_Role WITH (NOLOCK)  WHERE IsActive=1;
END
GO
/****** Object:  StoredProcedure [dbo].[usp_RoleMenu_READ_By_Role]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- exec [usp_RoleMenu_READ_By_Role] 7
-- =============================================
CREATE   PROCEDURE [dbo].[usp_RoleMenu_READ_By_Role]
	-- Add the parameters for the stored procedure here
	@paramRoleId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	 
	 SELECT
	 ROW_NUMBER() OVER(ORDER BY subm.ID  ASC)  as RowIndex,
	 subm.ID as ID_SubModule,
	 subm.Description as 'Description',
	 subm.Icon,
	 subm.Sequence,	 
	 subm.SubModuleID,
	 subm.SubModuleName_EN,
	 subm.SubModuleName_TH,
	  ISNULL(subm.URL,'')  AS SubModule_URL,

	 mol.ID AS ID_Module,
	 mol.ModuleID,
	 mol.ModuleName_EN,
	 mol.ModuleName_TH,
	 mol.Sequence as Module_Sequence,
	 ISNULL(mol.URL,'')   as Module_URL,
	 
	 rolm.ID as RolePermissionId,
	 rolm.PermissionType,
	 rolm.RolePermissionDescription, 

	 rol.ID as RoleId,
	 rol.RoleDescription,
	 rol.RoleName_EN,
	 rol.RoleName_TH  
	 FROM 
	 LDMS_M_RolePermission rolm   WITH (NOLOCK) 
	 INNER JOIN LDMS_M_Role rol   WITH (NOLOCK)  on rolm.ID_Role = rol.ID AND rol.IsActive =1
	 INNER JOIN LDMS_M_SubModule subm  WITH (NOLOCK)  on rolm.ID_SubModule = subm.ID and subm.IsActive =1
	 INNER JOIN LDMS_M_Module mol WITH (NOLOCK)  on subm.ID_Module = mol.ID and mol.IsActive =1
	 WHERE rolm.IsActive = 1 and rol.ID = @paramRoleId
END
GO
/****** Object:  StoredProcedure [dbo].[usp_Section_Create]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE   PROCEDURE [dbo].[usp_Section_Create]
	-- Add the parameters for the stored procedure here
	@param_derpartmentId int,
	@para_sectionCode nvarchar(50),
	@param_sectionNameEN nvarchar(50),
	@param_sectionNameTH nvarchar(50),
	@param_sectionDesc nvarchar(100),
	@paramCreateBy nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	INSERT INTO [dbo].[LDMS_M_Section]
           ([ID]
           ,[SectionID]
           ,[ID_Department]
           ,[SectionName_EN]
           ,[SectionName_TH]
           ,[Description]
           ,[CreateBy]
           ,[CreateDate] 
           ,[IsActive])
	values ((SELECT ISNULL(MAX(ID),0)+1 FROM [dbo].[LDMS_M_Section]),
	@para_sectionCode,
	@param_derpartmentId,
	@param_sectionNameEN,
	@param_sectionNameTH,
	@param_sectionDesc,
	@paramCreateBy,
	GETDATE(),
	1)
END
GO
/****** Object:  StoredProcedure [dbo].[usp_Section_Delete]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE   PROCEDURE [dbo].[usp_Section_Delete]
	-- Add the parameters for the stored procedure here
	@param_sectionId int, 
	@paramUpdateBy nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	UPDATE [dbo].[LDMS_M_Section]
   SET 
	   [UpdateBy] = @paramUpdateBy
      ,[UpdateDate] = GETDATE()
      ,[IsActive] = 0
   WHERE [ID] =@param_sectionId; 

   UPDATE LDMS_M_UserRole
   SET ID_Section = NULL 
   WHERE ID_Section = @param_sectionId;

END
GO
/****** Object:  StoredProcedure [dbo].[usp_Section_READ_ALL]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE   PROCEDURE [dbo].[usp_Section_READ_ALL] 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT ROW_NUMBER() OVER(PARTITION BY ID ORDER BY ID ASC)  as RowIndex,* FROM LDMS_M_Section WITH (NOLOCK) WHERE IsActive=1;
END
GO
/****** Object:  StoredProcedure [dbo].[usp_Section_Update]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE   PROCEDURE [dbo].[usp_Section_Update]
	-- Add the parameters for the stored procedure here
	@param_sectionId int,
	@param_derpartmentId int,
	@para_sectionCode nvarchar(50),
	@param_sectionNameEN nvarchar(50),
	@param_sectionNameTH nvarchar(50),
	@param_sectionDesc nvarchar(100),
	@paramUpdateBy nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	UPDATE [dbo].[LDMS_M_Section]
   SET  [SectionID] = @para_sectionCode 
      ,[SectionName_EN] = @param_sectionNameEN
      ,[SectionName_TH] = @param_sectionNameTH
      ,[Description] =@param_sectionDesc 
      ,[UpdateBy] = @paramUpdateBy
      ,[UpdateDate] = GETDATE()
      ,[IsActive] = 1
 WHERE [ID] =@param_sectionId; 
END
GO
/****** Object:  StoredProcedure [dbo].[usp_User_AllowGPP]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE     PROCEDURE [dbo].[usp_User_AllowGPP]
-- Add the parameters for the stored procedure here
		@EmployeeId nvarchar(50),
		@IsAllow tinyint,
		@UpdateBy nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF EXISTS (SELECT * FROM [dbo].[LDMS_M_UserRole] WHERE [EmployeeID]  = @EmployeeId)
		BEGIN
		-- Insert statements for procedure here
		UPDATE	[dbo].[LDMS_M_UserRole]
			SET  
				[IsActive] = 1 
			   ,IsAllowGPP = @IsAllow
			   ,[UpdateBy] = @UpdateBy
			   ,[UpdateDate] = GETDATE()
		   		WHERE [EmployeeID]  = @EmployeeId
		END
		ELSE
		BEGIN
		 SELECT -- As many or few of these as you care to return
			100 AS ErrorNumber
		   ,100 AS ErrorSeverity
		   ,100 AS ErrorState
		   ,'usp_User_AllowGPP' AS ErrorProcedure
		   ,0 AS ErrorLine
		   ,'Cannot Update user does'' exists' AS ErrorMessage;
	   END
END
GO
/****** Object:  StoredProcedure [dbo].[usp_User_ChangePassword]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE     PROCEDURE [dbo].[usp_User_ChangePassword]
-- Add the parameters for the stored procedure here
		@EmployeeId nvarchar(50),
		@OldPassword  nvarchar(1024),
		@Password  nvarchar(1024),
		@PasswordSalt  nvarchar(1024),
		@UpdateBy nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF EXISTS (SELECT * FROM [dbo].[LDMS_M_UserRole] WHERE [EmployeeID]  = @EmployeeId AND [Password] = @OldPassword)
		BEGIN

		-- Insert statements for procedure here
		UPDATE	[dbo].[LDMS_M_UserRole]
			SET 
				[Password] = @Password   
			   ,[IsActive] = 1
			   ,[PasswordSalt] = @PasswordSalt
			   ,[IsForceChangePass] = 0
			   ,[Is_FirstLogin] = 0
			   ,[UpdateBy] = @UpdateBy
			   ,[UpdateDate] = GETDATE()
		   		WHERE [EmployeeID]  = @EmployeeId AND [Password] = @OldPassword;
		END
		ELSE
		BEGIN
		 SELECT -- As many or few of these as you care to return
			100 AS ErrorNumber
		   ,100 AS ErrorSeverity
		   ,100 AS ErrorState
		   ,'usp_User_ChangePassword' AS ErrorProcedure
		   ,0 AS ErrorLine
		   ,'Cannot Update user does'' exists' AS ErrorMessage;
	   END
END
GO
/****** Object:  StoredProcedure [dbo].[usp_User_Create]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE     PROCEDURE [dbo].[usp_User_Create]
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
	@Password  nvarchar(1024),
	@PasswordSalt  nvarchar(1024),
	@Remark nvarchar(250)  =null,
	@PhoneNumber  nvarchar(50)  =null,
	@Email  nvarchar(50)  =null,
	@CreateBy  nvarchar(50)  =null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	DECLARE @PlantId INT;
	DECLARE @JoinDate DATETIME= GETDATE();
	SELECT @PlantId = ID_Plant FROM LDMS_M_Center  WHERE ID = @CenterId;
	IF EXISTS (SELECT * FROM LDMS_M_User WHERE EmployeeID = @EmployeeId)
		RAISERROR (100,-1,-1, 'An Employee ID already exists');  

BEGIN TRANSACTION;
BEGIN TRY
	INSERT INTO [dbo].[LDMS_M_User]
           ([EmployeeID]
           ,[IsAD]
           ,[DriverLicenseID]
           ,[IDCardNumber]
           ,[Gender]
           ,[Name]
           ,[Surname]
           ,[Nationality]
           ,[ID_JobGrade]
           ,[ID_JobTitle]
           ,[ID_Plant]
           ,[ID_Center]
           ,[ID_Division]
           ,[ID_Department]
           ,[JoinDate]
           ,[OutDate]
           ,[DateOfBirth]
           ,[PhoneNumber]
           ,[Email]
           ,[ProfilePath]
           ,[CreateBy]
           ,[CreateDate]
           ,[UpdateBy]
           ,[UpdateDate]
           ,[IsActive])
     VALUES
           (@EmployeeId
           ,0
           ,null
           ,null
           ,@Gender
           ,@EmployeeName
           ,@EmployeeSurName
           ,@Nationality
           ,@JobGradeId
           ,@JobTitleId
           ,@PlantId
           ,@CenterId
           ,@DivisionId
           ,@DepartmentId
           ,@JoinDate
           ,null
           ,null
           ,@PhoneNumber
           ,@Email
           ,null
           ,@CreateBy
           ,null
           ,null
           ,null
           ,1);

INSERT INTO [dbo].[LDMS_M_UserRole]
           ([EmployeeID]
           ,[ID_Role]
           ,[Password]
           --,[IsInstructor]
           --,[IsSectionHead]
           ,[ID_Section]
           ,[Remark]
           ,[CreateBy]
           ,[CreateDate] 
           ,[IsActive]
           ,[PasswordSalt]
		   ,[IsForceChangePass])
     VALUES
           (@EmployeeId
           ,@RoleId
           ,@Password
           --,@IsInstructer
           --,@IsSectionHead
           ,@SectionId
           ,@Remark
           ,@CreateBy
           ,GETDATE() 
           ,1
           ,@PasswordSalt,
		   0);
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
/****** Object:  StoredProcedure [dbo].[usp_User_Delete]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE   PROCEDURE [dbo].[usp_User_Delete]
	-- Add the parameters for the stored procedure here
	@paramEmployeeId nvarchar(50),
	@paramUpdateBy nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	UPDATE LDMS_M_User
	SET IsActive = 0 ,UpdateBy = @paramUpdateBy,UpdateDate =GETDATE()
	WHERE EmployeeID = @paramEmployeeId AND IsAD = 0;
END
GO
/****** Object:  StoredProcedure [dbo].[usp_User_READ_ALL]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- EXEC [dbo].[usp_User_READ_ALL] @paramEmployeeId=null,@paramEmployeeName=null,@paramdepartments='2'
-- =============================================
CREATE   PROCEDURE [dbo].[usp_User_READ_ALL]
	 @paramEmployeeId nvarchar(50) = null,
	 @paramEmployeeName nvarchar(50) = null,
	 @paramdepartments nvarchar(max) = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF @paramdepartments IS NULL
		SET @paramdepartments=''; 
    Declare @departments table (ID INT);

	INSERT INTO @departments SELECT  Item FROM dbo.SplitInts(@paramdepartments, ','); 

    -- Insert statements for procedure here
	SELECT  
	ROW_NUMBER() OVER(ORDER BY muser.EmployeeID ASC)  as RowIndex,
	muser.DateOfBirth,muser.DriverLicenseID,muser.email,muser.EmployeeID,muser.Gender,muser.ID_Center,
	muser.ID_Division,muser.ID_JobGrade,muser.ID_JobTitle,muser.IDCardNumber,muser.IsAD,muser.JoinDate ,
	muser.Name,muser.Nationality,muser.OutDate,muser.PhoneNumber,muser.Surname,muser.ID_Center,muser.ID_Division,muser.ID_Department,
	usrRole.ID as UserRoleId,usrRole.ID_Role,usrRole.IsInstructor,usrRole.Password,usrRole.Remark,usrRole.ID_Section,usrRole.passwordSalt,usrRole.IsSectionHead,
	rol.ID as RoleId,rol.RoleDescription,rol.RoleName_EN,rol.RoleName_TH,
	dep.ID as ID_Department,dep.DepartmentID,dep.DepartmentName_EN,dep.DepartmentName_TH,dep.Description,
	muser.ID_Plant,plant.PlantDescription,plant.PlantID,plant.PlantName_EN,plant.PlantName_TH
	FROM   LDMS_M_User  muser WITH (NOLOCK) 
	LEFT OUTER JOIN LDMS_M_UserRole usrRole WITH (NOLOCK)  on muser.EmployeeID = usrRole.EmployeeID
	LEFT OUTER JOIN LDMS_M_Role rol  WITH (NOLOCK) on usrRole.ID_Role = rol.ID
	LEFT OUTER JOIN LDMS_M_Department dep  WITH (NOLOCK) on muser.ID_Department = dep.ID
	LEFT OUTER JOIN LDMS_M_Plant plant  WITH (NOLOCK) on muser.ID_Plant = plant.ID
	WHERE muser.IsActive =1
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
END
GO
/****** Object:  StoredProcedure [dbo].[usp_User_READ_BY_DepartmentId]    Script Date: 4/24/2020 8:06:07 AM ******/
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
CREATE   PROCEDURE [dbo].[usp_User_READ_BY_DepartmentId]
	-- Add the parameters for the stored procedure here
	@param_DepartmentId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON; 

	SELECT ROW_NUMBER() OVER(ORDER BY muser.EmployeeID ASC)  as RowIndex,
	muser.DateOfBirth,muser.DriverLicenseID,muser.email AS Email,muser.EmployeeID,muser.Gender,muser.ID_Center,
	muser.ID_Division,muser.ID_JobGrade,muser.ID_JobTitle,muser.IDCardNumber,muser.IsAD,muser.JoinDate,
	muser.Name,muser.Nationality,muser.OutDate,muser.PhoneNumber,muser.Surname,




	usrRole.ID as UserRoleId,usrRole.ID_Role,usrRole.IsInstructor,usrRole.Password,usrRole.Remark,usrRole.ID_Section,usrRole.passwordSalt,usrRole.IsSectionHead,
	rol.ID as RoleId,rol.RoleDescription,rol.RoleName_EN,rol.RoleName_TH,IsForceChangePass,
	dep.ID as ID_Department,dep.DepartmentID,dep.DepartmentName_EN,dep.DepartmentName_TH,dep.Description,
	muser.ID_Plant,plant.PlantDescription,plant.PlantID,plant.PlantName_EN,plant.PlantName_TH
	FROM   LDMS_M_User  muser WITH (NOLOCK) 
	INNER JOIN LDMS_M_Department dep WITH (NOLOCK)  on muser.ID_Department = dep.ID
	LEFT OUTER JOIN LDMS_M_UserRole usrRole WITH (NOLOCK)  on muser.EmployeeID = usrRole.EmployeeID
	LEFT OUTER JOIN LDMS_M_Role rol WITH (NOLOCK)  on usrRole.ID_Role = rol.ID
	LEFT OUTER JOIN LDMS_M_Plant plant WITH (NOLOCK)  on muser.ID_Plant = plant.ID
	WHERE dep.DepartmentID = @param_DepartmentId AND muser.IsActive =1

END
GO
/****** Object:  StoredProcedure [dbo].[usp_User_READ_BY_EmployeeId]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- EXEC [dbo].[usp_User_READ_BY_EmployeeId] 'STD001'
-- =============================================
CREATE   PROCEDURE [dbo].[usp_User_READ_BY_EmployeeId]
	-- Add the parameters for the stored procedure here
	@param_EmployeeId nvarchar(50) 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON; 

	SELECT ROW_NUMBER() OVER(ORDER BY muser.EmployeeID ASC)  as RowIndex,
	muser.DateOfBirth,muser.DriverLicenseID,muser.email AS Email,muser.EmployeeID,muser.Gender,muser.ID_Center,
	muser.ID_Division,muser.ID_JobGrade,muser.ID_JobTitle,muser.IDCardNumber,muser.IsAD,muser.JoinDate,
	muser.Name,muser.Nationality,muser.OutDate,muser.PhoneNumber,muser.Surname,
		
	usrRole.ID as UserRoleId,usrRole.ID_Role,usrRole.IsInstructor,usrRole.Password,usrRole.Remark,usrRole.ID_Section,usrRole.passwordSalt,usrRole.IsSectionHead,
	usrRole.IsForceChangePass,usrRole.Is_FirstLogin,ISNULL(usrRole.IsAllowGPP,0) AS Is_AcceptGPP,
	rol.ID as RoleId,rol.RoleDescription,rol.RoleName_EN,rol.RoleName_TH,

	dep.ID as ID_Department,dep.DepartmentID,dep.DepartmentName_EN,dep.DepartmentName_TH,dep.Description,
	muser.ID_Plant,plant.PlantDescription,plant.PlantID,plant.PlantName_EN,plant.PlantName_TH
	FROM   LDMS_M_User  muser WITH (NOLOCK) 
	LEFT OUTER JOIN LDMS_M_UserRole usrRole WITH (NOLOCK)  on muser.EmployeeID = usrRole.EmployeeID
	LEFT OUTER JOIN LDMS_M_Role rol WITH (NOLOCK)  on usrRole.ID_Role = rol.ID
	LEFT OUTER JOIN LDMS_M_Department dep WITH (NOLOCK)  on muser.ID_Department = dep.ID
	LEFT OUTER JOIN LDMS_M_Plant plant WITH (NOLOCK)  on muser.ID_Plant = plant.ID
	WHERE USRROLE.EmployeeID = @param_EmployeeId AND muser.IsActive =1

END
GO
/****** Object:  StoredProcedure [dbo].[usp_User_READ_BY_SectionId]    Script Date: 4/24/2020 8:06:07 AM ******/
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
CREATE   PROCEDURE [dbo].[usp_User_READ_BY_SectionId]
	-- Add the parameters for the stored procedure here
	@param_SectionId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON; 

	SELECT ROW_NUMBER() OVER(ORDER BY muser.EmployeeID ASC)  as RowIndex,
	muser.DateOfBirth,muser.DriverLicenseID,muser.email AS Email,muser.EmployeeID,muser.Gender,muser.ID_Center,
	muser.ID_Division,muser.ID_JobGrade,muser.ID_JobTitle,muser.IDCardNumber,muser.IsAD,muser.JoinDate,
	muser.Name,muser.Nationality,muser.OutDate,muser.PhoneNumber,muser.Surname, 

	usrRole.ID as UserRoleId,usrRole.ID_Role,usrRole.IsInstructor,usrRole.Password,usrRole.Remark,usrRole.ID_Section,usrRole.passwordSalt,usrRole.IsSectionHead,
	rol.ID as RoleId,rol.RoleDescription,rol.RoleName_EN,rol.RoleName_TH,IsForceChangePass,
	dep.ID as ID_Department,dep.DepartmentID,dep.DepartmentName_EN,dep.DepartmentName_TH,dep.Description,
	muser.ID_Plant,plant.PlantDescription,plant.PlantID,plant.PlantName_EN,plant.PlantName_TH 
	FROM   LDMS_M_User  muser WITH (NOLOCK) 
	INNER JOIN LDMS_M_Department dep WITH (NOLOCK)  on muser.ID_Department = dep.ID
	LEFT OUTER JOIN LDMS_M_UserRole usrRole WITH (NOLOCK)  on muser.EmployeeID = usrRole.EmployeeID
	LEFT OUTER JOIN LDMS_M_Role rol WITH (NOLOCK)  on usrRole.ID_Role = rol.ID
	LEFT OUTER JOIN LDMS_M_Plant plant WITH (NOLOCK)  on muser.ID_Plant = plant.ID  
	WHERE usrRole.ID_Section = @param_SectionId AND muser.IsActive =1

END
GO
/****** Object:  StoredProcedure [dbo].[usp_User_ResetPassword]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE     PROCEDURE [dbo].[usp_User_ResetPassword]
-- Add the parameters for the stored procedure here
		@EmployeeId nvarchar(50), 
		@Password  nvarchar(1024),
		@PasswordSalt  nvarchar(1024),
		@UpdateBy nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF EXISTS (SELECT * FROM [dbo].[LDMS_M_UserRole] WHERE [EmployeeID]  = @EmployeeId)
		BEGIN

		-- Insert statements for procedure here
		UPDATE	[dbo].[LDMS_M_UserRole]
			SET 
				[Password] = @Password   
			   ,[IsActive] = 1
			   ,[PasswordSalt] = @PasswordSalt
			   ,[UpdateBy] = @UpdateBy
			   ,[UpdateDate] = GETDATE()
		   		WHERE [EmployeeID]  = @EmployeeId
		END
		ELSE
		BEGIN
		 SELECT -- As many or few of these as you care to return
			100 AS ErrorNumber
		   ,100 AS ErrorSeverity
		   ,100 AS ErrorState
		   ,'usp_User_ResetPassword' AS ErrorProcedure
		   ,0 AS ErrorLine
		   ,'Cannot reset user does'' exists' AS ErrorMessage;
	   END
END
GO
/****** Object:  StoredProcedure [dbo].[usp_User_Update]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE   PROCEDURE [dbo].[usp_User_Update]
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
	@Remark nvarchar(250)  =null,
	@PhoneNumber  nvarchar(50)  =null,
	@Email  nvarchar(50)  =null,
	@UpdateBy  nvarchar(50)  =null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	DECLARE @PlantId INT;
	DECLARE @JoinDate DATETIME= GETDATE();
	SELECT @PlantId = ID_Plant FROM LDMS_M_Center  WHERE ID = @CenterId;
	IF NOT EXISTS (SELECT * FROM LDMS_M_User WHERE EmployeeID = @EmployeeId)
		RAISERROR (101,-1,-1, 'An Employee ID doen''t exists');  

BEGIN TRANSACTION;
BEGIN TRY

	UPDATE  [dbo].[LDMS_M_User]
	SET 
		[Gender] = @Gender,
		[Name] = @EmployeeName,
		[Surname] = @EmployeeSurName,
		[Nationality] =@Nationality,
		[ID_JobGrade] = @JobGradeId,
		[ID_JobTitle] =@JobTitleId,
		[ID_Plant] = @PlantId,
		[ID_Center]=@CenterId,
		[ID_Division]=@DivisionId,
		[ID_Department] = @DepartmentId,
		[PhoneNumber] = @PhoneNumber,
		[Email]=@Email,
		[UpdateBy] = @UpdateBy,
		[UpdateDate] =GETDATE(),
		[IsActive] = 1
	WHERE [EmployeeID]  = @EmployeeId; 
	
	UPDATE	[dbo].[LDMS_M_UserRole]
		SET [ID_Role] = @RoleId 
           ,[ID_Section] = @SectionId
           ,[Remark] = @Remark 
           ,[IsActive] = 1
		   ,[UpdateBy] = @UpdateBy
		   ,[UpdateDate] =GETDATE()
		   	WHERE [EmployeeID]  = @EmployeeId; 

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
/****** Object:  StoredProcedure [dbo].[usp_UserRole_READ_BY_EmployeeId]    Script Date: 4/24/2020 8:06:07 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- exec [dbo].[usp_UserRole_READ_BY_EmployeeId] 'STT00001','azsx'
-- =============================================
CREATE   PROCEDURE [dbo].[usp_UserRole_READ_BY_EmployeeId]
	-- Add the parameters for the stored procedure here
	@param_EmployeeId nvarchar(50),
	@param_Password nvarchar(100)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON; 
	SELECT  ROW_NUMBER() OVER(ORDER BY usrRole.ID ASC)  as RowIndex, usrRole.ID as UserRoleId,usrRole.ID_Role,usrRole.IsInstructor,usrRole.Password,usrRole.Remark,usrRole.passwordSalt,
	rol.ID as RoleId,rol.RoleDescription,rol.RoleName_EN,rol.RoleName_TH
	FROM   LDMS_M_UserRole usrRole WITH (NOLOCK) 
	LEFT OUTER JOIN LDMS_M_Role rol  WITH (NOLOCK) on usrRole.ID_Role = rol.ID
	WHERE  usrRole.EmployeeID = @param_EmployeeId AND usrRole.[Password] = @param_Password
END
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'VW_Flat_Course_JobTitle'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'VW_Flat_Course_JobTitle'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "LDMS_M_Course"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 240
            End
            DisplayFlags = 280
            TopColumn = 10
         End
         Begin Table = "VW_Flat_Course_Employee"
            Begin Extent = 
               Top = 151
               Left = 187
               Bottom = 302
               Right = 357
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "VW_Flat_Course_JobGrade"
            Begin Extent = 
               Top = 42
               Left = 497
               Bottom = 138
               Right = 667
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "VW_Flat_Course_JobTitle"
            Begin Extent = 
               Top = 6
               Left = 694
               Bottom = 102
               Right = 864
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 2340
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'VW_M_Course_Display'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'VW_M_Course_Display'
GO
