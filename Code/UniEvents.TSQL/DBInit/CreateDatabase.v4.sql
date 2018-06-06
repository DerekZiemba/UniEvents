USE [master]
GO
/****** Object:  Database [dbUniHangouts]    Script Date: 4/20/2018 10:25:35 PM ******/
CREATE DATABASE [dbUniHangouts]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'dbUniHangouts', FILENAME = N'N:\Microsoft SQL Server\MSSQL14.SQLUNIHANGOUTS\MSSQL\DATA\dbUniHangouts.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'dbUniHangouts_log', FILENAME = N'N:\Microsoft SQL Server\MSSQL14.SQLUNIHANGOUTS\MSSQL\DATA\dbUniHangouts_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
GO
ALTER DATABASE [dbUniHangouts] SET COMPATIBILITY_LEVEL = 140
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [dbUniHangouts].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [dbUniHangouts] SET ANSI_NULL_DEFAULT ON 
GO
ALTER DATABASE [dbUniHangouts] SET ANSI_NULLS ON 
GO
ALTER DATABASE [dbUniHangouts] SET ANSI_PADDING ON 
GO
ALTER DATABASE [dbUniHangouts] SET ANSI_WARNINGS ON 
GO
ALTER DATABASE [dbUniHangouts] SET ARITHABORT ON 
GO
ALTER DATABASE [dbUniHangouts] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [dbUniHangouts] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [dbUniHangouts] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [dbUniHangouts] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [dbUniHangouts] SET CURSOR_DEFAULT  LOCAL 
GO
ALTER DATABASE [dbUniHangouts] SET CONCAT_NULL_YIELDS_NULL ON 
GO
ALTER DATABASE [dbUniHangouts] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [dbUniHangouts] SET QUOTED_IDENTIFIER ON 
GO
ALTER DATABASE [dbUniHangouts] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [dbUniHangouts] SET  DISABLE_BROKER 
GO
ALTER DATABASE [dbUniHangouts] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [dbUniHangouts] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [dbUniHangouts] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [dbUniHangouts] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [dbUniHangouts] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [dbUniHangouts] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [dbUniHangouts] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [dbUniHangouts] SET RECOVERY FULL 
GO
ALTER DATABASE [dbUniHangouts] SET  MULTI_USER 
GO
ALTER DATABASE [dbUniHangouts] SET PAGE_VERIFY NONE  
GO
ALTER DATABASE [dbUniHangouts] SET DB_CHAINING OFF 
GO
ALTER DATABASE [dbUniHangouts] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [dbUniHangouts] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
ALTER DATABASE [dbUniHangouts] SET DELAYED_DURABILITY = DISABLED 
GO
EXEC sys.sp_db_vardecimal_storage_format N'dbUniHangouts', N'ON'
GO
ALTER DATABASE [dbUniHangouts] SET QUERY_STORE = OFF
GO
USE [dbUniHangouts]
GO
ALTER DATABASE SCOPED CONFIGURATION SET IDENTITY_CACHE = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION SET LEGACY_CARDINALITY_ESTIMATION = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET LEGACY_CARDINALITY_ESTIMATION = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = 0;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET MAXDOP = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET PARAMETER_SNIFFING = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET PARAMETER_SNIFFING = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET QUERY_OPTIMIZER_HOTFIXES = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET QUERY_OPTIMIZER_HOTFIXES = PRIMARY;
GO
USE [dbUniHangouts]
GO
/****** Object:  User [UniEventReadWrite]    Script Date: 4/20/2018 10:25:35 PM ******/
CREATE USER [UniEventReadWrite] FOR LOGIN [UniEventReadWrite] WITH DEFAULT_SCHEMA=[dbo]
GO
/****** Object:  User [UniEventAdmin]    Script Date: 4/20/2018 10:25:35 PM ******/
CREATE USER [UniEventAdmin] FOR LOGIN [UniEventAdmin] WITH DEFAULT_SCHEMA=[dbo]
GO
/****** Object:  User [ClientApp]    Script Date: 4/20/2018 10:25:35 PM ******/
CREATE USER [ClientApp] FOR LOGIN [ClientApp] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_datareader] ADD MEMBER [UniEventReadWrite]
GO
ALTER ROLE [db_datawriter] ADD MEMBER [UniEventReadWrite]
GO
ALTER ROLE [db_owner] ADD MEMBER [UniEventAdmin]
GO
/****** Object:  Table [dbo].[AccountGroupMap]    Script Date: 4/20/2018 10:25:35 PM ******/
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
/****** Object:  Table [dbo].[Accounts]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Accounts](
	[AccountID] [bigint] IDENTITY(1,1) NOT NULL,
	[LocationID] [bigint] NULL,
	[PasswordHash] [binary](32) NULL,
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
	[IsAdmin] [bit] NULL,
 CONSTRAINT [PK_Accounts] PRIMARY KEY CLUSTERED 
(
	[AccountID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmailVerification]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmailVerification](
	[AccountID] [bigint] NOT NULL,
	[VerificationKey] [varchar](50) NOT NULL,
	[VerificationHash] [binary](32) NOT NULL,
	[Email] [varchar](50) NOT NULL,
	[Date] [smalldatetime] NOT NULL,
	[IsVerified] [bit] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EventDetails]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventDetails](
	[EventID] [bigint] NOT NULL,
	[Details] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EventFeed]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventFeed](
	[EventID] [bigint] IDENTITY(1,1) NOT NULL,
	[EventTypeID] [bigint] NOT NULL,
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
/****** Object:  Table [dbo].[EventRSVPs]    Script Date: 4/20/2018 10:25:36 PM ******/
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
/****** Object:  Table [dbo].[EventTagMap]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventTagMap](
	[EventID] [bigint] NOT NULL,
	[TagID] [bigint] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EventTypes]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventTypes](
	[EventTypeID] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[Description] [nvarchar](400) NULL,
 CONSTRAINT [PK_EventTypes] PRIMARY KEY CLUSTERED 
(
	[EventTypeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Locations]    Script Date: 4/20/2018 10:25:36 PM ******/
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
/****** Object:  Table [dbo].[Logins]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Logins](
	[UserName] [varchar](20) NOT NULL,
	[APIKey] [varchar](50) NOT NULL,
	[APIKeyHash] [binary](32) NOT NULL,
	[LoginDate] [datetime] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RSVPTypes]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RSVPTypes](
	[RSVPTypeID] [smallint] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[Description] [varchar](40) NOT NULL,
 CONSTRAINT [PK_RSVPTypes] PRIMARY KEY CLUSTERED 
(
	[RSVPTypeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tags]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tags](
	[TagID] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[Description] [nvarchar](160) NOT NULL,
 CONSTRAINT [PK_Tags] PRIMARY KEY CLUSTERED 
(
	[TagID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Accounts]    Script Date: 4/20/2018 10:25:36 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Accounts] ON [dbo].[Accounts]
(
	[UserName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_EmailVerification]    Script Date: 4/20/2018 10:25:36 PM ******/
