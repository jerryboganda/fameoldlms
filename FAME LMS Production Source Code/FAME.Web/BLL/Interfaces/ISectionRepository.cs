using First_Aid_Made_Easy.Models;
using System.Collections.Generic;
using System.Web.WebPages.Html;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface ISectionRepository
    {
        List<SectionVM> GetSectionsByCourse(int courseID, string StudentFid);
        SectionVM GetByIDForTakeLec(int sectionID, string StudentFid);
        bool DeleteSection(int id);
        int SaveSection(SectionVM section);
        SectionVM GetByID(int? id);
        List<SelectListItem> List(string id = "", bool isAdmin = true);
    }
}
