-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- [famedb].[Enrollment] '21samch@gmail.com'
-- =============================================
CREATE PROCEDURE [dbo].[Enrollment]
@Email nvarchar (128)
AS
BEGIN


declare 
@StudentID nvarchar (128);
select @StudentID =  id from aspnetusers where Email = @Email
Select @Email as email,e.Enrollment_Date ,e.Enrollment_EndDate, c.Course_Name , e.Enrollment_Price, d.IsExpired 
from tbl_EnrollmentMaster e
left join tbl_EnrollmentDetail d on d.EnrollmentID = e.Enrollment_Id
left join tbl_Section s on d.Section_Fid = s.Section_ID
left join tbl_Courses c on c.Course_Id = d.CourseID
group by c.Course_Name , e.StudentFid,e.IsExpired,e.Enrollment_EndDate,e.Enrollment_Date, e.Enrollment_Price,c.Course_Id, d.IsExpired

having StudentFid = @StudentID
 AND ISNULL(e.IsExpired , 'false')!='true'


END