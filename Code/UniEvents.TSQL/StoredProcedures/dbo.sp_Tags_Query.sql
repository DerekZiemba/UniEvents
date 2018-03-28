SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Tags_Query]
	@Query VARCHAR(50) = NULL
AS
SET NOCOUNT ON;

DECLARE @len TINYINT = LEN(@Query);

SELECT * FROM dbo.Tags AS tags WHERE (@len >= 2 AND tags.Name LIKE @Query + '%') OR (@len > 3 AND tags.[Description] LIKE '%' + @Query + '%'); 

GO
