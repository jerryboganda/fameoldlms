-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- sp_lstFavourites '',''
-- =============================================
CREATE PROCEDURE [dbo].[sp_lstFavourites]
@StudentID varchar(max),
@Type varchar(max)
AS
BEGIN

Select * from (
	Select q.Question ,u.User_Name,f.*
	from tbl_FavouriteList f
	left join tbl_Question q on q.Question_Id = f.Object_ID
	AND f.ObjectType =( CASE WHEN q.Type = 1 THEN 'Mcq' ELSE CASE WHEN  q.Type = 2 THEN 'Short'  ELSE CASE WHEN  q.Type = 3 THEN  'Long' ELSE '' END END END)
	left join tbl_User u on u.User_AspUser = f.Student_Fid

	UNION ALL

	Select v.Video_Name ,u.User_Name,f.*
	from tbl_FavouriteList f
	left join tbl_Video v on v.Video_Id = f.Object_ID 
	AND f.ObjectType = (CASE WHEN v.Type = 1 THEN 'Video' ELSE CASE WHEN  v.Type = 2 THEN 'Audio'  ELSE CASE WHEN  v.Type = 3 THEN 'File' ELSE '' END END END)
	left join tbl_User u on u.User_AspUser = f.Student_Fid
)dt
Where (dt.Student_Fid = @StudentID OR @StudentID = '')
AND (dt.ObjectType = @Type OR @Type = '')
END