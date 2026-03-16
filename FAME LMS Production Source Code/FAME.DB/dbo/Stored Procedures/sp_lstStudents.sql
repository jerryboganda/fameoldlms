-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_lstStudents '',null,6,'3/18/2023','5/18/2023'
-- =============================================
CREATE PROCEDURE [dbo].[sp_lstStudents]
@Text varchar(max),
@Type int,
@DateFrom date,
@DateTo date
AS
BEGIN
	DECLARE @DateNow datetime =DATEADD(HOUR,5,GETUTCDATE()) 

	Select  Convert(varchar,z.User_Mobile) As Student_Mobile,pd.Duration,z.Institute,z.User_Pic,z.City,z.JobLocation,z.YearOfMBBS,
			z.User_Name,u.Email,z.User_Id,z.User_AspUser,p.PackageName,em.Enrollment_Date,em.Enrollment_EndDate,
			z.CreateDT,ISNULL(r.IsAccepted,'false') AS IsAccepted,u.RegisteredFrom,z.Notes,z.ExamType,z.Type,u.IsActive
	FROM tbl_User z
	Right JOIN AspNetUsers u on u.Id = z.User_AspUser
	Left JOIN tbl_Request r on r.StudentID = z.User_AspUser AND (ISNULL(r.RequestFor,'Register') = 'Register')
	LEFT JOIN tbl_EnrollmentMaster em on em.Enrollment_Id = ( Select [dbo].[fn_GetCurrentEnrollmentofStudent](z.User_AspUser))
	LEFT JOIN tbl_PackageDuration pd on pd.ID = em.PackageDurationFid
	LEFT JOIN tbl_Package p on pd.PackageID = p.PackageID

	WHERE (z.User_Name LIKE '%'+@Text+'%'
			OR z.City LIKE '%'+@Text+'%'
			OR z.Institute LIKE '%'+@Text+'%'
			OR CONVERT(varchar,z.User_Mobile) LIKE '%'+@Text+'%'
			OR u.Email LIKE '%'+@Text+'%' OR @Text = ''
			OR z.JobLocation LIKE '%'+@Text+'%' OR @Text = ''
		)
	AND (ISNULL(z.Type,1) = @Type OR @Type = 0)
	AND (CONVERT(date,z.CreateDT) >= @DateFrom OR @DateFrom IS NULL  )
	AND (CONVERT(date,z.CreateDT) <= @DateTo OR @DateTo IS NULL   )

	Order by z.CreateDT desc
END