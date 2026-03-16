-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_GetAllUsersForFriendList '627b9a61-a340-488e-a2fd-1b1e1d55e1c9',1
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetAllUsersForFriendList] 
@UserID varchar(max),
@RoleID int
AS
BEGIN

Select  u.User_Id , u.User_Name , u.User_AspUser,rl.Name, u.User_Pic ,
		'+' + Convert(varchar,u.CountryID )+ Convert(varchar,u.User_Mobile) As Mobile,
		u.City , u.CreateDT ,oc.Master_Value AS Occupation,
		(SELECT [dbo].[fn_GetCurrentPackageofStudent](u.User_AspUser)) As Package,
		CONVERT(bit,CASE WHEN EXISTS(SELECT f.Status FROM tbl_Friends f 
				WHERE (f.FriendOne = @UserID AND f.FriendTwo = u.User_AspUser )
					OR (f.FriendTwo = @UserID AND f.FriendOne = u.User_AspUser) ) 
			THEN 1 ELSE 0 END) AS IsFriend ,

			CONVERT(bit,CASE WHEN 'Pending' = (SELECT Top 1 f.Status FROM tbl_FriendRequest f 
				WHERE (f.UserID = @UserID AND f.FutureFriendID = u.User_AspUser ) ) 
			THEN 1 ELSE 0 END) AS RequestSent,

			CONVERT(bit,CASE WHEN 'Pending' = (SELECT Top 1 f.Status FROM tbl_FriendRequest f 
				WHERE (f.UserID =u.User_AspUser   AND f.FutureFriendID =@UserID ) ) 
			THEN 1 ELSE 0 END) AS RequestReceived,

			CONVERT(bit,CASE WHEN 'Rejected' = (SELECT Top 1 f.Status FROM tbl_FriendRequest f 
				WHERE (f.UserID =u.User_AspUser   AND f.FutureFriendID =@UserID
				OR f.UserID =@UserID   AND f.FutureFriendID =u.User_AspUser ) ) 
			THEN 1 ELSE 0 END) AS RequestRejected

FROM tbl_User u
left join AspNetUserRoles r on r.UserId = u.User_AspUser
left join AspNetRoles rl on rl.Id = r.RoleId
Left Join tbl_Master oc on oc.Master_ID = u.OccupationID

where (r.RoleId = @RoleID OR @RoleID = 0)

Order by IsFriend desc, RequestSent desc, RequestReceived desc,RequestRejected
END