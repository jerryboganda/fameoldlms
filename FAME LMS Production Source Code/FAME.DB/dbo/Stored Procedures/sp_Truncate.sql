
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
--[dbo].[sp_Truncate]
-- =============================================
CREATE PROCEDURE [dbo].[sp_Truncate]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	TRUNCATE TABLE  [dbo].[tbl_Comment];
	TRUNCATE TABLE  [dbo].[tbl_Progress];
	TRUNCATE TABLE  [dbo].[tbl_Enrollment];
	TRUNCATE TABLE  [dbo].[tbl_Wishlist];
	TRUNCATE TABLE  [dbo].[tbl_User];
	TRUNCATE TABLE  [dbo].[tbl_Comment];
	TRUNCATE TABLE  [dbo].[tbl_CourseDetails];
	DELETE FROM [dbo].[tbl_Video]
	DBCC CHECKIDENT ('[FAMENew].[dbo].[tbl_Video]',RESEED, 0)
	DELETE FROM [dbo].[tbl_Section]
	DBCC CHECKIDENT ('[FAMENew].[dbo].[tbl_Section]',RESEED, 0)
	DELETE FROM [dbo].[tbl_Courses]
	DBCC CHECKIDENT ('[FAMENew].[dbo].[tbl_Courses]',RESEED, 0)
END