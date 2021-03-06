SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Tags_Search]
	@TagID BIGINT = NULL,
	@Name VARCHAR(50) = NULL,
	@Description NVARCHAR(160) = NULL
AS
SET NOCOUNT ON;

DECLARE @bHasName bit = IIF(LEN(@Name) >= 2, 1, 0), 
			@BHasDescription bit = IIF(LEN(@Description) >= 4, 1, 0);

If @TagID IS NOT NULL AND @bHasName = 0 AND @BHasDescription = 0 
   BEGIN
      SELECT * FROM dbo.Tags AS tags WHERE tags.TagID = @TagID
   END
ELSE
   BEGIN
      SELECT * FROM dbo.Tags AS tags WHERE (@bHasName = 0 OR tags.Name LIKE '%' + @Name + '%') AND (@BHasDescription = 0 OR tags.[Description] LIKE '%' + @Description + '%'); 
   END
 

GO
