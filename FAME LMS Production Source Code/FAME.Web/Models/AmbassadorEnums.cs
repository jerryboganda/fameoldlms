namespace First_Aid_Made_Easy.Models
{
    /// <summary>
    /// Ambassador account status
    /// </summary>
    public enum AmbassadorStatus
    {
        Pending = 0,
        Active = 1,
        Suspended = 2,
        Rejected = 3,
        Inactive = 4
    }

    /// <summary>
    /// Ambassador tier levels
    /// </summary>
    public enum AmbassadorTier
    {
        Bronze = 0,
        Silver = 1,
        Gold = 2,
        Platinum = 3
    }

    /// <summary>
    /// Referral status
    /// </summary>
    public enum ReferralStatus
    {
        Clicked = 0,
        Registered = 1,
        Converted = 2,
        Expired = 3
    }

    /// <summary>
    /// Earning status
    /// </summary>
    public enum EarningStatus
    {
        Pending = 0,
        Available = 1,
        PaidOut = 2,
        Cancelled = 3
    }

    /// <summary>
    /// Earning type
    /// </summary>
    public enum EarningType
    {
        Registration = 0,
        Subscription = 1,
        Renewal = 2,
        Bonus = 3,
        TierBonus = 4
    }

    /// <summary>
    /// Payout status
    /// </summary>
    public enum PayoutStatus
    {
        Requested = 0,
        Processing = 1,
        Paid = 2,
        Rejected = 3,
        Cancelled = 4
    }

    /// <summary>
    /// Commission type
    /// </summary>
    public enum CommissionType
    {
        Percentage = 0,
        FlatAmount = 1
    }

    /// <summary>
    /// Payment method type
    /// </summary>
    public enum PaymentMethodType
    {
        BankTransfer = 0,
        MobileMoney = 1,
        PayPal = 2,
        Other = 3
    }
}
