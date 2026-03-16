using First_Aid_Made_Easy.DAL;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface IFavouritesRepository
    {
        bool AddToFavourits(tbl_FavouriteList model);
        bool RemoveFavourits(tbl_FavouriteList model);
        List<sp_lstFavourites_Result> GetList(string StudentID, string Type = "");
    }
}
