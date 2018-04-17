SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET XACT_ABORT ON   

USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Tags_Create]
   @Name VARCHAR(50),
   @Description NVARCHAR(160),
   @TagID BIGINT = NULL OUTPUT 
AS
SET NOCOUNT ON;


INSERT INTO [dbo].Tags([Name], [Description]) 
   VALUES (@Name, @Description);

SET @TagID = SCOPE_IDENTITY();

GO
