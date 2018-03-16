SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Account_Search]
	@UserName VARCHAR(20) = NULL,	
	@DisplayName NVARCHAR(50) = NULL,		
	@FirstName NVARCHAR(50) = NULL,
	@LastName NVARCHAR(50) = NULL,
	@Email VARCHAR(50) = NULL,
	@PhoneNumber VARCHAR(20) = NULL
AS
SET NOCOUNT ON;

DECLARE @bHasUserName BIT = IIF(LEN(@UserName) >= 3, 1, 0), 
			@bHasDisplayName BIT = IIF(LEN(@DisplayName) >= 3, 1, 0), 
			@bHasFirstName BIT = IIF(LEN(@FirstName) >= 1, 1, 0), 
			@bHasLastName BIT = IIF(LEN(@LastName) >= 1, 1, 0), 
			@bHasEmail BIT = IIF(LEN(@Email) >= 3, 1, 0), 
			@bHasPhoneNumber BIT = IIF(LEN(@PhoneNumber) >= 3, 1, 0);


SELECT * FROM dbo.Accounts AS acct 
WHERE		(@bHasUserName = 0 OR acct.UserName LIKE @UserName + '%')
	AND	(@bHasDisplayName = 0 OR acct.DisplayName LIKE @DisplayName + '%')
	AND	(@bHasFirstName = 0 OR acct.FirstName LIKE @FirstName + '%')
	AND	(@bHasLastName = 0 OR acct.LastName LIKE @LastName + '%')
	AND	(@bHasEmail = 0  OR acct.SchoolEmail LIKE @Email + '%' OR acct.ContactEmail LIKE @Email + '%')
	AND	(@bHasPhoneNumber = 0 OR acct.PhoneNumber = @PhoneNumber)

GO
