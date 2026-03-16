using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace First_Aid_Made_Easy.BLL
{
    /// <summary>
    /// Service for managing ambassador payout methods and payout requests
    /// </summary>
    public class PayoutService : IPayoutService
    {
        private readonly AmbassadorDbContext _context;
        private readonly ILogger _logger;

        public PayoutService()
        {
            _context = new AmbassadorDbContext();
            _logger = Log.ForContext<PayoutService>();
        }

        #region Payout Methods

        public List<PayoutMethodVM> GetPayoutMethods(int ambassadorId)
        {
            return _context.tbl_PayoutMethod
                .Where(m => m.AmbassadorId == ambassadorId && m.IsActive)
                .OrderByDescending(m => m.IsDefault)
                .ThenByDescending(m => m.CreatedAt)
                .ToList()
                .Select(MapToMethodVM)
                .ToList();
        }

        public PayoutMethodVM GetPayoutMethod(int methodId)
        {
            var method = _context.tbl_PayoutMethod.Find(methodId);
            return method != null ? MapToMethodVM(method) : null;
        }

        public PayoutMethodVM GetDefaultMethod(int ambassadorId)
        {
            var method = _context.tbl_PayoutMethod
                .FirstOrDefault(m => m.AmbassadorId == ambassadorId && m.IsDefault && m.IsActive);
            return method != null ? MapToMethodVM(method) : null;
        }

        public int AddPayoutMethod(int ambassadorId, PayoutMethodVM model)
        {
            try
            {
                var normalizedModel = NormalizeMethodModel(model);
                if (!IsValidMethod(normalizedModel))
                {
                    return 0;
                }

                // If this is the first method or set as default, update existing defaults
                if (normalizedModel.IsDefault)
                {
                    var existingDefaults = _context.tbl_PayoutMethod
                        .Where(m => m.AmbassadorId == ambassadorId && m.IsDefault);
                    foreach (var m in existingDefaults)
                        m.IsDefault = false;
                }

                // Check if this is the first method
                var hasExisting = _context.tbl_PayoutMethod.Any(m => m.AmbassadorId == ambassadorId && m.IsActive);

                var method = new tbl_PayoutMethod
                {
                    AmbassadorId = ambassadorId,
                    MethodType = normalizedModel.MethodType,
                    AccountHolderName = normalizedModel.AccountHolderName,
                    AccountTitle = normalizedModel.AccountHolderName,
                    AccountNumber = normalizedModel.AccountNumber,
                    MaskedAccountNumber = MaskAccountNumber(normalizedModel.AccountNumber),
                    EncryptedPayload = BuildMethodPayload(normalizedModel),
                    BankName = normalizedModel.BankName,
                    BankBranch = normalizedModel.BankBranch,
                    BranchCode = normalizedModel.BankBranch,
                    MobileProvider = normalizedModel.MobileProvider,
                    IsDefault = normalizedModel.IsDefault || !hasExisting,
                    IsVerified = false, // Requires admin verification
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.tbl_PayoutMethod.Add(method);
                _context.SaveChanges();

                _logger.Information("Payout method added for ambassador {AmbId}: {Type}", ambassadorId, normalizedModel.MethodType);
                return method.Id;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error adding payout method for ambassador {AmbId}", ambassadorId);
                return 0;
            }
        }

        public bool SetDefaultMethod(int ambassadorId, int methodId)
        {
            try
            {
                var methods = _context.tbl_PayoutMethod.Where(m => m.AmbassadorId == ambassadorId && m.IsActive).ToList();

                foreach (var m in methods)
                {
                    m.IsDefault = (m.Id == methodId);
                }

                _context.SaveChanges();
                _logger.Information("Default payout method set: {MethodId} for ambassador {AmbId}", methodId, ambassadorId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error setting default method {MethodId}", methodId);
                return false;
            }
        }

        public bool DeletePayoutMethod(int methodId)
        {
            try
            {
                var method = _context.tbl_PayoutMethod.Find(methodId);
                if (method == null) return false;

                // Check for pending payouts using this method
                var hasPendingPayouts = _context.tbl_PayoutRequest
                    .Any(p => p.PayoutMethodId == methodId && (p.Status == "Requested" || p.Status == "Processing"));

                if (hasPendingPayouts)
                {
                    _logger.Warning("Cannot delete payout method {Id} - has pending payouts", methodId);
                    return false;
                }

                method.IsActive = false;
                _context.SaveChanges();

                _logger.Information("Payout method {Id} deactivated", methodId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting payout method {Id}", methodId);
                return false;
            }
        }

        public bool VerifyPayoutMethod(int methodId, bool isVerified)
        {
            try
            {
                var method = _context.tbl_PayoutMethod.Find(methodId);
                if (method == null) return false;

                method.IsVerified = isVerified;
                _context.SaveChanges();

                _logger.Information("Payout method {Id} verification status: {Verified}", methodId, isVerified);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error verifying payout method {Id}", methodId);
                return false;
            }
        }

        #endregion

        #region Payout Requests

        public List<PayoutRequestVM> GetPayoutRequests(int ambassadorId, string status = null, int page = 1, int pageSize = 20)
        {
            var query = _context.tbl_PayoutRequest
                .Include(p => p.tbl_PayoutMethod)
                .Where(p => p.AmbassadorId == ambassadorId);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            return query
                .OrderByDescending(p => p.RequestedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList()
                .Select(MapToRequestVM)
                .ToList();
        }

        public PayoutRequestVM GetPayoutRequest(int requestId)
        {
            var request = _context.tbl_PayoutRequest
                .Include(p => p.tbl_PayoutMethod)
                .FirstOrDefault(p => p.Id == requestId);
            return request != null ? MapToRequestVM(request) : null;
        }

        public (bool Success, string Message, int RequestId) RequestPayout(int ambassadorId, decimal amount, int payoutMethodId, string notes = null)
        {
            try
            {
                // Validate ambassador
                var ambassador = _context.tbl_Ambassador.Find(ambassadorId);
                if (ambassador == null || !ambassador.IsActive)
                    return (false, "Ambassador account is not active.", 0);

                // Validate payout method
                var method = _context.tbl_PayoutMethod.Find(payoutMethodId);
                if (method == null || method.AmbassadorId != ambassadorId || !method.IsActive)
                    return (false, "Invalid payout method.", 0);

                if (!method.IsVerified)
                    return (false, "Payout method is not verified yet.", 0);

                // Calculate available balance
                var balance = CalculateAvailableBalance(ambassadorId);
                if (amount > balance)
                    return (false, "Insufficient balance.", 0);

                // Check minimum payout
                var minPayout = 500m; // Could be from settings
                if (amount < minPayout)
                    return (false, $"Minimum payout amount is {AmbassadorSettingsHelper.GetDefaultCurrency()} {minPayout:N0}.", 0);

                // Check for pending requests
                var hasPending = _context.tbl_PayoutRequest
                    .Any(p => p.AmbassadorId == ambassadorId && (p.Status == "Requested" || p.Status == "Processing"));
                if (hasPending)
                    return (false, "You have a pending payout request. Please wait for it to be processed.", 0);

                // Create request
                var request = new tbl_PayoutRequest
                {
                    AmbassadorId = ambassadorId,
                    PayoutMethodId = payoutMethodId,
                    Amount = amount,
                    Currency = AmbassadorSettingsHelper.GetDefaultCurrency(),
                    Status = "Requested",
                    Notes = notes,
                    RequestedAt = DateTime.UtcNow
                };

                _context.tbl_PayoutRequest.Add(request);
                _context.SaveChanges();

                _logger.Information("Payout request created: {Amount} for ambassador {AmbId}", amount, ambassadorId);
                return (true, "Payout request submitted successfully.", request.Id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating payout request for ambassador {AmbId}", ambassadorId);
                return (false, "An error occurred processing your request.", 0);
            }
        }

        public bool CancelPayoutRequest(int requestId, int ambassadorId)
        {
            try
            {
                var request = _context.tbl_PayoutRequest.Find(requestId);
                if (request == null || request.AmbassadorId != ambassadorId)
                    return false;

                if (request.Status != "Requested")
                    return false; // Can only cancel pending requests

                request.Status = "Cancelled";
                _context.SaveChanges();

                _logger.Information("Payout request {Id} cancelled by ambassador {AmbId}", requestId, ambassadorId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error cancelling payout request {Id}", requestId);
                return false;
            }
        }

        #endregion

        #region Admin Operations

        public List<PayoutRequestVM> GetAllPayoutRequests(string status = null, string search = null, int page = 1, int pageSize = 20)
        {
            var query = _context.tbl_PayoutRequest
                .Include(p => p.tbl_PayoutMethod)
                .Include(p => p.tbl_Ambassador)
                .Include(p => p.tbl_Ambassador.AspNetUsers)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    (p.tbl_Ambassador.FullName != null && p.tbl_Ambassador.FullName.Contains(search)) ||
                    p.tbl_Ambassador.AspNetUsers.UserName.Contains(search) ||
                    p.tbl_Ambassador.AspNetUsers.Email.Contains(search));
            }

            return query
                .OrderByDescending(p => p.RequestedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList()
                .Select(MapToRequestVM)
                .ToList();
        }

        public PagedResult<PayoutRequestVM> GetAllRequests(PayoutFilterVM filter)
        {
            var query = _context.tbl_PayoutRequest
                .Include(p => p.tbl_PayoutMethod)
                .Include(p => p.tbl_Ambassador)
                .Include(p => p.tbl_Ambassador.AspNetUsers)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(p => p.Status == filter.Status);

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(p =>
                    (p.tbl_Ambassador.FullName != null && p.tbl_Ambassador.FullName.Contains(filter.SearchTerm)) ||
                    p.tbl_Ambassador.AspNetUsers.UserName.Contains(filter.SearchTerm) ||
                    p.tbl_Ambassador.AspNetUsers.Email.Contains(filter.SearchTerm));
            }

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(p => p.RequestedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList()
                .Select(MapToRequestVM)
                .ToList();

            return new PagedResult<PayoutRequestVM>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public bool ApproveRequest(int requestId, string adminUserId, string notes = null)
        {
            return StartProcessing(requestId, adminUserId);
        }

        public bool RejectRequest(int requestId, string adminUserId, string reason, string notes = null)
        {
            return RejectPayout(requestId, reason, adminUserId);
        }

        public PayoutSummaryVM GetPayoutSummary()
        {
            var requests = _context.tbl_PayoutRequest.ToList();
            var thisMonth = DateTime.UtcNow.Date.AddDays(-DateTime.UtcNow.Day + 1);

            return new PayoutSummaryVM
            {
                PendingCount = requests.Count(r => r.Status == "Requested"),
                PendingAmount = requests.Where(r => r.Status == "Requested").Sum(r => r.Amount),
                ProcessingCount = requests.Count(r => r.Status == "Processing"),
                ProcessingAmount = requests.Where(r => r.Status == "Processing").Sum(r => r.Amount),
                PaidCount = requests.Count(r => r.Status == "Paid" && r.PaidAt >= thisMonth),
                PaidAmount = requests.Where(r => r.Status == "Paid" && r.PaidAt >= thisMonth).Sum(r => r.Amount),
                RejectedCount = requests.Count(r => r.Status == "Rejected"),
                RejectedAmount = requests.Where(r => r.Status == "Rejected").Sum(r => r.Amount),
                Currency = AmbassadorSettingsHelper.GetDefaultCurrency()
            };
        }

        public bool StartProcessing(int requestId, string adminUserId)
        {
            try
            {
                var request = _context.tbl_PayoutRequest.Find(requestId);
                if (request == null || request.Status != "Requested")
                    return false;

                request.Status = "Processing";
                request.ProcessedBy = adminUserId;
                request.ProcessedAt = DateTime.UtcNow;
                _context.SaveChanges();

                _logger.Information("Payout request {Id} marked as processing by {Admin}", requestId, adminUserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error starting processing for request {Id}", requestId);
                return false;
            }
        }

        public bool MarkAsPaid(int requestId, string transactionRef, string adminUserId, string notes = null)
        {
            try
            {
                var request = _context.tbl_PayoutRequest.Find(requestId);
                if (request == null || (request.Status != "Requested" && request.Status != "Processing"))
                    return false;

                request.Status = "Paid";
                request.TransactionRef = transactionRef;
                request.ProcessedBy = adminUserId;
                request.ProcessedAt = DateTime.UtcNow;
                request.PaidAt = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(notes))
                    request.Notes = (request.Notes ?? "") + "\n[Admin] " + notes;

                _context.SaveChanges();

                _logger.Information("Payout request {Id} marked as paid. Ref: {Ref}", requestId, transactionRef);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error marking request {Id} as paid", requestId);
                return false;
            }
        }

        public bool RejectPayout(int requestId, string reason, string adminUserId)
        {
            try
            {
                var request = _context.tbl_PayoutRequest.Find(requestId);
                if (request == null || (request.Status != "Requested" && request.Status != "Processing"))
                    return false;

                request.Status = "Rejected";
                request.RejectionReason = reason;
                request.ProcessedBy = adminUserId;
                request.ProcessedAt = DateTime.UtcNow;
                _context.SaveChanges();

                _logger.Information("Payout request {Id} rejected: {Reason}", requestId, reason);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error rejecting request {Id}", requestId);
                return false;
            }
        }

        #endregion

        #region Controller Helpers

        public bool ValidateMethodOwnership(int methodId, int ambassadorId)
        {
            var method = _context.tbl_PayoutMethod.Find(methodId);
            return method != null && method.AmbassadorId == ambassadorId;
        }

        public List<PayoutMethodVM> GetMethods(int ambassadorId)
        {
            return GetPayoutMethods(ambassadorId);
        }

        public decimal GetMinPayoutAmount()
        {
            return AmbassadorSettingsHelper.GetMinPayoutAmount();
        }

        public (bool Success, string Message) CanRequestPayout(int ambassadorId, decimal amount)
        {
             // Check min amount
             if (amount < GetMinPayoutAmount())
                 return (false, $"Minimum payout amount is {AmbassadorSettingsHelper.GetDefaultCurrency()} {GetMinPayoutAmount():N0}.");

             // Check balance
             var balance = CalculateAvailableBalance(ambassadorId);
             if (amount > balance)
                 return (false, "Insufficient wallet balance.");

             // Check pending
             var hasPending = _context.tbl_PayoutRequest
                 .Any(p => p.AmbassadorId == ambassadorId && (p.Status == "Requested" || p.Status == "Processing"));
             
             if (hasPending)
                 return (false, "You already have a pending payout request.");

             return (true, "Success");
        }

        public int RequestPayout(int ambassadorId, int payoutMethodId, decimal amount)
        {
            var result = RequestPayout(ambassadorId, amount, payoutMethodId);
            if (!result.Success)
            {
               throw new InvalidOperationException(result.Message);
            }
            return result.RequestId;
        }

        public bool CancelRequest(int requestId, int ambassadorId)
        {
            return CancelPayoutRequest(requestId, ambassadorId);
        }

        #endregion

        #region Helpers

        private decimal CalculateAvailableBalance(int ambassadorId)
        {
            var now = DateTime.UtcNow;

            var earnings = _context.tbl_AmbassadorEarning
                .Where(e => e.AmbassadorId == ambassadorId &&
                           e.Status != "Cancelled" &&
                           (e.Status == "Available" || e.AvailableAt <= now))
                .Sum(e => (decimal?)e.Amount) ?? 0;

            var paidOut = _context.tbl_PayoutRequest
                .Where(p => p.AmbassadorId == ambassadorId && p.Status == "Paid")
                .Sum(p => (decimal?)p.Amount) ?? 0;

            var pending = _context.tbl_PayoutRequest
                .Where(p => p.AmbassadorId == ambassadorId && (p.Status == "Requested" || p.Status == "Processing"))
                .Sum(p => (decimal?)p.Amount) ?? 0;

            return earnings - paidOut - pending;
        }

        private PayoutMethodVM MapToMethodVM(tbl_PayoutMethod method)
        {
            var maskedAccount = string.IsNullOrWhiteSpace(method.MaskedAccountNumber)
                ? method.AccountNumber
                : method.MaskedAccountNumber;
            if (!string.IsNullOrEmpty(maskedAccount) && maskedAccount.Length > 4)
            {
                maskedAccount = new string('*', maskedAccount.Length - 4) + maskedAccount.Substring(maskedAccount.Length - 4);
            }

            return new PayoutMethodVM
            {
                Id = method.Id,
                AmbassadorId = method.AmbassadorId,
                MethodType = method.MethodType,
                AccountHolderName = string.IsNullOrWhiteSpace(method.AccountHolderName) ? method.AccountTitle : method.AccountHolderName,
                AccountNumber = method.AccountNumber,
                MaskedAccountNumber = maskedAccount,
                BankName = method.BankName,
                BankBranch = string.IsNullOrWhiteSpace(method.BankBranch) ? method.BranchCode : method.BankBranch,
                MobileProvider = method.MobileProvider,
                IsDefault = method.IsDefault,
                IsVerified = method.IsVerified,
                CreatedAt = method.CreatedAt,
                MethodTypeDisplay = method.MethodType == "BankTransfer" ? "Bank Transfer" : method.MethodType == "MobileMoney" ? "Mobile Money" : method.MethodType,
                MethodIcon = method.MethodType == "BankTransfer" ? "account_balance" : "phone_android"
            };
        }

        private PayoutRequestVM MapToRequestVM(tbl_PayoutRequest request)
        {
            var statusBadge = "badge-secondary";
            switch (request.Status)
            {
                case "Requested": statusBadge = "badge-warning"; break;
                case "Processing": statusBadge = "badge-info"; break;
                case "Paid": statusBadge = "badge-success"; break;
                case "Rejected": statusBadge = "badge-danger"; break;
                case "Cancelled": statusBadge = "badge-secondary"; break;
            }

            var vm = new PayoutRequestVM
            {
                Id = request.Id,
                AmbassadorId = request.AmbassadorId,
                Amount = request.Amount,
                Currency = request.Currency,
                Status = request.Status,
                StatusBadgeClass = statusBadge,
                Notes = request.Notes,
                TransactionRef = request.TransactionRef,
                RejectionReason = request.RejectionReason,
                RequestedAt = request.RequestedAt,
                PaidAt = request.PaidAt
            };

            if (request.tbl_PayoutMethod != null)
            {
                vm.PayoutMethod = MapToMethodVM(request.tbl_PayoutMethod);
            }

            if (request.tbl_Ambassador != null)
            {
                var ambassador = request.tbl_Ambassador;
                var user = ambassador.AspNetUsers;
                
                vm.AmbassadorName = !string.IsNullOrWhiteSpace(ambassador.FullName) ? ambassador.FullName : user?.UserName ?? "Unknown";
                vm.AmbassadorEmail = user?.Email ?? "Unknown";
                
                var initials = "";
                if (!string.IsNullOrEmpty(vm.AmbassadorName))
                {
                    var parts = vm.AmbassadorName.Split(' ');
                    initials = string.Concat(parts.Take(2).Select(p => p.Length > 0 ? p[0].ToString().ToUpper() : ""));
                }
                vm.AmbassadorInitials = initials;
            }

            return vm;
        }

        private static PayoutMethodVM NormalizeMethodModel(PayoutMethodVM model)
        {
            if (model == null)
            {
                return null;
            }

            var methodType = string.Equals(model.MethodType, "MobileMoney", StringComparison.OrdinalIgnoreCase)
                ? "MobileMoney"
                : "BankTransfer";

            return new PayoutMethodVM
            {
                MethodType = methodType,
                AccountHolderName = (model.AccountHolderName ?? model.MobileAccountName ?? string.Empty).Trim(),
                AccountNumber = (model.AccountNumber ?? model.PhoneNumber ?? string.Empty).Trim(),
                BankName = string.IsNullOrWhiteSpace(model.BankName) ? null : model.BankName.Trim(),
                BankBranch = string.IsNullOrWhiteSpace(model.BankBranch) ? null : model.BankBranch.Trim(),
                MobileProvider = string.IsNullOrWhiteSpace(model.MobileProvider) ? null : model.MobileProvider.Trim(),
                IsDefault = model.IsDefault
            };
        }

        private static bool IsValidMethod(PayoutMethodVM model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.MethodType) || string.IsNullOrWhiteSpace(model.AccountHolderName) || string.IsNullOrWhiteSpace(model.AccountNumber))
            {
                return false;
            }

            if (model.MethodType == "BankTransfer")
            {
                return !string.IsNullOrWhiteSpace(model.BankName);
            }

            return !string.IsNullOrWhiteSpace(model.MobileProvider);
        }

        private static string MaskAccountNumber(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                return string.Empty;
            }

            var trimmed = accountNumber.Trim();
            return trimmed.Length <= 4 ? trimmed : new string('*', trimmed.Length - 4) + trimmed.Substring(trimmed.Length - 4);
        }

        private static string BuildMethodPayload(PayoutMethodVM model)
        {
            return string.Join("|",
                model.MethodType ?? string.Empty,
                model.AccountHolderName ?? string.Empty,
                model.AccountNumber ?? string.Empty,
                model.BankName ?? string.Empty,
                model.BankBranch ?? string.Empty,
                model.MobileProvider ?? string.Empty);
        }

        #endregion

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
