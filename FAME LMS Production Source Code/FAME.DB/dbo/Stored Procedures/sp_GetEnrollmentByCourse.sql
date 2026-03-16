-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_GetEnrollmentByCourse 5 , 0 , 0
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetEnrollmentByCourse]
@CourseIDs varchar,
@PackageIDs varchar,
@IgnoreExpiry bit
AS
BEGIN

DECLARE @CDate datetime = DATEADD(HOUR, 5, GETUTCDATE());

SELECT DISTINCT u.Id , u.Email 

FROM tbl_EnrollmentDetail d 
Left join tbl_EnrollmentMaster m  on m.Enrollment_Id = d.EnrollmentID
Left join AspNetUsers u  on u.Id = m.StudentFid
left join tbl_PackageDetail p on d.CourseID = p.CourseID

Where (d.CourseID IN (SELECT * FROM splitstring(@CourseIDs)) OR 
	p.PackageID IN (SELECT * FROM splitstring(@PackageIDs) ))
AND ( m.Enrollment_EndDate > @CDate
AND ISNULL(m.IsExpired,'false') = 'false'
AND ISNULL(d.IsExpired,'false') = 'false') OR ISNULL(@IgnoreExpiry,'false') = 'true'

END