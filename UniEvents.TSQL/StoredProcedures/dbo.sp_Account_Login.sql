SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Account_Login]
	@AccountID BIGINT,
	@UserName VARCHAR(20),
	@APIKey BINARY(256),
	@LoginDate DATETIME
AS
SET NOCOUNT ON;


INSERT INTO dbo.Logins (AccountID, UserName, APIKey, LoginDate)
	VALUES (@AccountID, @UserName, @APIKey, @LoginDate);
GO
