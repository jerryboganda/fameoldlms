-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_lstDeleteDeviceReq '4768a333-4f6d-4914-a21a-271834bef9f2',true
-- =============================================
CREATE PROCEDURE [dbo].[sp_lstDeleteDeviceReq]
@UserID nvarchar(max),
@IsAccepted bit
AS
BEGIN
	Select r.* ,u.User_Name,d.IsMobile,d.CreatedDT As DevCreatedDT,d.UserAgent,
	u2.Email,'+' + Convert(varchar,u.CountryID )+ Convert(varchar,u.User_Mobile) As Student_Mobile
	from tbl_UserDevRemReq r
	Left Join tbl_User u on u.User_AspUser = r.UserID
	Left Join tbl_UserDevices d on d.DeviceID = r.DeviceID AND r.UserID = d.UserID AND (d.IsActive = 'true' OR @IsAccepted IS NOT NULL)
	Left JOIN AspNetUsers u2 on u2.Id = r.UserID
	Where 
	(( @IsAccepted IS NOT NULL AND r.IsAccepted IS NOT NULL ) OR 
	 (r.IsAccepted IS NULL AND @IsAccepted IS NULL) )
	AND ( r.UserID = @UserID OR @UserID = '' )
END