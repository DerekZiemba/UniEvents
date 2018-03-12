SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Account_Login]
	@UserName VARCHAR(20),
	@Password NVARCHAR(50) = NULL,
	@PasswordHash BINARY(256) = NULL,
	
	@AccountID BIGINT OUTPUT,
	@DisplayName NVARCHAR(50) OUTPUT
AS
SET NOCOUNT ON;

IF @Password IS NOT NULL AND @PasswordHash IS NULL SET @PasswordHash = HASHBYTES('SHA2_256', @Password + '.nevents');
IF @PasswordHash IS NULL RAISERROR('Password_Invalid', 11, 4);

SELECT TOP 1 @DisplayName = [DisplayName], @AccountID = [AccountID]
	FROM dbo.Accounts AS acct 
	WHERE acct.UserName = @UserName AND acct.PasswordHash = @PasswordHash;

GO
