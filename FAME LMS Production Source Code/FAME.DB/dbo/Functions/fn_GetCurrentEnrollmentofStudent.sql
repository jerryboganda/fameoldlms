CREATE function [dbo].[fn_GetCurrentEnrollmentofStudent]  
(   
   @StudentID nvarchar(max)
)  
returns nvarchar(max)  
as  
begin return(
	ISNULL((Select top 1 m.Enrollment_Id from tbl_EnrollmentMaster m
	WHERE m.StudentFid = @StudentID
	AND ISNULL(m.IsExpired,'true') = 'false'
	AND m.Enrollment_EndDate >= DATEADD(HOUR,5,GETUTCDATE())
	Order By m.Enrollment_Date desc),0)
)  
end