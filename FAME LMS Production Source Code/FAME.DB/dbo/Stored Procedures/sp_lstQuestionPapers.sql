-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_lstQuestionPapers '29aa5c22-0b5c-449b-a218-cb863fcdcfca','true'
-- =============================================


CREATE PROCEDURE [dbo].[sp_lstQuestionPapers]
    @ForStudent VARCHAR(MAX),
	@MockTest bit
AS
BEGIN

    SELECT q.*,
           (SELECT COUNT(d.ID)
            FROM tbl_QuestionPaperDetail d
            WHERE d.PaperID = q.PaperID) AS Questions,
           CONVERT(BIT, IIF(EXISTS (
            SELECT 1
            FROM tbl_ResultMaster r
            WHERE r.QuestionPaperID = q.PaperID
              AND r.StudentID = @ForStudent
        ), 1, 0)) AS IsSolved
    FROM tbl_QuestionPaper q
         LEFT JOIN tbl_User u ON u.User_AspUser = @ForStudent
    WHERE 
		( @MockTest IS NULL OR (@MockTest = 'false' AND q.MockTestType IS NULL  ) OR ( @MockTest='true' AND q.MockTestType IS NOT NULL ))
       AND (@ForStudent = ''
           OR (EXISTS(SELECT 1
                         FROM tbl_User u
                         WHERE u.User_AspUser = @ForStudent
                         AND u.Type IN (SELECT Name FROM dbo.splitstring(q.UniversityID))
					)
			)
	)
	Order by q.PaperID desc , q.CreatedDT desc
END;
GO

