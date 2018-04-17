SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET XACT_ABORT ON   

USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_EventTypes_Create]
	@EventTypeID Int = NULL OUTPUT,
	@Name VARCHAR(50),
	@Description NVARCHAR(400)
AS
SET NOCOUNT ON;


INSERT INTO [dbo].EventTypes([Name], [Description]) 
	VALUES (@Name, @Description);

SET @EventTypeID = SCOPE_IDENTITY();

GO
