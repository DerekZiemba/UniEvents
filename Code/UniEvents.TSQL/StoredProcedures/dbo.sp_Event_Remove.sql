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


IF NOT EXISTS (SELECT TOP 1 * FROM dbo.EventFeed WHERE EventID = @EventID AND AccountID = @AccountID) THROW 50000, 'Event_Not_Exist_Or_Insufficient_Permission', 1;

DELETE FROM dbo.EventRSVPs WHERE EventID = @EventID;
DELETE FROM dbo.EventTagMap WHERE EventID = @EventID;
DELETE FROM dbo.EventDetails WHERE EventID = @EventID;
DELETE FROM dbo.EventFeed WHERE EventID = @EventID AND AccountID = @AccountID;


GO
