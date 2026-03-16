
CREATE PROCEDURE [dbo].[sp_ValidateCoupon]
	
     -- Add the parameters for the stored procedure here
	
     @ID int,
	
     @Secret varchar(50),
	
     @IsCourse bit,
	
     @Date date
	
AS
BEGIN
	
     -- SET NOCOUNT ON added to prevent extra result sets from
	
     -- interfering with SELECT statements.
	
     SET NOCOUNT ON;
	
	
 -- Insert statements for procedure here
	
     SELECT * FROM tbl_Coupon cp
	
     left join tbl_Enrollment en on cp.Id=en.CouponID
	
     where
	
     @ID = Case When @IsCourse = 'True' Then cp.CourseFid else cp.SectionID end
	
     AND  @Secret = cp.CouponSecret
	
     AND cp.ExpiryDate>=@Date
	
     AND cp.IsActive='True'  
	
     AND cp.NoOfUses > (select count(tbl_Enrollment.CouponID) from tbl_Enrollment where  tbl_Enrollment.CouponID = cp.Id)
	
     AND cp.CourseFid = @ID End