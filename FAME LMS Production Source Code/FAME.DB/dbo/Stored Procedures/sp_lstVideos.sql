-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_lstVideos '','','','','',''
-- =============================================
CREATE PROCEDURE [dbo].[sp_lstVideos]
@StudentID varchar(max), 
@CourseID varchar(max),
@PackageID varchar(max),
@SectionID varchar(max),
@VideoID varchar(max),
@VideoType varchar(max)
AS
BEGIN

	Select  v.Video_Name ,v.Video_Path,v.Video_ShortDescription,v.Video_Tags,v.Video_Length,v.Difficulty,v.Type,
			s.User_Name,s.User_Mobile,u.Email,v.SortID,
			sec.Section_Name,cor.Course_Name,cor.Course_Id,sec.Section_ID,v.Video_Id,
			p.LastOpenDT,p.ScreenTime,p.isCompleted
	from   tbl_Video v 
	LEFT JOIN tbl_Courses cor on v.Course_Fid = cor.Course_Id
	LEFT JOIN tbl_Section sec on sec.Section_ID = v.Section_Fid
	LEFT JOIN tbl_Progress p on p.Video_Fid = v.Video_Id AND p.Student_Fid = @StudentID
	LEFT JOIN tbl_User s on s.User_AspUser = @StudentID
	LEFT JOIN AspNetUsers u on u.ID = @StudentID
	
	WHERE  (v.Video_Id IN (SELECT * FROM splitstring(@VideoID  )) OR @VideoID = '')
	AND (v.Section_Fid IN (SELECT * FROM splitstring(@SectionID)) OR @SectionID = '')
	AND (v.Course_Fid  IN (SELECT * FROM splitstring(@CourseID ))OR @CourseID = '')
	AND (v.Type  IN (SELECT * FROM splitstring(@VideoType ))OR @VideoType = '')

	ORDER BY v.SortID
END