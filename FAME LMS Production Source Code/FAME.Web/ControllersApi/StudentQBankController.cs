using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using Google.Type;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace First_Aid_Made_Easy.Controllers
{
    public class StudentBankV2Controller : ApiController
    {
        private readonly IQuestionPaperRepository _questionPaperRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IGeneralRepository _generalRepository;
        private readonly IFavouritesRepository _favouritesRepository;

        public StudentBankV2Controller(
            IQuestionPaperRepository questionPaperRepository,
            IEnrollmentRepository enrollmentRepository,
            IGeneralRepository generalRepository,
            IFavouritesRepository favouritesRepository)
        {
            _questionPaperRepository = questionPaperRepository;
            _enrollmentRepository = enrollmentRepository;
            _generalRepository = generalRepository;
            _favouritesRepository = favouritesRepository;
        }

        #region Test CRUD
        [HttpGet]
        public IHttpActionResult GetDataForTest(string id, string Type)
        {
            List<sp_QuestionCountByStudent_Result> QuestionCount = _questionPaperRepository.GetQuestionCount(id, "");
            List<SystemVM> Systems;
            var user = Common.GetUserNameByASpUserID(id);
            if (user != null)
            {
                Systems = _questionPaperRepository.GetSystems(id);
            }
            else
            {
                Systems = _questionPaperRepository.GetFreeSystems(Type);
            }
            return Ok(new
            {
                success = true,
                data = new { QuestionCount, Systems }
            });
        }

        [HttpGet]
        public IHttpActionResult GetQuestionToSolve(int id)
        {
            var data = _questionPaperRepository.GetQuestionToSolve(id);
            return Ok(new { success = true, data });
        }

        [HttpGet]
        public IHttpActionResult GetPerformance(string id)
        {
            var user = Common.GetUserNameByASpUserID(id);
            if (user != null)
            {
                var data = _questionPaperRepository.TestsPerformance(id);
                return Ok(new { success = true, data });
            }
            return Ok(new { success = false, message = "Invalid ID" });
        }

        [HttpPost]
        public IHttpActionResult CreateTest(CreateTestInput model)
        {
            var user = Common.GetUserNameByASpUserID(model.StudentID);
            if (user != null)
            {
                int id;
                if (model.ID > 0)
                {
                    id = _questionPaperRepository.AutoCreateResultFromPaper(model.ID, "Exam", model.StudentID);
                }
                else
                {
                    id = _questionPaperRepository.AutoCreateResult(model).ID;
                }

                SolvePaperDto data = _questionPaperRepository.GetQuestionToSolve(id);
                return Ok(new { success = true, data });
            }
            return Ok(new { success = false, message = "Invalid ID" });
        }

        [HttpPost]
        public IHttpActionResult SaveResultDetail(ResultDetailVM vid)
        {
            ResultDetailVM id = _questionPaperRepository.SaveResult(vid);
            return Ok(new { success = true, data = true });
        }

        [HttpGet]
        public IHttpActionResult Completed(int ID)
        {
            _questionPaperRepository.Completed(ID);
            return Ok(new { success = true, data = true });
        }
        public IHttpActionResult DeleteResult(int ID)
        {
            _questionPaperRepository.DeleteResult(ID);
            return Ok(new { success = true, data = true, });
        }

        [HttpGet]
        public IHttpActionResult ResultList(string id)
        {
            var user = Common.GetUserNameByASpUserID(id);
            if (user != null)
            {
                var ResultList = _questionPaperRepository.ResultList(id);
                return Ok(new { success = true, data = ResultList });
            }
            return Ok(new { success = false, message = "Invalid ID" });
        }
        [HttpGet]
        public IHttpActionResult CanCreateTest(string id)
        {
            var user = Common.GetUserNameByASpUserID(id);
            if (user != null)
            {
                var f = _enrollmentRepository.CanCreateTest(id);
                return Ok(new { success = true, data = f });
            }
            return Ok(new { success = false, message = "Invalid ID" });
        }


        #endregion

        #region Papers

        [HttpGet]
        public IHttpActionResult PaperList(string id, bool? MockTest = null)
        {
            var user = Common.GetUserNameByASpUserID(id);
            if (user != null)
            {
                var List = _questionPaperRepository.GetList(id, MockTest).ToList();
                return Ok(new { success = true, data = List });
            }
            return Ok(new { success = false, message = "Invalid ID" });
        }

        [HttpGet]
        public IHttpActionResult SelectMockTests(string id, int MockId)
        {
            var user = Common.GetUserNameByASpUserID(id);
            if (user != null)
            {
                var c = _generalRepository.SelectMockTests(MockId, id);
                return Json(new { success = true, data = c });
            }
            return Ok(new { success = false, message = "Invalid ID" });
        }
        #endregion

        #region Favourite

        [HttpGet]
        public IHttpActionResult AddToFavourits(string id, int QuestionId, bool Remove = false)
        {
            var user = Common.GetUserNameByASpUserID(id);
            if (user != null)
            {
                var model = new tbl_FavouriteList()
                {
                    Student_Fid = id,
                    ObjectType = QuestionType.Mcq.ToString(),
                    Object_ID = QuestionId,
                };
                bool f;
                if (Remove)
                    f = _favouritesRepository.RemoveFavourits(model);
                else
                    f = _favouritesRepository.AddToFavourits(model);
                return Ok(new { success = f, data = f, });
            }
            return Ok(new { success = false, message = "Invalid ID" });
        }

        #endregion
    }
}
