using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace First_Aid_Made_Easy.BLL
{
    // =============================================
    // AMBASSADOR ENUMS
    // =============================================
    
    public enum AmbassadorStatus { Pending, Active, Suspended, Rejected }
    
    public enum AmbassadorTier { Bronze, Silver, Gold }
    
    public enum ReferralSource { Link, Code, Manual }
    
    public enum ReferralStatus { Registered, Verified, Converted, Invalid }
    
    public enum ConversionStatus { Pending, Approved, Revoked, Disputed }
    
    public enum EarningStatus { Pending, Available, PaidOut, Revoked }
    
    public enum EarningType { Commission, Bonus, Adjustment, Reversal }
    
    public enum PayoutMethodType { BankTransfer, JazzCash, EasyPaisa, Other }
    
    public enum PayoutRequestStatus { Requested, Approved, Paid, Rejected, Cancelled }
    
    public enum CommissionType { Fixed, Percentage }
    
    public enum CommissionAppliesTo { FirstPaymentOnly, AllPayments }
}
