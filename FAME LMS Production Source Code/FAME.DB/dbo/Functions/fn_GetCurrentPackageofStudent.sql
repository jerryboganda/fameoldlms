CREATE function [dbo].[fn_GetCurrentPackageofStudent]  
(   
   @StudentID nvarchar(max)
)  
returns nvarchar(max)  
as  
begin return(
	ISNULL((Select top 1 pa.PackageName from tbl_EnrollmentMaster m
	Inner Join tbl_PackageDuration p on m.PackageDurationFid = p.ID
	Inner join tbl_Package pa on pa.PackageID = p.PackageID
	WHERE m.StudentFid = @StudentID
	Order By m.Enrollment_Date desc),'No Package')
)  
end