SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_RSVPTypes_Get]
AS
SET NOCOUNT ON;

SELECT * FROM dbo.RSVPTypes;

GO