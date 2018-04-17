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

   INSERT INTO dbo.EventFeed (EventTypeID, DateStart, DateEnd, AccountID, LocationID, Title, Caption) 
      VALUES (@EventTypeID, @DateStart, @DateEnd, @AccountID, @LocationID, @Title, @Caption);

   SET @EventID = SCOPE_IDENTITY();

   INSERT INTO dbo.EventDetails(EventId, Details) VALUES (@EventID, @Details);


   --I have no idea how to do this. 

   --INSERT INTO dbo.EventTagMap(EventID, TagID)
   --   SELECT @EventID, VALUE
   --   FROM (SELECT CAST(TRIM(value) AS BIGINT) AS ID FROM STRING_SPLIT(@TagIDs, ';'));


   --SELECT EventID = @EventID, CAST(TRIM(value) AS BIGINT) as TagID
   --INTO dbo.EventTagMap
   --FROM STRING_SPLIT(@TagIDs, ';');


   COMMIT TRANSACTION;
END TRY  
BEGIN CATCH  
   ROLLBACK TRANSACTION;  
   THROW;
END CATCH 


GO