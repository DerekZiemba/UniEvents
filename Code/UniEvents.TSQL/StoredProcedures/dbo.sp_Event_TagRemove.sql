SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET XACT_ABORT ON  

USE [$(dbUniHangouts)]
GO
CREATE OR ALTER PROCEDURE [dbo].[sp_Event_TagRemove]
   @EventID BIGINT,
   @TagID BIGINT
AS

DELETE FROM dbo.EventTagMap WHERE EventID = @EventID AND TagID = @TagID;

GO
