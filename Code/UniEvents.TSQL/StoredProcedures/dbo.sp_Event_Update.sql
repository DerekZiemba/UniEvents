SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET XACT_ABORT ON  

USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Event_Update]
   @EventID BIGINT,
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


BEGIN TRANSACTION;
BEGIN TRY 

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

   COMMIT TRANSACTION;
END TRY  
BEGIN CATCH  
   ROLLBACK TRANSACTION;  
   THROW;
END CATCH 

GO