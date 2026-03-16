-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_rptReultReport 5,0,'','','',''
-- =============================================
CREATE PROCEDURE [dbo].[sp_rptReultReport]
@UniversityID int,
@PaperID int,
@SectionID varchar(max),
@StudentID varchar(max),
@DateFrom date,
@DateTo date
AS
BEGIN
SELECT * FROM (
	Select rm.[ID] ,[Datetime] ,us.Id As [StudentID] ,[QuestionPaperID] ,[ObtainedMarks] ,[TotalMarks] ,[Mode] ,[TimeGiven] ,[TimeElapsed] ,ISNULL(PassPer,50) PassPer,
	qp.PaperTitle,u.User_Name,u.User_Mobile,u.User_Pic,us.Email,ISNULL(g.GroupTitle,'No Group') GroupTitle,
	ISNULL((SELECT COUNT(rd.ID) FROM tbl_ResultDetail rd WHERE rm.ID = rd.ResultID),0) AS Total,
	ISNULL((SELECT COUNT(rd.ID) FROM tbl_ResultDetail rd WHERE rm.ID = rd.ResultID AND rd.IsTrue = 'true'),0) AS Correct,
	ISNULL((SELECT COUNT(rd.ID) FROM tbl_ResultDetail rd WHERE rm.ID = rd.ResultID AND rd.Answer IS NOT NULL),0) AS Solved,
	ISNULL((SELECT CONVERT(INT, SUM( CASE WHEN ScreenTime > Video_Length THEN Video_Length ELSE p.ScreenTime END )) FROM tbl_Progress p INNER JOIN tbl_Video v on v.Video_Id = p.Video_Fid WHERE p.Student_Fid = us.Id AND ( v.Section_Fid IN (Select * from splitstring(@SectionID))  Or @SectionID = '' ) ),0) ScreenTime,
	ISNULL((SELECT SUM(v.Video_Length) FROM tbl_Video v WHERE (v.Section_Fid IN (Select * from splitstring(@SectionID)))),0) TotalTime
	FROM  tbl_User u
	LEFT JOIN AspNetUsers us on us.ID = u.User_AspUser
	LEFT JOIN tbl_ResultMaster rm  on u.User_AspUser = rm.StudentID AND (rm.QuestionPaperID = @PaperID OR  @PaperID = 0)
	LEFT JOIN tbl_QuestionPaper qp on qp.PaperID = rm.QuestionPaperID 
	LEFT JOIN tbl_StudentGroupDetail gd on gd.StudentID = u.User_AspUser
	LEFT JOIN tbl_StudentGroup g on g.Id = gd.GroupID

	WHERE (u.Type = @UniversityID OR  (@UniversityID = 0 AND rm.QuestionPaperID IS NOT NULL))
	AND (us.Id IN (Select * from splitstring(@StudentID)) OR @StudentID = '')
	) dt  Order By ScreenTime desc
END

SET ANSI_NULLS ON