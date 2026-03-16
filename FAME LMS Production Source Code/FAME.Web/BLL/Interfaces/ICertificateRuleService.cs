using First_Aid_Made_Easy.Models.Certificate;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface ICertificateRuleService
    {
        // CRUD
        CertificateRuleSetVM GetRuleSetById(int id);
        List<CertificateRuleSetVM> GetRuleSetsForTemplate(int templateId);
        List<CertificateRuleSetVM> GetActiveRuleSets();
        int SaveRuleSet(CertificateRuleSetVM model, string userId);
        void DeactivateRuleSet(int id, string userId);

        // Eligibility evaluation
        EligibilityResult EvaluateEligibility(string userId, int courseId, tbl_CertificateRuleSet ruleSet);
        EligibilityResult EvaluateEligibility(string userId, int courseId, int ruleSetId);

        // Check and auto-issue (called from completion hooks)
        void CheckAndIssueIfEligible(string userId, int courseId);

        // Helpers
        List<CourseSelectItem> GetCoursesForDropdown();
    }

    public class CourseSelectItem
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }

        // View aliases
        public int Id => CourseId;
        public string Name => CourseName;
    }
}
