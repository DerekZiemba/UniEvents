SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Account_Logins_Get]
	@UserName VARCHAR(20)
AS
SET NOCOUNT ON;

SELECT * FROM dbo.Logins AS logins 
WHERE (logins.UserName = @UserName)

GO
