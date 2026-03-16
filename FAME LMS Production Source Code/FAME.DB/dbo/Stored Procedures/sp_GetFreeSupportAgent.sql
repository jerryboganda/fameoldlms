-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_GetFreeSupportAgent 122
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetFreeSupportAgent] 
@CategoryID int
AS
BEGIN

Select Top 1 u.User_AspUser AS ID  , Count(t.Ticket_ID) Tickets 
from tbl_User u
left join tbl_Ticket t on t.IssuedTo = u.User_AspUser AND t.Status = 'Open'

Where u.CategoryID = @CategoryID

Group by u.User_AspUser,u.User_Name
Order by  Count(t.Ticket_ID)



END