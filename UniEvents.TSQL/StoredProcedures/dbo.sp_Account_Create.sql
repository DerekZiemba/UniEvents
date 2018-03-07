SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Derek Ziemba>
-- Create date: <2-16-2018>
-- =============================================
USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Account_Create]
	@UserName VARCHAR(20),
	@Password NVARCHAR(50) = NULL,
	@PasswordHash BINARY(256) = NULL,
	@LocationID BIGINT,
	@DisplayName NVARCHAR(50) = NULL,		
	@FirstName NVARCHAR(20) = NULL,
	@LastName NVARCHAR(20) = NULL,
	@ContactEmail VARCHAR(50) = NULL,
	@PhoneNumber VARCHAR(20) = NULL,
	@Description NVARCHAR(4000) = NULL,	
	@GroupOwnerAccountID BIGINT = NULL
AS
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
SET NOCOUNT ON;

IF EXISTS (SELECT TOP 1 * FROM [dbo].Accounts WHERE UserName = @UserName) RAISERROR('UserName_Taken', 11, 1);

IF NOT EXISTS (SELECT  TOP 1 * FROM [dbo].Locations WHERE LocationID = @LocationID) RAISERROR('Location_Invalid', 12, 2);

IF @GroupOwnerAccountID > 0
	BEGIN
		IF NOT EXISTS (SELECT TOP 1 * FROM [dbo].Accounts WHERE AccountID = @GroupOwnerAccountID) RAISERROR('GroupOwner_Invalid', 13, 3);

		INSERT INTO dbo.Accounts (UserName, DisplayName, PasswordHash, IsGroup) VALUES (@UserName, @DisplayName, @PasswordHash, 1);
		DECLARE @GroupID BIGINT = SCOPE_IDENTITY();

		INSERT INTO [dbo].AccountDetails(AccountID, LocationID, FirstName, LastName, ContactEmail, PhoneNumber, [Description]) 
			VALUES (@GroupID, @LocationID, @FirstName, @LastName, @ContactEmail, @PhoneNumber, @Description);

		INSERT INTO dbo.MapAccountsAndGroups(AccountID, GroupID, IsGroupOwner, IsGroupAdmin, IsGroupFriend, IsPendingFriend, IsGroupFollower) VALUES 
			(@GroupOwnerAccountID, @GroupID, 1, 0, 0, 0, 0);

		RETURN @GroupID;

	END
ELSE
	BEGIN
		IF @Password IS NOT NULL AND @PasswordHash IS NULL SET @PasswordHash = HASHBYTES('SHA2_256', @Password + '.nevents');
		IF @PasswordHash IS NULL RAISERROR('Password_Invalid', 11, 4);

		INSERT INTO dbo.Accounts (UserName, DisplayName, PasswordHash, IsGroup) VALUES (@UserName, @DisplayName, @PasswordHash, 0);
		DECLARE @AccountID bigint = SCOPE_IDENTITY();

		INSERT INTO [dbo].AccountDetails(AccountID, LocationID, FirstName, LastName, ContactEmail, PhoneNumber, [Description]) 
			VALUES (@AccountID, @LocationID, @FirstName, @LastName, @ContactEmail, @PhoneNumber, @Description);

		RETURN @AccountID;
	END

GO