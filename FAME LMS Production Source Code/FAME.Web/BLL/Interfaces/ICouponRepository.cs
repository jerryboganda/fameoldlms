using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface ICouponRepository
    {
        bool Create(CouponVM model);
        CouponVM GetByID(int id);
        List<CouponVM> GetList(string id);
        tbl_Coupon IsCouponTrue(int id, string secret, bool isCourse);
    }
}
