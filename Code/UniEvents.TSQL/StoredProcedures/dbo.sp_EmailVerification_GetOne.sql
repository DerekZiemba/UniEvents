SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET XACT_ABORT ON   

USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_EmailVerification_GetOne]
   @AccountID BIGINT,
   @VerificationKey varchar(50) = null,
   @Email varchar(50) = null
AS
SET NOCOUNT ON;



SELECT TOP 1 * FROM dbo.EmailVerification
WHERE AccountID = @AccountID 
   AND (@VerificationKey IS NULL OR VerificationKey = @VerificationKey)
   AND (@Email IS NULL OR Email = @Email);

GO
