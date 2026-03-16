-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_MySections '4768a333-4f6d-4914-a21a-271834bef9f2'
-- =============================================
CREATE PROCEDURE [dbo].[sp_MySections]
@StudentID nvarchar(128)
AS
BEGIN

select s.* ,  c.Course_Name,c.Course_Pic,
		(Select Count(v.Video_Id) from tbl_Video v Where v.Section_Fid = s.Section_ID AND ISNULL(v.Type,1 )= 1) As Videos,
		--(Select Count(v.Video_Id) from tbl_Video v Where v.Section_Fid = s.Section_ID AND v.Type = 2) As Audios,
		--(Select Count(v.Question_Id) from tbl_Question v Where v.Section_Fid = s.Section_ID AND v.Type = 1) As Mcqs,

		(Select Sum(v.Video_Length) from tbl_Video v Where v.Section_Fid = s.Section_ID AND ISNULL(v.Type,1 )= 1) As VideosDur,
		--(Select Sum(v.Video_Length) from tbl_Video v Where v.Section_Fid = s.Section_ID AND v.Type = 2) As AudiosDur,
		--0 As Exams,
		--0 As ExamsDur,

		--ISNULL((Select Sum(p.ScreenTime) from tbl_Progress p  LEFT JOIN tbl_Video v on
		--v.Video_Id = p.Video_Fid WHERE p.Student_Fid = StudentFid AND v.Section_Fid = s.Section_ID 
		--),0) As WatchedDur,

		(Select Count(v.Video_Id) from tbl_Video v Where v.Section_Fid = s.Section_ID 
		AND Exists ( Select * from tbl_Progress p where v.Video_Id = p.Video_Fid AND p.Student_Fid = StudentFid AND p.isCompleted = 'true' )
		) As WatchedVideos,
		(Select Count(v.ID) from tbl_EnrollmentDetail v Where v.CourseID = c.Course_Id ) As Views


From [dbo].[tbl_EnrollmentMaster] e
	INNER JOIN [dbo].[tbl_EnrollmentDetail] d on e.Enrollment_Id = d.EnrollmentID
	INNER join tbl_Section s on s.Section_ID = d.Section_Fid
	LEFT join tbl_Courses c on s.Course_Fid = c.Course_Id
where StudentFid = @StudentID
	AND e.IsExpired !='true' AND d.IsExpired !='true'
	AND e.Enrollment_EndDate >= DATEADD(HOUR,5,GETUTCDATE())
END