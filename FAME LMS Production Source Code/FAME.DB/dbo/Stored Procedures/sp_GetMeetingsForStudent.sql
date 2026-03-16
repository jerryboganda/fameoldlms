-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- sp_GetMeetingsForStudent '10485a3f-ff3a-4389-969b-74b368b38138' ,'',null
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetMeetingsForStudent]
@StudentID nvarchar(max),
@Status nvarchar(max),
@DateFrom date
AS
BEGIN
DECLARE @CDate datetime = DATEADD(HOUR, 5, GETUTCDATE());

Select distinct  mt.[ID] ,[MeetingNo] ,[Duration] ,mt.[CourseIDs],mt.PackageIDs
      ,[Topic] ,[JoinUrl] ,ISNULL([Status],'UpComing' ) AS Status,[Password] 
      ,[StartTime] ,[CreatedDT] ,Tech.User_Name As TeacherName
FROM tbl_Meeting mt
Left join tbl_User Tech on Tech.User_AspUser = mt.CreatedBy
Where  EXISTS (  
	Select * from tbl_EnrollmentMaster m
	Left Join tbl_EnrollmentDetail d on d.EnrollmentID = m.Enrollment_Id 
	left join tbl_PackageDetail p on p.CourseID = d.CourseID
	WHERE ( d.CourseID IN (SELECT * FROM splitstring(mt.CourseIDs)) OR 
	p.PackageID IN (SELECT * FROM splitstring(mt.PackageIDs) ))
	AND m.StudentFid = @StudentID 
	AND  m.Enrollment_EndDate > @CDate
	AND ISNULL(m.IsExpired,'false') = 'false'
	AND ISNULL(d.IsExpired,'false') = 'false'
)
AND (mt.Status IN (Select * from splitstring(@Status)) OR @Status = '')
AND (mt.StartTime >= @DateFrom OR @DateFrom IS NULL)

END