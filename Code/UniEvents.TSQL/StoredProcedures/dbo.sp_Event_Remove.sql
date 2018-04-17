SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET XACT_ABORT ON  

USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Event_Remove]
   @EventID BIGINT, 
   @AccountID BIGINT
AS
SET NOCOUNT ON;


IF NOT EXISTS (SELECT TOP 1 * FROM dbo.EventFeed WHERE EventID = @EventID AND AccountID = @AccountID) 
   AND NOT EXISTS(SELECT TOP 1 * FROM dbo.Accounts WHERE AccountID = @AccountID AND IsAdmin = 1) THROW 50000, 'Unauthorized. Must be creator or Admin.', 10;

BEGIN TRANSACTION;
BEGIN TRY 

   DELETE FROM dbo.EventRSVPs WHERE EventID = @EventID;
   DELETE FROM dbo.EventTagMap WHERE EventID = @EventID;
   DELETE FROM dbo.EventDetails WHERE EventID = @EventID;
   DELETE FROM dbo.EventFeed WHERE EventID = @EventID;

   COMMIT TRANSACTION;
END TRY  
BEGIN CATCH  
   ROLLBACK TRANSACTION;  
   THROW;
END CATCH 

GO
