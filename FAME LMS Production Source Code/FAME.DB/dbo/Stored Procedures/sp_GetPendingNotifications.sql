-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- [sp_GetPendingNotifications] '627b9a61-a340-488e-a2fd-1b1e1d55e1c9'
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetPendingNotifications]
@StudentID varchar(max)
AS
BEGIN
	declare @Role table ( Role varchar(1000) )


	Insert Into @Role (Role) (Select r.Name from AspNetUserRoles ur
	Left Join AspNetRoles r on r.Id = ur.RoleId
	Where ur.UserId = @StudentID)

	Select  n.ID,MessageTitle , MaessageBody, PicturePath
	from tbl_StudentNotif n
	left join tbl_StudentNotifFor sfor on sfor.NotifID = n.ID

	--===== Check If Notifi is for Given Student 
	where ((AllStudents = 'true' AND 'Student' IN (Select Role From @Role ) )
	OR ((AllTeacher = 'true' AND 'Teacher' IN (Select Role From @Role )) )
	OR sfor.StudentID = @StudentID) AND n.MessageTitle is Not Null
	--===== Check If student havent seen it
	AND NOT EXISTS (Select StudentID from tbl_SeenNotif nseen where nseen.NotifID = n.ID AND StudentID = @StudentID)
	--AND Convert(varchar,DATEDIFF(DAY,n.CreatedDT, GETDATE())) < 6
	AND n.CreatedBy != 'Welcome'

	UNION ALL
	
	Select ID , MessageTitle ,(Convert(varchar,dt.RemDays) +  MaessageBody)  AS MaessageBody , PicturePath  
	FROM (
		Select  0 AS ID,'Pending Installment' MessageTitle ,
				' Days Remaining For Next Installment' MaessageBody,
				NULL AS PicturePath,GETDATE() AS CREAtedDT ,Convert(varchar,DATEDIFF(DAY, GETDATE(),InstallmentDate)) As RemDays

		FROM tbl_StudentInstallments 
	
		Where StudentID = @StudentID
		AND ISNULL(IsPaid,'false') != 'true'
	)dt 
	WHERE  dt.RemDays <= 15

END