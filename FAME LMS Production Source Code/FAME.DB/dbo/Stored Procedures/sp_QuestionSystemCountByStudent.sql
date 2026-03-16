-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_QuestionSystemCountByStudent 'ddc8ffd1-3fe6-4e2d-9108-1ee8ac40b968'
-- =============================================
CREATE PROCEDURE [dbo].[sp_QuestionSystemCountByStudent]
@StudentID varchar(max)
AS
BEGIN

WITH AllQuestions AS (
	SELECT q.Question_Id, q.SystemID, q.PartitionID
	FROM tbl_Question q
	LEFT JOIN tbl_QuestionPartition qp ON qp.ID = q.PartitionID
	WHERE q.Type = 1
	AND (qp.IsForStudent = 1 OR qp.IsForStudent IS NULL)
),
StudentResults AS (
	SELECT rd.QuestionID, rd.IsTrue
	FROM tbl_ResultMaster rm
	INNER JOIN tbl_ResultDetail rd ON rd.ResultID = rm.ID
	WHERE rm.StudentID = @StudentID
),
MarkedQuestions AS (
	SELECT f.Object_ID
	FROM tbl_FavouriteList f
	WHERE f.ObjectType = 'Mcq' AND f.Student_Fid = @StudentID
),
FilteredQuestions AS (
	SELECT aq.Question_Id, aq.SystemID, 
		   sr.QuestionID AS AnsweredID, sr.IsTrue,
		   mq.Object_ID AS MarkedID
	FROM AllQuestions aq
	LEFT JOIN StudentResults sr ON sr.QuestionID = aq.Question_Id
	LEFT JOIN MarkedQuestions mq ON mq.Object_ID = aq.Question_Id
),
QTypes AS (
	SELECT 'Unused' AS QType
	UNION ALL SELECT 'Incorrect'
	UNION ALL SELECT 'Marked'
	UNION ALL SELECT 'ALL'
),
SystemQTypeCross AS (
	SELECT DISTINCT qs.ID AS SystemID, qs.Name AS SystemName,qs.Section, qt.QType
	FROM tbl_QuestionSystem qs
	CROSS JOIN QTypes qt
	WHERE qs.IsActive = 1
),
ActualCounts AS (
	SELECT 
		fq.SystemID,
		CASE 
			WHEN sr.QuestionID IS NULL THEN 'Unused'
			WHEN sr.IsTrue = 'false' THEN 'Incorrect'
			WHEN mq.Object_ID IS NOT NULL THEN 'Marked'
			ELSE NULL
		END AS QType,
		fq.Question_Id
	FROM FilteredQuestions fq
	LEFT JOIN StudentResults sr ON fq.Question_Id = sr.QuestionID
	LEFT JOIN MarkedQuestions mq ON fq.Question_Id = mq.Object_ID
)
SELECT 
	sq.SystemID,
	sq.SystemName,
	sq.QType,
	sq.Section,
	COUNT(DISTINCT ac.Question_Id) AS NoOfQ
FROM SystemQTypeCross sq
LEFT JOIN ActualCounts ac 
	ON ac.SystemID = sq.SystemID 
	AND (
		(sq.QType = 'Unused' AND ac.QType = 'Unused') OR
		(sq.QType = 'Incorrect' AND ac.QType = 'Incorrect') OR
		(sq.QType = 'Marked' AND ac.QType = 'Marked') OR
		(sq.QType = 'ALL')
	)
GROUP BY sq.SystemID, sq.SystemName, sq.QType,sq.Section

END
GO

