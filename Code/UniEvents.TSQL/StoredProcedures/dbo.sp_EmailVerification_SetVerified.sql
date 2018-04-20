SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET XACT_ABORT ON   

USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_EmailVerification_SetVerified]
   @AccountID BIGINT,
   @VerificationKey varchar(50),
   @IsVerified bit
AS
SET NOCOUNT ON;

Update dbo.EmailVerification SET IsVerified = @IsVerified WHERE AccountID = @AccountID AND VerificationKey = @VerificationKey;

GO
