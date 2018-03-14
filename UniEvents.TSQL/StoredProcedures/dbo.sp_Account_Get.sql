SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Account_Get]
	@AccountID BIGINT,
   @UserName VARCHAR(20) = NULL
AS
SET NOCOUNT ON;

SELECT TOP 1 * FROM dbo.Accounts AS acct WHERE (@AccountID = 0 OR acct.AccountID = @AccountID) AND (@UserName IS NULL OR acct.UserName = @UserName);

GO
