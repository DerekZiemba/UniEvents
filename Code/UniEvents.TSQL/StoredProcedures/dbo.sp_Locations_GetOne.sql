SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Derek Ziemba>
-- Create date: <2-12-2018>
-- =============================================
USE [dbUniHangouts]
GO
CREATE OR ALTER PROCEDURE dbo.sp_Locations_GetOne
	@LocationID bigint
AS
SET NOCOUNT ON;
	SELECT TOP(1) * FROM dbo.Locations AS loc WHERE loc.LocationID = @LocationID
GO
