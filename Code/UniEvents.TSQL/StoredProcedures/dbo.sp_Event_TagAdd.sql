SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET XACT_ABORT ON  

USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Event_TagAdd]
   @EventID BIGINT,
   @TagID BIGINT
AS
SET NOCOUNT ON;

IF NOT EXISTS (SELECT TOP 1 * FROM [dbo].EventFeed WHERE EventID = @EventID) THROW 50000, 'Event_Not_Exists', 1;
IF NOT EXISTS (SELECT TOP 1 * FROM [dbo].Tags WHERE TagID = @TagID) THROW 50000, 'Tag_Not_Exists', 1;
IF EXISTS (SELECT TOP 1 * FROM [dbo].EventTagMap WHERE EventID = @EventID AND TagID = @TagID) THROW 50000, 'Duplicate_Tag', 1;


INSERT INTO dbo.EventTagMap(EventID, TagID) VALUES (@EventID, @TagID);


GO
