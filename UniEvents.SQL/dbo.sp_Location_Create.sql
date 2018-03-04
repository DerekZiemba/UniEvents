/****** Object:  StoredProcedure [dbo].[sp_Location_Create]    Script Date: 2/14/2018 4:58:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
USE [dbUniHangouts]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Location_Create]
	-- Add the parameters for the stored procedure here
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
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
SET NOCOUNT ON;

DECLARE @Lat int = NULL, @Lon int = NULL;
IF (@Latitude IS NOT NULL) SET @Lat = CAST((@Longitude*1000000) AS int);
IF (@Longitude IS NOT NULL) SET @Lon = CAST((@Longitude*1000000) AS int);
IF (@Latitude6x IS NOT NULL) SET @Lat = @Latitude6x;
IF (@Longitude6x IS NOT NULL) SET @Lon = @Longitude6x;

DECLARE @eLocationID bigint,
		@eParentLocationID bigint = NULL,
		@eName varchar(80) = NULL,
		@eAddressLine varchar(80) = NULL,
		@eLocality varchar(40) = NULL,
		@eAdminDistrict varchar(40) = NULL,
		@ePostalCode varchar(20) = NULL,
		@eCountryRegion varchar(40),
		@eLatitude6x int = NULL,
		@eLongitude6x int = NULL,
		@eDescription varchar(140) = NULL

SELECT	@eLocationID = [LocationID], 
		@eParentLocationID = [ParentLocationID],
		@eName = [Name],
		@eAddressLine = [AddressLine],
		@eLocality = [Locality],
		@eAdminDistrict = [AdminDistrict],
		@ePostalCode = [PostalCode],
		@eCountryRegion = [CountryRegion],
		@eLatitude6x = [Latitude6x],
		@eLongitude6x = [Longitude6x],
		@eDescription = [Description]
	FROM dbo.Locations AS loc
	WHERE	loc.Name = @Name 
		AND loc.AddressLine = @AddressLine
		AND loc.Locality = @Locality
		AND loc.AdminDistrict = @AdminDistrict
		AND loc.PostalCode = @PostalCode
		AND loc.CountryRegion = @CountryRegion

IF @eLocationID > 0 
	BEGIN
		IF @eLatitude6x = NULL AND @Lat <> NULL SET @eLatitude6x = @Lat
		IF @eLongitude6x = NULL AND @Lon <> NULL SET @eLongitude6x = @Lon
		IF @eParentLocationID = NULL AND @ParentLocationID <> NULL SET @eParentLocationID = @ParentLocationID
		IF @eDescription = NULL AND @Description <> NULL SET @eDescription = @Description 

		UPDATE dbo.Locations
			SET [ParentLocationID] = @eParentLocationID, 
				[Name] = @eName, 
				[AddressLine] = @eAddressLine, 
				[Locality] = @eLocality, 
				[AdminDistrict] = @eAdminDistrict, 
				[PostalCode] = @ePostalCode, 
				[CountryRegion] = @eCountryRegion, 
				[Latitude6x] = @eLatitude6x, 
				[Longitude6x] = @eLongitude6x, 
				[Description] = @eDescription
		WHERE [LocationID] = @eLocationID
		RETURN @eLocationID
	END
ELSE
	BEGIN
		INSERT INTO dbo.Locations
			([ParentLocationID], [Name], [AddressLine], [Locality], [AdminDistrict], [PostalCode], [CountryRegion], [Latitude6x], [Longitude6x], [Description])
		VALUES
			(@ParentLocationID, @Name, @AddressLine, @Locality, @AdminDistrict, @PostalCode, @CountryRegion, @Lat, @Lon, @Description)
		RETURN SCOPE_IDENTITY()
	END
