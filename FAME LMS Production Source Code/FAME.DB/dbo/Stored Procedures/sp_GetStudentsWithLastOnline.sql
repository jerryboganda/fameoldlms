-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_GetStudentsWithLastOnline 1,18
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetStudentsWithLastOnline]
@DayFrom int,
@DayTo int
AS
BEGIN
	DECLARE @CDate Date = DATEADD(HOUR, 5, GETUTCDATE());
	DECLARE @Uni varchar(max) = (SELECT TOP 1 Value FROM tbl_Settings WHERE Name = 'Settings.Email.Universities');
	
	WITH Universities AS ( SELECT Name FROM splitstring(@Uni)),
	UserLastOnline AS (
		SELECT
			u.Id,
			u.Email,
			ISNULL(tu.FatherEmail,u.Email) AS FatherEmail,
			COALESCE(MAX(p.LastOpenDT), r.RequestDate) AS LastOnline
		FROM AspNetUsers u
		LEFT JOIN tbl_Request r ON r.StudentID = u.Id
		LEFT JOIN tbl_Progress p ON p.Student_Fid = u.Id
		LEFT JOIN tbl_User tu ON tu.User_AspUser = u.Id
		WHERE u.EmailConfirmed = 1
			AND r.IsAccepted = 1
			AND r.StudentEmail IS NOT NULL
			AND (tu.Type IN (SELECT * FROM Universities) OR @Uni IS NULL )
		GROUP BY u.Id, u.Email,r.RequestDate,tu.FatherEmail
	)
	SELECT
		ulo.*,
		DATEDIFF(DAY, CONVERT(Date, ulo.LastOnline), @CDate) - 1 AS Days
	FROM UserLastOnline ulo
	WHERE DATEDIFF(DAY, ulo.LastOnline, @CDate) - 1 BETWEEN @DayFrom AND @DayTo
	ORDER BY DATEDIFF(DAY, ulo.LastOnline, @CDate)

END