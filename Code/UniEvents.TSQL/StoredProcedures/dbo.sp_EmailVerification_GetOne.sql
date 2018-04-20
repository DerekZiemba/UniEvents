SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET XACT_ABORT ON   

USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_EmailVerification_GetOne]
   @AccountID BIGINT,
   @VerificationKey varchar(50)
AS
SET NOCOUNT ON;

SELECT TOP 1 * FROM dbo.EmailVerification WHERE AccountID = @AccountID AND VerificationKey = @VerificationKey;

GO
