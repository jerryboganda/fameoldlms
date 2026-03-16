using First_Aid_Made_Easy.Models;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface IWebsitePackageRepository
    {
        List<WebsitePackageVM> GetActivePackages();
        List<WebsitePackageVM> GetAllPackages();
        WebsitePackageVM GetById(int id);
        int Save(WebsitePackageVM model);
        bool Delete(int id);
        bool ToggleActive(int id);
    }
}
