-- =============================================
-- Author:		<Your Name>
-- Create date: <Date>
-- sp_QuestionCountByStudent '4768a333-4f6d-4914-a21a-271834bef9f2',''
-- =============================================
CREATE PROCEDURE [dbo].[sp_QuestionCountByStudent]
	@StudentID VARCHAR(MAX),
	@SystemID VARCHAR(MAX)
AS
BEGIN

	-- CTE to get all valid questions
	WITH AllQuestions AS (
		SELECT q.Question_Id
		FROM tbl_Question q
		LEFT JOIN tbl_QuestionPartition qp ON qp.ID = q.PartitionID
		LEFT JOIN tbl_QuestionSystem qs ON qs.ID = q.SystemID
		WHERE q.Type = 1
		AND (qp.IsForStudent = 1 OR qp.IsForStudent IS NULL)
		AND (q.SystemID IN (SELECT * FROM splitstring(@SystemID)) OR (@SystemID = '' AND qs.IsActive = 1 ))
	),
	-- CTE for student's results
	StudentResults AS (
		SELECT rd.QuestionID, rd.IsTrue
		FROM tbl_ResultMaster rm
		INNER JOIN tbl_ResultDetail rd ON rd.ResultID = rm.ID
		WHERE rm.StudentID = @StudentID
	),
	-- CTE for marked questions by the student
	MarkedQuestions AS (
		SELECT f.Object_ID
		FROM tbl_FavouriteList f
		WHERE f.ObjectType = 'Mcq' AND f.Student_Fid = @StudentID
	),
	-- CTE that merges all needed information per question
	FilteredQuestions AS (
		SELECT 
			q.Question_Id,
			CASE 
				WHEN sr.QuestionID IS NULL THEN 'Unused'
				WHEN sr.IsTrue = 'false' THEN 'Incorrect'
				WHEN mq.Object_ID IS NOT NULL THEN 'Marked'
				ELSE NULL
			END AS QType
		FROM AllQuestions q
		LEFT JOIN StudentResults sr ON sr.QuestionID = q.Question_Id
		LEFT JOIN MarkedQuestions mq ON mq.Object_ID = q.Question_Id
	)
	-- Final SELECT using UNION ALL to match expected output
	SELECT 'Unused' AS QType,
		   COUNT(DISTINCT Question_Id) AS NoOfQ,
		   STRING_AGG(CONVERT(VARCHAR(MAX), Question_Id), ',') WITHIN GROUP (ORDER BY Question_Id) AS QuestionIDs
	FROM FilteredQuestions
	WHERE QType = 'Unused'

	UNION ALL

	SELECT 'Incorrect' AS QType,
		   COUNT(DISTINCT Question_Id) AS NoOfQ,
		   STRING_AGG(CONVERT(VARCHAR(MAX), Question_Id), ',') WITHIN GROUP (ORDER BY Question_Id) AS QuestionIDs
	FROM FilteredQuestions
	WHERE QType = 'Incorrect'

	UNION ALL

	SELECT 'Marked' AS QType,
		   COUNT(DISTINCT Question_Id) AS NoOfQ,
		   STRING_AGG(CONVERT(VARCHAR(MAX), Question_Id), ',') WITHIN GROUP (ORDER BY Question_Id) AS QuestionIDs
	FROM FilteredQuestions
	WHERE QType = 'Marked'

	UNION ALL

	SELECT 'ALL' AS QType,
		   COUNT(DISTINCT Question_Id) AS NoOfQ,
		   STRING_AGG(CONVERT(VARCHAR(MAX), Question_Id), ',') WITHIN GROUP (ORDER BY Question_Id) AS QuestionIDs
	FROM AllQuestions;

END
GO

