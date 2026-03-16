using System;
using System.Collections.Generic;
using First_Aid_Made_Easy.Models;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    /// <summary>
    /// Repository interface for Ambassador entity operations
    /// </summary>
    public interface IAmbassadorRepository
    {
        #region Ambassador CRUD

        /// <summary>
        /// Get ambassador by ID
        /// </summary>
        AmbassadorVM GetById(int id);

        /// <summary>
        /// Get ambassador by User ID
        /// </summary>
        AmbassadorVM GetByUserId(string userId);

        /// <summary>
        /// Get ambassador by referral code
        /// </summary>
        AmbassadorVM GetByReferralCode(string code);

        /// <summary>
        /// Get filtered list of ambassadors
        /// </summary>
        PagedResultVM<AmbassadorListItemVM> GetList(AmbassadorFilterVM filter);

        /// <summary>
        /// Get all active ambassadors (for dropdowns)
        /// </summary>
        List<AmbassadorListItemVM> GetActiveAmbassadors();

        /// <summary>
        /// Get pending applications
        /// </summary>
        List<AmbassadorListItemVM> GetPendingApplications();

        #endregion

        #region Ambassador Lifecycle

        /// <summary>
        /// Submit ambassador application
        /// </summary>
        /// <returns>Ambassador ID</returns>
        int Apply(string userId, AmbassadorApplicationVM model, string fullName = null);

        /// <summary>
        /// Approve ambassador application
        /// </summary>
        bool Approve(int ambassadorId, string approvedBy);

        /// <summary>
        /// Reject ambassador application
        /// </summary>
        bool Reject(int ambassadorId, string reason, string rejectedBy);

        /// <summary>
        /// Suspend ambassador
        /// </summary>
        bool Suspend(int ambassadorId, string reason, string suspendedBy);

        /// <summary>
        /// Reactivate suspended ambassador
        /// </summary>
        bool Reactivate(int ambassadorId, string reactivatedBy);

        /// <summary>
        /// Update ambassador tier
        /// </summary>
        bool UpdateTier(int ambassadorId, string tier, string updatedBy);

        /// <summary>
        /// Update ambassador profile
        /// </summary>
        bool UpdateProfile(int ambassadorId, AmbassadorApplicationVM model);

        #endregion

        #region Dashboard

        /// <summary>
        /// Get ambassador dashboard data
        /// </summary>
        AmbassadorDashboardVM GetDashboard(int ambassadorId);

        /// <summary>
        /// Get dashboard statistics
        /// </summary>
        DashboardStatsVM GetStats(int ambassadorId);

        /// <summary>
        /// Get chart data for clicks/conversions
        /// </summary>
        ChartDataVM GetChartData(int ambassadorId, string chartType, int days = 30);

        #endregion

        #region Admin Dashboard

        /// <summary>
        /// Get admin dashboard
        /// </summary>
        AdminAmbassadorDashboardVM GetAdminDashboard();

        /// <summary>
        /// Get admin statistics
        /// </summary>
        AdminStatsVM GetAdminStats();

        /// <summary>
        /// Get top performing ambassadors
        /// </summary>
        List<AmbassadorListItemVM> GetTopAmbassadors(int count = 10);

        #endregion

        #region Validation

        /// <summary>
        /// Check if referral code is valid and belongs to an active ambassador
        /// </summary>
        bool IsValidReferralCode(string code);

        /// <summary>
        /// Check if user can apply to become an ambassador
        /// </summary>
        bool CanApply(string userId);

        /// <summary>
        /// Check if user is an active ambassador
        /// </summary>
        bool IsActiveAmbassador(string userId);

        /// <summary>
        /// Generate unique referral code
        /// </summary>
        string GenerateReferralCode();

        #endregion
    }
}
