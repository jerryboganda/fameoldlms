using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using System;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface IEnrollmentRepository
    {
        bool Save(EnrollmentMasterVM model, bool Installments = false, bool Expire = false);
        bool Enroll(AddSubscriptioVM en, bool ByManual, bool ByRequest, decimal? DiscountedPrice);
        bool UpdateStatus(int ID, int Price, string Status);
        bool SendMail(string Email);
        List<sp_lstEnrollStatus_Result> GetInvoiceList(string datef, string datet, string StudentID, string status, bool? IsEmailSent = null);
        sp_CurrentPackDetail_Result GetCurrentEnroll(string ID);
        bool CanCreateTest(string student_Fid);
        bool HasActiveSubscription(string studentId);
        List<vw_StudetsList> GetStudentsList();
        string GetStudentID(string email);
        bool Block(string Email, bool IsActive);
        bool ResetPassword(string Email);
        bool VerifyEmail(string Email);
        bool CopySection(int Toid, int Sid, int Fromid);
        bool PasswordChanged(string id);
        AddPhoneNumberViewModel UserMobile(string id);
        bool ChangeUserMobile(string id, string Number);
        Tuple<string, int> GenerateVerificationCode(string id, CodeType CodeType, bool Again = false);
        string GenVerifCodeSession(string Email, bool Again = false);
        bool ConfirmEmail(string Email, string Code);
        bool ConfirmNewEmail(string Email, string Code);
        bool SaveExpire(UserLockoutVM model);
        bool DeleteExpire(int ID);
        tbl_UserLockout IsExpired(string ID);
        List<UserLockoutVM> GetExpireableStudent();
        UserLockoutVM GetExpire(int ID);
        int? GetType(string ID);
        bool AcceptRequest(int id, string UserID, decimal? Price);
        bool AcceptRequest(int id, int Price, int Duration, string Status, string UserID);
        bool AddRequest(RequestVM model);
        List<sp_lstRequests_Result> GetList(DateTime? DateFrom, DateTime? DateTo, int Type, string ReqFor);
        void DeleteFiles(tbl_User tuser);
        bool DeleteRequest(string id);
        bool HasPendingRequest(string studentId);
        SubscriptionsVM GetInvoiceDetail(int id);
        bool AddExtensionRequest(int enrollId, string studentId, string studentEmail);
    }
}
