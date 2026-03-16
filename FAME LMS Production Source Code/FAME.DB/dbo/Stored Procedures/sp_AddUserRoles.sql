-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_AddUserRoles '20047f0b-4bcc-4158-9f1c-5918bed074ee','4'
-- =============================================
CREATE PROCEDURE [dbo].[sp_AddUserRoles]
@UserID varchar(max),
@RoleId int
AS
BEGIN
	INSERT INTO AspNetUserRoles
	VALUES (@UserID,@RoleId);


END