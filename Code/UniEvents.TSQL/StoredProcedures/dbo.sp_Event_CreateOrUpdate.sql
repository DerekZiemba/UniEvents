SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET XACT_ABORT ON  

USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Event_CreateOrUpdate]
   @EventID BIGINT = NULL OUTPUT,
   @EventTypeID BIGINT,
   @DateStart datetime,
   @DateEnd datetime,
   @AccountID BIGINT,
   @LocationID BIGINT,
   @Title varchar(80),
   @Caption nvarchar(160),
   @Details nvarchar(max) = NULL
AS
SET NOCOUNT ON;

IF NOT EXISTS (SELECT TOP 1 * FROM [dbo].EventTypes WHERE EventTypeID = @EventTypeID) THROW 50000, 'EventType_Not_Exists', 1;
IF NOT EXISTS (SELECT TOP 1 * FROM [dbo].Accounts WHERE AccountID = @AccountID) THROW 50000, 'Account_Not_Exists', 1;
IF NOT EXISTS (SELECT TOP 1 * FROM [dbo].Locations WHERE LocationID = @LocationID) THROW 50000, 'Location_Not_Exists', 1;

IF @EventID IS NOT NULL
   BEGIN
      IF NOT EXISTS (SELECT TOP 1 * FROM [dbo].EventFeed WHERE EventID = @EventID) THROW 50000, 'Event_Not_Exists', 1;
      IF NOT EXISTS (SELECT TOP 1 * FROM [dbo].EventFeed WHERE EventID = @EventID AND AccountID = @AccountID) THROW 50000, 'Account_Not_Creator', 1;

      UPDATE dbo.EventFeed 
         SET EventTypeID = @EventTypeID, DateStart = @DateStart, DateEnd = @DateEnd, LocationID = @LocationID, Title = @Title, Caption = @Caption
         WHERE EventID = @EventID;

      IF EXISTS (SELECT TOP 1 * FROM [dbo].EventDetails WHERE EventID = @EventID) 
         BEGIN
            UPDATE dbo.EventDetails SET Details = @Details WHERE EventID = @EventID;
         END
      ELSE
         BEGIN
            INSERT INTO dbo.EventDetails(EventId, Details) VALUES (@EventID, @Details);
         END
   END
ELSE
   BEGIN
      INSERT INTO dbo.EventFeed (EventTypeID, DateStart, DateEnd, AccountID, LocationID, Title, Caption) 
         VALUES (@EventTypeID, @DateStart, @DateEnd, @AccountID, @LocationID, @Title, @Caption);

      SET @EventID = SCOPE_IDENTITY();

      INSERT INTO dbo.EventDetails(EventId, Details) VALUES (@EventID, @Details);
   END

GO
