-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_lstApproveComments 'All'
-- =============================================
CREATE PROCEDURE [dbo].[sp_lstApproveComments]
@IsApproved varchar(50),
@Text varchar(50)
AS
BEGIN

Select c.* ,u.User_Name,'/Images/' + ISNULL( u.User_Pic,'userLogo.jpg' ) AS User_Pic,q.Question,v.Video_Name,
		s.Section_Name , crs.Course_Name , ua.User_Name AS ApprovedByName
from tbl_Comment c
Left join tbl_User u on u.User_AspUser = c.UserFid
Left Join tbl_Question q on q.Question_Id = c.QuestionID
Left Join tbl_Video v on v.Video_Id = c.Video_Fid
Left Join tbl_Section s on s.Section_ID = (CASE WHEN q.Section_Fid IS NULL THEN v.Section_Fid ELSE q.Section_Fid END)
Left Join tbl_Courses crs on crs.Course_Id = s.Course_Fid
Left Join tbl_User ua on ua.User_AspUser = c.ApprovedBy
where (c.IsApproved = 'true' AND @IsApproved = 'Approved' )
OR (c.IsApproved = 'false' AND @IsApproved = 'UnApproved' )
OR (c.IsApproved IS NULL AND @IsApproved = 'Pending' )
OR (@IsApproved = 'All' )

END