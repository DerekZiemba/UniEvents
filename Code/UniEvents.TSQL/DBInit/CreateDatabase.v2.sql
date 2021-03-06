USE master;
GO
CREATE DATABASE [$(dbUniHangouts)]
GO

USE [$(dbUniHangouts)]
GO

/****** Object:  Table [dbo].[AccountGroupMap]    Script Date: 3/14/2018 3:54:01 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AccountGroupMap](
	[AccountID] [bigint] IDENTITY(1,1) NOT NULL,
	[GroupID] [bigint] NOT NULL,
	[IsGroupOwner] [bit] NOT NULL,
	[IsGroupAdmin] [bit] NOT NULL,
	[IsGroupFriend] [bit] NOT NULL,
	[IsPendingFriend] [bit] NOT NULL,
	[IsGroupFollower] [bit] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Accounts]    Script Date: 3/14/2018 3:54:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Accounts](
	[AccountID] [bigint] IDENTITY(1,1) NOT NULL,
	[LocationID] [bigint] NULL,
	[PasswordHash] [binary](256) NULL,
	[Salt] [varchar](20) NULL,
	[UserName] [varchar](20) NOT NULL,
	[DisplayName] [nvarchar](50) NULL,
	[FirstName] [nvarchar](50) NULL,
	[LastName] [nvarchar](50) NULL,
	[SchoolEmail] [varchar](50) NULL,
	[ContactEmail] [varchar](50) NULL,
	[PhoneNumber] [varchar](20) NULL,
	[IsGroup] [bit] NOT NULL,
	[VerifiedSchoolEmail] [bit] NULL,
	[VerifiedContactEmail] [bit] NULL,
 CONSTRAINT [PK_Accounts] PRIMARY KEY CLUSTERED 
(
	[AccountID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EventDetails]    Script Date: 3/14/2018 3:54:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventDetails](
	[EventID] [bigint] NOT NULL,
	[Details] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EventFeed]    Script Date: 3/14/2018 3:54:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventFeed](
	[EventID] [bigint] IDENTITY(1,1) NOT NULL,
	[EventTypeID] [int] NOT NULL,
	[DateStart] [smalldatetime] NOT NULL,
	[DateEnd] [smalldatetime] NOT NULL,
	[AccountID] [bigint] NOT NULL,
	[LocationID] [bigint] NOT NULL,
	[Title] [varchar](80) NOT NULL,
	[Caption] [nvarchar](160) NULL,
 CONSTRAINT [PK_EventFeed] PRIMARY KEY CLUSTERED 
(
	[EventID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EventRSVPs]    Script Date: 3/14/2018 3:54:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventRSVPs](
	[EventID] [bigint] NOT NULL,
	[AccountID] [bigint] NOT NULL,
	[RSVPTypeId] [smallint] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EventTagMap]    Script Date: 3/14/2018 3:54:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventTagMap](
	[EventID] [bigint] NOT NULL,
	[TagID] [int] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EventTypes]    Script Date: 3/14/2018 3:54:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventTypes](
	[EventTypeID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[Description] [nvarchar](400) NULL,
 CONSTRAINT [PK_EventTypes] PRIMARY KEY CLUSTERED 
(
	[EventTypeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Locations]    Script Date: 3/14/2018 3:54:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Locations](
	[LocationID] [bigint] IDENTITY(1,1) NOT NULL,
	[ParentLocationID] [bigint] NULL,
	[Name] [varchar](80) NULL,
	[AddressLine] [varchar](80) NULL,
	[Locality] [varchar](40) NULL,
	[AdminDistrict] [varchar](40) NULL,
	[PostalCode] [varchar](20) NULL,
	[CountryRegion] [varchar](40) NOT NULL,
	[Latitude6x] [int] NULL,
	[Longitude6x] [int] NULL,
	[Description] [varchar](160) NULL,
 CONSTRAINT [PK_Locations] PRIMARY KEY CLUSTERED 
(
	[LocationID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Logins]    Script Date: 3/14/2018 3:54:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Logins](
	[UserName] [varchar](20) NOT NULL,
	[APIKey] [varchar](50) NOT NULL,
	[APIKeyHash] [binary](256) NOT NULL,
	[LoginDate] [datetime] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RSVPTypes]    Script Date: 3/14/2018 3:54:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RSVPTypes](
	[RSVPTypeID] [smallint] IDENTITY(1,1) NOT NULL,
	[Name] [char](10) NOT NULL,
	[Description] [nchar](40) NOT NULL,
 CONSTRAINT [PK_RSVPTypes] PRIMARY KEY CLUSTERED 
(
	[RSVPTypeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tags]    Script Date: 3/14/2018 3:54:02 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tags](
	[TagID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[Description] [nvarchar](160) NOT NULL,
 CONSTRAINT [PK_Tags] PRIMARY KEY CLUSTERED 
(
	[TagID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AccountGroupMap]  WITH CHECK ADD  CONSTRAINT [FK_AccountGroupMap_Accounts] FOREIGN KEY([AccountID])
REFERENCES [dbo].[Accounts] ([AccountID])
GO
ALTER TABLE [dbo].[AccountGroupMap] CHECK CONSTRAINT [FK_AccountGroupMap_Accounts]
GO
ALTER TABLE [dbo].[AccountGroupMap]  WITH CHECK ADD  CONSTRAINT [FK_AccountGroupMap_Groups] FOREIGN KEY([GroupID])
REFERENCES [dbo].[Accounts] ([AccountID])
GO
ALTER TABLE [dbo].[AccountGroupMap] CHECK CONSTRAINT [FK_AccountGroupMap_Groups]
GO
ALTER TABLE [dbo].[Accounts]  WITH CHECK ADD  CONSTRAINT [FK_Accounts_Locations] FOREIGN KEY([LocationID])
REFERENCES [dbo].[Locations] ([LocationID])
GO
ALTER TABLE [dbo].[Accounts] CHECK CONSTRAINT [FK_Accounts_Locations]
GO
ALTER TABLE [dbo].[EventDetails]  WITH CHECK ADD  CONSTRAINT [FK_EventDetails_EventFeed] FOREIGN KEY([EventID])
REFERENCES [dbo].[EventFeed] ([EventID])
GO
ALTER TABLE [dbo].[EventDetails] CHECK CONSTRAINT [FK_EventDetails_EventFeed]
GO
ALTER TABLE [dbo].[EventFeed]  WITH CHECK ADD  CONSTRAINT [FK_EventFeed_Accounts] FOREIGN KEY([AccountID])
REFERENCES [dbo].[Accounts] ([AccountID])
GO
ALTER TABLE [dbo].[EventFeed] CHECK CONSTRAINT [FK_EventFeed_Accounts]
GO
ALTER TABLE [dbo].[EventFeed]  WITH CHECK ADD  CONSTRAINT [FK_EventFeed_EventTypes] FOREIGN KEY([EventTypeID])
REFERENCES [dbo].[EventTypes] ([EventTypeID])
GO
ALTER TABLE [dbo].[EventFeed] CHECK CONSTRAINT [FK_EventFeed_EventTypes]
GO
ALTER TABLE [dbo].[EventFeed]  WITH CHECK ADD  CONSTRAINT [FK_EventFeed_Locations] FOREIGN KEY([LocationID])
REFERENCES [dbo].[Locations] ([LocationID])
GO
ALTER TABLE [dbo].[EventFeed] CHECK CONSTRAINT [FK_EventFeed_Locations]
GO
ALTER TABLE [dbo].[EventRSVPs]  WITH CHECK ADD  CONSTRAINT [FK_EventRSVPs_EventFeed] FOREIGN KEY([EventID])
REFERENCES [dbo].[EventFeed] ([EventID])
GO
ALTER TABLE [dbo].[EventRSVPs] CHECK CONSTRAINT [FK_EventRSVPs_EventFeed]
GO
ALTER TABLE [dbo].[EventRSVPs]  WITH CHECK ADD  CONSTRAINT [FK_EventRSVPs_RSVPTypes] FOREIGN KEY([RSVPTypeId])
REFERENCES [dbo].[RSVPTypes] ([RSVPTypeID])
GO
ALTER TABLE [dbo].[EventRSVPs] CHECK CONSTRAINT [FK_EventRSVPs_RSVPTypes]
GO
ALTER TABLE [dbo].[EventTagMap]  WITH CHECK ADD  CONSTRAINT [FK_MapEventAndTags_EventFeed] FOREIGN KEY([EventID])
REFERENCES [dbo].[EventFeed] ([EventID])
GO
ALTER TABLE [dbo].[EventTagMap] CHECK CONSTRAINT [FK_MapEventAndTags_EventFeed]
GO
ALTER TABLE [dbo].[EventTagMap]  WITH CHECK ADD  CONSTRAINT [FK_MapEventAndTags_Tags] FOREIGN KEY([TagID])
REFERENCES [dbo].[Tags] ([TagID])
GO
ALTER TABLE [dbo].[EventTagMap] CHECK CONSTRAINT [FK_MapEventAndTags_Tags]
GO
ALTER TABLE [dbo].[Logins]  WITH CHECK ADD  CONSTRAINT [FK_Logins_Accounts1] FOREIGN KEY([UserName])
REFERENCES [dbo].[Accounts] ([UserName])
GO
ALTER TABLE [dbo].[Logins] CHECK CONSTRAINT [FK_Logins_Accounts1]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Locations'
GO





/****** Object:  User [UniEventAdmin]    Script Date: 3/14/2018 3:54:01 PM ******/
CREATE USER [UniEventAdmin] FOR LOGIN [UniEventAdmin] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [UniEventAdmin]
GO

/****** Object:  User [ClientApp]    Script Date: 3/14/2018 3:54:01 PM ******/
CREATE USER [ClientApp] FOR LOGIN [ClientApp] WITH DEFAULT_SCHEMA=[dbo]
GO
GRANT EXECUTE TO [ClientApp];
GO

/****** Object:  User [UniEventReadWrite]    Script Date: 3/14/2018 3:54:01 PM ******/
CREATE USER [UniEventReadWrite] FOR LOGIN [UniEventReadWrite] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [UniEventAdmin]
GO
ALTER ROLE [db_datareader] ADD MEMBER [UniEventReadWrite]
GO
ALTER ROLE [db_datawriter] ADD MEMBER [UniEventReadWrite]
GO
GRANT EXECUTE TO [UniEventReadWrite];
GO