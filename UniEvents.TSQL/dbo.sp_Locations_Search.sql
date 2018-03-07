SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Derek Ziemba>
-- Create date: <2-12-2018>
-- Description:	Search for a location
-- =============================================
USE [dbUniHangouts]
GO
CREATE OR ALTER PROCEDURE dbo.sp_Locations_Search
	@Name varchar(80) = NULL, 
	@AddressLine varchar(80) = NULL,
	@Locality varchar(40) = NULL,
	@AdminDistrict varchar(40) = NULL,
	@PostalCode varchar(20) = NULL,
	@CountryRegion varchar(40) = NULL,
	@STName tinyint = 0,  	--QType: 0 = Exact, 1 = StartsWith, 2 = EndsWith, 3 = Contains. 
	@STAddressLine tinyint = 0,
	@STLocality tinyint = 0,
	@STAdminDistrict tinyint = 0,
	@STPostalCode tinyint = 0,
	@STCountryRegion tinyint = 0
AS
SET NOCOUNT ON;
DECLARE @bHasName bit = IIF(LEN(@Name) >= 3, 1, 0), 
		@bHasAddressLine bit = IIF(LEN(@AddressLine) >= 3, 1, 0), 
		@bHasLocality bit = IIF(LEN(@Locality) >= 3, 1, 0), 
		@bHasAdminDistrict bit = IIF(LEN(@AdminDistrict) >= 2, 1, 0), 
		@bHasPostalCode bit = IIF(LEN(@PostalCode) >= 3, 1, 0), 
		@bHasCountryRegion bit = IIF(LEN(@CountryRegion) >= 2, 1, 0)

DECLARE	@sName varchar = IIF(@STName & 1 > 0, '%', '') + @Name + IIF(@STName & 2 > 0, '%', ''),
		@sAddressLine varchar = IIF(@STAddressLine & 1 > 0, '%', '') + @AddressLine + IIF(@STAddressLine & 2 > 0, '%', ''),
		@sLocality varchar = IIF(@STLocality & 1 > 0, '%', '') + @Locality + IIF(@STLocality & 2 > 0, '%', ''),
		@sAdminDistrict varchar = IIF(@STAdminDistrict & 1 > 0, '%', '') + @AdminDistrict + IIF(@STAdminDistrict & 2 > 0, '%', ''),
		@sPostalCode varchar = IIF(@STPostalCode & 1 > 0, '%', '') + @PostalCode + IIF(@STPostalCode & 2 > 0, '%', ''),
		@sCountryRegion varchar = IIF(@STCountryRegion & 1 > 0, '%', '') + @CountryRegion + IIF(@STCountryRegion & 2 > 0, '%', '')

	SELECT * FROM dbo.Locations AS loc
	WHERE (@bHasName = 0 OR (@STName = 0 AND loc.Name = @Name) OR (@STName > 0 AND loc.Name LIKE @sName))
		AND (@bHasAddressLine = 0 OR (@STAddressLine = 0 AND loc.AddressLine = @AddressLine) OR (@STAddressLine > 0 AND loc.AddressLine LIKE @sAddressLine))
		AND (@bHasLocality = 0 OR (@STLocality = 0 AND loc.Locality = @Locality) OR (@STLocality > 0 AND loc.Locality LIKE @sLocality))
		AND (@bHasAdminDistrict = 0 OR (@STAdminDistrict = 0 AND loc.AdminDistrict = @AdminDistrict) OR (@STAdminDistrict > 0 AND loc.AdminDistrict LIKE @sAdminDistrict))
		AND (@bHasPostalCode = 0 OR (@STPostalCode = 0 AND loc.PostalCode = @PostalCode) OR (@STPostalCode > 0 AND loc.PostalCode LIKE @sPostalCode))
		AND (@bHasCountryRegion = 0 OR (@STCountryRegion  = 0 AND loc.CountryRegion  = @CountryRegion ) OR (@STCountryRegion > 0 AND loc.CountryRegion  LIKE @sCountryRegion))
	RETURN
GO
