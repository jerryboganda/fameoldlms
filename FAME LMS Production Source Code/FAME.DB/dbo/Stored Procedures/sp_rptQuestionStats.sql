-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_rptQuestionStats 5,0,0
-- =============================================
CREATE PROCEDURE [dbo].[sp_rptQuestionStats]

@QuestionPaperID int,
@SystemID int,
@Top int



AS
BEGIN
SELECT * FROM (
		Select q.Question_Id , q.Question ,
				ISNULL( (SELECT COUNT(*) FROM tbl_ResultDetail rd WHERE rd.QuestionID = q.Question_Id  ),0) Attempts,
				ISNULL( (SELECT COUNT(*) FROM tbl_ResultDetail rd WHERE rd.QuestionID = q.Question_Id AND rd.IsTrue IS NULL ),0) Ommitted,
				ISNULL( (SELECT COUNT(*) FROM tbl_ResultDetail rd WHERE rd.QuestionID = q.Question_Id AND rd.IsTrue = 1 ),0) Correct,
				ISNULL( (SELECT COUNT(*) FROM tbl_ResultDetail rd WHERE rd.QuestionID = q.Question_Id AND rd.IsTrue = 0 ),0) Wrong
		From tbl_Question q

		WHERE (@SystemID = 0  OR @SystemID = q.SystemID)
		AND (@QuestionPaperID = 0 OR EXISTS ( SELECT qd.ID FROM tbl_QuestionPaperDetail qd WHERE qd.PaperID = @QuestionPaperID AND qd.QuestionID = q.Question_Id ))
		) dt
		ORDER BY dt.Attempts DESC
END