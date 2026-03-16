-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- If the device is latest device to be (Logged IN) then it is valid
-- sp_IsThisDeviceValidNow 'bdb428e8-49a8-4954-a24a-e5efa13c7dfb','4768a333-4f6d-4914-a21a-271834bef9f2'
-- =============================================
CREATE PROCEDURE [dbo].[sp_IsThisDeviceValidNow]
@DeviceID varchar(max),
@UserID varchar(max)
AS
BEGIN
		Declare @LatestDeviceID varchar(max)= ''

		SELECT TOP 1 @LatestDeviceID = DeviceID FROM [tbl_UserLogins] 
		where UserID = @UserID 
		Order by [datetime]  desc

		Declare @IsValid bit = (CASE WHEN  @LatestDeviceID = @DeviceID THEN 1 ELSE 0 END)

		SELECT  @IsValid AS IsValid
END