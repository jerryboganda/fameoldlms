-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_DeleteUserRoles '20047f0b-4bcc-4158-9f1c-5918bed074ee'
-- =============================================
CREATE PROCEDURE [dbo].[sp_DeleteUserRoles]
@UserID varchar(max)
AS
BEGIN

	delete from  AspNetUserRoles Where @UserID = UserId

END