-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_GetPendingInstallments null,''
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetPendingInstallments]
@IsPaid bit,
@StudentFid nvarchar(128)
AS
BEGIN

	Select Si.* , St.UserName ,p.PackageName
	
	From tbl_StudentInstallments Si
	left join tbl_EnrollmentMaster e on e.Enrollment_Id = Si.EnrollmentID
	left join tbl_Package p on p.PackageID = Si.PackageID
	left join AspNetUsers St on St.Id = e.StudentFid

	where (Si.StudentID = @StudentFid OR  @StudentFid = '')
	AND ( ISNULL(Si.IsPaid,'false') = @IsPaid OR @IsPaid IS NULL)

END