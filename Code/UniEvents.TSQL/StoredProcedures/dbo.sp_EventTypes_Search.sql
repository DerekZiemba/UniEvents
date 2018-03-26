SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_EventTypes_Search]
	@EventTypeID Int = NULL,
	@Name VARCHAR(50) = NULL,
	@Description NVARCHAR(400) = NULL
AS
SET NOCOUNT ON;

DECLARE @bHasName bit = IIF(LEN(@Name) >= 2, 1, 0), 
			@BHasDescription bit = IIF(LEN(@Description) >= 4, 1, 0);


SELECT * FROM dbo.EventTypes AS tags 
WHERE (@EventTypeID IS NULL OR tags.EventTypeID = @EventTypeID)
	AND (@bHasName = 0 OR tags.Name LIKE '%' + @Name + '%')
	AND (@BHasDescription = 0 OR tags.[Description] LIKE '%' + @Description + '%'); 

GO
