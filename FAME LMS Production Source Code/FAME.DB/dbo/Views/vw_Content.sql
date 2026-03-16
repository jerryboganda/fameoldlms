




CREATE VIEW [dbo].[vw_Content]
AS
SELECT 'Video' As Type, '/Student/TakeCourse/' + CONVERT(varchar(max),v.Course_Fid) + '?v=' + CONVERT(varchar(max),v.Video_Id)+ '&s=' + CONVERT(varchar(max),v.Section_Fid)   As Url ,
		v.Video_Id AS ID , v.Video_Name AS Name,c.Course_Name + '<span class="jznav"> > </span>' + s.Section_Name as Navigation
FROM   tbl_Video v
LEFT JOIN tbl_Courses c on c.Course_Id = v.Course_Fid 
LEFT JOIN tbl_Section s on s.Section_ID = v.Section_Fid 

Union ALL

SELECT 'Section' As Type, '/Student/TakeCourse/' + CONVERT(varchar(max),v.Course_Fid)+ '?s=' + CONVERT(varchar(max),v.Section_ID) As Url ,
		v.Section_ID AS ID , v.Section_Name AS Name,c.Course_Name  As Navigation
FROM   tbl_Section v
LEFT JOIN tbl_Courses c on c.Course_Id = v.Course_Fid 

Union ALL

SELECT 'Course' As Type, '/Student/TakeCourse/' + CONVERT(varchar(max),v.Course_Id) As Url ,
		v.Course_Id AS ID , v.Course_Name AS Name,NULL  As Navigation
FROM   tbl_Courses v