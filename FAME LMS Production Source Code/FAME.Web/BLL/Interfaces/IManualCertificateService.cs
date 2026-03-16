using First_Aid_Made_Easy.Models.Certificate;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface IManualCertificateService
    {
        /// <summary>Get all active courses for the dropdown.</summary>
        List<CourseDropdownItem> GetAllCourses();

        /// <summary>Get all active packages for the dropdown.</summary>
        List<PackageDropdownItem> GetAllPackages();

        /// <summary>Get enrolled users for a specific course.</summary>
        List<EnrolledUserRow> GetUsersByCourse(int courseId);

        /// <summary>Get enrolled users for a specific package.</summary>
        List<EnrolledUserRow> GetUsersByPackage(int packageId);

        /// <summary>Upload a manual certificate for a user.</summary>
        int UploadManualCertificate(string userId, int? courseId, int? packageId,
            string learnerName, string fileName, string filePath, string title, string uploadedBy);

        /// <summary>Get all manual certificates for a student.</summary>
        List<ManualCertificateVM> GetManualCertificatesForUser(string userId);

        /// <summary>Delete (soft) a manual certificate.</summary>
        void DeleteManualCertificate(int id, string adminUserId);
    }
}
