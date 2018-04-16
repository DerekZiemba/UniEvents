SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET XACT_ABORT ON  

USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Event_GetById]
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
      STUFF((SELECT ', ' + CONVERT(VARCHAR(20), TagID) FROM dbo.EventTagMap WHERE EventID = feed.EventID FOR XML PATH('')), 1, 1, '') AS TagIds,
      acc.DisplayName as UserDisplayName,
      acc.UserName,
      loc.ParentLocationID,
      loc.LocationID,
      loc.[Name] as LocationName,
      loc.AddressLine,
      loc.Locality,
      loc.PostalCode,
      loc.AdminDistrict,
      loc.CountryRegion,
      deets.Details
   FROM dbo.EventFeed AS feed
      LEFT OUTER JOIN dbo.EventRSVPs AS rsvp ON feed.EventID = rsvp.EventID
      LEFT OUTER JOIN dbo.Accounts AS acc ON feed.AccountID = acc.AccountID
      LEFT OUTER JOIN dbo.Locations AS loc ON feed.LocationID = loc.LocationID
      LEFT OUTER JOIN dbo.EventDetails as deets ON feed.EventID = deets.EventID
   WHERE feed.EventID = @EventID
   GROUP BY feed.EventID, feed.EventTypeID, feed.AccountID, feed.LocationID, feed.DateStart, feed.DateEnd, feed.Title, feed.Caption, 
      acc.DisplayName, acc.UserName,
      loc.CountryRegion, loc.AdminDistrict, loc.PostalCode, loc.Locality, loc.[Name], loc.AddressLine, loc.LocationID, loc.ParentLocationID,
      deets.Details
   ORDER BY feed.DateStart;


GO
