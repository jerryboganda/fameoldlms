-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_VideoForFirst 0,28,'0b48fd0c-f132-4e94-bca6-0c17de45823a'
-- =============================================
CREATE PROCEDURE [dbo].[sp_VideoForFirst]
@SectionID int,
@CourseID int,
@UserID varchar(max)

AS
BEGIN
	DECLARE @VideoID int;

	SELECT TOP 1 @VideoID = v.Video_Id  FROM tbl_Progress p
	LEFT JOIN tbl_Video v on v.Video_Id = p.Video_Fid 
	WHERE v.Course_Fid = @CourseID AND p.Student_Fid = @UserID
	Order BY  p.LastOpenDT desc

	if(@VideoID IS NULL)
		SELECT @VideoID = Video_Id FROM tbl_Video WHERE Course_Fid = @CourseID
	ELSE 
	    SET @VideoID = @VideoID

	SELECT @VideoID
END