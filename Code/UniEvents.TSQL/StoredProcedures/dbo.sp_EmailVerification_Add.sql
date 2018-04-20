SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET XACT_ABORT ON   

USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_EmailVerification_Add]
   @AccountID BIGINT,
   @VerificationKey varchar(50),
   @VerificationHash BINARY(32),
   @Email varchar(50),
   @Date smalldatetime
AS
SET NOCOUNT ON;

INSERT INTO dbo.EmailVerification (AccountID, VerificationKey, VerificationHash, Email, [Date], IsVerified) 
   VALUES (@AccountID, @VerificationKey, @VerificationHash, @Email, @Date, 0);

GO
