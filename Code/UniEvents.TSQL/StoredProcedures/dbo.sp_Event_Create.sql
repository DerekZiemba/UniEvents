SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET XACT_ABORT ON  

USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Event_Create]
   @EventID BIGINT = NULL OUTPUT,
   @EventTypeID BIGINT,
   @DateStart datetime,
   @DateEnd datetime,
   @AccountID BIGINT,
   @LocationID BIGINT,
   @Title varchar(80),
   @Caption nvarchar(160),
   @Details nvarchar(max) = NULL
   --,  @TagIDs varchar(400)
AS
SET NOCOUNT ON;


BEGIN TRANSACTION;
BEGIN TRY 

   IF EXISTS(SELECT TOP 1 * FROM dbo.EventFeed WHERE EventTypeID = @EventTypeID AND DateStart = @DateStart AND DateEnd = @DateEnd AND AccountID = @AccountID AND LocationID = @LocationID AND Title = @Title) THROW 50000, 'Duplicate Event.', 10;


   INSERT INTO dbo.EventFeed (EventTypeID, DateStart, DateEnd, AccountID, LocationID, Title, Caption) 
      VALUES (@EventTypeID, @DateStart, @DateEnd, @AccountID, @LocationID, @Title, @Caption);

   SET @EventID = SCOPE_IDENTITY();

   INSERT INTO dbo.EventDetails(EventId, Details) VALUES (@EventID, @Details);



   COMMIT TRANSACTION;
END TRY  
BEGIN CATCH  
   ROLLBACK TRANSACTION;  
   THROW;
END CATCH 


GO