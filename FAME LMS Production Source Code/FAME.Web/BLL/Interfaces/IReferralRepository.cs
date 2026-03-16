using System;
using System.Collections.Generic;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    /// <summary>
    /// Repository interface for Referral tracking operations
    /// </summary>
    public interface IReferralRepository : IDisposable
    {
        #region Click Tracking

        int TrackClick(string referralCode, string ipAddress, string userAgent, string landingPage, string source);
        int GetClickCount(int ambassadorId, DateTime? from = null, DateTime? to = null);

        #endregion

        #region Referral Management

        tbl_Referral GetById(int id);
        tbl_Referral GetByReferredUserId(string userId);
        List<ReferralVM> GetReferralsByAmbassador(int ambassadorId, string status = null, int page = 1, int pageSize = 20);
        PagedResult<ReferralVM> GetByAmbassador(int ambassadorId, ReferralFilterVM filter);
        int GetReferralCount(int ambassadorId, string status = null, DateTime? from = null, DateTime? to = null);
        int GetCountByStatus(int ambassadorId, string status);
        Dictionary<string, int> GetAllStatusCounts(int ambassadorId);
        int CreateReferral(int ambassadorId, string referredUserId, string referredEmail, string referredName, string source, int? clickId = null);
        bool UpdateReferralStatus(int referralId, string status);

        #endregion

        #region Conversion Tracking

        int RecordConversion(int referralId, string conversionType, decimal? amount = null, int? orderId = null);
        List<ConversionVM> GetConversionsByAmbassador(int ambassadorId, DateTime? from = null, DateTime? to = null);
        PagedResult<ConversionVM> GetAllConversions(ConversionFilterVM filter);
        bool ApproveConversion(int conversionId, string adminUserId);
        bool RevokeConversion(int conversionId, string reason, string adminUserId);
        int GetConversionCount(int ambassadorId, DateTime? from = null, DateTime? to = null);
        decimal GetConversionValue(int ambassadorId, DateTime? from = null, DateTime? to = null);

        #endregion

        #region Analytics

        Dictionary<string, int> GetReferralsBySource(int ambassadorId);
        List<ChartDataPointVM> GetDailyReferrals(int ambassadorId, int days = 30);
        List<ChartDataPointVM> GetMonthlyReferrals(int ambassadorId, int months = 6);

        #endregion
    }
}
