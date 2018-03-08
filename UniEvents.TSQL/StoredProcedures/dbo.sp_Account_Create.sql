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
CREATE OR ALTER PROCEDURE [dbo].[sp_Account_Create]
	@UserName VARCHAR(20),
	@Password NVARCHAR(50) = NULL,
	@PasswordHash BINARY(256) = NULL,
	@DisplayName NVARCHAR(50) = NULL,		
	@FirstName NVARCHAR(20) = NULL,
	@LastName NVARCHAR(20) = NULL,
	@ContactEmail VARCHAR(50) = NULL,
	@PhoneNumber VARCHAR(20) = NULL,
	@Description NVARCHAR(4000) = NULL,
	@LocationID BIGINT = NULL,
	@AccountID BIGINT OUTPUT
AS
SET NOCOUNT ON;

IF @DisplayName IS NULL SET @DisplayName = @UserName;
IF @LocationID IS NOT NULL AND NOT EXISTS (SELECT TOP 1 * FROM [dbo].Locations WHERE LocationID = @LocationID) RAISERROR('Location_Invalid', 11, 1);
IF EXISTS (SELECT TOP 1 * FROM [dbo].Accounts WHERE UserName = @UserName) RAISERROR('UserName_Taken', 11, 1);

IF @Password IS NOT NULL AND @PasswordHash IS NULL SET @PasswordHash = HASHBYTES('SHA2_256', @Password + '.nevents');
IF @PasswordHash IS NULL RAISERROR('Password_Invalid', 11, 4);

INSERT INTO dbo.Accounts (UserName, DisplayName, PasswordHash, IsGroup) 
	VALUES (@UserName, @DisplayName, @PasswordHash, 0);

SET @AccountID = SCOPE_IDENTITY();

INSERT INTO [dbo].AccountDetails(AccountID, LocationID, FirstName, LastName, ContactEmail, PhoneNumber, [Description]) 
	VALUES (@AccountID, @LocationID, @FirstName, @LastName, @ContactEmail, @PhoneNumber, @Description);

GO
