using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LinqKit;
using First_Aid_Made_Easy.DAL;
using System.Data.Entity;

namespace First_Aid_Made_Easy.BLL
{
    public class InstallmentRepository : IInstallmentRepository
    {
        public List<StudentInstallmentVM> GetInstallmentList(string datef, string datet, string Teacherid, string Semail)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    var pre = PredicateBuilder.New<tbl_StudentInstallments>();

                    if (!string.IsNullOrEmpty(Semail))
                        pre.And(x => x.AspNetUsers.Email.Contains(Semail));
                    if ((!string.IsNullOrEmpty(datet)) && (!string.IsNullOrEmpty(datef)))
                    {
                        DateTime datefrom = Common.StringToDate(datef);
                        DateTime dateto = Common.StringToDate(datet);
                        pre.And(x => datefrom.Date <= x.InstallmentDate.Value.Date && x.InstallmentDate.Value.Date <= dateto.Date);
                    }

                    var List = db.tbl_StudentInstallments.Where(pre.Compile()).ToList();

                    var listvm = List.ConvertAll(z => new StudentInstallmentVM
                    {
                        InstallmentPrice = z.InstallmentPrice,
                        InstallmentDate = Common.DateToString(z.InstallmentDate),
                        IsPaid = z.IsPaid,
                        Student = z.AspNetUsers?.UserName,
                        ID = z.ID,
                        PackageName = z.tbl_Package?.PackageName,
                        PackageDur = "",
                        InstallmentNo = z.InstallmentNo,
                    });
                    return listvm;
                }
            }
            catch
            {
                return new List<StudentInstallmentVM>();
            }
        }

        public bool InstallmentPaid(int id)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    var req = db.tbl_StudentInstallments.Find(id);
                    req.IsPaid = true;
                    db.Entry(req).State = EntityState.Modified;
                    db.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        public bool IsPendingInstallment(string id)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    var CDate = Common.GetCurrentDate();
                    return db.tbl_StudentInstallments.Any(x => x.tbl_EnrollmentMaster.StudentFid == id && x.IsPaid != true && x.InstallmentDate < CDate);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}