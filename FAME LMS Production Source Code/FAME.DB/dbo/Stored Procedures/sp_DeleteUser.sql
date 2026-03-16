-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[sp_DeleteUser]
@UserID varchar(max)
AS
BEGIN
BEGIN TRANSACTION;


DELETE FROM tbl_TicketReply WHERE SendBy = @UserId OR SentTo = @UserId;

DELETE FROM tbl_Ticket WHERE CreatedBy = @UserId OR IssuedTo = @UserId;

DELETE FROM tbl_Friends WHERE FriendOne = @UserId OR FriendTwo = @UserId;

DELETE FROM AspNetUsers WHERE Id = @UserId;

COMMIT TRANSACTION;

END