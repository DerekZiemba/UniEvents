SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET XACT_ABORT ON  

USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Event_RSVP_AddOrUpdate]
   @EventID BIGINT,
   @AccountID BIGINT,
   @RSVPTypeID SMALLINT
AS
SET NOCOUNT ON;

IF NOT EXISTS (SELECT TOP 1 * FROM [dbo].EventFeed WHERE EventID = @EventID) THROW 50000, 'Event_Not_Exists', 1;
IF NOT EXISTS (SELECT TOP 1 * FROM [dbo].Accounts WHERE AccountID = @AccountID) THROW 50000, 'Account_Not_Exists', 1;
IF NOT EXISTS (SELECT TOP 1 * FROM [dbo].RSVPTypes WHERE RSVPTypeID = @RSVPTypeID) THROW 50000, 'RSVP_Not_Exists', 1;

IF EXISTS (SELECT TOP 1 * FROM [dbo].EventRSVPs WHERE EventID = @EventID AND AccountID = @AccountID)
   BEGIN
      UPDATE dbo.EventRSVPs SET RSVPTypeId = @RSVPTypeID WHERE EventID = @EventID AND AccountID = @AccountID
   END 
ELSE 
   BEGIN
      INSERT INTO dbo.EventRSVPs(EventID, AccountID, RSVPTypeId) VALUES (@EventID, @AccountID, @RSVPTypeID);
   END

GO
