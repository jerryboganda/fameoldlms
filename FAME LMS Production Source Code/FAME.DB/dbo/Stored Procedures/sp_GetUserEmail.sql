-- =============================================
-- Author:		Jahanzaib
-- Create date: 02/10/2021
-- Description:	To get User Email To Send Email
-- sp_GetUserEmail
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetUserEmail]
@UserID nvarchar(128)
AS
BEGIN
Select Email From AspNetUsers 
where ID = @UserID
END