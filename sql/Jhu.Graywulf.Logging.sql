IF SCHEMA_ID('dev') IS NULL
EXEC('CREATE SCHEMA [dev] AUTHORIZATION [dbo]')

GO

---------------------------------------------------------------

IF OBJECT_ID('[dbo].[Event]') IS NOT NULL
DROP TABLE [dbo].[Event]

GO

CREATE TABLE [dbo].[Event]
(
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[UserGuid] [uniqueidentifier] NULL,
	[UserName] [varchar](255) NULL,
	[TaskName] [varchar](255) NULL,
	[JobGuid] [uniqueidentifier] NULL,
	[JobName] [varchar](255) NULL,
	[SessionGuid] [uniqueidentifier] NULL,
	[ContextGuid] [uniqueidentifier] NULL,
	[Source] [int] NOT NULL,
	[Severity] [tinyint] NOT NULL,
	[DateTime] [datetime] NOT NULL,
	[Order] [bigint] NOT NULL,
	[ExecutionStatus] [tinyint] NOT NULL,
	[Operation] [varchar](255) NOT NULL,
	[Server] [varchar](255) NULL,
	[Client] [varchar](255) NULL,
	[Request] [varchar](1024) NULL,
	[Message] [varchar](1024) NULL,
	CONSTRAINT [PK_Event] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	)
)

GO

---------------------------------------------------------------

IF OBJECT_ID('[dbo].[EventData]') IS NOT NULL
DROP TABLE [dbo].[EventData]

GO

CREATE TABLE [dbo].[EventData]
(
	[EventId] [bigint] NOT NULL,
	[Key] [varchar](50) NOT NULL,
	[Data] [sql_variant] NOT NULL,
	CONSTRAINT [PK_EventData] PRIMARY KEY CLUSTERED 
	(
		[EventId] ASC,
		[Key] ASC
	) 
)

GO

---------------------------------------------------------------

IF OBJECT_ID('[dbo].[EventException]') IS NOT NULL
DROP TABLE [dbo].[EventException]

GO

CREATE TABLE [dbo].[EventException]
(
	[EventId] [bigint] NOT NULL,
	[Type] [varchar](255) NULL,
	[StackTrace] [varchar](max) NULL,
	
	CONSTRAINT [PK_EventException] PRIMARY KEY CLUSTERED 
	(
		[EventId] ASC
	) 
)

GO

---------------------------------------------------------------

IF OBJECT_ID('[dbo].[EventBookmark]') IS NOT NULL
DROP TABLE [dbo].[EventBookmark]

GO

CREATE TABLE [dbo].[EventBookmark]
(
	[EventId] [bigint] NOT NULL,
	[BookmarkGuid] [uniqueidentifier] NOT NULL,
	
	CONSTRAINT [PK_EventBookmark] PRIMARY KEY CLUSTERED 
	(
		[BookmarkGuid] ASC
	) 
)

GO

---------------------------------------------------------------

IF OBJECT_ID('[dbo].[spGetEvent]') IS NOT NULL
DROP PROC [dbo].[spGetEvent]

GO

CREATE PROC [dbo].[spGetEvent]
	@EventID bigint
AS
	SELECT e.*, ex.Type, ex.StackTrace
	FROM Event e
	LEFT OUTER JOIN EventException ex ON e.ID = ex.EventId
	WHERE e.ID = @EventID
GO

---------------------------------------------------------------

IF OBJECT_ID('[dbo].[spGetEventData]') IS NOT NULL
DROP PROC [dbo].[spGetEventData]

GO

CREATE PROC [dbo].[spGetEventData]
	@EventID bigint
AS
	SELECT EventData.*
	FROM EventData
	WHERE EventData.EventId = @EventID

GO

---------------------------------------------------------------

IF OBJECT_ID('[dbo].[spCreateEvent]') IS NOT NULL
DROP PROC [dbo].[spCreateEvent]

GO

CREATE PROC [dbo].[spCreateEvent]
	@EventId bigint OUTPUT,
	@UserGuid uniqueidentifier,
	@UserName varchar(255),
	@TaskName varchar(255),
	@JobGuid uniqueidentifier,
	@JobName varchar(255),
	@SessionGuid uniqueidentifier,
	@ContextGuid uniqueidentifier,
	@Source int,
	@Severity tinyint,
	@DateTime datetime,
	@Order bigint,
	@ExecutionStatus tinyint,
	@Operation varchar(255),
	@Server varchar(255),
	@Client varchar(255),
	@Request varchar(1024),
	@Message varchar(1024),
	@ExceptionType varchar(255),
	@ExceptionStackTrace varchar(max),
	@BookmarkGuid uniqueidentifier
AS
	INSERT Event
		(UserGuid, UserName, TaskName, JobGuid, JobName,
		 SessionGuid, ContextGuid, Source, Severity, [DateTime], [Order], ExecutionStatus,
		 Operation, Server, Client, Request, Message)
	VALUES
		(@UserGuid, @UserName, @Taskname, @JobGuid, @JobName,
		 @SessionGuid, @ContextGuid, @Source, @Severity, @DateTime, @Order, @ExecutionStatus,
		 @Operation, @Server, @Client, @Request, @Message)

	SET @EventId = @@IDENTITY

	IF (@ExceptionType IS NOT NULL)
	BEGIN
		INSERT EventException
			(EventID, Type, StackTrace)
		VALUES
			(@EventID, @ExceptionType, @ExceptionStackTrace)
	END

	IF (@BookmarkGuid IS NOT NULL)
	BEGIN
		INSERT EventBookmark
			(EventID, BookmarkGuid)
		VALUES
			(@EventID, @BookmarkGuid)
	END

GO

---------------------------------------------------------------

IF OBJECT_ID('[dbo].[spCreateEventData]') IS NOT NULL
DROP PROC [dbo].[spCreateEventData]

GO

CREATE PROC [dbo].[spCreateEventData]
	@EventId bigint,
	@Key varchar(50),
	@Data sql_variant
AS
	INSERT [EventData]
		(EventId, [Key], Data)
	VALUES
		(@EventId, @Key, @Data)

GO

---------------------------------------------------------------

IF OBJECT_ID('[dev].[spCleanup]') IS NOT NULL
DROP PROC [dev].[spCleanup]

GO

CREATE PROC [dev].[spCleanup]
AS
	TRUNCATE TABLE EventData
	TRUNCATE TABLE EventException
	TRUNCATE TABLE EventBookmark
	TRUNCATE TABLE Event
GO
