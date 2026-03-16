-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_lstVideoFaqs 397
-- =============================================
CREATE PROCEDURE [dbo].[sp_lstVideoFaqs]
@VideoID int 
AS
BEGIN

Select q.* ,c.Master_Value As Category
FROM tbl_Question q
Left join tbl_Master c on c.Master_ID = q.CategoryID
Where ( q.VideoID = @VideoID)

END