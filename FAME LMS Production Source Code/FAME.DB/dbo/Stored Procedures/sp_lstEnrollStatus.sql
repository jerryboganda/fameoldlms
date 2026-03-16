-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_lstEnrollStatus null,null,'',0,'',null
-- =============================================
CREATE PROCEDURE [dbo].[sp_lstEnrollStatus]
@DateFrom date,
@DateTo date,
@StudentID nvarchar(128),
@PackageID int,
@Status nvarchar(50),
@IsEmailSent bit
AS
BEGIN


Select  m.Enrollment_Id , m.Enrollment_Date,m.Enrollment_Status,m.Enrollment_Price,pd.Duration,us.User_Id,
		m.Enrollment_EndDate,us.User_Name AS StudentName,s.Email,m.ByRequest,m.ByManual,ISNULL(m.IsEmailSent,'false') IsEmailSent,
		('+'+CONVERT(varchar,us.CountryID) + CONVERT(varchar, us.User_Mobile)) As MobileNo,
		m.IsExpired,ab.User_Name AS ApproveBy ,p.PackageName
FROM  tbl_EnrollmentMaster m 
LEFT JOIN tbl_User us on us.User_AspUser = m.StudentFid
LEFT JOIN tbl_PackageDuration pd on pd.ID = m.PackageDurationFid
LEFT JOIN tbl_Package p on p.PackageID = pd.PackageID
Left JOIN tbl_User ab on ab.User_AspUser = m.ApprovedBy
Left JOIN AspNetUsers s on s.id = m.StudentFid


WHERE (CONVERT(date,m.Enrollment_Date )>= @DateFrom OR @DateFrom IS NULL)
AND (CONVERT(date,m.Enrollment_Date ) <= @DateTo OR @DateTo IS NULL)
AND (m.StudentFid = @StudentID OR @StudentID = '')
AND (pd.PackageID = @PackageID OR @PackageID = 0)
AND (ISNULL(m.IsEmailSent,'false') = @IsEmailSent OR @IsEmailSent IS NULL)
AND ByRequest = 'true'


END