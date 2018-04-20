SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET XACT_ABORT ON  

USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Event_RSVP_Get]
   @EventID BIGINT,
   @AccountID BIGINT
AS
SET NOCOUNT ON;


(SELECT TOP 1 * FROM [dbo].EventRSVPs WHERE EventID = @EventID AND AccountID = @AccountID)


GO