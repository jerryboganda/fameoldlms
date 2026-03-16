-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_SearchContent '1'
-- =============================================
CREATE PROCEDURE [dbo].[sp_SearchContent]
@Text varchar(max)
AS
BEGIN

Select * from vw_Content c
Where c.Name LIKE '%'+@Text+'%'

END