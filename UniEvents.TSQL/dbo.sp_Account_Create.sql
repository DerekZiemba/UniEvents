SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Derek Ziemba>
-- Create date: <2-16-2018>
-- =============================================
USE [dbUniHangouts]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Account_Create]
	@UserName varchar(20),
	@Password nvarchar(40),
	@DisplayName nvarchar(50) = NULL,		
	@FirstName nvarchar(20) = NULL,
	@LastName nvarchar(20) = NULL,
	@ContactEmail varchar(50) = NULL,
	@PhoneNumber varchar(20) = NULL,
	@Description nvarchar(4000) = NULL,
	@LocationID bigint = NULL,
	@IsGroup bit = 0,
	@GroupOwnerAccountID bigint = NULL
AS
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
SET NOCOUNT ON;

IF (@IsGroup = 1)
	BEGIN
		
	END
ELSE
	BEGIN
	END