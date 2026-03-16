using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace First_Aid_Made_Easy.Models
{
    public class CheckOutResponseModel
    {
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public string InstrumentToken { get; set; }
        public string pp_Version { get; set; }
        public string pp_TxnType { get; set; }
        public string pp_MerchantID { get; set; }
        public string pp_Language { get; set; }
        public string pp_SubMerchantID { get; set; }
        public string pp_Password { get; set; }
        public string pp_BankID { get; set; }
        public string pp_ProductID { get; set; }
        public string pp_TxnRefNo { get; set; }
        public string pp_Amount { get; set; }
        public string pp_DiscountedAmount { get; set; }
        public string pp_DiscountBank { get; set; }
        public string pp_TxnCurrency { get; set; }
        public string pp_TxnDateTime { get; set; }
        public string pp_TxnExpiryDateTime { get; set; }
        public string pp_BillReference { get; set; }
        public string pp_Description { get; set; }
        public string pp_ReturnURL { get; set; }
        public string ppmpf_1 { get; set; }
        public string ppmpf_2 { get; set; }
        public string ppmpf_3 { get; set; }
        public string ppmpf_4 { get; set; }
        public string ppmpf_5 { get; set; }
        public string ppmpf_6 { get; set; }
        public string ppmbf_1 { get; set; }
        public string ppmbf_2 { get; set; }
        public string ppmbf_3 { get; set; }
        public string ppmbf_4 { get; set; }
        public string ppmbf_5 { get; set; }
        public string ppmbf_6 { get; set; }
        public string pp_SecureHash { get; set; }
        public string pp_CustomerCardCvv { get; set; }
        public string pp_CustomerCardExpiry { get; set; }
        public string pp_CustomerCardNumber { get; set; }
        public string pp_Frequency { get; set; }
        public string pp_InstrToken { get; set; }
        public string pp_SettlementExpiry { get; set; }
        public string pp_AuthCode { get; set; }
        public string pp_ResponseCode { get; set; }
        public string pp_ResponseMessage { get; set; }
        public string pp_RetreivalReferenceNo { get; set; }
        public string Result_CardEnrolled { get; set; }
        public string C3DSecureID { get; set; }
        public string AR_Simple_Html { get; set; }
        public string SecureHash { get; set; }
        public string SummaryStatus { get; set; }
        public string GateWayCode { get; set; }
    }

}