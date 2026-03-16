--[dbo].[sp_GetCoursesByStudent]'74f75f81-d4db-4d01-8af4-56f9c405f292'
CREATE PROCEDURE [dbo].[sp_GetCoursesByStudent]

@StudentID nvarchar(128)
as
BEGIN

select c.Course_Id
from tbl_Courses c 
join tbl_Section s on s.Course_Fid = c.Course_Id
join tbl_Enrollment e on e.Section_Fid = s.Section_ID
where e.StudentFid=@StudentID

end