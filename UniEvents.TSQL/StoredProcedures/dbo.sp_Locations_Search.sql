SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Derek Ziemba>
-- Create date: <2-12-2018>
-- Description:	Search for a location
-- =============================================
USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE dbo.sp_Locations_Search
	@ParentLocationID bigint = NULL,
	@Name varchar(80) = NULL, 
	@AddressLine varchar(80) = NULL,
	@Locality varchar(40) = NULL,
	@AdminDistrict varchar(40) = NULL,
	@PostalCode varchar(20) = NULL,
	@CountryRegion varchar(40) = NULL,
	@Description NVARCHAR(80) = NULL
AS
SET NOCOUNT ON;
DECLARE @bHasName bit = IIF(LEN(@Name) >= 3, 1, 0), 
			@bHasAddressLine bit = IIF(LEN(@AddressLine) >= 3, 1, 0), 
			@bHasLocality bit = IIF(LEN(@Locality) >= 3, 1, 0), 
			@bHasAdminDistrict bit = IIF(LEN(@AdminDistrict) >= 2, 1, 0), 
			@bHasPostalCode bit = IIF(LEN(@PostalCode) >= 3, 1, 0), 
			@bHasCountryRegion bit = IIF(LEN(@CountryRegion) >= 2, 1, 0),
			@bHasDescription BIT = IIF(LEN(@Description) >= 5, 1, 0)

SELECT * FROM dbo.Locations AS loc
	WHERE (@ParentLocationID IS NULL OR loc.ParentLocationID = @ParentLocationID)
		AND (@bHasName = 0 OR loc.Name LIKE '%' + @Name + '%')
		AND (@bHasAddressLine = 0 OR loc.AddressLine LIKE '%' + @AddressLine + '%')
		AND (@bHasLocality = 0 OR loc.Locality LIKE '%' + @Locality + '%')
		AND (@bHasAdminDistrict = 0 OR loc.AdminDistrict LIKE @AdminDistrict + '%')
		AND (@bHasPostalCode = 0 OR loc.PostalCode LIKE @PostalCode + '%')
		AND (@bHasCountryRegion = 0 OR loc.CountryRegion LIKE @CountryRegion)
		AND (@bHasDescription = 0 OR loc.[Description] LIKE '%' + @Description + '%')

GO
