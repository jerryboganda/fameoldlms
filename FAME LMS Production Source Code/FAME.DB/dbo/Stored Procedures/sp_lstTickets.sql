-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_lstTickets null,null,null,'',''
-- =============================================
CREATE PROCEDURE  [dbo].[sp_lstTickets]
@DateFrom date,
@DateTo date,
@Status varchar(max),
@Student varchar(max),
@IssuedTo varchar(max)
AS
BEGIN

Select t.* , u.User_Name  As TicketFrom, us.Email , u.User_Mobile, c.Master_Value As Category,
		ISNULL((Select top 1 r.SendDT From tbl_TicketReply r 
				where r.Ticket_ID = t.Ticket_ID),t.CreatedDT) 
		As LastReply,
		ISNULL((Select COUNT(r.SendDT) From tbl_TicketReply r 
				where r.Ticket_ID = t.Ticket_ID AND ISNULL(r.IsRead,'false') = 'false'),0) 
		As NewMasgs

FROM tbl_Ticket t
Left join tbl_User u on u.User_AspUser = t.CreatedBy
Left join AspNetUsers us on us.Id = t.CreatedBy
Left join tbl_Master c on c.Master_ID = t.CategoryID

Where ( CONVERT(date,t.CreatedDT ) >= @DateFrom OR @DateFrom IS NULL)
AND (CONVERT(date,t.CreatedDT ) <= @DateTo OR @DateTo IS NULL)
AND (t.Status = @Status OR @Status IS NULL OR @Status = '')
AND (t.CreatedBy = @Student OR @Student IS NULL OR @Student = '')
AND (t.IssuedTo = @IssuedTo OR @IssuedTo IS NULL OR @IssuedTo = '')

END