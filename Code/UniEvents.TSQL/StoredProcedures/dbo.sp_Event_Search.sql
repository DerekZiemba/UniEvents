SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET XACT_ABORT ON  

USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Event_Search]
   @EventID BIGINT = NULL,
   @EventTypeID BIGINT = NULL,
   @AccountID BIGINT = NULL,
   @LocationID BIGINT = NULL,
   @DateFrom datetime = NULL,
   @DateTo datetime = NULL,
   @Title varchar(80) = NULL,
   @Caption nvarchar(160) = NULL
AS
SET NOCOUNT ON;

DECLARE @bHasTitle bit = IIF(LEN(@Title) >= 3, 1, 0), 
         @bHasCaption bit = IIF(LEN(@Caption) >= 3, 1, 0);


SELECT * FROM dbo.EventFeed AS feed
   WHERE (@EventID IS NULL OR feed.EventID = @EventID)
      AND(@EventTypeID IS NULL OR feed.EventTypeID = @EventTypeID)
      AND(@AccountID IS NULL OR feed.AccountID = @AccountID)
      AND(@LocationID IS NULL OR feed.LocationID = @LocationID)
      AND(@DateFrom IS NULL OR feed.DateStart > @DateFrom OR feed.DateEnd > @DateFrom)
      AND(@DateTo IS NULL OR feed.DateStart < @DateTo OR feed.DateEnd < @DateTo)
      AND (@bHasTitle = 0 OR feed.Title LIKE '%' + @Title + '%')
      AND (@bHasCaption = 0 OR feed.Caption LIKE '%' + @Caption + '%')


GO
