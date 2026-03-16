using First_Aid_Made_Easy.Filters;
using System.Web;
using System.Web.Mvc;

namespace First_Aid_Made_Easy
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            //filters.Add(new BrowserFilter());
            filters.Add(new EnrollmentFilter());
        }
    }
}
