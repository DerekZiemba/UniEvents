SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET XACT_ABORT ON   

USE [dbUniHangouts]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Account_Create]
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

IF @LocationID IS NOT NULL AND NOT EXISTS (SELECT TOP 1 * FROM [dbo].Locations WHERE LocationID = @LocationID) Throw 50000, 'Location_Invalid', 1;
IF EXISTS (SELECT TOP 1 * FROM [dbo].Accounts WHERE UserName = @UserName) THROW 50000, 'UserName_Taken',1;

IF @PasswordHash IS NULL THROW 50000, 'Password_Invalid', 1;

INSERT INTO dbo.Accounts (UserName, DisplayName, PasswordHash, Salt, LocationID, FirstName, LastName, SchoolEmail, ContactEmail, PhoneNumber, IsGroup) 
	VALUES (@UserName, @DisplayName, @PasswordHash, @Salt, @LocationID, @FirstName, @LastName, @SchoolEmail, @ContactEmail, @PhoneNumber, 0);

SET @AccountID = SCOPE_IDENTITY();

GO
