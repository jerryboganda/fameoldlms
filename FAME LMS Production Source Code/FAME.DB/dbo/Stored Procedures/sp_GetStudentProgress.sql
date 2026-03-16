-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_GetStudentProgress 'df715270-8d4c-4b6b-b1a9-c21161103a83',0,0,0,0
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetStudentProgress]
@StudentID varchar(max), 
@CourseID int,
@PackageID int,
@SectionID int,
@VideoID int
AS
BEGIN

	Select  p.*,v.Video_Length,v.Video_Name ,
			s.User_Name,s.User_Mobile,u.Email,
			sec.Section_Name,cor.Course_Name,cor.Course_Id,sec.Section_ID,v.Video_Id
	from  tbl_Courses cor
	LEFT JOIN tbl_Video v on v.Course_Fid = cor.Course_Id
	LEFT JOIN tbl_Section sec on sec.Section_ID = v.Section_Fid
	LEFT JOIN tbl_Progress p on p.Video_Fid = v.Video_Id AND p.Student_Fid = @StudentID
	LEFT JOIN tbl_User s on s.User_AspUser = @StudentID
	LEFT JOIN AspNetUsers u on u.ID = @StudentID
	
	WHERE    (cor.Course_Id IN (select d.CourseID
								From [dbo].[tbl_EnrollmentMaster] e
								join [dbo].[tbl_EnrollmentDetail] d on e.Enrollment_Id = d.EnrollmentID
								where StudentFid = @StudentID
								AND e.IsExpired !='true' AND d.IsExpired !='true'
								AND e.Enrollment_EndDate > GETDATE())
							)
	AND (v.Video_Id = @VideoID OR @VideoID = 0)
	AND (v.Section_Fid = @SectionID OR @SectionID = 0)
	AND (v.Course_Fid = @CourseID OR @CourseID = 0)
	--AND (p.Student_Fid IN (Select * from splitstring(@StudentID))  OR @StudentID = '')
	--Order By p.Student_Fid
END