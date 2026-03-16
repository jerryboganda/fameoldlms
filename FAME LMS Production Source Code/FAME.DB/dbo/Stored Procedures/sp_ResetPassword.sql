-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	Set Password to "Abc@123"
-- =============================================
CREATE PROCEDURE [dbo].[sp_ResetPassword]
@Email varchar(max)
AS
BEGIN
	update  Aspnetusers
		set PasswordHash = 'AErDP4R+brf8DZFy2jQ9kyyLIJx8W1IiuvqhyAB+LSWBKO7M2PP3413r1xwiYp12ew=='
	where email = @Email
END