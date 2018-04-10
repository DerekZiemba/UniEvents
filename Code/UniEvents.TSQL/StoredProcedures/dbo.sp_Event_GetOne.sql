SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET XACT_ABORT ON  

USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Event_Search]
   @EventID BIGINT
AS
SET NOCOUNT ON;


SELECT 
      feed.EventID,
      feed.EventTypeID,
      feed.AccountID,
      feed.LocationID,
      feed.DateStart,
      feed.DateEnd,
      feed.Title,
      feed.Caption,
      COUNT(DISTINCT CASE when rsvp.RSVPTypeId = 5 THEN rsvp.AccountID END) AS RSVP_Attending,
      COUNT(DISTINCT CASE when rsvp.RSVPTypeId = 4 THEN rsvp.AccountID END) AS RSVP_Later,
      COUNT(DISTINCT CASE WHEN rsvp.RSVPTypeId = 3 THEN rsvp.AccountID END) AS RSVP_StopBy,
      COUNT(DISTINCT CASE WHEN rsvp.RSVPTypeId = 2 THEN rsvp.AccountID END) AS RSVP_Maybe,
      COUNT(DISTINCT CASE WHEN rsvp.RSVPTypeId = 1 THEN rsvp.AccountID END) AS RSVP_No,
      STUFF((SELECT ', ' + CONVERT(VARCHAR(20), TagID) FROM dbo.EventTagMap WHERE EventID = feed.EventID FOR XML PATH('')), 1, 1, '') AS TagIds
   FROM dbo.EventFeed AS feed
   LEFT OUTER JOIN dbo.EventRSVPs AS rsvp ON feed.EventID = rsvp.EventID
   WHERE feed.EventID = @EventID
   GROUP BY feed.EventID, feed.EventTypeID, feed.AccountID, feed.LocationID, feed.DateStart, feed.DateEnd, feed.Title, feed.Caption


GO
