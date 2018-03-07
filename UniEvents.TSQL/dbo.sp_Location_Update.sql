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
CREATE OR ALTER PROCEDURE dbo.sp_Location_Update
	-- Add the parameters for the stored procedure here
	@LocationID bigint,
	@ParentLocationID bigint = NULL,
	@Name varchar(80) = NULL,
	@AddressLine varchar(80) = NULL,
	@Locality varchar(40) = NULL,
	@AdminDistrict varchar(40) = NULL,
	@PostalCode varchar(20) = NULL,
	@CountryRegion varchar(40),
	@Latitude real = NULL,
	@Longitude real = NULL,
	@Latitude6x int = NULL,
	@Longitude6x int = NULL,
	@Description varchar(140) = NULL
AS
SET NOCOUNT ON;

DECLARE @Lat int, @Lon int;
IF (@Latitude IS NOT NULL) SET @Lat = CAST((@Longitude*1000000) AS int);
IF (@Longitude IS NOT NULL) SET @Lon = CAST((@Longitude*1000000) AS int);
IF (@Latitude6x IS NOT NULL) SET @Lat = @Latitude6x;
IF (@Longitude6x IS NOT NULL) SET @Lon = @Longitude6x;


UPDATE dbo.Locations
	SET [ParentLocationID] = @ParentLocationID, 
		[Name] = @Name, 
		[AddressLine] = @AddressLine, 
		[Locality] = @Locality, 
		[AdminDistrict] = @AdminDistrict, 
		[PostalCode] = @PostalCode, 
		[CountryRegion] = @CountryRegion, 
		[Latitude6x] = @Lat, 
		[Longitude6x] = @Lon, 
		[Description] = @Description
WHERE [LocationID] = @LocationID

GO
