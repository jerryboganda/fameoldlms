using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace First_Aid_Made_Easy.BLL
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IEnrollmentRepository _enrollmentRepo;
        private readonly ICourseRepository _courseRepo;
        private readonly ISectionRepository _sectionRepo;
        private readonly IPackageRepository _packageRepo;
        private readonly IUserRepository _userRepo;

        public EnrollmentService(
            IEnrollmentRepository enrollmentRepo,
            ICourseRepository courseRepo,
            ISectionRepository sectionRepo,
            IPackageRepository packageRepo,
            IUserRepository userRepo)
        {
            _enrollmentRepo = enrollmentRepo;
            _courseRepo = courseRepo;
            _sectionRepo = sectionRepo;
            _packageRepo = packageRepo;
            _userRepo = userRepo;
        }

        public bool ProcessEnrollment(AddSubscriptioVM en, bool ByManual, bool ByRequest, decimal? DiscountedPrice)
        {
            // The logic from EnrollmentRepository.Enroll moves here
            // but coordinates multiple repositories
            var result = _enrollmentRepo.Enroll(en, ByManual, ByRequest, DiscountedPrice);
            
            // AMBASSADOR PROGRAM: Award commission if enrollment successful
            // This is SAFE - wrapped in try-catch, checks feature flags, never throws
            if (result && !string.IsNullOrEmpty(en.StudentFid))
            {
                try
                {
                    var purchaseAmount = DiscountedPrice ?? 0;
                    if (purchaseAmount > 0)
                    {
                        PurchaseCommissionHelper.TryAwardPurchaseCommission(
                            en.StudentFid, 
                            purchaseAmount, 
                            "Subscription", 
                            en.EnrollmentFor);
                    }
                }
                catch
                {
                    // Safely ignore - commission award should never break enrollment
                }
            }
            
            return result;
        }

        public bool ApproveEnrollmentRequest(int requestId, string approvedBy, decimal? customizedPrice)
        {
            // Business logic for approving a request, potentially sending notifications
            return _enrollmentRepo.AcceptRequest(requestId, approvedBy, customizedPrice);
        }

        public bool ExtendEnrollment(int requestId, int newPrice, int newDuration, string status, string approvedBy)
        {
            return _enrollmentRepo.AcceptRequest(requestId, newPrice, newDuration, status, approvedBy);
        }

        public bool ToggleStudentBlockStatus(string email, bool isActive)
        {
            // Business logic: check if student has active enrollments before blocking?
            // For now, delegate to repo
            return _enrollmentRepo.Block(email, isActive);
        }
    }
}
