-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_lstRequests '1/1/2024','1/1/2025',1,''
-- =============================================
CREATE PROCEDURE [dbo].[sp_lstRequests]
@DateFrom date,
@DateTo date,
@Type int, -- ======= Student Type =======
@RequestFor varchar(50)
AS
BEGIN

	Select '+' + Convert(varchar,z.CountryID )+ Convert(varchar,z.User_Mobile) As Student_Mobile,ISNULL(r.RequestFor,'Register') RequestFor,
			r.RequestDate,r.Duration,'' AS DurationS,r.StudentID,r.CourseID,r.SectionID,pd.Price,c.Course_Name,p.PackageName,p.PackageID,
			z.User_Name AS StudentName,r.StudentEmail,r.IsAccepted,r.Payment,r.RequestID,z.User_Id,z.Type,z.ExamType,u.RegisteredFrom

	FROM tbl_Request r
	Left JOIN tbl_User z on r.StudentID = z.User_AspUser
	Left JOIN AspNetUsers u on u.Id = z.User_AspUser
	LEFT JOIN tbl_Package p on r.PackageID = p.PackageID
	LEFT JOIN tbl_Courses c on r.CourseID= c.Course_Id
	LEFT JOIN tbl_PackageDuration pd on pd.Duration = p.Duration

	WHERE @DateFrom <= Convert(date,r.RequestDate) AND Convert(date,r.RequestDate) <= @DateTo
	AND (ISNULL(r.RequestFor,'Register') = @RequestFor OR @RequestFor = '')
	AND ISNULL(z.Type,1) = @Type

	Order by r.RequestDate desc
END