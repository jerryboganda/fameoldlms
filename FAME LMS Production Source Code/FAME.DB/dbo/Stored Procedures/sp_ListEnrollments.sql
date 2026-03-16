-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_ListEnrollments null,null,'',0,0,1,0,0,null
-- =============================================
CREATE PROCEDURE [dbo].[sp_ListEnrollments]
@DateFrom date,
@DateTo date,
@StudentID nvarchar(128),
@ApproveBy nvarchar(128),
@CourseID int,
@PackageID int,
@ByRequest bit,
@ByManual bit,
@ByALL bit,
@IsExpire bit,
@City nvarchar(100),
@Institute nvarchar(100)

AS
BEGIN


Select  m.Enrollment_Id , m.Enrollment_Date,m.Enrollment_Status,m.Enrollment_Price,pd.Duration,p.PackageID,
		m.Enrollment_EndDate,us.User_Name AS StudentName,s.Email,m.ByRequest,m.ByManual,
		('+'+CONVERT(varchar,us.CountryID) + CONVERT(varchar, us.User_Mobile)) As MobileNo,
		us.City, us.Institute,
		c.Course_Name, ab.User_Name AS ApproveBy ,p.PackageName,
		CONVERT(bit,CASE WHEN m.IsExpired = 1 OR  m.Enrollment_EndDate < DATEADD(HOUR,5,GETUTCDATE()) THEN 1 ELSE 0 END) as IsExpired
FROM tbl_EnrollmentMaster m 
LEFT JOIN  tbl_EnrollmentDetail d on m.Enrollment_Id = d.EnrollmentID
LEFT JOIN tbl_User us on us.User_AspUser = m.StudentFid
LEFT JOIN tbl_Courses c on c.Course_Id = d.CourseID
LEFT JOIN tbl_PackageDuration pd on pd.ID = m.PackageDurationFid
LEFT JOIN tbl_Package p on p.PackageID = pd.PackageID
Left JOIN tbl_User ab on ab.User_AspUser = m.ApprovedBy
Left JOIN AspNetUsers s on s.id = m.StudentFid


WHERE (CONVERT(date,m.Enrollment_Date )>= @DateFrom OR @DateFrom IS NULL)
AND (CONVERT(date,m.Enrollment_Date ) <= @DateTo OR @DateTo IS NULL)
AND (m.StudentFid = @StudentID OR @StudentID = '' OR @StudentID IS NULL)
AND (m.ApprovedBy = @ApproveBy OR @ApproveBy = '' OR @ApproveBy IS NULL)
AND (us.City LIKE @City OR @City = '' OR @City IS NULL)
AND (us.Institute LIKE @Institute OR @Institute = '' OR @Institute IS NULL)
AND (d.CourseID = @CourseID OR @CourseID = 0)
AND (pd.PackageID = @PackageID OR @PackageID = 0)
AND (CONVERT(bit,CASE WHEN m.IsExpired = 1 OR  m.Enrollment_EndDate < DATEADD(HOUR,5,GETUTCDATE()) THEN 1 ELSE 0 END) = @IsExpire OR @IsExpire IS NULL)
AND (m.ByRequest = @ByRequest OR @ByALL = 'true')
AND (m.ByManual = @ByManual OR @ByALL = 'true')

ORDER BY m.Enrollment_Date DESC

END