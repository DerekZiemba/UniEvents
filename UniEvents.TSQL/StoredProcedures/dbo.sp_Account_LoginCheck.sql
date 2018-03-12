SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Account_LoginCheck]
	@UserName VARCHAR(20),
	@APIKey BINARY(256)
AS
SET NOCOUNT ON;

SELECT TOP 1 * FROM dbo.Logins AS logins 
WHERE (logins.UserName = @UserName AND logins.APIKey = APIKey)

GO
