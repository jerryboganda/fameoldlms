using System;
using System.Collections.Generic;
using First_Aid_Made_Easy.Models;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    /// <summary>
    /// Service interface for payout method and request management
    /// </summary>
    public interface IPayoutService : IDisposable
    {
        #region Payout Methods

        /// <summary>
        /// Get all payout methods for an ambassador
        /// </summary>
        List<PayoutMethodVM> GetPayoutMethods(int ambassadorId);

        /// <summary>
        /// Get payout method by ID
        /// </summary>
        PayoutMethodVM GetPayoutMethod(int methodId);

        /// <summary>
        /// Get default payout method for ambassador
        /// </summary>
        PayoutMethodVM GetDefaultMethod(int ambassadorId);

        /// <summary>
        /// Add a new payout method
        /// </summary>
        int AddPayoutMethod(int ambassadorId, PayoutMethodVM model);

        /// <summary>
        /// Set a method as default
        /// </summary>
        bool SetDefaultMethod(int ambassadorId, int methodId);

        /// <summary>
        /// Delete (deactivate) payout method
        /// </summary>
        bool DeletePayoutMethod(int methodId);

        /// <summary>
        /// Verify payout method
        /// </summary>
        bool VerifyPayoutMethod(int methodId, bool isVerified);

        /// <summary>
        /// Validate method ownership
        /// </summary>
        bool ValidateMethodOwnership(int methodId, int ambassadorId);

        /// <summary>
        /// Get methods (alias)
        /// </summary>
        List<PayoutMethodVM> GetMethods(int ambassadorId);

        #endregion

        #region Payout Requests - Ambassador

        /// <summary>
        /// Get min payout amount
        /// </summary>
        decimal GetMinPayoutAmount();

        /// <summary>
        /// Check if payout can be requested
        /// </summary>
        (bool Success, string Message) CanRequestPayout(int ambassadorId, decimal amount);

        /// <summary>
        /// Request a payout (Controller overload)
        /// </summary>
        int RequestPayout(int ambassadorId, int payoutMethodId, decimal amount);

        /// <summary>
        /// Cancel payout request (alias)
        /// </summary>
        bool CancelRequest(int requestId, int ambassadorId);

        /// <summary>
        /// Get payout requests for an ambassador
        /// </summary>
        List<PayoutRequestVM> GetPayoutRequests(int ambassadorId, string status = null, int page = 1, int pageSize = 20);

        /// <summary>
        /// Get payout request by ID
        /// </summary>
        PayoutRequestVM GetPayoutRequest(int requestId);

        /// <summary>
        /// Request a payout
        /// </summary>
        (bool Success, string Message, int RequestId) RequestPayout(int ambassadorId, decimal amount, int payoutMethodId, string notes = null);

        /// <summary>
        /// Cancel a pending payout request
        /// </summary>
        bool CancelPayoutRequest(int requestId, int ambassadorId);

        #endregion

        #region Admin Operations

        /// <summary>
        /// Get all payout requests (admin)
        /// </summary>
        List<PayoutRequestVM> GetAllPayoutRequests(string status = null, string search = null, int page = 1, int pageSize = 20);

        /// <summary>
        /// Get all payout requests with filter (admin)
        /// </summary>
        PagedResult<PayoutRequestVM> GetAllRequests(PayoutFilterVM filter);

        /// <summary>
        /// Approve a payout request
        /// </summary>
        bool ApproveRequest(int requestId, string adminUserId, string notes = null);

        /// <summary>
        /// Reject a payout request (with reason and notes)
        /// </summary>
        bool RejectRequest(int requestId, string adminUserId, string reason, string notes = null);

        /// <summary>
        /// Get payout summary for admin dashboard
        /// </summary>
        PayoutSummaryVM GetPayoutSummary();

        /// <summary>
        /// Start processing a payout request
        /// </summary>
        bool StartProcessing(int requestId, string adminUserId);

        /// <summary>
        /// Mark payout as paid
        /// </summary>
        bool MarkAsPaid(int requestId, string transactionRef, string adminUserId, string notes = null);

        /// <summary>
        /// Reject a payout request
        /// </summary>
        bool RejectPayout(int requestId, string reason, string adminUserId);

        #endregion
    }
}
