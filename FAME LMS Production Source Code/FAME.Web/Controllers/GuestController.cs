using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace First_Aid_Made_Easy.Controllers
{
    public class GuestController : ApiController
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly IQuestionRepository _questionRepository;

        public GuestController(
            ICourseRepository courseRepository,
            IPackageRepository packageRepository,
            IQuestionRepository questionRepository)
        {
            _courseRepository = courseRepository;
            _packageRepository = packageRepository;
            _questionRepository = questionRepository;
        }

        [HttpGet]
        public IHttpActionResult AllCourses()
        {
            List<CourseVM> course = _courseRepository.GetListVM("", true);
            return Ok(new
            {
                success = true,
                data = course.ConvertAll(x => new
                {
                    x.Course_Description,
                    x.Course_Name,
                    x.Course_Id,
                    x.Course_Pic,
                    x.Lessons,
                    x.DurationInMin,
                    x.Teacher_Name,
                    x.Teacher_Pic,
                    x.Details
                })
            });
        }

        [HttpGet]
        public IHttpActionResult AllPackages()
        {
            List<PackageVM> course = _packageRepository.GetList();
            return Ok(new
            {
                success = true,
                data = course.ConvertAll(x => new
                {
                    x.PackageID,
                    x.PackageName,
                    x.PackagePrice,
                    x.PackageDescription,
                    x.Detail
                })
            });
        }
        [HttpGet]
        public IHttpActionResult AllMcqs()
        {
            var course = _questionRepository.AllMcqs((int)QuestionType.Mcq);
            return Ok(course);
        }

    }
}
