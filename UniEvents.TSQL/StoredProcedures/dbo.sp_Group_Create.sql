SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
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
IF @LocationID IS NOT NULL AND NOT EXISTS (SELECT TOP 1 * FROM [dbo].Locations WHERE LocationID = @LocationID) RAISERROR('Location_Invalid', 11, 1);
IF EXISTS (SELECT TOP 1 * FROM [dbo].Accounts WHERE UserName = @GroupName) RAISERROR('UserName_Taken', 12, 1);
IF NOT EXISTS (SELECT TOP 1 * FROM [dbo].Accounts WHERE AccountID = @GroupOwnerAccountID) RAISERROR('GroupOwner_Invalid', 12, 1);


INSERT INTO dbo.Accounts (UserName, DisplayName, PasswordHash, LocationID, FirstName, LastName, SchoolEmail, ContactEmail, PhoneNumber, IsGroup) 
	VALUES (@GroupName, @DisplayName, null, @LocationID, null, null, @SchoolEmail, @ContactEmail, @PhoneNumber, 1);

SET @GroupID = SCOPE_IDENTITY();

INSERT INTO dbo.AccountGroupMap(AccountID, GroupID, IsGroupOwner, IsGroupAdmin, IsGroupFriend, IsPendingFriend, IsGroupFollower) 
	VALUES (@GroupOwnerAccountID, @GroupID, 1, 0, 0, 0, 0);

GO
