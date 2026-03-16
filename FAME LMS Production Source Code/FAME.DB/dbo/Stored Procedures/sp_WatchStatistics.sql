-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
create PROCEDURE [dbo].[sp_WatchStatistics]
AS
BEGIN


SELECT  COUNT(DISTINCT Student_Fid) AS Users,COUNT(*) AS Videos, SUM(p.ScreenTime) as ScreenTime,
		COUNT(*) / COUNT(DISTINCT Student_Fid) as Average,
		MONTH(LastOpenDT) As _Month, YEAR (LastOpenDT) as _Year
FROM [dbo].[tbl_Progress] p
Group By MONTH(LastOpenDT) , YEAR (LastOpenDT)
ORDER BY _Year, _Month



END
GO

