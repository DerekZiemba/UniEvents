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
AS
SET NOCOUNT ON;


INSERT INTO dbo.EventFeed (DateStart, DateEnd, AccountID, LocationID, Title, Caption) 
	VALUES (@DateEnd, @DateEnd, @AccountID, @LocationID, @Title, @Caption);

SET @EventID = SCOPE_IDENTITY();

Insert INTO dbo.EventDescription(EventId, Details)
	VALUES (@EventID, @Details)


GO
