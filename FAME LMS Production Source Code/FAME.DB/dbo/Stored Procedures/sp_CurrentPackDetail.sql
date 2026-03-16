-- sp_CurrentPackDetail '4768a333-4f6d-4914-a21a-271834bef9f2' 
CREATE Proc [dbo].[sp_CurrentPackDetail]  
(   
   @StudentID nvarchar(max)
)  
AS
Select  ISNULL(pa.PackageName,'No Package') PackageName , pa.PackageID,p.Duration,m.*,
		(Select Count(v.Question_Id) 
		from tbl_Question v , tbl_PackageDetail c
		Where v.Course_Fid = c.CourseID AND c.PackageID = pa.PackageID AND v.Type = 1) As Mcqs
from tbl_EnrollmentMaster m
	Inner Join tbl_PackageDuration p on m.PackageDurationFid = p.ID
	Inner join tbl_Package pa on pa.PackageID = p.PackageID
	WHERE m.StudentFid = @StudentID 
	Order By m.Enrollment_Date desc