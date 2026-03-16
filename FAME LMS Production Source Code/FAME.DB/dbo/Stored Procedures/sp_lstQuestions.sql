-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_lstQuestions 1,0,0,'' , '',''
-- =============================================
CREATE PROCEDURE [dbo].[sp_lstQuestions]
@Type int ,
@PartitionID int,
@SystemID int,
@Tags varchar(max),
@Text varchar(max),
@CreatedBy varchar(max)
AS
BEGIN

Select q.* ,c.Master_Value As Category,u.User_Name As CreatedByName,p.Name AS PartitionName
FROM tbl_Question q
LEFT JOIN tbl_QuestionPartition p on p.ID = q.PartitionID
Left join tbl_Master c on c.Master_ID = q.CategoryID
LEFT JOIN tbl_User u on u.User_AspUser = q.CreatedBy 
Where ( q.Type = @Type OR @Type = 0)
AND ( q.SystemID = @SystemID OR @SystemID = 0)
AND ( q.PartitionID = @PartitionID OR @PartitionID = 0)
AND (  ( (select Count(t.Name) from  splitstring(q.Tags) t ,
			splitstring(@Tags) n Where t.Name = n.Name) > 0 
		)
		OR @Tags = '' 
	)
AND ( q.Question LIKE ('%' + @Text + '%') OR @Text = '' )
AND ( q.CreatedBy = @CreatedBy  OR @CreatedBy = '' )

END