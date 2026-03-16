-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_GetConversations '4768a333-4f6d-4914-a21a-271834bef9f2'
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetConversations]
@UserID nvarchar(max)
AS
BEGIN
DECLARE @CDate datetime = DATEADD(HOUR, 5, GETUTCDATE());

Select dt.FriendID , dt.ConvID , dt.LastReply,
		ISNULL(u1.User_Name,dt.Title) As FriendName ,
		Case WHEN dt.FriendID IS NOT NULL THEN ISNULL( u1.User_Pic,'userLogo.jpg' ) ELSE 'groupLogo.jpg' END  As FriendPic 
FROM(
Select  (Case When f.UserID_One = @UserID THEN f.UserID_Two ELSE Case When f.UserID_Two IS NULL THEN NULL ELSE f.UserID_One END END) AS  FriendID,
		f.ConvID,f.Title,f.Descriptiom , 
		(SELECT top 1 r.SentAt FROM tbl_ConversationReply r WHERE r.ConvID = f.ConvID order by r.ID desc) AS LastReply
	From tbl_Conversation f

	Where f.UserID_One = @UserID OR f.UserID_Two = @UserID
	OR EXISTS (  
	Select * from tbl_EnrollmentMaster m
	Left Join tbl_EnrollmentDetail d on d.EnrollmentID = m.Enrollment_Id 
	left join tbl_PackageDetail p on p.CourseID = d.CourseID
	WHERE ( d.CourseID = f.CourseID OR p.PackageID =f.PackageID) 
	AND m.StudentFid = @UserID 
	AND  m.Enrollment_EndDate > @CDate
	AND ISNULL(m.IsExpired,'false') = 'false'
	AND ISNULL(d.IsExpired,'false') = 'false'
)
) dt
Left Join tbl_User u1 on u1.User_AspUser = dt.FriendID
Order by LastReply desc
END