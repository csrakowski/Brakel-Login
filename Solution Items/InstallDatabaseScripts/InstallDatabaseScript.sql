USE [master]
GO
/****** Object:  Database [Brakel-Login]    Script Date: 12/14/2012 10:57:27 ******/
CREATE DATABASE [Brakel-Login] ON  PRIMARY 
( NAME = N'Brakel-Login', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER\MSSQL\DATA\Brakel-Login.mdf' , SIZE = 3072KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'Brakel-Login_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL10_50.MSSQLSERVER\MSSQL\DATA\Brakel-Login_1.ldf' , SIZE = 1024KB , MAXSIZE = 2048MB , FILEGROWTH = 10%)
GO
ALTER DATABASE [Brakel-Login] SET COMPATIBILITY_LEVEL = 100
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Brakel-Login].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Brakel-Login] SET ANSI_NULL_DEFAULT OFF
GO
ALTER DATABASE [Brakel-Login] SET ANSI_NULLS OFF
GO
ALTER DATABASE [Brakel-Login] SET ANSI_PADDING OFF
GO
ALTER DATABASE [Brakel-Login] SET ANSI_WARNINGS OFF
GO
ALTER DATABASE [Brakel-Login] SET ARITHABORT OFF
GO
ALTER DATABASE [Brakel-Login] SET AUTO_CLOSE OFF
GO
ALTER DATABASE [Brakel-Login] SET AUTO_CREATE_STATISTICS ON
GO
ALTER DATABASE [Brakel-Login] SET AUTO_SHRINK ON
GO
ALTER DATABASE [Brakel-Login] SET AUTO_UPDATE_STATISTICS ON
GO
ALTER DATABASE [Brakel-Login] SET CURSOR_CLOSE_ON_COMMIT OFF
GO
ALTER DATABASE [Brakel-Login] SET CURSOR_DEFAULT  GLOBAL
GO
ALTER DATABASE [Brakel-Login] SET CONCAT_NULL_YIELDS_NULL OFF
GO
ALTER DATABASE [Brakel-Login] SET NUMERIC_ROUNDABORT OFF
GO
ALTER DATABASE [Brakel-Login] SET QUOTED_IDENTIFIER OFF
GO
ALTER DATABASE [Brakel-Login] SET RECURSIVE_TRIGGERS OFF
GO
ALTER DATABASE [Brakel-Login] SET  DISABLE_BROKER
GO
ALTER DATABASE [Brakel-Login] SET AUTO_UPDATE_STATISTICS_ASYNC OFF
GO
ALTER DATABASE [Brakel-Login] SET DATE_CORRELATION_OPTIMIZATION ON
GO
ALTER DATABASE [Brakel-Login] SET TRUSTWORTHY OFF
GO
ALTER DATABASE [Brakel-Login] SET ALLOW_SNAPSHOT_ISOLATION OFF
GO
ALTER DATABASE [Brakel-Login] SET PARAMETERIZATION SIMPLE
GO
ALTER DATABASE [Brakel-Login] SET READ_COMMITTED_SNAPSHOT OFF
GO
ALTER DATABASE [Brakel-Login] SET HONOR_BROKER_PRIORITY OFF
GO
ALTER DATABASE [Brakel-Login] SET  READ_WRITE
GO
ALTER DATABASE [Brakel-Login] SET RECOVERY FULL
GO
ALTER DATABASE [Brakel-Login] SET  MULTI_USER
GO
ALTER DATABASE [Brakel-Login] SET PAGE_VERIFY CHECKSUM
GO
ALTER DATABASE [Brakel-Login] SET DB_CHAINING OFF
GO
EXEC sys.sp_db_vardecimal_storage_format N'Brakel-Login', N'ON'
GO
USE [Brakel-Login]
GO
/****** Object:  User [BrakelApplication]    Script Date: 12/14/2012 10:57:27 ******/
CREATE USER [BrakelApplication] FOR LOGIN [BrakelApplication] WITH DEFAULT_SCHEMA=[dbo]
GO
/****** Object:  Table [dbo].[group]    Script Date: 12/14/2012 10:57:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[group](
	[GroupId] [int] NOT NULL,
	[GroupName] [text] NOT NULL,
	[BuildingId] [int] NOT NULL,
	[ChangeValue] [int] NOT NULL,
 CONSTRAINT [PK_group] PRIMARY KEY CLUSTERED 
(
	[GroupId] ASC,
	[BuildingId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[room]    Script Date: 12/14/2012 10:57:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[room](
	[roomId] [int] IDENTITY(1,1) NOT NULL,
	[roomName] [varchar](255) NOT NULL,
	[buildingId] [int] NOT NULL,
	[xCoordinate] [int] NOT NULL,
	[yCoordinate] [int] NOT NULL,
	[width] [int] NOT NULL,
	[height] [int] NOT NULL,
	[enabled] [bit] NOT NULL,
	[hasAlarm] [bit] NOT NULL,
 CONSTRAINT [PK_room] PRIMARY KEY CLUSTERED 
(
	[roomId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[building]    Script Date: 12/14/2012 10:57:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[building](
	[buildingId] [int] IDENTITY(1,1) NOT NULL,
	[name] [varchar](255) NOT NULL,
	[parentId] [int] NULL,
	[endpoint] text NULL,
 CONSTRAINT [PK_building] PRIMARY KEY CLUSTERED 
(
	[buildingId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[userBuildingCouple]    Script Date: 12/14/2012 10:57:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[userBuildingCouple](
	[userId] [int] NOT NULL,
	[buildingId] [int] NOT NULL,
	[accessRights] [varchar](16) NOT NULL,
	[screenLayout] [text] NOT NULL,
 CONSTRAINT [PK_userBuildingRights] PRIMARY KEY CLUSTERED 
(
	[userId] ASC,
	[buildingId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[user]    Script Date: 12/14/2012 10:57:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[user](
	[userId] [int] IDENTITY(1,1) NOT NULL,
	[username] [varchar](255) NOT NULL,
	[hash] [varchar](255) NOT NULL,
	[friendlyName] [text] NOT NULL,
 CONSTRAINT [PK_user] PRIMARY KEY CLUSTERED 
(
	[userId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[username] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[token]    Script Date: 12/14/2012 10:57:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[token](
	[username] [varchar](255) NOT NULL,
	[token] [uniqueidentifier] NOT NULL,
	[createDateTime] [datetime] NOT NULL,
	[deviceId] [varchar](255) NULL,
 CONSTRAINT [PK_token] PRIMARY KEY CLUSTERED 
(
	[token] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  StoredProcedure [dbo].[invalidateOlderTokens]    Script Date: 12/14/2012 10:57:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Christiaan Rakowski>
-- Create date: <2012-10-12>
-- Description:	<Stored procedure to drop old login tokens>
-- =============================================
CREATE PROCEDURE [dbo].[invalidateOlderTokens]
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    DELETE FROM [token] WHERE DATEDIFF(minute, [createDateTime], cast(getDate() as datetime)) >= 20
END
GO
/****** Object:  Default [DF__group__ChangeVal__4D94879B]    Script Date: 12/14/2012 10:57:28 ******/
ALTER TABLE [dbo].[group] ADD  DEFAULT ((0)) FOR [ChangeValue]
GO
/****** Object:  Default [DF__room__xCoordinat__44FF419A]    Script Date: 12/14/2012 10:57:28 ******/
ALTER TABLE [dbo].[room] ADD  DEFAULT ((0)) FOR [xCoordinate]
GO
/****** Object:  Default [DF__room__yCoordinat__45F365D3]    Script Date: 12/14/2012 10:57:28 ******/
ALTER TABLE [dbo].[room] ADD  DEFAULT ((0)) FOR [yCoordinate]
GO
/****** Object:  Default [DF__room__width__46E78A0C]    Script Date: 12/14/2012 10:57:28 ******/
ALTER TABLE [dbo].[room] ADD  DEFAULT ((50)) FOR [width]
GO
/****** Object:  Default [DF__room__height__47DBAE45]    Script Date: 12/14/2012 10:57:28 ******/
ALTER TABLE [dbo].[room] ADD  DEFAULT ((50)) FOR [height]
GO
/****** Object:  Default [DF__room__enabled__48CFD27E]    Script Date: 12/14/2012 10:57:28 ******/
ALTER TABLE [dbo].[room] ADD  DEFAULT ((1)) FOR [enabled]
GO
/****** Object:  Default [DF__room__hasAlarm__49C3F6B7]    Script Date: 12/14/2012 10:57:28 ******/
ALTER TABLE [dbo].[room] ADD  DEFAULT ((0)) FOR [hasAlarm]
GO
/****** Object:  Default [DF__userBuild__scree__534D60F1]    Script Date: 12/14/2012 10:57:28 ******/
ALTER TABLE [dbo].[userBuildingCouple] ADD  DEFAULT ('{"pages":[]}') FOR [screenLayout]
GO
/****** Object:  Default [DF_user_friendlyName]    Script Date: 12/14/2012 10:57:28 ******/
ALTER TABLE [dbo].[user] ADD  CONSTRAINT [DF_user_friendlyName]  DEFAULT ('') FOR [friendlyName]
GO
/****** Object:  Check [CK__userBuild__acces__52593CB8]    Script Date: 12/14/2012 10:57:28 ******/
ALTER TABLE [dbo].[userBuildingCouple]  WITH CHECK ADD CHECK  (([accessRights]='None' OR [accessRights]='ReadOnly' OR [accessRights]='Administrator'))
GO
/****** Object:  ForeignKey [FK__group__BuildingI__4CA06362]    Script Date: 12/14/2012 10:57:28 ******/
ALTER TABLE [dbo].[group]  WITH CHECK ADD FOREIGN KEY([BuildingId])
REFERENCES [dbo].[building] ([buildingId])
GO
/****** Object:  ForeignKey [FK__room__buildingId__440B1D61]    Script Date: 12/14/2012 10:57:28 ******/
ALTER TABLE [dbo].[room]  WITH CHECK ADD FOREIGN KEY([buildingId])
REFERENCES [dbo].[building] ([buildingId])
GO
/****** Object:  ForeignKey [FK__building__parent__38996AB5]    Script Date: 12/14/2012 10:57:28 ******/
ALTER TABLE [dbo].[building]  WITH CHECK ADD FOREIGN KEY([parentId])
REFERENCES [dbo].[building] ([buildingId])
GO
/****** Object:  ForeignKey [FK__userBuild__build__5165187F]    Script Date: 12/14/2012 10:57:28 ******/
ALTER TABLE [dbo].[userBuildingCouple]  WITH CHECK ADD FOREIGN KEY([buildingId])
REFERENCES [dbo].[building] ([buildingId])
GO
/****** Object:  ForeignKey [FK__userBuild__userI__5070F446]    Script Date: 12/14/2012 10:57:28 ******/
ALTER TABLE [dbo].[userBuildingCouple]  WITH CHECK ADD FOREIGN KEY([userId])
REFERENCES [dbo].[user] ([userId])
GO

USE [msdb]
GO

/****** Object:  Job [InvalidateOlderTokens_sp]    Script Date: 11/06/2012 11:39:43 ******/
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0
/****** Object:  JobCategory [Data Collector]    Script Date: 11/06/2012 11:39:43 ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'Data Collector' AND category_class=1)
BEGIN
EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'Data Collector'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

DECLARE @jobId BINARY(16)
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'InvalidateOlderTokens_sp', 
		@enabled=1, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'No description available.', 
		@category_name=N'Data Collector', 
		@owner_login_name=N'ATM-VSERVER2\Administrator', @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Call StoredProcedure]    Script Date: 11/06/2012 11:39:44 ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Call StoredProcedure', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'exec [invalidateOlderTokens];', 
		@database_name=N'Brakel-Login', 
		@database_user_name=N'dbo', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=N'InvalidateOlderTokens_sp', 
		@enabled=1, 
		@freq_type=4, 
		@freq_interval=1, 
		@freq_subday_type=4, 
		@freq_subday_interval=5, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=0, 
		@active_start_date=20121012, 
		@active_end_date=99991231, 
		@active_start_time=0, 
		@active_end_time=235959, 
		@schedule_uid=N'919026e4-009e-4e13-aa1c-2a991b30869e'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:

GO

