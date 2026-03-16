namespace First_Aid_Made_Easy.DAL
{
    using System;
    using System.Collections.Generic;

    public partial class tbl_EmailSenderAccounts
    {
        public int Id { get; set; }
        public string AccountName { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public string SmtpHost { get; set; }
        public Nullable<int> SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public bool EnableSSL { get; set; }
        public bool UseApi { get; set; }
        public string ApiKey { get; set; }
        public string ApiProvider { get; set; }
        public int DailyLimit { get; set; }
        public int SentToday { get; set; }
        public Nullable<System.DateTime> LastResetDate { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public Nullable<System.DateTime> UpdatedAt { get; set; }
    }
}
