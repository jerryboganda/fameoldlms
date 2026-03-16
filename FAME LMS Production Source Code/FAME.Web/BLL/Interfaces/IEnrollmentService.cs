using First_Aid_Made_Easy.Models;
using System.Collections.Generic;
using First_Aid_Made_Easy.DAL;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface IEnrollmentService
    {
        bool ProcessEnrollment(AddSubscriptioVM en, bool ByManual, bool ByRequest, decimal? DiscountedPrice);
        bool ApproveEnrollmentRequest(int requestId, string approvedBy, decimal? customizedPrice);
        bool ExtendEnrollment(int requestId, int newPrice, int newDuration, string status, string approvedBy);
        bool ToggleStudentBlockStatus(string email, bool isActive);
    }
}
