using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace First_Aid_Made_Easy.Controllers
{
    public class StudentDDLController : ApiController
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IGeneralRepository _generalRepository;
        private readonly ISettingRepository _settingRepository;

        public StudentDDLController(
            IPackageRepository packageRepository,
            IGeneralRepository generalRepository,
            ISettingRepository settingRepository)
        {
            _packageRepository = packageRepository;
            _generalRepository = generalRepository;
            _settingRepository = settingRepository;
        }

        [HttpGet]
        public IHttpActionResult Occupationddl()
        {
            var data = _generalRepository.GetList((int)MasterGroup.Occupation);
            var Occupation = data.OrderBy(x => x.Master_Value).ToList().ConvertAll(x => new SelectListVM
            {
                Text = x.Master_Value,
                Value = x.Master_ID,
            });
            return Ok(new { success = true, data = Occupation });
        }
        [HttpGet]
        public IHttpActionResult Packagesddl()
        {
            var Packages = _packageRepository.GetList().ToList().ConvertAll(x => new SelectListVM
            {
                Value = x.PackageID,
                Text = x.PackageName
            });
            return Ok(new { success = true, data = Packages });
        }
        [HttpGet]
        public IHttpActionResult Durationddl(int id)
        {
            List<PackageDurationVM> list = _packageRepository.getDurations(id);
            var options = list.ConvertAll(x => new SelectListVM
            {
                Value = x.Duration ?? 0,
                Text = _packageRepository.getText(x.Duration) + string.Format("{0:n0}", x.Price),
            });
            return Ok(new { success = true, data = options.OrderBy(x => x.Value) });
        }

        [HttpGet]
        public IHttpActionResult CheckInstallments(int id)
        {
            InstallmentVM model = _packageRepository.CheckInstallments(id);
            return Ok(new { success = true, data = model.List.Select(x => x.DurationS) });
        }

        [HttpGet]
        public IHttpActionResult AgentCategoryddl()
        {
            var List = _generalRepository.GetList((int)MasterGroup.AgentCategory);
            var options = List.ConvertAll(x => new SelectListVM
            {
                Value = x.Master_ID,
                Text = x.Master_Value,
            });
            return Ok(new { success = true, data = options.OrderBy(x => x.Value) });
        }

        [HttpGet]
        public IHttpActionResult GetUniversities()
        {
            return Ok(new { success = true, data = Common.Universities.Where(x => x.Value > 0 && x.IsActive) });
        }
        [HttpGet]
        public IHttpActionResult GetMockTestType()
        {
            return Ok(new { success = true, data = Common.MockTestTypesDdl });
        }

        [HttpGet]
        public IHttpActionResult GetSettings()
        {
            return Ok(new { success = true, data = _settingRepository.GetRegisterSettings() });
        }
    }
}
