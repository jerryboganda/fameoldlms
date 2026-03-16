using System.Collections.Generic;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    /// <summary>
    /// Configuration service interface for dropdown lists and settings.
    /// Used by Common.cs static properties via DependencyResolver.
    /// </summary>
    public interface IConfigurationService
    {
        List<SelectListItem> GetMockTestTypes();
        List<SelectListItem> GetExamTypes();
        List<SelectListItem> GetRoles();
        List<UniversityVM> GetUniversities();
        List<CountryVM> GetCountries();
        List<SelectListItem> GetDurationInstList();
        List<SelectListItem> GetDurationList();
        List<SelectListItem> GetDifficultyList();
        List<SelectListItem> GetStatusList();
    }
}
