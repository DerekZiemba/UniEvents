/****** Object:  StoredProcedure [dbo].[sp_Location_Create]    Script Date: 2/14/2018 4:58:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET XACT_ABORT ON  
USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Locations_CreateOne]
   -- Add the parameters for the stored procedure here
   @Name VARCHAR(80) = NULL,
   @AddressLine VARCHAR(80) = NULL,
   @Locality VARCHAR(40) = NULL,
   @AdminDistrict VARCHAR(40) = NULL,
   @PostalCode VARCHAR(20) = NULL,	
   @CountryRegion varchar(40) = NULL,
   @Description varchar(140) = NULL,
   @Latitude6x int = NULL,
   @Longitude6x int = NULL,
   @ParentLocationID BIGINT = NULL OUTPUT,
   @LocationID BIGINT OUTPUT
AS
   -- SET NOCOUNT ON added to prevent extra result sets from
   -- interfering with SELECT statements.
SET NOCOUNT ON;


DECLARE @eLocationID BIGINT = NULL;
DECLARE @eParentLocationID BIGINT = NULL;

SELECT @eLocationID = loc.[LocationID], @eParentLocationID = loc.[ParentLocationID] 
FROM dbo.Locations AS loc 
WHERE ([Name] IS NULL AND @Name IS NULL OR [Name] = @Name)
   AND ([AddressLine] IS NULL AND @AddressLine IS NULL OR [AddressLine] = @AddressLine) 
   AND ([Locality] IS NULL AND @Locality IS NULL OR [Locality] = @Locality) 
   AND ([AdminDistrict] IS NULL AND @AdminDistrict IS NULL OR [AdminDistrict] = @AdminDistrict)
   AND ([PostalCode] IS NULL AND @PostalCode IS NULL OR [PostalCode] = @PostalCode) 
   AND ([CountryRegion] = @CountryRegion)
   AND ([Description] IS NULL AND @Description IS NULL OR [Description] = @Description);


IF @eLocationID > 0
   BEGIN
      SET @LocationID = @eLocationID;
      SET @ParentLocationID = @eParentLocationID;
   END
ELSE
   BEGIN
      IF @ParentLocationID IS NULL 
         BEGIN
            IF @AddressLine IS NOT NULL 
               BEGIN
                  SELECT TOP 1 @ParentLocationID = [LocationID] FROM dbo.Locations AS loc WHERE loc.AddressLine IS NULL AND loc.Locality = @Locality AND loc.AdminDistrict = @AdminDistrict AND loc.PostalCode = @PostalCode AND loc.CountryRegion = @CountryRegion;

                  IF @ParentLocationID IS NULL 
                     BEGIN
                        SELECT TOP 1 @ParentLocationID = [LocationID] FROM dbo.Locations AS loc WHERE loc.AddressLine IS NULL AND loc.Locality = @Locality AND loc.AdminDistrict = @AdminDistrict AND loc.CountryRegion = @CountryRegion;
                     END      
               END

            ELSE IF @AddressLine IS NULL AND @Locality IS NOT NULL
               BEGIN
                  SELECT TOP 1 @ParentLocationID = [LocationID] FROM dbo.Locations AS loc WHERE loc.AddressLine IS NULL AND loc.Locality IS NULL AND loc.AdminDistrict = @AdminDistrict AND loc.PostalCode = @PostalCode AND loc.CountryRegion = @CountryRegion;
               END
               IF @ParentLocationID IS NULL 
                  BEGIN
                     SELECT TOP 1 @ParentLocationID = [LocationID] FROM dbo.Locations AS loc WHERE loc.AddressLine IS NULL AND loc.Locality IS NULL AND loc.AdminDistrict = @AdminDistrict AND loc.CountryRegion = @CountryRegion;
                  END 

            ELSE IF @AddressLine IS NULL AND @Locality IS NULL AND @AdminDistrict IS NOT NULL
               BEGIN
                  SELECT TOP 1 @ParentLocationID = [LocationID] FROM dbo.Locations AS loc WHERE loc.AddressLine IS NULL AND loc.Locality IS NULL AND loc.AdminDistrict IS NULL AND loc.PostalCode = @PostalCode AND loc.CountryRegion = @CountryRegion;
               END
               IF @ParentLocationID IS NULL 
                  BEGIN
                     SELECT TOP 1 @ParentLocationID = [LocationID] FROM dbo.Locations AS loc WHERE loc.AddressLine IS NULL AND loc.Locality IS NULL AND loc.AdminDistrict IS NULL AND loc.CountryRegion = @CountryRegion;
                  END 
         END;


      INSERT INTO dbo.Locations ([ParentLocationID], [Name], [AddressLine], [Locality], [AdminDistrict], [PostalCode], [CountryRegion], [Latitude6x], [Longitude6x], [Description])
      VALUES (@ParentLocationID, @Name, @AddressLine, @Locality, @AdminDistrict, @PostalCode, @CountryRegion, @Latitude6x, @Longitude6x, @Description)

      SET @LocationID = SCOPE_IDENTITY()

   END

GO