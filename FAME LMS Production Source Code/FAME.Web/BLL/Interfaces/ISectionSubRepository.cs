using First_Aid_Made_Easy.Models;
using System.Collections.Generic;
using System.Web.WebPages.Html;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface ISectionSubRepository
    {
        bool DeleteSection(int id);
        int SaveSection(SectionSubVM section);
        SectionSubVM GetByID(int? id);
        List<SelectListItem> List(string id = "", bool isAdmin = true);
    }
}
