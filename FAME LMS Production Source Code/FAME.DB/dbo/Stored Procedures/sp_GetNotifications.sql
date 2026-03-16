-- =============================================
-- Author:		<Author,,Name>
-- Select * from AspNetUsers where email like '%aid%'
-- sp_GetNotifications 'c1544456-6ce2-4750-b199-992ea0d7e420',null
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetNotifications]
@StudentID varchar(max),
@OnlyPend bit
AS
BEGIN
	--===== Get Roles in a table to query to get users by role 

	DECLARE @CDate Date = DATEADD(HOUR, 5, GETUTCDATE());

	--===== Notifications from table  ===========================
	WITH cte AS (
    SELECT DISTINCT
        NotifID,
        StudentID
    FROM
        tbl_SeenNotif
    WHERE
        StudentID = @StudentID
	)
	SELECT  
		n.ID,
		n.MessageTitle,
		n.Description AS MaessageBody,
		n.PicturePath,
		n.SendAt as CreatedDT,
		'javascript:void(0);' AS Url,
		Convert(bit,CASE WHEN cte.NotifID IS NOT NULL THEN 1 ELSE 0 END) AS HaveSeen,
		'Notification' as Type,n.MaessageBody as Detail
	FROM
		tbl_StudentNotif n
	LEFT JOIN
		cte ON n.ID = cte.NotifID
	WHERE
		(n.UniversityIds IS NULL OR (
			(SELECT TOP 1 u.Type FROM tbl_User u WHERE u.User_AspUser = @StudentID) IN (
				SELECT * FROM splitstring(n.UniversityIds)
			)
		))
		AND (n.PackageIds IS NULL OR EXISTS (
			SELECT 1
			FROM tbl_EnrollmentMaster em
			WHERE
				em.PackageId IN (SELECT * FROM splitstring(n.PackageIds))
				AND em.Enrollment_EndDate >= @CDate
				AND em.IsExpired != 1
				AND em.StudentFid = @StudentID
		))
		AND (@OnlyPend IS NULL OR NOT EXISTS (
			SELECT 1
			FROM cte
			WHERE cte.NotifID = n.ID
		))
    AND n.Status != 'Pending'

	UNION ALL
	
	--===== For Installments ===========================
	Select ID , MessageTitle , Convert(varchar(20),RemDays) + MaessageBody , PicturePath , CREAtedDT , 'javascript:void(0);' AS Url ,
			Convert(bit,0) As HaveSeen, 'Installment' as Type , NULL as Detail
	From (

	Select  0 AS ID,'Pending Installment' MessageTitle , ' Days Remaining For Next Installment' MaessageBody,
			NULL PicturePath ,
			null AS CREAtedDT ,DATEDIFF(DAY, GETDATE(),InstallmentDate) As RemDays

	FROM tbl_StudentInstallments 
	
	Where StudentID = @StudentID
	AND ISNULL(IsPaid,'false') != 'true')dt 
	WHERE  dt.RemDays <= 15

	--===== For Task Reminder ===========================

	UNION ALL 

	Select r.RemID AS ID ,'Task Reminder' MessageTitle ,r.Reminder AS MaessageBody ,null PicturePath ,null AS CREAtedDT ,
			'/Reminder/Index' AS Url,Convert(bit,0) As HaveSeen, 'Reminder' as Type , NULL as Detail
	FROM tbl_Reminder r
	WHERE r.DateTo = @CDate AND r.UserID = @StudentID

	--========== For Enrollment expiry ===========================

	UNION ALL 

	Select ID,'Subscription Expiry' MessageTitle , 
			'Your Subscription is going to expire in ' + Convert(varchar(20),RemDays) +
			' Days. To extend your subscription please contact us via WhatsApp 03180049742.' MaessageBody ,
			null PicturePath ,Enrollment_EndDate AS CreatedDT ,'javascript:void(0);'  AS Url,Convert(bit,0) As HaveSeen, 'Expiry' as Type , NULL as Detail
	From (
			Select TOP 1 DATEDIFF(DAY, GETDATE(),Enrollment_EndDate) As RemDays, em.Enrollment_Id As ID , em.Enrollment_EndDate
			From tbl_EnrollmentMaster em
			WHERE em.IsExpired != 'true' AND em.StudentFid = @StudentID 
			ORDER BY em.Enrollment_EndDate DESC
		) dt
	Where dt.RemDays > 0 AND dt.RemDays <= 5
END