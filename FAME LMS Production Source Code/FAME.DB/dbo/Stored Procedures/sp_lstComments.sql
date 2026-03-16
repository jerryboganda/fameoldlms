-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_lstComments 0 , 0
-- =============================================
CREATE PROCEDURE [dbo].[sp_lstComments]
@QuestionID int,
@VideoID int,
@IsApproved bit
AS
BEGIN

Select c.* ,u.User_Name,'/Images/' + ISNULL( u.User_Pic,'userLogo.jpg' ) AS User_Pic

from tbl_Comment c
Left join tbl_User u on u.User_AspUser = c.UserFid

where (c.QuestionID = @QuestionID OR @QuestionID = 0 )
AND (c.Video_Fid = @VideoID OR @VideoID = 0 )
AND (c.IsApproved = @IsApproved OR @IsApproved IS NULL )

END