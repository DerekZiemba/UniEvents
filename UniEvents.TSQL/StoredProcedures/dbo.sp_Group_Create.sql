SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET XACT_ABORT ON  
-- =============================================
-- Author:		<Derek Ziemba>
-- Create date: <3-7-2018>
-- =============================================
USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Group_Create]
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