CREATE NONCLUSTERED INDEX [IX_EmailVerification] ON [dbo].[EmailVerification]
(
	[AccountID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_EventTypes]    Script Date: 4/20/2018 10:25:36 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_EventTypes] ON [dbo].[EventTypes]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Logins]    Script Date: 4/20/2018 10:25:36 PM ******/
CREATE NONCLUSTERED INDEX [IX_Logins] ON [dbo].[Logins]
(
	[UserName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Tags]    Script Date: 4/20/2018 10:25:36 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Tags] ON [dbo].[Tags]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
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
ALTER TABLE [dbo].[EmailVerification]  WITH CHECK ADD  CONSTRAINT [FK_EmailVerification_Accounts] FOREIGN KEY([AccountID])
REFERENCES [dbo].[Accounts] ([AccountID])
GO
ALTER TABLE [dbo].[EmailVerification] CHECK CONSTRAINT [FK_EmailVerification_Accounts]
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
ALTER TABLE [dbo].[Logins]  WITH CHECK ADD  CONSTRAINT [FK_Logins_Accounts] FOREIGN KEY([UserName])
REFERENCES [dbo].[Accounts] ([UserName])
GO
ALTER TABLE [dbo].[Logins] CHECK CONSTRAINT [FK_Logins_Accounts]
GO
/****** Object:  StoredProcedure [dbo].[sp_Account_Create]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Account_Create]
	@AccountID BIGINT OUTPUT,
	@LocationID BIGINT = NULL,
	@PasswordHash BINARY(256) = NULL,
	@Salt VARCHAR(20) = NULL,
	@UserName VARCHAR(20),
	@DisplayName NVARCHAR(50) = NULL,		
	@FirstName NVARCHAR(50) = NULL,
	@LastName NVARCHAR(50) = NULL,
	@SchoolEmail VARCHAR(50) = NULL,
	@ContactEmail VARCHAR(50) = NULL,
	@PhoneNumber VARCHAR(20) = NULL
AS
SET NOCOUNT ON;

BEGIN TRANSACTION;
BEGIN TRY 

	IF @PasswordHash IS NULL THROW 50000, 'Password_Invalid', 1;

	INSERT INTO dbo.Accounts (UserName, DisplayName, PasswordHash, Salt, LocationID, FirstName, LastName, SchoolEmail, ContactEmail, PhoneNumber, IsGroup) 
		VALUES (@UserName, @DisplayName, @PasswordHash, @Salt, @LocationID, @FirstName, @LastName, @SchoolEmail, @ContactEmail, @PhoneNumber, 0);

	SET @AccountID = SCOPE_IDENTITY();

	COMMIT TRANSACTION;
END TRY  
BEGIN CATCH  
	ROLLBACK TRANSACTION;  
	THROW;
END CATCH 


GO
/****** Object:  StoredProcedure [dbo].[sp_Account_GetOne]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Account_GetOne]
	@AccountID BIGINT,
	@UserName VARCHAR(20) = NULL
AS
SET NOCOUNT ON;

SELECT TOP 1 * FROM dbo.Accounts AS acct WHERE (@AccountID = 0 OR acct.AccountID = @AccountID) AND (@UserName IS NULL OR acct.UserName = @UserName);

GO
/****** Object:  StoredProcedure [dbo].[sp_Account_Login]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Account_Login]
	@UserName VARCHAR(20),
	@APIKey VARCHAR(50),
	@APIKeyHash BINARY(256),
	@LoginDate DATETIME OUTPUT
AS
SET NOCOUNT ON;

SET @LoginDate = GETUTCDATE();

INSERT INTO dbo.Logins (UserName, APIKey, APIKeyHash, LoginDate)
	VALUES ( @UserName, @APIKey, @APIKeyHash, @LoginDate);
GO
/****** Object:  StoredProcedure [dbo].[sp_Account_Login_Get]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Account_Login_Get]
	@UserName VARCHAR(20),
	@APIKey varchar(50)
AS
SET NOCOUNT ON;

SELECT TOP 1 * FROM dbo.Logins AS logins 
WHERE (logins.UserName = @UserName AND logins.APIKey = @APIKey)

GO
/****** Object:  StoredProcedure [dbo].[sp_Account_Logins_Get]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Account_Logins_Get]
	@UserName VARCHAR(20)
AS
SET NOCOUNT ON;

SELECT * FROM dbo.Logins AS logins 
WHERE (logins.UserName = @UserName)

GO
/****** Object:  StoredProcedure [dbo].[sp_Account_Logout]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Account_Logout]
	@UserName VARCHAR(20),
	@APIKey VARCHAR(50) = NULL --If Null, removes all logins
AS
SET NOCOUNT OFF;

DELETE FROM dbo.Logins WHERE [UserName] = @UserName AND (@APIKey IS NULL OR [APIKey] = @APIKey);

SELECT @@ROWCOUNT AS DELETED;

GO
/****** Object:  StoredProcedure [dbo].[sp_Account_Search]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Account_Search]
	@UserName VARCHAR(20) = NULL,	
	@DisplayName NVARCHAR(50) = NULL,		
	@FirstName NVARCHAR(50) = NULL,
	@LastName NVARCHAR(50) = NULL,
	@Email VARCHAR(50) = NULL,
	@PhoneNumber VARCHAR(20) = NULL
AS
SET NOCOUNT ON;

DECLARE @bHasUserName BIT = IIF(LEN(@UserName) >= 3, 1, 0), 
			@bHasDisplayName BIT = IIF(LEN(@DisplayName) >= 3, 1, 0), 
			@bHasFirstName BIT = IIF(LEN(@FirstName) >= 1, 1, 0), 
			@bHasLastName BIT = IIF(LEN(@LastName) >= 1, 1, 0), 
			@bHasEmail BIT = IIF(LEN(@Email) >= 3, 1, 0), 
			@bHasPhoneNumber BIT = IIF(LEN(@PhoneNumber) >= 3, 1, 0);


SELECT * FROM dbo.Accounts AS acct 
WHERE		(@bHasUserName = 0 OR acct.UserName LIKE @UserName + '%')
	AND	(@bHasDisplayName = 0 OR acct.DisplayName LIKE @DisplayName + '%')
	AND	(@bHasFirstName = 0 OR acct.FirstName LIKE @FirstName + '%')
	AND	(@bHasLastName = 0 OR acct.LastName LIKE @LastName + '%')
	AND	(@bHasEmail = 0  OR acct.SchoolEmail LIKE @Email + '%' OR acct.ContactEmail LIKE @Email + '%')
	AND	(@bHasPhoneNumber = 0 OR acct.PhoneNumber = @PhoneNumber)

GO
/****** Object:  StoredProcedure [dbo].[sp_EmailVerification_Add]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_EmailVerification_Add]
	@AccountID BIGINT,
	@VerificationKey varchar(50),
	@VerificationHash BINARY(32),
	@Email varchar(50),
	@Date smalldatetime
AS
SET NOCOUNT ON;

INSERT INTO dbo.EmailVerification (AccountID, VerificationKey, VerificationHash, Email, [Date], IsVerified) 
	VALUES (@AccountID, @VerificationKey, @VerificationHash, @Email, @Date, 0);

GO
/****** Object:  StoredProcedure [dbo].[sp_EmailVerification_GetOne]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_EmailVerification_GetOne]
	@AccountID BIGINT,
	@VerificationKey varchar(50) = null,
	@Email varchar(50) = null
AS
SET NOCOUNT ON;



SELECT TOP 1 * FROM dbo.EmailVerification
WHERE AccountID = @AccountID 
	AND (@VerificationKey IS NULL OR VerificationKey = @VerificationKey)
	AND (@Email IS NULL OR Email = @Email);

GO
/****** Object:  StoredProcedure [dbo].[sp_EmailVerification_SetVerified]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_EmailVerification_SetVerified]
	@AccountID BIGINT,
	@VerificationKey varchar(50),
	@IsVerified bit
AS
SET NOCOUNT ON;

Update dbo.EmailVerification SET IsVerified = @IsVerified WHERE AccountID = @AccountID AND VerificationKey = @VerificationKey;

GO
/****** Object:  StoredProcedure [dbo].[sp_Event_Create]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Event_Create]
	@EventID BIGINT = NULL OUTPUT,
	@EventTypeID BIGINT,
	@DateStart datetime,
	@DateEnd datetime,
	@AccountID BIGINT,
	@LocationID BIGINT,
	@Title varchar(80),
	@Caption nvarchar(160),
	@Details nvarchar(max) = NULL
	--,  @TagIDs varchar(400)
AS
SET NOCOUNT ON;


BEGIN TRANSACTION;
BEGIN TRY 

	IF EXISTS(SELECT TOP 1 * FROM dbo.EventFeed WHERE EventTypeID = @EventTypeID AND DateStart = @DateStart AND DateEnd = @DateEnd AND AccountID = @AccountID AND LocationID = @LocationID AND Title = @Title) THROW 50000, 'Duplicate Event.', 10;


	INSERT INTO dbo.EventFeed (EventTypeID, DateStart, DateEnd, AccountID, LocationID, Title, Caption) 
		VALUES (@EventTypeID, @DateStart, @DateEnd, @AccountID, @LocationID, @Title, @Caption);

	SET @EventID = SCOPE_IDENTITY();

	INSERT INTO dbo.EventDetails(EventId, Details) VALUES (@EventID, @Details);



	COMMIT TRANSACTION;
END TRY  
BEGIN CATCH  
	ROLLBACK TRANSACTION;  
	THROW;
END CATCH 


GO
/****** Object:  StoredProcedure [dbo].[sp_Event_CreateOrUpdate]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Event_CreateOrUpdate]
	@EventID BIGINT = NULL OUTPUT,
	@EventTypeID BIGINT,
	@DateStart datetime,
	@DateEnd datetime,
	@AccountID BIGINT,
	@LocationID BIGINT,
	@Title varchar(80),
	@Caption nvarchar(160),
	@Details nvarchar(max) = NULL,
	@TagIDs varchar(400)
AS
SET NOCOUNT ON;



IF NOT EXISTS (SELECT TOP 1 * FROM [dbo].EventTypes WHERE EventTypeID = @EventTypeID) THROW 50000, 'EventType_Not_Exists', 1;
IF NOT EXISTS (SELECT TOP 1 * FROM [dbo].Accounts WHERE AccountID = @AccountID) THROW 50000, 'Account_Not_Exists', 1;
IF NOT EXISTS (SELECT TOP 1 * FROM [dbo].Locations WHERE LocationID = @LocationID) THROW 50000, 'Location_Not_Exists', 1;



INSERT INTO dbo.EventFeed (EventTypeID, DateStart, DateEnd, AccountID, LocationID, Title, Caption) 
	VALUES (@EventTypeID, @DateStart, @DateEnd, @AccountID, @LocationID, @Title, @Caption);

SET @EventID = SCOPE_IDENTITY();

INSERT INTO dbo.EventDetails(EventId, Details) VALUES (@EventID, @Details);





INSERT INTO dbo.EventTagMap(EventID, TagID)
	VALUES (@EventID, (SELECT CAST(TRIM(value) AS BIGINT) as ID FROM STRING_SPLIT(@TagIDs, ';')));


GO
/****** Object:  StoredProcedure [dbo].[sp_Event_Details_GetOne]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Event_Details_GetOne] 
	@EventID BIGINT
AS
SET NOCOUNT ON;

SELECT * FROM dbo.EventDetails WHERE EventID = @EventID;

GO
/****** Object:  StoredProcedure [dbo].[sp_Event_GetById]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Event_GetById]
	@EventID BIGINT
AS
SET NOCOUNT ON;


SELECT TOP 1
		feed.EventID,
		feed.EventTypeID,
		feed.AccountID,
		feed.LocationID,
		feed.DateStart,
		feed.DateEnd,
		feed.Title,
		feed.Caption,
		COUNT(DISTINCT CASE when rsvp.RSVPTypeId = 5 THEN rsvp.AccountID END) AS RSVP_Attending,
		COUNT(DISTINCT CASE when rsvp.RSVPTypeId = 4 THEN rsvp.AccountID END) AS RSVP_Later,
		COUNT(DISTINCT CASE WHEN rsvp.RSVPTypeId = 3 THEN rsvp.AccountID END) AS RSVP_StopBy,
		COUNT(DISTINCT CASE WHEN rsvp.RSVPTypeId = 2 THEN rsvp.AccountID END) AS RSVP_Maybe,
		COUNT(DISTINCT CASE WHEN rsvp.RSVPTypeId = 1 THEN rsvp.AccountID END) AS RSVP_No,
		STUFF((SELECT ', ' + CONVERT(VARCHAR(20), TagID) FROM dbo.EventTagMap WHERE EventID = feed.EventID FOR XML PATH('')), 1, 1, '') AS TagIds,
		acc.DisplayName as UserDisplayName,
		acc.UserName,
		loc.ParentLocationID,
		loc.LocationID,
		loc.[Name] as LocationName,
		loc.AddressLine,
		loc.Locality,
		loc.PostalCode,
		loc.AdminDistrict,
		loc.CountryRegion,
		deets.Details
	FROM dbo.EventFeed AS feed
		LEFT OUTER JOIN dbo.EventRSVPs AS rsvp ON feed.EventID = rsvp.EventID
		LEFT OUTER JOIN dbo.Accounts AS acc ON feed.AccountID = acc.AccountID
		LEFT OUTER JOIN dbo.Locations AS loc ON feed.LocationID = loc.LocationID
		LEFT OUTER JOIN dbo.EventDetails as deets ON feed.EventID = deets.EventID
	WHERE feed.EventID = @EventID
	GROUP BY feed.EventID, feed.EventTypeID, feed.AccountID, feed.LocationID, feed.DateStart, feed.DateEnd, feed.Title, feed.Caption, 
		acc.DisplayName, acc.UserName,
		loc.CountryRegion, loc.AdminDistrict, loc.PostalCode, loc.Locality, loc.[Name], loc.AddressLine, loc.LocationID, loc.ParentLocationID,
		deets.Details;



GO
/****** Object:  StoredProcedure [dbo].[sp_Event_GetById_UserView]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Event_GetById_UserView]
	@EventID BIGINT,
	@RequestingUserID BIGINT 
AS
SET NOCOUNT ON;


SELECT TOP 1
		feed.EventID,
		feed.EventTypeID,
		feed.AccountID,
		feed.LocationID,
		feed.DateStart,
		feed.DateEnd,
		feed.Title,
		feed.Caption,
		COUNT(DISTINCT CASE when rsvp.RSVPTypeId = 5 THEN rsvp.AccountID END) AS RSVP_Attending,
		COUNT(DISTINCT CASE when rsvp.RSVPTypeId = 4 THEN rsvp.AccountID END) AS RSVP_Later,
		COUNT(DISTINCT CASE WHEN rsvp.RSVPTypeId = 3 THEN rsvp.AccountID END) AS RSVP_StopBy,
		COUNT(DISTINCT CASE WHEN rsvp.RSVPTypeId = 2 THEN rsvp.AccountID END) AS RSVP_Maybe,
		COUNT(DISTINCT CASE WHEN rsvp.RSVPTypeId = 1 THEN rsvp.AccountID END) AS RSVP_No,
		STUFF((SELECT ', ' + CONVERT(VARCHAR(20), TagID) FROM dbo.EventTagMap WHERE EventID = feed.EventID FOR XML PATH('')), 1, 1, '') AS TagIds,
		acc.DisplayName as UserDisplayName,
		acc.UserName,
		loc.ParentLocationID,
		loc.LocationID,
		loc.[Name] as LocationName,
		loc.AddressLine,
		loc.Locality,
		loc.PostalCode,
		loc.AdminDistrict,
		loc.CountryRegion,
		deets.Details,
		(SELECT TOP 1 RSVPTypeID FROM dbo.EventRSVPs WHERE [EventID] = @EventID AND [AccountID] = @RequestingUserID) AS UserRsvpID
	FROM dbo.EventFeed AS feed
		LEFT OUTER JOIN dbo.EventRSVPs AS rsvp ON feed.EventID = rsvp.EventID
		LEFT OUTER JOIN dbo.Accounts AS acc ON feed.AccountID = acc.AccountID
		LEFT OUTER JOIN dbo.Locations AS loc ON feed.LocationID = loc.LocationID
		LEFT OUTER JOIN dbo.EventDetails as deets ON feed.EventID = deets.EventID
	WHERE feed.EventID = @EventID
	GROUP BY feed.EventID, feed.EventTypeID, feed.AccountID, feed.LocationID, feed.DateStart, feed.DateEnd, feed.Title, feed.Caption, 
		acc.DisplayName, acc.UserName,
		loc.CountryRegion, loc.AdminDistrict, loc.PostalCode, loc.Locality, loc.[Name], loc.AddressLine, loc.LocationID, loc.ParentLocationID,
		deets.Details;


GO
/****** Object:  StoredProcedure [dbo].[sp_Event_Remove]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Event_Remove]
	@EventID BIGINT, 
	@AccountID BIGINT
AS
SET NOCOUNT ON;


IF NOT EXISTS (SELECT TOP 1 * FROM dbo.EventFeed WHERE EventID = @EventID AND AccountID = @AccountID) 
	AND NOT EXISTS(SELECT TOP 1 * FROM dbo.Accounts WHERE AccountID = @AccountID AND IsAdmin = 1) THROW 50000, 'Unauthorized. Must be creator or Admin.', 10;

BEGIN TRANSACTION;
BEGIN TRY 

	DELETE FROM dbo.EventRSVPs WHERE EventID = @EventID;
	DELETE FROM dbo.EventTagMap WHERE EventID = @EventID;
	DELETE FROM dbo.EventDetails WHERE EventID = @EventID;
	DELETE FROM dbo.EventFeed WHERE EventID = @EventID;

	COMMIT TRANSACTION;
END TRY  
BEGIN CATCH  
	ROLLBACK TRANSACTION;  
	THROW;
END CATCH 

GO
/****** Object:  StoredProcedure [dbo].[sp_Event_RSVP_AddOrUpdate]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Event_RSVP_AddOrUpdate]
	@EventID BIGINT,
	@AccountID BIGINT,
	@RSVPTypeID SMALLINT
AS
SET NOCOUNT ON;


BEGIN TRANSACTION;
BEGIN TRY 

	IF EXISTS (SELECT TOP 1 * FROM [dbo].EventRSVPs WHERE EventID = @EventID AND AccountID = @AccountID)
		BEGIN
			UPDATE dbo.EventRSVPs SET RSVPTypeId = @RSVPTypeID WHERE EventID = @EventID AND AccountID = @AccountID
		END 
	ELSE 
		BEGIN
			INSERT INTO dbo.EventRSVPs(EventID, AccountID, RSVPTypeId) VALUES (@EventID, @AccountID, @RSVPTypeID);
		END

	COMMIT TRANSACTION;
END TRY  
BEGIN CATCH  
	ROLLBACK TRANSACTION;  
	THROW;
END CATCH 



GO
/****** Object:  StoredProcedure [dbo].[sp_Event_RSVP_Get]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Event_RSVP_Get]
	@EventID BIGINT,
	@AccountID BIGINT
AS
SET NOCOUNT ON;


(SELECT TOP 1 * FROM [dbo].EventRSVPs WHERE EventID = @EventID AND AccountID = @AccountID)


GO
/****** Object:  StoredProcedure [dbo].[sp_Event_Search]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Event_Search]
	@EventID BIGINT = NULL,
	@EventTypeID BIGINT = NULL,
	@AccountID BIGINT = NULL,
	@LocationID BIGINT = NULL,
	@DateFrom datetime = NULL,
	@DateTo datetime = NULL,
	@Title varchar(80) = NULL,
	@Caption nvarchar(160) = NULL
AS
SET NOCOUNT ON;

DECLARE @bHasTitle bit = IIF(LEN(@Title) >= 3, 1, 0), 
			@bHasCaption bit = IIF(LEN(@Caption) >= 3, 1, 0);


SELECT 
		feed.EventID,
		feed.EventTypeID,
		feed.AccountID,
		feed.LocationID,
		feed.DateStart,
		feed.DateEnd,
		feed.Title,
		feed.Caption,
		COUNT(DISTINCT CASE when rsvp.RSVPTypeId = 5 THEN rsvp.AccountID END) AS RSVP_Attending,
		COUNT(DISTINCT CASE when rsvp.RSVPTypeId = 4 THEN rsvp.AccountID END) AS RSVP_Later,
		COUNT(DISTINCT CASE WHEN rsvp.RSVPTypeId = 3 THEN rsvp.AccountID END) AS RSVP_StopBy,
		COUNT(DISTINCT CASE WHEN rsvp.RSVPTypeId = 2 THEN rsvp.AccountID END) AS RSVP_Maybe,
		COUNT(DISTINCT CASE WHEN rsvp.RSVPTypeId = 1 THEN rsvp.AccountID END) AS RSVP_No,
		STUFF((SELECT ', ' + CONVERT(VARCHAR(20), TagID) FROM dbo.EventTagMap WHERE EventID = feed.EventID FOR XML PATH('')), 1, 1, '') AS TagIds,
		acc.DisplayName as UserDisplayName,
		acc.UserName,
		loc.[Name] as LocationName,
		loc.AddressLine,
		loc.Locality,
		loc.PostalCode,
		loc.AdminDistrict,
		loc.CountryRegion
	FROM dbo.EventFeed AS feed
	LEFT OUTER JOIN dbo.EventRSVPs AS rsvp ON feed.EventID = rsvp.EventID
	LEFT OUTER JOIN dbo.Accounts AS acc ON feed.AccountID = acc.AccountID
	LEFT OUTER JOIN dbo.Locations AS loc ON feed.LocationID = loc.LocationID
	WHERE (@EventID IS NULL OR feed.EventID = @EventID)
		AND(@EventTypeID IS NULL OR feed.EventTypeID = @EventTypeID)
		AND(@AccountID IS NULL OR feed.AccountID = @AccountID)
		AND(@LocationID IS NULL OR feed.LocationID = @LocationID)
		AND(@DateFrom IS NULL OR feed.DateStart > @DateFrom OR feed.DateEnd > @DateFrom)
		AND(@DateTo IS NULL OR feed.DateStart < @DateTo OR feed.DateEnd < @DateTo)
		AND (@bHasTitle = 0 OR feed.Title LIKE '%' + @Title + '%')
		AND (@bHasCaption = 0 OR feed.Caption LIKE '%' + @Caption + '%')
	GROUP BY feed.EventID, feed.EventTypeID, feed.AccountID, feed.LocationID, feed.DateStart, feed.DateEnd, feed.Title, feed.Caption, 
		acc.DisplayName, acc.UserName,
		loc.CountryRegion, loc.AdminDistrict, loc.PostalCode, loc.Locality, loc.[Name], loc.AddressLine
	ORDER BY feed.DateStart;


GO
/****** Object:  StoredProcedure [dbo].[sp_Event_TagAdd]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Event_TagAdd]
	@EventID BIGINT,
	@TagID BIGINT
AS
SET NOCOUNT ON;


IF NOT EXISTS(SELECT TOP 1 * FROM dbo.EventTagMap WHERE EventID = @EventID AND TagID = @TagID)
	Begin
		INSERT INTO dbo.EventTagMap(EventID, TagID) VALUES (@EventID, @TagID);
	END


GO
/****** Object:  StoredProcedure [dbo].[sp_Event_TagRemove]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Event_TagRemove]
	@EventID BIGINT,
	@TagID BIGINT
AS

DELETE FROM dbo.EventTagMap WHERE EventID = @EventID AND TagID = @TagID;

GO
/****** Object:  StoredProcedure [dbo].[sp_Event_Update]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Event_Update]
	@EventID BIGINT,
	@EventTypeID BIGINT,
	@DateStart datetime,
	@DateEnd datetime,
	@AccountID BIGINT,
	@LocationID BIGINT,
	@Title varchar(80),
	@Caption nvarchar(160),
	@Details nvarchar(max) = NULL
AS
SET NOCOUNT ON;

IF NOT EXISTS (SELECT TOP 1 * FROM dbo.EventFeed WHERE EventID = @EventID AND AccountID = @AccountID) 
	AND NOT EXISTS(SELECT TOP 1 * FROM dbo.Accounts WHERE AccountID = @AccountID AND IsAdmin = 1) THROW 50000, 'Unauthorized. Must be creator or Admin.', 10;


BEGIN TRANSACTION;
BEGIN TRY 

	UPDATE dbo.EventFeed 
		SET EventTypeID = @EventTypeID, DateStart = @DateStart, DateEnd = @DateEnd, LocationID = @LocationID, Title = @Title, Caption = @Caption
		WHERE EventID = @EventID;

	IF EXISTS (SELECT TOP 1 * FROM [dbo].EventDetails WHERE EventID = @EventID) 
		BEGIN
			UPDATE dbo.EventDetails SET Details = @Details WHERE EventID = @EventID;
		END
	ELSE
		BEGIN
			INSERT INTO dbo.EventDetails(EventId, Details) VALUES (@EventID, @Details);
		END

	COMMIT TRANSACTION;
END TRY  
BEGIN CATCH  
	ROLLBACK TRANSACTION;  
	THROW;
END CATCH 

GO
/****** Object:  StoredProcedure [dbo].[sp_EventTypes_Create]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_EventTypes_Create]
	@EventTypeID Int = NULL OUTPUT,
	@Name VARCHAR(50),
	@Description NVARCHAR(400)
AS
SET NOCOUNT ON;

IF EXISTS (SELECT TOP 1 * FROM [dbo].EventTypes WHERE [Name] = @Name) THROW 50000, 'EventType_Already_Exists', 1;

INSERT INTO [dbo].EventTypes([Name], [Description]) 
	VALUES (@Name, @Description);

SET @EventTypeID = SCOPE_IDENTITY();

GO
/****** Object:  StoredProcedure [dbo].[sp_EventTypes_GetOne]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_EventTypes_GetOne]
	@EventTypeID BIGINT = NULL,
	@Name VARCHAR(50) = NULL
AS
SET NOCOUNT ON;

DECLARE @bHasName bit = IIF(LEN(@Name) >= 2, 1, 0);

SELECT Top 1 * FROM dbo.EventTypes AS tags WHERE (@EventTypeID IS NULL OR tags.EventTypeID = @EventTypeID) AND (@bHasName = 0 OR tags.Name LIKE '%' + @Name + '%'); 

GO
/****** Object:  StoredProcedure [dbo].[sp_EventTypes_Search]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_EventTypes_Search]
	@EventTypeID Int = NULL,
	@Name VARCHAR(50) = NULL,
	@Description NVARCHAR(400) = NULL
AS
SET NOCOUNT ON;

DECLARE @bHasName bit = IIF(LEN(@Name) >= 2, 1, 0), 
			@BHasDescription bit = IIF(LEN(@Description) >= 4, 1, 0);


SELECT * FROM dbo.EventTypes AS tags 
WHERE (@EventTypeID IS NULL OR tags.EventTypeID = @EventTypeID)
	AND (@bHasName = 0 OR tags.Name LIKE '%' + @Name + '%')
	AND (@BHasDescription = 0 OR tags.[Description] LIKE '%' + @Description + '%'); 

GO
/****** Object:  StoredProcedure [dbo].[sp_Group_Create]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Group_Create]
	@GroupOwnerAccountID BIGINT,
	@GroupName VARCHAR(20),
	@DisplayName NVARCHAR(50) = NULL,	
	@SchoolEmail VARCHAR(50) = NULL,
	@ContactEmail VARCHAR(50) = NULL,
	@PhoneNumber VARCHAR(20) = NULL,
	@LocationID BIGINT = NULL,
	@GroupID BIGINT OUTPUT
AS
SET NOCOUNT ON;

IF @DisplayName IS NULL SET @DisplayName = @GroupName;
IF @LocationID IS NOT NULL AND NOT EXISTS (SELECT TOP 1 * FROM [dbo].Locations WHERE LocationID = @LocationID) Throw 50000, 'Location_Invalid', 1;
IF EXISTS (SELECT TOP 1 * FROM [dbo].Accounts WHERE UserName = @GroupName) THROW 50000, 'UserName_Taken',1;
IF NOT EXISTS (SELECT TOP 1 * FROM [dbo].Accounts WHERE AccountID = @GroupOwnerAccountID) Throw 50000, 'GroupOwner_Invalid', 1;


INSERT INTO dbo.Accounts (UserName, DisplayName, PasswordHash, LocationID, FirstName, LastName, SchoolEmail, ContactEmail, PhoneNumber, IsGroup) 
	VALUES (@GroupName, @DisplayName, null, @LocationID, null, null, @SchoolEmail, @ContactEmail, @PhoneNumber, 1);

SET @GroupID = SCOPE_IDENTITY();

INSERT INTO dbo.AccountGroupMap(AccountID, GroupID, IsGroupOwner, IsGroupAdmin, IsGroupFriend, IsPendingFriend, IsGroupFollower) 
	VALUES (@GroupOwnerAccountID, @GroupID, 1, 0, 0, 0, 0);

GO
/****** Object:  StoredProcedure [dbo].[sp_Location_Update]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Location_Update]
	-- Add the parameters for the stored procedure here
	@LocationID bigint,	
	@ParentLocationID bigint = NULL,
	@Name varchar(80) = NULL,
	@AddressLine varchar(80) = NULL,
	@Locality varchar(40) = NULL,
	@AdminDistrict varchar(40) = NULL,
	@PostalCode varchar(20) = NULL,
	@CountryRegion varchar(40) = NULL,
	@Description varchar(140) = NULL,
	@Latitude real = NULL,
	@Longitude real = NULL
AS
SET NOCOUNT ON;

IF @LocationID <= 0 THROW 50000, 'LocationID_Invalid', 1;

IF @CountryRegion IS NULL THROW 50000, 'CountryRegion_NULL', 1;
IF (@Latitude IS NULL AND @Longitude IS NOT NULL) OR (@Latitude IS NOT NULL AND @Longitude IS NULL) THROW 50000, 'LATITUDELONGITUDE_INVALID', 1;

DECLARE @Lat int = NULL, @Lon int = NULL;
IF (@Latitude IS NOT NULL) SET @Lat = CAST((@Longitude*1000000) AS int);
IF (@Longitude IS NOT NULL) SET @Lon = CAST((@Longitude*1000000) AS int);


UPDATE dbo.Locations
	SET [ParentLocationID] = @ParentLocationID, 
		[Name] = @Name, 
		[AddressLine] = @AddressLine, 
		[Locality] = @Locality, 
		[AdminDistrict] = @AdminDistrict, 
		[PostalCode] = @PostalCode, 
		[CountryRegion] = @CountryRegion, 
		[Latitude6x] = @Lat, 
		[Longitude6x] = @Lon, 
		[Description] = @Description
WHERE [LocationID] = @LocationID

GO
/****** Object:  StoredProcedure [dbo].[sp_Locations_CreateOne]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Locations_CreateOne]
	-- Add the parameters for the stored procedure here
	@Name VARCHAR(80) = NULL,
	@AddressLine VARCHAR(80) = NULL,
	@Locality VARCHAR(40) = NULL,
	@AdminDistrict VARCHAR(40) = NULL,
	@PostalCode VARCHAR(20) = NULL,	
	@CountryRegion varchar(40) = NULL,
	@Description varchar(140) = NULL,
	@Latitude6x int = NULL,
	@Longitude6x int = NULL,
	@ParentLocationID BIGINT = NULL OUTPUT,
	@LocationID BIGINT OUTPUT
AS
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
SET NOCOUNT ON;


DECLARE @eLocationID BIGINT = NULL;
DECLARE @eParentLocationID BIGINT = NULL;

SELECT @eLocationID = loc.[LocationID], @eParentLocationID = loc.[ParentLocationID] 
FROM dbo.Locations AS loc 
WHERE ([Name] IS NULL AND @Name IS NULL OR [Name] = @Name)
	AND ([AddressLine] IS NULL AND @AddressLine IS NULL OR [AddressLine] = @AddressLine) 
	AND ([Locality] IS NULL AND @Locality IS NULL OR [Locality] = @Locality) 
	AND ([AdminDistrict] IS NULL AND @AdminDistrict IS NULL OR [AdminDistrict] = @AdminDistrict)
	AND ([PostalCode] IS NULL AND @PostalCode IS NULL OR [PostalCode] = @PostalCode) 
	AND ([CountryRegion] = @CountryRegion)
	AND ([Description] IS NULL AND @Description IS NULL OR [Description] = @Description);


IF @eLocationID > 0
	BEGIN
		SET @LocationID = @eLocationID;
		SET @ParentLocationID = @eParentLocationID;
	END
ELSE
	BEGIN
		IF @ParentLocationID IS NULL 
			BEGIN
				IF @AddressLine IS NOT NULL 
					BEGIN
						SELECT TOP 1 @ParentLocationID = [LocationID] FROM dbo.Locations AS loc WHERE loc.AddressLine IS NULL AND loc.Locality = @Locality AND loc.AdminDistrict = @AdminDistrict AND loc.PostalCode = @PostalCode AND loc.CountryRegion = @CountryRegion;

						IF @ParentLocationID IS NULL 
							BEGIN
								SELECT TOP 1 @ParentLocationID = [LocationID] FROM dbo.Locations AS loc WHERE loc.AddressLine IS NULL AND loc.Locality = @Locality AND loc.AdminDistrict = @AdminDistrict AND loc.CountryRegion = @CountryRegion;
							END      
					END

				ELSE IF @AddressLine IS NULL AND @Locality IS NOT NULL
					BEGIN
						SELECT TOP 1 @ParentLocationID = [LocationID] FROM dbo.Locations AS loc WHERE loc.AddressLine IS NULL AND loc.Locality IS NULL AND loc.AdminDistrict = @AdminDistrict AND loc.PostalCode = @PostalCode AND loc.CountryRegion = @CountryRegion;
					END
					IF @ParentLocationID IS NULL 
						BEGIN
							SELECT TOP 1 @ParentLocationID = [LocationID] FROM dbo.Locations AS loc WHERE loc.AddressLine IS NULL AND loc.Locality IS NULL AND loc.AdminDistrict = @AdminDistrict AND loc.CountryRegion = @CountryRegion;
						END 

				ELSE IF @AddressLine IS NULL AND @Locality IS NULL AND @AdminDistrict IS NOT NULL
					BEGIN
						SELECT TOP 1 @ParentLocationID = [LocationID] FROM dbo.Locations AS loc WHERE loc.AddressLine IS NULL AND loc.Locality IS NULL AND loc.AdminDistrict IS NULL AND loc.PostalCode = @PostalCode AND loc.CountryRegion = @CountryRegion;
					END
					IF @ParentLocationID IS NULL 
						BEGIN
							SELECT TOP 1 @ParentLocationID = [LocationID] FROM dbo.Locations AS loc WHERE loc.AddressLine IS NULL AND loc.Locality IS NULL AND loc.AdminDistrict IS NULL AND loc.CountryRegion = @CountryRegion;
						END 
			END;


		INSERT INTO dbo.Locations ([ParentLocationID], [Name], [AddressLine], [Locality], [AdminDistrict], [PostalCode], [CountryRegion], [Latitude6x], [Longitude6x], [Description])
		VALUES (@ParentLocationID, @Name, @AddressLine, @Locality, @AdminDistrict, @PostalCode, @CountryRegion, @Latitude6x, @Longitude6x, @Description)

		SET @LocationID = SCOPE_IDENTITY()

	END

GO
/****** Object:  StoredProcedure [dbo].[sp_Locations_GetOne]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Locations_GetOne]
	@LocationID bigint
AS
SET NOCOUNT ON;
	SELECT TOP(1) * FROM dbo.Locations AS loc WHERE loc.LocationID = @LocationID
GO
/****** Object:  StoredProcedure [dbo].[sp_Locations_Search]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Locations_Search]
	@ParentLocationID bigint = NULL,
	@LocationID bigint = NULL,
	@Name varchar(80) = NULL, 
	@AddressLine varchar(80) = NULL,
	@Locality varchar(40) = NULL,
	@AdminDistrict varchar(40) = NULL,
	@PostalCode varchar(20) = NULL,
	@CountryRegion varchar(40) = NULL,
	@Description NVARCHAR(80) = NULL,
	@SearchMode tinyint = 0
AS
SET NOCOUNT ON;

DECLARE @bHasName bit = IIF(LEN(@Name) >= 3, 1, 0), 
			@bHasAddressLine bit = IIF(LEN(@AddressLine) >= 3, 1, 0), 
			@bHasLocality bit = IIF(LEN(@Locality) >= 3, 1, 0), 
			@bHasAdminDistrict bit = IIF(LEN(@AdminDistrict) >= 2, 1, 0), 
			@bHasPostalCode bit = IIF(LEN(@PostalCode) >= 3, 1, 0), 
			@bHasCountryRegion bit = IIF(LEN(@CountryRegion) >= 2, 1, 0),
			@bHasDescription BIT = IIF(LEN(@Description) >= 5, 1, 0)

IF @SearchMode = 1
	BEGIN
		SELECT * FROM dbo.Locations AS loc
			WHERE (@LocationID IS NULL OR loc.LocationID = @LocationID)
				AND (@ParentLocationID IS NULL OR loc.ParentLocationID = @ParentLocationID)
				AND (@bHasName = 0 OR loc.Name LIKE @Name)
				AND (@bHasAddressLine = 0 OR loc.AddressLine LIKE @AddressLine)
				AND (@bHasLocality = 0 OR loc.Locality LIKE @Locality)
				AND (@bHasAdminDistrict = 0 OR loc.AdminDistrict LIKE @AdminDistrict)
				AND (@bHasPostalCode = 0 OR loc.PostalCode LIKE @PostalCode)
				AND (@bHasCountryRegion = 0 OR loc.CountryRegion LIKE @CountryRegion)
				AND (@bHasDescription = 0 OR loc.[Description] LIKE @Description)
	END

ELSE IF @SearchMode = 2
	BEGIN
		SELECT * FROM dbo.Locations AS loc
			WHERE (loc.LocationID = @LocationID)
				AND (loc.ParentLocationID = @ParentLocationID)
				AND (loc.Name LIKE @Name)
				AND (loc.AddressLine LIKE @AddressLine)
				AND (loc.Locality LIKE @Locality)
				AND (loc.AdminDistrict LIKE @AdminDistrict)
				AND (loc.PostalCode LIKE @PostalCode)
				AND (loc.CountryRegion LIKE @CountryRegion)
				AND (loc.[Description] LIKE @Description)
	END

ELSE
	BEGIN
		SELECT * FROM dbo.Locations AS loc
			WHERE (@LocationID IS NULL OR loc.LocationID = @LocationID)
				AND (@ParentLocationID IS NULL OR loc.ParentLocationID = @ParentLocationID)
				AND (@bHasName = 0 OR loc.Name LIKE '%' + @Name + '%')
				AND (@bHasAddressLine = 0 OR loc.AddressLine LIKE '%' + @AddressLine + '%')
				AND (@bHasLocality = 0 OR loc.Locality LIKE '%' + @Locality + '%')
				AND (@bHasAdminDistrict = 0 OR loc.AdminDistrict LIKE @AdminDistrict + '%')
				AND (@bHasPostalCode = 0 OR loc.PostalCode LIKE @PostalCode + '%')
				AND (@bHasCountryRegion = 0 OR loc.CountryRegion LIKE @CountryRegion)
				AND (@bHasDescription = 0 OR loc.[Description] LIKE '%' + @Description + '%')
	END


GO
/****** Object:  StoredProcedure [dbo].[sp_RSVPTypes_Get]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_RSVPTypes_Get]
AS
SET NOCOUNT ON;

SELECT * FROM dbo.RSVPTypes;

GO
/****** Object:  StoredProcedure [dbo].[sp_Tags_Create]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Tags_Create]
	@Name VARCHAR(50),
	@Description NVARCHAR(160),
	@TagID BIGINT = NULL OUTPUT 
AS
SET NOCOUNT ON;


INSERT INTO [dbo].Tags([Name], [Description]) 
	VALUES (@Name, @Description);

SET @TagID = SCOPE_IDENTITY();

GO
/****** Object:  StoredProcedure [dbo].[sp_Tags_GetOne]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Tags_GetOne]
	@TagID BIGINT = NULL,
	@Name VARCHAR(50) = NULL
AS
SET NOCOUNT ON;

DECLARE @bHasName bit = IIF(LEN(@Name) >= 2, 1, 0);

SELECT Top 1 * FROM dbo.Tags AS tags WHERE (@TagID IS NULL OR tags.TagID = @TagID) AND (@bHasName = 0 OR tags.Name LIKE '%' + @Name + '%'); 

GO
/****** Object:  StoredProcedure [dbo].[sp_Tags_Query]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Tags_Query]
	@Query VARCHAR(50) = NULL
AS
SET NOCOUNT ON;

DECLARE @len TINYINT = LEN(@Query);

SELECT * FROM dbo.Tags AS tags WHERE (@len >= 2 AND tags.Name LIKE @Query + '%') OR (@len > 3 AND tags.[Description] LIKE '%' + @Query + '%'); 

GO
/****** Object:  StoredProcedure [dbo].[sp_Tags_Search]    Script Date: 4/20/2018 10:25:36 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[sp_Tags_Search]
	@TagID BIGINT = NULL,
	@Name VARCHAR(50) = NULL,
	@Description NVARCHAR(160) = NULL
AS
SET NOCOUNT ON;

DECLARE @bHasName bit = IIF(LEN(@Name) >= 2, 1, 0), 
			@BHasDescription bit = IIF(LEN(@Description) >= 4, 1, 0);

If @TagID IS NOT NULL AND @bHasName = 0 AND @BHasDescription = 0 
	BEGIN
		SELECT * FROM dbo.Tags AS tags WHERE tags.TagID = @TagID
	END
ELSE
	BEGIN
		SELECT * FROM dbo.Tags AS tags WHERE (@bHasName = 0 OR tags.Name LIKE '%' + @Name + '%') AND (@BHasDescription = 0 OR tags.[Description] LIKE '%' + @Description + '%'); 
	END
 

GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Locations'
GO
USE [master]
GO
ALTER DATABASE [dbUniHangouts] SET  READ_WRITE 
GO
