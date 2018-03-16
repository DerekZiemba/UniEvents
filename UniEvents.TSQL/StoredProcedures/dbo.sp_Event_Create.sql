SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

USE [dbUniHangouts]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Event_Create]
	@EventID BIGINT OUTPUT,
	@EventTypeID int = NULL,
	@DateStart datetime = NULL,
	@DateEnd datetime = NULL,
	@AccountID BIGINT = NULL,
	@LocationID BIGINT = NULL,
	@Title varchar(80) = NULL,
	@Caption nvarchar(160) = NULL,
	@Details nvarchar(max) = NULL
AS
SET NOCOUNT ON;


INSERT INTO dbo.EventFeed (DateStart, DateEnd, AccountID, LocationID, Title, Caption) 
	VALUES (@DateEnd, @DateEnd, @AccountID, @LocationID, @Title, @Caption);

SET @EventID = SCOPE_IDENTITY();

Insert INTO dbo.EventDescription(EventId, Details)
	VALUES (@EventID, @Details)


	
GO
