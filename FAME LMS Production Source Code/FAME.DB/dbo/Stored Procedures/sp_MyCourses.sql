-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_MyCourses '17cbd498-43cc-42fe-9bc2-b2e5e713819d'
-- =============================================
CREATE PROCEDURE [dbo].[sp_MyCourses]
@StudentID nvarchar(128)
AS
BEGIN

SELECT Distinct c.* ,
		(Select Count(v.Video_Id) from tbl_Video v Where v.Course_Fid = c.Course_Id AND ISNULL(v.Type,1 )= 1) As Videos,
		(Select Count(v.Video_Id) from tbl_Video v Where v.Course_Fid = c.Course_Id 
		AND Exists ( Select * from tbl_Progress p where v.Video_Id = p.Video_Fid AND p.Student_Fid = StudentFid AND p.isCompleted = 'true' )
		) As WatchedVideos,
		(Select Sum(v.Video_Length) from tbl_Video v Where v.Course_Fid = c.Course_Id AND ISNULL(v.Type,1 )= 1) As VideosDur,
		(Select Count(v.ID) from tbl_EnrollmentDetail v Where v.CourseID = c.Course_Id ) As Views

From [dbo].[tbl_EnrollmentMaster] e
	LEFT JOIN [dbo].[tbl_EnrollmentDetail] d on e.Enrollment_Id = d.EnrollmentID
	LEFT JOIN tbl_PackageDetail pd on pd.PackageID = e.PackageId
	INNER join tbl_Courses c on  c.Course_Id = pd.CourseID OR c.Course_Id = d.CourseID
where e.StudentFid = @StudentID
	AND e.IsExpired !='true' AND (d.IsExpired !='true' OR d.IsExpired IS NULL)
	AND e.Enrollment_EndDate >= DATEADD(HOUR,5,GETUTCDATE())
END