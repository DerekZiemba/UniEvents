SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Tags_GetOne]
	@TagID BIGINT = NULL,
	@Name VARCHAR(50) = NULL
AS
SET NOCOUNT ON;

DECLARE @bHasName bit = IIF(LEN(@Name) >= 2, 1, 0);

SELECT Top 1 * FROM dbo.Tags AS tags WHERE (@TagID IS NULL OR tags.TagID = @TagID) AND (@bHasName = 0 OR tags.Name LIKE '%' + @Name + '%'); 

GO
