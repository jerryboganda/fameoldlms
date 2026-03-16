CREATE function [dbo].[fn_CurrentPackDetail]  
(   
   @StudentID nvarchar(max)
)  
returns TABLE  
Return 	

Select top 1  ISNULL(pa.PackageName,'No Package') PackageName , pa.PackageID,m.Enrollment_EndDate , m.Enrollment_Date,
		(Select Count(v.Question_Id) 
		from tbl_Question v , tbl_PackageDetail c
		Where v.Course_Fid = c.CourseID AND c.PackageID = pa.PackageID AND v.Type = 1) As Mcqs
from tbl_EnrollmentMaster m
	Inner Join tbl_PackageDuration p on m.PackageDurationFid = p.ID
	Inner join tbl_Package pa on pa.PackageID = p.PackageID
	WHERE m.StudentFid = @StudentID
	Order By m.Enrollment_Date desc