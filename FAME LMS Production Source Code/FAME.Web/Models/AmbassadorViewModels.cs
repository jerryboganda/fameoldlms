using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Models
{
    // =============================================
    // AMBASSADOR VIEW MODELS
    // =============================================

    #region Ambassador Profile

    /// <summary>
    /// Ambassador application form model
    /// </summary>
    public class AmbassadorApplicationVM
    {
        [Required(ErrorMessage = "Phone number is required")]
        [Display(Name = "Phone Number")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string PhoneNumber { get; set; }

        [Display(Name = "University/Institution")]
        [StringLength(200, ErrorMessage = "University name cannot exceed 200 characters")]
        public string University { get; set; }

        [Display(Name = "Country")]
        [StringLength(100, ErrorMessage = "Country name cannot exceed 100 characters")]
        public string Country { get; set; }

        [Display(Name = "Why do you want to become an ambassador?")]
        [DataType(DataType.MultilineText)]
        [StringLength(2000, ErrorMessage = "Application notes cannot exceed 2000 characters")]
        public string ApplicationNotes { get; set; }

        [Required(ErrorMessage = "You must accept the terms and conditions")]
        [Display(Name = "I accept the Ambassador Terms and Conditions")]
        public bool AcceptTerms { get; set; }
    }

    /// <summary>
    /// Combined registration and ambassador application model
    /// Used for anonymous users applying for the first time
    /// </summary>
    public class AmbassadorRegistrationVM : AmbassadorApplicationVM
    {
        [Required(ErrorMessage = "Full Name is required")]
        [Display(Name = "Full Name")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email Address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    /// <summary>
    /// Full ambassador profile view model
    /// </summary>
    public class AmbassadorVM
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string ReferralCode { get; set; }
        public string Status { get; set; }
        public string Tier { get; set; }
        public string University { get; set; }
        public string Country { get; set; }
        public string PhoneNumber { get; set; }
        public string ApplicationNotes { get; set; }
        public string ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string RejectionReason { get; set; }
        public string SuspendedReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Computed URLs
        public string ReferralUrl => $"/ref/{ReferralCode}";
        public string FullReferralUrl { get; set; } // Set by controller with domain

        // Status helpers
        public bool IsActive => Status == "Active";
        public bool IsPending => Status == "Pending";
        public bool IsSuspended => Status == "Suspended";
    }

    /// <summary>
    /// Profile page view model for ambassador's own profile page
    /// </summary>
    public class AmbassadorProfileVM
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Institution { get; set; }
        public string Country { get; set; }
        public string Status { get; set; }
        public string Tier { get; set; }
        public string ReferralCode { get; set; }
        public string ReferralLink { get; set; }
        public DateTime JoinedAt { get; set; }

        // Stats
        public int TotalReferrals { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal ConversionRate { get; set; }
        public string Currency { get; set; }

        // Application details
        public string PromotionMethod { get; set; }
        public string ExpectedReach { get; set; }
        public string WhyAmbassador { get; set; }

        // Social links
        public Dictionary<string, string> SocialLinks { get; set; }

        // Commission rates
        public List<CommissionRateDisplay> CommissionRates { get; set; }

        // Computed helpers
        public string Initials => string.IsNullOrEmpty(FullName) ? "AB"
            : string.Join("", FullName.Split(' ').Where(s => s.Length > 0).Take(2).Select(s => s[0])).ToUpper();

        public string StatusBadgeClass
        {
            get
            {
                switch (Status)
                {
                    case "Active": return "badge-success";
                    case "Pending": return "badge-warning";
                    case "Suspended": return "badge-danger";
                    default: return "badge-secondary";
                }
            }
        }

        public string TierBadgeClass
        {
            get
            {
                switch (Tier)
                {
                    case "Gold": return "badge-gold";
                    case "Silver": return "badge-silver";
                    default: return "badge-bronze";
                }
            }
        }

        public string TierIcon
        {
            get
            {
                switch (Tier)
                {
                    case "Gold": return "emoji_events";
                    case "Silver": return "military_tech";
                    default: return "star";
                }
            }
        }
    }

    public class CommissionRateDisplay
    {
        public string RateDisplay { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Status page view model for pending/rejected/suspended ambassadors
    /// </summary>
    public class AmbassadorStatusVM
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public DateTime? AppliedAt { get; set; }
        public string RejectionReason { get; set; }
        public string SuspensionReason { get; set; }
        public int TotalReferrals { get; set; }
        public string Currency { get; set; }
        public decimal TotalEarned { get; set; }
        public decimal PendingBalance { get; set; }
    }

    /// <summary>
    /// Ambassador list item for admin views
    /// </summary>
    public class AmbassadorListItemVM
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string ReferralCode { get; set; }
        public string Status { get; set; }
        public string Tier { get; set; }
        public string University { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalReferrals { get; set; }
        public int ConvertedReferrals { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal AvailableBalance { get; set; }

        // Additional properties for admin views
        public string PhoneNumber { get; set; }
        public string ApplicationNotes { get; set; }
        public string Currency { get; set; } = "ETB";
        public DateTime JoinedAt { get => CreatedAt; }
        public DateTime AppliedAt { get => CreatedAt; }
        public string Institution { get => University; }
        public string PromotionMethod { get => ApplicationNotes; }
        public string ExpectedReach { get; set; }
        public string WhyAmbassador { get; set; }
        public Dictionary<string, string> SocialLinks { get; set; }
        public string Initials
        {
            get
            {
                if (string.IsNullOrEmpty(FullName)) return "?";
                var parts = FullName.Trim().Split(' ');
                if (parts.Length >= 2)
                    return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
                return parts[0][0].ToString().ToUpper();
            }
        }

        // Badge CSS classes
        public string StatusBadgeClass
        {
            get
            {
                switch (Status)
                {
                    case "Active": return "badge-success";
                    case "Pending": return "badge-warning";
                    case "Suspended": return "badge-danger";
                    case "Rejected": return "badge-secondary";
                    default: return "badge-light";
                }
            }
        }

        public string TierBadgeClass
        {
            get
            {
                switch (Tier)
                {
                    case "Gold": return "badge-warning";
                    case "Silver": return "badge-secondary";
                    case "Bronze": return "badge-bronze";
                    default: return "badge-light";
                }
            }
        }
    }

    /// <summary>
    /// Filter for ambassador list
    /// </summary>
    public class AmbassadorFilterVM
    {
        public string SearchTerm { get; set; }
        public string Status { get; set; }
        public string Tier { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public string SortBy { get; set; } = "CreatedAt";
        public string SortDir { get; set; } = "desc";

        public List<SelectListItem> StatusOptions => new List<SelectListItem>
        {
            new SelectListItem { Text = "All Statuses", Value = "" },
            new SelectListItem { Text = "Pending", Value = "Pending" },
            new SelectListItem { Text = "Active", Value = "Active" },
            new SelectListItem { Text = "Suspended", Value = "Suspended" },
            new SelectListItem { Text = "Rejected", Value = "Rejected" }
        };

        public List<SelectListItem> TierOptions => new List<SelectListItem>
        {
            new SelectListItem { Text = "All Tiers", Value = "" },
            new SelectListItem { Text = "Bronze", Value = "Bronze" },
            new SelectListItem { Text = "Silver", Value = "Silver" },
            new SelectListItem { Text = "Gold", Value = "Gold" }
        };
    }

    #endregion

    #region Dashboard

    /// <summary>
    /// Ambassador dashboard view model
    /// </summary>
    public class AmbassadorDashboardVM
    {
        public AmbassadorVM Ambassador { get; set; }
        public DashboardStatsVM Stats { get; set; }
        public List<ReferralVM> RecentReferrals { get; set; }
        public List<EarningVM> RecentEarnings { get; set; }
        public List<PayoutRequestVM> RecentPayouts { get; set; }
        public ChartDataVM ClicksChartData { get; set; }
        public ChartDataVM ConversionsChartData { get; set; }
    }

    /// <summary>
    /// Dashboard statistics
    /// </summary>
    public class DashboardStatsVM
    {
        // Referrals
        public int TotalClicks { get; set; }
        public int TotalReferrals { get; set; }
        public int PendingReferrals { get; set; }
        public int ConvertedReferrals { get; set; }
        public decimal ConversionRate => TotalReferrals > 0 ? 
            Math.Round((decimal)ConvertedReferrals / TotalReferrals * 100, 1) : 0;

        // Earnings
        public decimal TotalEarnings { get; set; }
        public decimal PendingEarnings { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal PaidOutTotal { get; set; }

        // This month
        public int ClicksThisMonth { get; set; }
        public int ReferralsThisMonth { get; set; }
        public int ConversionsThisMonth { get; set; }
        public decimal EarningsThisMonth { get; set; }

        // Currency
        public string Currency { get; set; } = "ETB";
    }

    /// <summary>
    /// Chart data for dashboard graphs
    /// </summary>
    public class ChartDataVM
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<decimal> Values { get; set; } = new List<decimal>();
        public string ChartType { get; set; } = "line"; // line, bar, pie
    }

    #endregion

    #region Referrals

    /// <summary>
    /// Referral view model
    /// </summary>
    public class ReferralVM
    {
        public int Id { get; set; }
        public int AmbassadorId { get; set; }
        public string ReferredUserId { get; set; }
        public string ReferredUserEmail { get; set; }
        public string ReferredUserName { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string Source { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }

        // Alias properties for compatibility
        public string ReferredEmail { get { return ReferredUserEmail; } set { ReferredUserEmail = value; } }
        public string ReferredName { get { return ReferredUserName; } set { ReferredUserName = value; } }
        public DateTime ReferredAt { get { return RegisteredAt; } set { RegisteredAt = value; } }
        public bool HasConverted { get; set; }
        public string ConversionType { get; set; }
        public DateTime? ConvertedAt { get; set; }
        public decimal ConversionAmount { get; set; }
        public string Currency { get; set; } = "ETB";

        // Related conversion
        public ConversionVM Conversion { get; set; }

        // Status badge
        public string StatusBadgeClass
        {
            get
            {
                switch (Status)
                {
                    case "Converted": return "badge-success";
                    case "Verified": return "badge-info";
                    case "Registered": return "badge-warning";
                    case "Invalid": return "badge-danger";
                    default: return "badge-light";
                }
            }
        }

        public string SourceIcon
        {
            get
            {
                switch (Source)
                {
                    case "Link": return "link";
                    case "Code": return "qr_code";
                    default: return "help";
                }
            }
        }
    }

    /// <summary>
    /// Referral filter
    /// </summary>
    public class ReferralFilterVM
    {
        public string SearchTerm { get; set; }
        public string Status { get; set; }
        public string Source { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    /// <summary>
    /// Referral list page view model
    /// </summary>
    public class ReferralListPageVM
    {
        public List<ReferralVM> Referrals { get; set; }
        public ReferralFilterVM Filter { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Filter.PageSize);
    }

    #endregion

    #region Conversions

    /// <summary>
    /// Conversion view model
    /// </summary>
    public class ConversionVM
    {
        public int Id { get; set; }
        public int ReferralId { get; set; }
        public int EnrollmentId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public DateTime ConvertedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string RevokeReason { get; set; }
        public string ConversionType { get; set; }
        public int? OrderId { get; set; }

        // Related entities
        public string StudentEmail { get; set; }
        public string StudentName { get; set; }
        public string ReferredName { get; set; }
        public string ReferredUserName { get => StudentName ?? ReferredName; set => StudentName = value; }
        public string ReferredUserEmail { get => StudentEmail; set => StudentEmail = value; }
        public string AmbassadorName { get; set; }
        public string AmbassadorEmail { get; set; }
        public string PackageName { get; set; }

        // Commission earned
        public decimal CommissionAmount { get; set; }

        // Status badge
        public string StatusBadgeClass
        {
            get
            {
                switch (Status)
                {
                    case "Approved": return "badge-success";
                    case "Pending": return "badge-warning";
                    case "Revoked": return "badge-danger";
                    case "Disputed": return "badge-secondary";
                    default: return "badge-light";
                }
            }
        }
    }

    /// <summary>
    /// Conversion filter
    /// </summary>
    public class ConversionFilterVM
    {
        public int? AmbassadorId { get; set; }
        public string Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    #endregion

    #region Earnings

    /// <summary>
    /// Earning view model
    /// </summary>
    public class EarningVM
    {
        public int Id { get; set; }
        public int AmbassadorId { get; set; }
        public int? ConversionId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? AvailableAt { get; set; }
        public DateTime? PaidOutAt { get; set; }
        public int? PayoutRequestId { get; set; }

        // Alias properties
        public string EarningType { get => Type; set => Type = value; }
        public DateTime EarnedAt { get => CreatedAt; set => CreatedAt = value; }

        // Display properties
        public bool IsCredit => Type != "Reversal" && Amount > 0;
        public string AmountDisplay => IsCredit ? $"+{Amount:N0}" : $"-{Math.Abs(Amount):N0}";
        public string AmountClass => IsCredit ? "text-success" : "text-danger";

        // Status badge
        public string StatusBadgeClass
        {
            get
            {
                switch (Status)
                {
                    case "Available": return "badge-success";
                    case "Pending": return "badge-warning";
                    case "PaidOut": return "badge-info";
                    case "Revoked": return "badge-danger";
                    default: return "badge-light";
                }
            }
        }

        // Type icon
        public string TypeIcon
        {
            get
            {
                switch (Type)
                {
                    case "Commission": return "monetization_on";
                    case "Bonus": return "card_giftcard";
                    case "Adjustment": return "tune";
                    case "Reversal": return "undo";
                    default: return "attach_money";
                }
            }
        }
    }

    /// <summary>
    /// Earnings page view model
    /// </summary>
    public class EarningsPageVM
    {
        public BalanceVM Balance { get; set; }
        public List<EarningVM> Earnings { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    /// <summary>
    /// Balance summary
    /// </summary>
    public class BalanceVM
    {
        public decimal PendingBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal RequestedBalance { get; set; }
        public decimal PendingPayouts { get; set; }
        public decimal TotalPaidOut { get; set; }
        public decimal TotalEarnings { get; set; }
        public string Currency { get; set; } = "ETB";
        public decimal MinPayoutAmount { get; set; } = 500;

        public bool CanRequestPayout { get; set; }
    }

    #endregion

    #region Payout Methods

    /// <summary>
    /// Payout method view model
    /// </summary>
    public class PayoutMethodVM
    {
        public int Id { get; set; }
        public int AmbassadorId { get; set; }

        [Required(ErrorMessage = "Payment method type is required")]
        [Display(Name = "Payment Method")]
        public string MethodType { get; set; }

        [Required(ErrorMessage = "Account holder name is required")]
        [Display(Name = "Account Holder Name")]
        [StringLength(200, ErrorMessage = "Account holder name cannot exceed 200 characters")]
        public string AccountHolderName { get; set; }

        // Alias for AccountHolderName
        public string AccountTitle { get => AccountHolderName; set => AccountHolderName = value; }

        [Required(ErrorMessage = "Account number is required")]
        [Display(Name = "Account Number / Mobile Number")]
        [StringLength(50, ErrorMessage = "Account number cannot exceed 50 characters")]
        public string AccountNumber { get; set; }

        public string PhoneNumber { get => AccountNumber; set => AccountNumber = value; }

        public string MaskedAccountNumber { get; set; }

        [Display(Name = "Bank Name")]
        [StringLength(100, ErrorMessage = "Bank name cannot exceed 100 characters")]
        public string BankName { get; set; }

        [Display(Name = "Branch Code/Name")]
        [StringLength(20, ErrorMessage = "Branch code cannot exceed 20 characters")]
        public string BankBranch { get; set; }

        // Alias for BankBranch
        public string BranchCode { get => BankBranch; set => BankBranch = value; }

        [Display(Name = "Mobile Provider")]
        public string MobileProvider { get; set; }

        public string MobileAccountName { get => AccountHolderName; set => AccountHolderName = value; }

        public bool IsDefault { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Display helpers
        private string _methodTypeDisplay;
        public string MethodTypeDisplay
        {
            get
            {
                if (!string.IsNullOrEmpty(_methodTypeDisplay)) return _methodTypeDisplay;
                switch (MethodType)
                {
                    case "BankTransfer": return "Bank Transfer";
                    case "MobileMoney": return "Mobile Money";
                    case "JazzCash": return "JazzCash";
                    case "EasyPaisa": return "EasyPaisa";
                    default: return MethodType;
                }
            }
            set { _methodTypeDisplay = value; }
        }

        private string _methodIcon;
        public string MethodIcon
        {
            get
            {
                if (!string.IsNullOrEmpty(_methodIcon)) return _methodIcon;
                switch (MethodType)
                {
                    case "BankTransfer": return "account_balance";
                    case "MobileMoney": return "phone_android";
                    case "JazzCash": return "phone_android";
                    case "EasyPaisa": return "phone_android";
                    default: return "payment";
                }
            }
            set { _methodIcon = value; }
        }

        public static List<SelectListItem> MethodTypeOptions => new List<SelectListItem>
        {
            new SelectListItem { Text = "Bank Transfer", Value = "BankTransfer" },
            new SelectListItem { Text = "Mobile Money", Value = "MobileMoney" },
            new SelectListItem { Text = "JazzCash", Value = "JazzCash" },
            new SelectListItem { Text = "EasyPaisa", Value = "EasyPaisa" },
            new SelectListItem { Text = "Other", Value = "Other" }
        };
    }

    /// <summary>
    /// Payout methods page view model
    /// </summary>
    public class PayoutMethodsPageVM
    {
        public List<PayoutMethodVM> Methods { get; set; }
        public PayoutMethodVM NewMethod { get; set; }
        public bool CanAddMore => Methods == null || Methods.Count < 5;
    }

    #endregion

    #region Payout Requests

    /// <summary>
    /// Payout request view model
    /// </summary>
    public class PayoutRequestVM
    {
        public int Id { get; set; }
        public int AmbassadorId { get; set; }
        public int PayoutMethodId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string ProcessedByName { get; set; }
        public string AdminNotes { get; set; }
        public string TransactionRef { get; set; }
        public string RejectionReason { get; set; }

        // Related payout method
        public PayoutMethodVM PayoutMethod { get; set; }

        public string PayoutMethodType => PayoutMethod?.MethodTypeDisplay ?? "Unknown";

        public string PayoutMethodDetails => PayoutMethod?.MaskedAccountNumber ?? "Not available";

        public string PayoutMethodIcon => PayoutMethod?.MethodIcon ?? "payment";

        // For admin view
        public string AmbassadorEmail { get; set; }
        public string AmbassadorName { get; set; }
        public string AmbassadorInitials { get; set; }

        // Status badge (settable for service)
        public string StatusBadgeClass { get; set; }

        public string GetStatusBadgeClass()
        {
            switch (Status)
            {
                case "Paid": return "badge-success";
                case "Approved": return "badge-info";
                case "Requested": return "badge-warning";
                case "Rejected": return "badge-danger";
                case "Cancelled": return "badge-secondary";
                default: return "badge-light";
            }
        }
    }

    /// <summary>
    /// Request payout form
    /// </summary>
    public class RequestPayoutVM
    {
        [Required(ErrorMessage = "Please select a payout method")]
        [Display(Name = "Payout Method")]
        public int PayoutMethodId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(1, 10000000, ErrorMessage = "Amount must be between 1 and 10,000,000")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        public decimal AvailableBalance { get; set; }
        public decimal MinAmount { get; set; } = 1000;
        public string Currency { get; set; } = "ETB";

        public List<SelectListItem> PayoutMethodOptions { get; set; }
    }

    /// <summary>
    /// Payout filter for admin
    /// </summary>
    public class PayoutFilterVM
    {
        public int? AmbassadorId { get; set; }
        public string Status { get; set; }
        public string SearchTerm { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;

        public List<SelectListItem> StatusOptions => new List<SelectListItem>
        {
            new SelectListItem { Text = "All Statuses", Value = "" },
            new SelectListItem { Text = "Requested", Value = "Requested" },
            new SelectListItem { Text = "Approved", Value = "Approved" },
            new SelectListItem { Text = "Paid", Value = "Paid" },
            new SelectListItem { Text = "Rejected", Value = "Rejected" },
            new SelectListItem { Text = "Cancelled", Value = "Cancelled" }
        };
    }

    /// <summary>
    /// Payouts page view model
    /// </summary>
    public class PayoutsPageVM
    {
        public BalanceVM Balance { get; set; }
        public List<PayoutRequestVM> Requests { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // For admin
        public PayoutFilterVM Filter { get; set; }
    }

    /// <summary>
    /// Process payout form (admin)
    /// </summary>
    public class ProcessPayoutVM
    {
        public int RequestId { get; set; }

        [Required(ErrorMessage = "Action is required")]
        public string Action { get; set; } // Approve, Reject, MarkPaid

        [Display(Name = "Transaction Reference")]
        [StringLength(100, ErrorMessage = "Transaction reference cannot exceed 100 characters")]
        public string TransactionRef { get; set; }

        [Display(Name = "Admin Notes")]
        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string AdminNotes { get; set; }

        [Display(Name = "Rejection Reason")]
        [StringLength(500, ErrorMessage = "Rejection reason cannot exceed 500 characters")]
        public string RejectionReason { get; set; }
    }

    #endregion

    #region Commission Rules

    /// <summary>
    /// Commission rule view model
    /// </summary>
    public class CommissionRuleVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Rule name is required")]
        [Display(Name = "Rule Name")]
        [StringLength(100, ErrorMessage = "Rule name cannot exceed 100 characters")]
        public string RuleName { get; set; }

        [Display(Name = "Package")]
        public int? PackageId { get; set; }
        public string PackageName { get; set; }

        [Display(Name = "Duration (Months)")]
        public int? DurationMonths { get; set; }

        [Required(ErrorMessage = "Commission type is required")]
        [Display(Name = "Commission Type")]
        public string CommissionType { get; set; }

        [Required(ErrorMessage = "Commission value is required")]
        [Range(0, 100000, ErrorMessage = "Commission value must be between 0 and 100,000")]
        [Display(Name = "Commission Value")]
        public decimal CommissionValue { get; set; }

        [Required(ErrorMessage = "Applies to is required")]
        [Display(Name = "Applies To")]
        public string AppliesTo { get; set; }

        [Display(Name = "Minimum Tier")]
        public string MinTier { get; set; }

        [Display(Name = "Priority")]
        [Range(0, 1000, ErrorMessage = "Priority must be between 0 and 1000")]
        public int Priority { get; set; }

        [Required(ErrorMessage = "Effective from date is required")]
        [Display(Name = "Effective From")]
        [DataType(DataType.Date)]
        public DateTime EffectiveFrom { get; set; }

        [Display(Name = "Effective To")]
        [DataType(DataType.Date)]
        public DateTime? EffectiveTo { get; set; }

        public bool IsActive { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }

        // Alias properties for compatibility
        public string Name { get => RuleName; set => RuleName = value; }
        public decimal Rate { get => CommissionType == "Percentage" ? CommissionValue : 0; set { if (CommissionType == "Percentage") CommissionValue = value; } }
        public decimal FlatAmount
        {
            get => CommissionType == "Fixed" || CommissionType == "Flat" || CommissionType == "FlatAmount" ? CommissionValue : 0;
            set
            {
                if (CommissionType == "Fixed" || CommissionType == "Flat" || CommissionType == "FlatAmount")
                {
                    CommissionValue = value;
                }
            }
        }
        public string Tier { get => MinTier; set => MinTier = value; }
        public string Currency { get; set; } = "ETB";

        // Display helpers
        public string CommissionDisplay
        {
            get
            {
                if (CommissionType == "Percentage")
                    return $"{CommissionValue}%";
                return $"{Currency} {CommissionValue:N0}";
            }
        }

        public string Description { get; set; }

        public static List<SelectListItem> CommissionTypeOptions => new List<SelectListItem>
        {
            new SelectListItem { Text = "Percentage", Value = "Percentage" },
            new SelectListItem { Text = "Fixed Amount", Value = "Fixed" }
        };

        public static List<SelectListItem> AppliesToOptions => new List<SelectListItem>
        {
            new SelectListItem { Text = "First Payment Only", Value = "FirstPaymentOnly" },
            new SelectListItem { Text = "All Payments", Value = "AllPayments" }
        };

        public static List<SelectListItem> TierOptions => new List<SelectListItem>
        {
            new SelectListItem { Text = "All Tiers", Value = "" },
            new SelectListItem { Text = "Bronze & Above", Value = "Bronze" },
            new SelectListItem { Text = "Silver & Above", Value = "Silver" },
            new SelectListItem { Text = "Gold Only", Value = "Gold" }
        };
    }

    /// <summary>
    /// Commission rules page
    /// </summary>
    public class CommissionRulesPageVM
    {
        public List<CommissionRuleVM> Rules { get; set; }
        public CommissionRuleVM NewRule { get; set; }
        public List<SelectListItem> PackageOptions { get; set; }
    }

    #endregion

    #region Admin View Models

    /// <summary>
    /// Admin ambassador dashboard (flat structure matching Index.cshtml)
    /// </summary>
    public class AdminAmbassadorDashboardVM
    {
        // Stats
        public int TotalAmbassadors { get; set; }
        public int ActiveAmbassadors { get; set; }
        public int PendingApplications { get; set; }
        public int TotalReferrals { get; set; }
        public int ConvertedReferrals { get; set; }
        public decimal TotalCommissionsPaid { get; set; }
        public decimal PendingCommissions { get; set; }
        public int PendingPayoutRequests { get; set; }
        public string Currency { get; set; } = "ETB";

        // Chart data
        public List<string> ChartLabels { get; set; } = new List<string>();
        public List<int> ReferralData { get; set; } = new List<int>();
        public List<int> ConversionData { get; set; } = new List<int>();
        public List<string> TierLabels { get; set; } = new List<string>();
        public List<int> TierData { get; set; } = new List<int>();

        // Lists
        public List<TopPerformerVM> TopPerformers { get; set; } = new List<TopPerformerVM>();
        public List<First_Aid_Made_Easy.BLL.RecentActivityVM> RecentActivity { get; set; } = new List<First_Aid_Made_Easy.BLL.RecentActivityVM>();
    }

    /// <summary>
    /// Top performer for admin dashboard
    /// </summary>
    public class TopPerformerVM
    {
        public int AmbassadorId { get; set; }
        public string Name { get; set; }
        public int Referrals { get; set; }
        public decimal Earnings { get; set; }
    }

    /// <summary>
    /// Admin ambassador list page (Ambassadors.cshtml)
    /// </summary>
    public class AdminAmbassadorListVM
    {
        public List<AmbassadorListItemVM> Ambassadors { get; set; } = new List<AmbassadorListItemVM>();
        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
        public string SearchTerm { get; set; }
        public string StatusFilter { get; set; }
        public string TierFilter { get; set; }
        public string SortBy { get; set; }
    }

    /// <summary>
    /// Admin applications page (Applications.cshtml)
    /// </summary>
    public class AdminApplicationsVM
    {
        public List<AmbassadorListItemVM> Applications { get; set; } = new List<AmbassadorListItemVM>();
    }

    /// <summary>
    /// Admin commission rules page (Rules.cshtml)
    /// </summary>
    public class AdminCommissionRulesVM
    {
        public List<CommissionRuleVM> Rules { get; set; } = new List<CommissionRuleVM>();
        public string Currency { get; set; } = "ETB";
        public decimal MinPayoutAmount { get; set; } = 500;
        public int HoldPeriodDays { get; set; } = 7;
    }

    /// <summary>
    /// Admin payouts page (Payouts.cshtml)
    /// </summary>
    public class AdminPayoutsVM
    {
        public List<PayoutRequestVM> Requests { get; set; } = new List<PayoutRequestVM>();
        public int PendingCount { get; set; }
        public decimal PendingAmount { get; set; }
        public int ProcessingCount { get; set; }
        public decimal ProcessingAmount { get; set; }
        public int PaidCount { get; set; }
        public decimal PaidAmount { get; set; }
        public int RejectedCount { get; set; }
        public decimal RejectedAmount { get; set; }
        public string Currency { get; set; } = "ETB";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public int TotalCount { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
        public string StatusFilter { get; set; }
        public string SearchTerm { get; set; }
    }

    /// <summary>
    /// Admin statistics
    /// </summary>
    public class AdminStatsVM
    {
        // Ambassadors
        public int TotalAmbassadors { get; set; }
        public int ActiveAmbassadors { get; set; }
        public int PendingApplications { get; set; }
        public int SuspendedAmbassadors { get; set; }

        // Referrals
        public int TotalReferrals { get; set; }
        public int TotalConversions { get; set; }
        public decimal OverallConversionRate => TotalReferrals > 0 ?
            Math.Round((decimal)TotalConversions / TotalReferrals * 100, 1) : 0;

        // Financial
        public decimal TotalCommissionsPaid { get; set; }
        public decimal PendingPayouts { get; set; }
        public decimal TotalRevenue { get; set; }
        public string Currency { get; set; } = "ETB";

        // This month
        public int NewAmbassadorsThisMonth { get; set; }
        public int ConversionsThisMonth { get; set; }
        public decimal CommissionsThisMonth { get; set; }
    }

    #endregion

    #region Audit Log

    /// <summary>
    /// Audit log entry
    /// </summary>
    public class AuditLogVM
    {
        public long Id { get; set; }
        public int? AmbassadorId { get; set; }
        public string AmbassadorEmail { get; set; }
        public string Action { get; set; }
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string PerformedByName { get; set; }
        public DateTime PerformedAt { get; set; }

        public string ActionIcon
        {
            get
            {
                if (Action.Contains("Create") || Action.Contains("Add")) return "add_circle";
                if (Action.Contains("Update") || Action.Contains("Edit")) return "edit";
                if (Action.Contains("Delete") || Action.Contains("Remove")) return "remove_circle";
                if (Action.Contains("Approve")) return "check_circle";
                if (Action.Contains("Reject") || Action.Contains("Suspend")) return "block";
                return "info";
            }
        }
    }

    #endregion

    #region Pagination Helper

    /// <summary>
    /// Generic paginated result
    /// </summary>
    public class PagedResultVM<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    #endregion

    #region Chart Data

    /// <summary>
    /// Data point for charts
    /// </summary>
    public class ChartDataPointVM
    {
        public string Label { get; set; }
        public decimal Value { get; set; }
    }

    #endregion

    #region Payout Summary

    /// <summary>
    /// Payout summary for admin dashboard
    /// </summary>
    public class PayoutSummaryVM
    {
        public int PendingCount { get; set; }
        public decimal PendingAmount { get; set; }
        public int ProcessingCount { get; set; }
        public decimal ProcessingAmount { get; set; }
        public int PaidCount { get; set; }
        public decimal PaidAmount { get; set; }
        public int RejectedCount { get; set; }
        public decimal RejectedAmount { get; set; }
        public string Currency { get; set; } = "ETB";
    }

    #endregion

    #region Generic Paged Result

    /// <summary>
    /// Generic paged result for list endpoints
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    #endregion
}
