SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

USE [dbUniHangouts]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Account_Create]
	@AccountID BIGINT OUTPUT,
	@LocationID BIGINT = NULL,
	@Password NVARCHAR(50) = NULL,
	@PasswordHash BINARY(256) = NULL,
	@UserName VARCHAR(20),
	@DisplayName NVARCHAR(50) = NULL,		
	@FirstName NVARCHAR(50) = NULL,
	@LastName NVARCHAR(50) = NULL,
	@SchoolEmail VARCHAR(50) = NULL,
	@ContactEmail VARCHAR(50) = NULL,
	@PhoneNumber VARCHAR(20) = NULL
AS
SET NOCOUNT ON;

IF @DisplayName IS NULL SET @DisplayName = @UserName;
IF @LocationID IS NOT NULL AND NOT EXISTS (SELECT TOP 1 * FROM [dbo].Locations WHERE LocationID = @LocationID) RAISERROR('Location_Invalid', 11, 1);
IF EXISTS (SELECT TOP 1 * FROM [dbo].Accounts WHERE UserName = @UserName) RAISERROR('UserName_Taken', 11, 1);

IF @Password IS NOT NULL AND @PasswordHash IS NULL SET @PasswordHash = HASHBYTES('SHA2_256', @Password + '.nevents');
IF @PasswordHash IS NULL RAISERROR('Password_Invalid', 11, 4);

INSERT INTO dbo.Accounts (UserName, DisplayName, PasswordHash, LocationID, FirstName, LastName, SchoolEmail, ContactEmail, PhoneNumber, IsGroup) 
	VALUES (@UserName, @DisplayName, @PasswordHash, @LocationID, @FirstName, @LastName, @SchoolEmail, @ContactEmail, @PhoneNumber, 0);

SET @AccountID = SCOPE_IDENTITY();

GO
