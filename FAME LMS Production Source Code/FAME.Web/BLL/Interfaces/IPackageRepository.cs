using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface IPackageRepository
    {
        bool Create(PackageVM model);
        PackageDurationVM GetDuration(int? id);
        bool SaveDuration(PackageDurationVM model);
        InstallmentVM CheckInstallments(int id);
        bool SaveInstallments(InstallmentVM model);
        InstallmentVM GetInstallments(int id);
        object Deactivate(int id);
        List<PackageDurationVM> getDurations(int id);
        PackageVM GetByID(int id);
        List<PackageVM> GetList();
        List<PackageVM> GetNameList();
        tbl_Package IsPackageTrue(int id, string secret, bool isCourse);
        string getText(int? Duration);
        string getInstText(int? Duration, int? numbr, decimal? Price, int? TotalDuration);
    }
}
