-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_GetDevicesNotification '',''
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetDevicesNotification]
@UniversityIDs varchar(max),
@PackageIDs varchar(max)
AS
BEGIN

	DECLARE @CDate datetime = DATEADD(HOUR, 5, GETUTCDATE());

	SELECT ud.DeviceID,ud.UserID,ud.CreatedDT

	FROM tbl_UserDevices ud
	LEFT JOIN tbl_User u on u.User_AspUser = ud.UserID

	WHERE ud.IsMobile = 1 AND ud.IsActive = 1 AND  ud.IsApp = 1
	AND (@UniversityIDs = '' OR u.Type IN ( SELECT * FROM splitstring(@UniversityIDs)))
	AND (@PackageIDs = '' OR EXISTS( 
		SELECT em.Enrollment_Id FROM tbl_EnrollmentMaster em 
		WHERE em.PackageId IN ( SELECT * FROM splitstring(@PackageIDs))
		AND em.Enrollment_EndDate >= @CDate AND em.IsExpired != 1 
		AND em.StudentFid = ud.UserID
	))
END