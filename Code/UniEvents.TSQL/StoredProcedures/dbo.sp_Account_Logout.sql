SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET XACT_ABORT ON  
USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Account_Logout]
	@UserName VARCHAR(20),
	@APIKey BINARY(256) = NULL --If Null, removes all logins
AS
SET NOCOUNT ON;

DELETE FROM dbo.Logins 
WHERE [UserName] = @UserName AND (@APIKey IS NULL OR [APIKey] = @APIKey);

GO
