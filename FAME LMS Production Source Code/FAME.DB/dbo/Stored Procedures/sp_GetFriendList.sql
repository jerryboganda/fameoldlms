-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_GetFriendList '627b9a61-a340-488e-a2fd-1b1e1d55e1c9'
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetFriendList]
@UserID nvarchar(max)
AS
BEGIN
Select dt.* , u1.User_Name As FriendName , u1.User_Pic As FriendPic ,
		'+' + Convert(varchar,u1.CountryID )+ Convert(varchar,u1.User_Mobile) As Mobile,
		u1.City , u1.User_Id , u1.CreateDT ,oc.Master_Value AS Occupation,
		(SELECT [dbo].[fn_GetCurrentPackageofStudent](FriendID)) As Package
FROM (
Select  (Case When f.FriendOne = @UserID THEN f.FriendTwo ELSE f.FriendOne END) AS  FriendID
	From tbl_Friends f
	Where f.FriendOne = @UserID OR f.FriendTwo = @UserID
) dt
Left Join tbl_User u1 on u1.User_AspUser = dt.FriendID
Left Join tbl_Master oc on oc.Master_ID = u1.OccupationID
Where dt.FriendID IS NOT NULL
END