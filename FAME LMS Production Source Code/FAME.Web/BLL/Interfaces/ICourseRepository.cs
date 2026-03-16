using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface ICourseRepository
    {
        List<CourseVM> GetOurCourses();
        List<CourseVM> GetList(string UserID);
        int SaveCourse(CourseVM course);
        MyCoursesVM GetListByStudent(string student_Fid);
        List<CourseVM> GetListVM(string ID, bool IsAdmin);
        List<tbl_Courses> GetList(string id = "", bool isAdmin = true);
        CourseVM GetByID(int id, string Uid);
        CourseVM GetDetailsByID(int id);
        CourseVM GetByID(int id);
        ChapterVM GetChapterByID(int ChapterID);
        int SaveChapter(ChapterVM x);
        bool DeleteChapter(int id);
        bool CopyVideos(int sectionId, int? SectionSubID, string videoIds);
        bool SaveSorting(SortingVM d);
    }
}
