SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET XACT_ABORT ON  
USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Account_Login]
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
