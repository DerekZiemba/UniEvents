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
      loc.[Name] as LocationName,
      loc.AddressLine,
      loc.Locality,
      loc.PostalCode,
      loc.AdminDistrict,
      loc.CountryRegion
   FROM dbo.EventFeed AS feed
   LEFT OUTER JOIN dbo.EventRSVPs AS rsvp ON feed.EventID = rsvp.EventID
   LEFT OUTER JOIN dbo.Accounts AS acc ON feed.AccountID = acc.AccountID
   LEFT OUTER JOIN dbo.Locations AS loc ON feed.LocationID = loc.LocationID
   WHERE (@EventID IS NULL OR feed.EventID = @EventID)
      AND(@EventTypeID IS NULL OR feed.EventTypeID = @EventTypeID)
      AND(@AccountID IS NULL OR feed.AccountID = @AccountID)
      AND(@LocationID IS NULL OR feed.LocationID = @LocationID)
      AND(@DateFrom IS NULL OR feed.DateStart > @DateFrom OR feed.DateEnd > @DateFrom)
      AND(@DateTo IS NULL OR feed.DateStart < @DateTo OR feed.DateEnd < @DateTo)
      AND (@bHasTitle = 0 OR feed.Title LIKE '%' + @Title + '%')
      AND (@bHasCaption = 0 OR feed.Caption LIKE '%' + @Caption + '%')
   GROUP BY feed.EventID, feed.EventTypeID, feed.AccountID, feed.LocationID, feed.DateStart, feed.DateEnd, feed.Title, feed.Caption, 
      acc.DisplayName, acc.UserName,
      loc.CountryRegion, loc.AdminDistrict, loc.PostalCode, loc.Locality, loc.[Name], loc.AddressLine


GO
