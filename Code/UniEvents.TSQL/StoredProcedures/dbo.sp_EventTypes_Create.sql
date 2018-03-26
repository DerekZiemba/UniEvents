SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET XACT_ABORT ON   

USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_EventTypes_Create]
	@EventTypeID Int = NULL OUTPUT,
	@Name VARCHAR(50) = NULL,
	@Description NVARCHAR(400) = NULL
AS
SET NOCOUNT ON;

IF EXISTS (SELECT TOP 1 * FROM [dbo].EventTypes WHERE [Name] = @Name) THROW 50000, 'EventType_Already_Exists', 1;

INSERT INTO [dbo].EventTypes([Name], [Description]) 
	VALUES (@Name, @Description);

SET @EventTypeID = SCOPE_IDENTITY();

GO
