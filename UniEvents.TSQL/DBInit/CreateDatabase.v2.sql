USE master;
GO
CREATE DATABASE [$(dbUniHangouts)]
GO

USE [$(dbUniHangouts)]
GO


/****** Object:  Table [dbo].[AccountGroupMap]    Script Date: 3/12/2018 11:40:48 AM ******/
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
/****** Object:  Table [dbo].[Accounts]    Script Date: 3/12/2018 11:40:48 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Accounts](
	[AccountID] [bigint] IDENTITY(1,1) NOT NULL,
	[LocationID] [bigint] NULL,
	[PasswordHash] [binary](256) NULL,
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
/****** Object:  Table [dbo].[APIKeys]    Script Date: 3/12/2018 11:40:48 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[APIKeys](
	[APIKey] [binary](256) NOT NULL,
	[DateIssued] [datetime] NOT NULL,
	[DateLastUsed] [datetime] NOT NULL,
	[AccountID] [bigint] NULL,
	[IsBanned] [bit] NOT NULL,
 CONSTRAINT [PK_APIKeys] PRIMARY KEY CLUSTERED 
(
	[APIKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EventDetails]    Script Date: 3/12/2018 11:40:48 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventDetails](
	[EventID] [bigint] NOT NULL,
	[Details] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EventFeed]    Script Date: 3/12/2018 11:40:48 AM ******/
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
/****** Object:  Table [dbo].[EventRSVPs]    Script Date: 3/12/2018 11:40:48 AM ******/
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
/****** Object:  Table [dbo].[EventTagMap]    Script Date: 3/12/2018 11:40:48 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventTagMap](
	[EventID] [bigint] NOT NULL,
	[TagID] [int] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EventTypes]    Script Date: 3/12/2018 11:40:48 AM ******/
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
/****** Object:  Table [dbo].[Locations]    Script Date: 3/12/2018 11:40:48 AM ******/
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
/****** Object:  Table [dbo].[RSVPTypes]    Script Date: 3/12/2018 11:40:48 AM ******/
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
/****** Object:  Table [dbo].[Tags]    Script Date: 3/12/2018 11:40:48 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tags](
	[TagID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [char](30) NOT NULL,
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
ALTER TABLE [dbo].[APIKeys]  WITH CHECK ADD  CONSTRAINT [FK_APIKeys_Accounts] FOREIGN KEY([AccountID])
REFERENCES [dbo].[Accounts] ([AccountID])
GO
ALTER TABLE [dbo].[APIKeys] CHECK CONSTRAINT [FK_APIKeys_Accounts]
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
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Locations'
GO


INSERT INTO [dbo].[Locations] (ParentLocationID, [Name], AddressLine, Locality, AdminDistrict, PostalCode, CountryRegion, Latitude6x, Longitude6x, [Description])
	 VALUES ( NULL, 'Solar System', NULL, NULL, NULL, NULL, 'Milky Way', NULL, NULL, 'Our Solar System');

INSERT INTO [dbo].[Locations] (ParentLocationID, [Name], AddressLine, Locality, AdminDistrict, PostalCode, CountryRegion, Latitude6x, Longitude6x, [Description])
	 VALUES ((select top 1 locs.LocationID from [dbo].Locations as locs where locs.[Name] = 'Solar System'), 'Earth', NULL, NULL, NULL, NULL, 'Solar System', NULL, NULL, 'Planet');

INSERT INTO [dbo].[Locations] (ParentLocationID, [Name], AddressLine, Locality, AdminDistrict, PostalCode, CountryRegion, Latitude6x, Longitude6x, [Description])
	 VALUES ((select top 1 locs.LocationID from [dbo].Locations as locs where locs.[Name] = 'Earth'), 'North America', NULL, NULL, NULL, NULL, 'Earth', 54526000, -105255100, 'Continent');

INSERT INTO [dbo].[Locations] (ParentLocationID, [Name], AddressLine, Locality, AdminDistrict, PostalCode, CountryRegion, Latitude6x, Longitude6x, [Description])
	 VALUES ((select top 1 locs.LocationID from [dbo].Locations as locs where locs.[Name] = 'North America'), 'United States of America', NULL, NULL, NULL, NULL, 'USA', 37090200, -95712900, 'Murica');

INSERT INTO [dbo].[Locations] (ParentLocationID, [Name], AddressLine, Locality, AdminDistrict, PostalCode, CountryRegion, Latitude6x, Longitude6x, [Description])
	 VALUES ((select top 1 locs.LocationID from [dbo].Locations as locs where locs.CountryRegion = 'USA'), 'Nebraska', NULL, NULL, 'Nebraska', NULL, 'USA', 41492500, -99901800, 'Home of the Huskers');

INSERT INTO [dbo].[Locations] (ParentLocationID, [Name], AddressLine, Locality, AdminDistrict, PostalCode, CountryRegion, Latitude6x, Longitude6x, [Description])
	 VALUES ((select top 1 locs.LocationID from [dbo].Locations as locs where locs.AdminDistrict = 'Nebraska'), NULL, NULL, 'Lincoln', 'Nebraska', NULL, 'USA', 40825800, -96685200, 'City of Lincoln Nebraska');

INSERT INTO [dbo].[Locations] (ParentLocationID, [Name], AddressLine, Locality, AdminDistrict, PostalCode, CountryRegion, Latitude6x, Longitude6x, [Description])
	 VALUES ((select top 1 locs.LocationID from [dbo].Locations as locs where locs.Locality = 'Lincoln' AND locs.AdminDistrict = 'Nebraska'),
		'UNL', '1400R Street', 'Lincoln', 'Nebraska', 68588, 'USA', 40824310, -96698840, 'University of Nebraska at Lincoln');

INSERT INTO [dbo].[Locations] (ParentLocationID, [Name], AddressLine, Locality, AdminDistrict, PostalCode, CountryRegion, Latitude6x, Longitude6x, [Description])
	 VALUES ((select top 1 locs.LocationID from [dbo].Locations as locs where locs.Name = 'UNL' AND locs.Locality = 'Lincoln' AND locs.AdminDistrict = 'Nebraska'),
		'Love Library', '14th & R Street', 'Lincoln', 'Nebraska', 68588, 'USA', 40818010, -96704760, NULL);

INSERT INTO [dbo].[Locations] (ParentLocationID, [Name], AddressLine, Locality, AdminDistrict, PostalCode, CountryRegion, Latitude6x, Longitude6x, [Description])
	 VALUES ((select top 1 locs.LocationID from [dbo].Locations as locs where locs.Name = 'UNL' AND locs.Locality = 'Lincoln' AND locs.AdminDistrict = 'Nebraska'),
		'Avery Hall', '1144 T Street', 'Lincoln', 'Nebraska', 68588, 'USA', 40819460, -96706640, NULL);

INSERT INTO [dbo].[Locations] (ParentLocationID, [Name], AddressLine, Locality, AdminDistrict, PostalCode, CountryRegion, Latitude6x, Longitude6x, [Description])
	 VALUES ((select top 1 locs.LocationID from [dbo].Locations as locs where locs.Name = 'UNL' AND locs.Locality = 'Lincoln' AND locs.AdminDistrict = 'Nebraska'),
		'College of Business', '730 North 14th Street', 'Lincoln', 'Nebraska', 68588, 'USA', 40820180, -96703390, 'University of Nebraska-Lincoln College of Business');

INSERT INTO [dbo].[Locations] (ParentLocationID, [Name], AddressLine, Locality, AdminDistrict, PostalCode, CountryRegion, Latitude6x, Longitude6x, [Description])
	 VALUES ((select top 1 locs.LocationID from [dbo].Locations as locs where locs.Name = 'UNL' AND locs.Locality = 'Lincoln' AND locs.AdminDistrict = 'Nebraska'),
		'Memorial Stadium', 'One Memorial Stadium Drive', 'Lincoln', 'Nebraska', 68588, 'USA', -96707830, -96707830, 'Home field of Nebraska Cornhuskers holding over 85,000 football fans with a history dating to 1922.');

INSERT INTO [dbo].[Locations] (ParentLocationID, [Name], AddressLine, Locality, AdminDistrict, PostalCode, CountryRegion, Latitude6x, Longitude6x, [Description])
	 VALUES ((select top 1 locs.LocationID from [dbo].Locations as locs where locs.AdminDistrict ='Nebraska'), NULL, NULL, 'Columbus', 'Nebraska', NULL, 'USA', 41429734, -97368375, null);

GO


INSERT INTO [dbo].[RSVPTypes] ([Name], [Description]) VALUES ('No', 'Can''t attend');
INSERT INTO [dbo].[RSVPTypes] ([Name], [Description]) VALUES ('Maybe', 'Not sure yet');
INSERT INTO [dbo].[RSVPTypes] ([Name], [Description]) VALUES ('Stop By', 'I''ll Stop By');
INSERT INTO [dbo].[RSVPTypes] ([Name], [Description]) VALUES ('Later', 'I''ll be late');
INSERT INTO [dbo].[RSVPTypes] ([Name], [Description]) VALUES ('Attending', 'I''ll be there!');
GO


INSERT INTO [dbo].[EventTypes] ([Name], [Description]) VALUES ('Birthday', NULL);
INSERT INTO [dbo].[EventTypes] ([Name], [Description]) VALUES ('Party', NULL);
INSERT INTO [dbo].[EventTypes] ([Name], [Description]) VALUES ('Sports', NULL);
INSERT INTO [dbo].[EventTypes] ([Name], [Description]) VALUES ('ImpendingDoom', 'An event you''re dreading.');
GO

INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Football', 'American Football');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Soccer', 'Foreign Football');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Study', 'Informal Study Session');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Basketball-Game', 'Basketball Game');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('School-Sponsored', 'Sponsored by UNL');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('FREE FOOD', 'Free food');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('TV-Binge', 'Watch a TV Show');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Club', 'Sponsored by a Club');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Newbie', 'Beginner who needs guidance');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Rally', 'Gathering of People in Protest');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Concert', 'Music concert');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Free', 'Doesn’t cost anything');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Paid', 'Costs money');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Greek Life', 'Sponsored by Fraternity/Sorority');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Off-Campus', 'Takes place off campus');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('On-Campus', 'Takes place on campus');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('East-Campus', 'Takes place on East Campus');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Lit', 'This event will be lit');
INSERT INTO [dbo].[Tags] ([Name], [Description]) VALUES ('Help', 'Pls help');
GO



/****** Object:  User [ClientApp]    Script Date: 3/12/2018 11:40:48 AM ******/
CREATE USER [ClientApp] FOR LOGIN [ClientApp] WITH DEFAULT_SCHEMA=[dbo]
GO
GRANT EXECUTE TO [ClientApp];
GO
/****** Object:  User [PublicReadOnly]    Script Date: 3/12/2018 11:40:48 AM ******/
CREATE USER [PublicReadOnly] FOR LOGIN [PublicReadOnly] WITH DEFAULT_SCHEMA=[dbo]
GO
/****** Object:  User [UniEventAdmin]    Script Date: 3/12/2018 11:40:48 AM ******/
CREATE USER [UniEventAdmin] FOR LOGIN [UniEventAdmin] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [UniEventAdmin]
GO