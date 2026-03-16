-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetAssistantTeacherID]
@AssistantID nvarchar(128)
AS
BEGIN

Select AssistantTeacher As TeacherID from AspNetUsers where id= @AssistantID

END