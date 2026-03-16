using First_Aid_Made_Easy.Models;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface IInstallmentRepository
    {
        List<StudentInstallmentVM> GetInstallmentList(string datef, string datet, string Teacherid, string Semail);
        bool InstallmentPaid(int id);
        bool IsPendingInstallment(string id);
    }
}
