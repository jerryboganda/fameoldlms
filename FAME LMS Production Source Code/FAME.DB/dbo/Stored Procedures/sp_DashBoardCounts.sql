-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_DashBoardCounts
-- =============================================
CREATE PROCEDURE [dbo].[sp_DashBoardCounts]
AS
BEGIN
	DECLARE @dnow DATETIME = DATEADD(HOUR, 5, GETUTCDATE())
	DECLARE @d7days DATETIME = DATEADD(DAY, -7, @dnow)

	DECLARE @Student int = (Select COUNT(u.Id) from AspNetUsers u)
	DECLARE @ActiveStudent int = (Select COUNT(u.Id) from AspNetUsers u Where EXISTS (Select 1 Enrollment_Id from tbl_EnrollmentMaster e Where u.Id = e.StudentFid AND IsExpired != 'true' and Enrollment_EndDate >= @dnow) )

 Select 2 as SortID, 'Total Enrollments' As Name, COUNT(Enrollment_Id) as Counts from tbl_EnrollmentMaster Where IsExpired != 'true'
 UNION
 Select 3 as SortID, 'Total Students' As Name, @Student as Counts 
 UNION
 Select  4 as SortID, 'Active Students' As Name, @ActiveStudent as Counts
 UNION
 Select 5 as SortID, 'In-Active Students' As Name,@Student - @ActiveStudent as Counts
 UNION
 Select 1 as SortID, 'Enrollments This Week' As Name, COUNT(Enrollment_Id) as Counts from tbl_EnrollmentMaster Where IsExpired != 'true' AND Enrollment_Date >= @d7days
 UNION
 Select 6 as SortID, 'Pending Requests This Week' As Name, COUNT(RequestID) as Counts from tbl_Request Where IsAccepted != 'true' AND RequestDate >= @d7days
END