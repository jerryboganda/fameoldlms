using System;
using System.Collections.Generic;
using First_Aid_Made_Easy.Models;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    /// <summary>
    /// Service interface for commission calculation and management
    /// </summary>
    public interface ICommissionService : IDisposable
    {
        #region Rule Management

        /// <summary>
        /// Get commission rules (all or active only)
        /// </summary>
        List<CommissionRuleVM> GetRules(bool activeOnly = true);

        /// <summary>
        /// Get active commission rules, optionally filtered by tier
        /// </summary>
        List<CommissionRuleVM> GetActiveRules(string tier = null);

        /// <summary>
        /// Get commission rule by ID
        /// </summary>
        CommissionRuleVM GetRule(int ruleId);

        /// <summary>
        /// Create a new commission rule
        /// </summary>
        int CreateRule(CommissionRuleVM model);

        /// <summary>
        /// Update commission rule
        /// </summary>
        bool UpdateRule(CommissionRuleVM model);

        /// <summary>
        /// Toggle commission rule active state
        /// </summary>
        bool ToggleRule(int ruleId, bool isActive);

        #endregion

        #region Commission Calculation

        /// <summary>
        /// Calculate commission for an ambassador based on event type and order amount
        /// </summary>
        decimal CalculateCommission(int ambassadorId, string eventType, decimal orderAmount = 0);

        /// <summary>
        /// Award commission to an ambassador
        /// </summary>
        int AwardCommission(int ambassadorId, int? referralId, int? conversionId, string earningType, decimal amount, string description);

        /// <summary>
        /// Update earning status
        /// </summary>
        bool UpdateEarningStatus(int earningId, string status);

        #endregion

        #region Earnings Queries

        /// <summary>
        /// Get earnings for an ambassador with optional filters
        /// </summary>
        List<EarningVM> GetEarnings(int ambassadorId, string status = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 20);

        /// <summary>
        /// Get balance summary for an ambassador
        /// </summary>
        BalanceVM GetBalance(int ambassadorId);

        /// <summary>
        /// Get total earnings for an ambassador within a date range
        /// </summary>
        decimal GetTotalEarnings(int ambassadorId, DateTime? from = null, DateTime? to = null);

        /// <summary>
        /// Get monthly earnings data for charts
        /// </summary>
        List<ChartDataPointVM> GetMonthlyEarnings(int ambassadorId, int months = 6);

        #endregion

        #region Settings

        /// <summary>
        /// Get minimum payout amount
        /// </summary>
        decimal GetMinPayoutAmount();

        /// <summary>
        /// Get hold period days for commissions
        /// </summary>
        int GetHoldPeriodDays();

        /// <summary>
        /// Add manual adjustment to ambassador earnings
        /// </summary>
        int AddAdjustment(int ambassadorId, decimal amount, string type, string description, string adminUserId);

        #endregion
    }
}
