-- =============================================
-- Author:		<Author,,Name>
-- select * from tbl_Ratings
-- sp_GetReview 1131,'Video',''
-- =============================================
CREATE PROCEDURE [dbo].[sp_GetReview]
@ID int,
@Type varchar(50),
@UserID varchar(max)
AS
BEGIN

	Select CONVERT(DECIMAL(10,2),AVG(CONVERT(DECIMAL(10,2),r.Stars))) AvgRatings,
		   Count(r.Stars) TotalRatings,
		   (Select Count(*) from  tbl_Ratings rr where r.ObjectID = rr.ObjectID AND r.ObjectType = rr.ObjectType AND rr.Stars = 5 ) AS star5,
		   (Select Count(*) from  tbl_Ratings rr where r.ObjectID = rr.ObjectID AND r.ObjectType = rr.ObjectType AND rr.Stars = 4 ) AS star4,
		   (Select Count(*) from  tbl_Ratings rr where r.ObjectID = rr.ObjectID AND r.ObjectType = rr.ObjectType AND rr.Stars = 3 ) AS star3,
		   (Select Count(*) from  tbl_Ratings rr where r.ObjectID = rr.ObjectID AND r.ObjectType = rr.ObjectType AND rr.Stars = 2 ) AS star2,
		   (Select Count(*) from  tbl_Ratings rr where r.ObjectID = rr.ObjectID AND r.ObjectType = rr.ObjectType AND rr.Stars = 1 ) AS star1,
		   ISNULL((Select top 1 rr.Stars from  tbl_Ratings rr where r.ObjectID = rr.ObjectID AND r.ObjectType = rr.ObjectType AND rr.UserID = @UserID ),0) AS UserStar
	From tbl_Ratings r
	WHERE (r.ObjectID = @ID OR @ID = 0 ) 
	AND (r.ObjectType = @Type OR @Type = '')
	Group by r.ObjectID,r.ObjectType

END