-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_TestsPerformance 'ddc8ffd1-3fe6-4e2d-9108-1ee8ac40b968'
-- =============================================
CREATE PROCEDURE sp_TestsPerformance
@StudentID varchar(max)
AS
BEGIN
	
	Select 'Total Correct' as Name , 
	(Select Count(rd.ID) 
		From tbl_ResultMaster rm
		left join tbl_ResultDetail rd on rd.ResultID  = rm.ID
		WHere rm.StudentID = @StudentID AND rd.IsTrue = 1
	)as Counts

	UNION ALL
	
	Select 'Total InCorrect' , 
	(Select Count(rd.ID) 
		From tbl_ResultMaster rm
		left join tbl_ResultDetail rd on rd.ResultID  = rm.ID
		WHere rm.StudentID = @StudentID AND rd.IsTrue = 0
	)as Counts

	UNION ALL
	
	Select 'Total Ommitted' , 
	(Select Count(rd.ID) 
		From tbl_ResultMaster rm
		left join tbl_ResultDetail rd on rd.ResultID  = rm.ID
		WHere rm.StudentID = @StudentID AND rd.IsTrue IS NULL
	)as Counts

	UNION ALL

	Select 'Used Questions' , 
	(Select Count(DISTINCT  rd.QuestionID) 
		From tbl_ResultMaster rm
		left join tbl_ResultDetail rd on rd.ResultID  = rm.ID
		WHere rm.StudentID = @StudentID 
	)as Counts

	UNION ALL

	Select 'Total Questions' , 
	(Select Count(q.Question_Id) 
		From tbl_Question q
	)as Counts

	UNION ALL

	Select 'Tests Solved' , (Select Count(q.id) 
		From tbl_ResultMaster q WHere q.StudentID = @StudentID
	)as Counts 

	UNION ALL

	Select 'Tests Completed' , (Select Count(q.id) 
		From tbl_ResultMaster q WHere q.StudentID = @StudentID AND q.ObtainedMarks IS NOT NULL
	)as Counts  

	UNION ALL

	Select 'Tests Suspended' , (Select Count(q.id) 
		From tbl_ResultMaster q WHere q.StudentID = @StudentID AND q.ObtainedMarks IS NULL
	)as Counts   

	UNION ALL

	Select 'Average Time Spent' , 
	(Select Avg(rd.TimeSpent) 
		From tbl_ResultMaster rm
		left join tbl_ResultDetail rd on rd.ResultID  = rm.ID
		WHere rm.StudentID = @StudentID AND rd.TimeSpent IS NOT NULL
	)as Counts
END