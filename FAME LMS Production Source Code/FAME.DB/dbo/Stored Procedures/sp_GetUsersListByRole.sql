-- =============================================
-- Author:		Jahanzaib
-- Create date: 02/10/2021
-- Description:	To get List For Users With Roles
-- sp_GetUsersListByRole 5,0,''
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetUsersListByRole]
@RoleID int,
@CategoryID int,
@Email varchar(max) 
AS
BEGIN
Select 
	Email as Email , 
	tu.User_Name as Name,
	u.Id As ID , ISNULL(u.IsActive,1) IsActive,
	rl.Name As Role,
	cat.Master_Value As Category,
	(select count(Enrollment_Id) from tbl_EnrollmentMaster em where em.ApprovedBy = u.Id) As ApprEnroll,
	(select count(Enrollment_Id) from tbl_EnrollmentMaster em where em.StudentFid = u.Id) As Enrollments
	From AspNetUsers u
		left join AspNetUserRoles r on r.UserId = u.Id
		left join tbl_User tu on tu.User_AspUser = u.Id
		left join AspNetRoles rl on rl.Id = r.RoleId
		left join tbl_Master cat on cat .Master_ID = tu.CategoryID
	where (r.RoleId = @RoleID OR @RoleID = 0)
	AND (cat.Master_ID = @CategoryID OR @CategoryID = 0)
	AND (u.Email Like '%' + @Email + '%'OR @Email  = '')
END