-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_lstExams 0,'What ,' , ''
-- =============================================
CREATE PROCEDURE [dbo].[sp_lstExams]
@DifficultyLevel int,
@Tags varchar(max),
@Text varchar(max),
@CreatedBy varchar(max)

AS
BEGIN

Select * 
FROM tbl_Exam q
Where ( q.DifficultyLevel = @DifficultyLevel OR @DifficultyLevel = 0)
AND (  ( (select Count(t.Name) from  splitstring(q.Tags) t ,
			splitstring(@Tags) n Where t.Name = n.Name) > 0 
		)
		OR @Tags = '' 
	)
AND ( q.ExamTitle LIKE ('%' + @Text + '%') OR @Text = '' )
AND ( q.CreatedBy = @CreatedBy  OR @CreatedBy = '' )

END