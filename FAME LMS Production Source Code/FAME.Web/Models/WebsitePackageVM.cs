using System;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.Models
{
    public class WebsitePackageVM
    {
        public int ID { get; set; }
        public string PlanName { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
        public string BuyButtonUrl { get; set; }
        public string BuyButtonText { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int LeftColumnWidth { get; set; }
        public int RightColumnWidth { get; set; }
        public bool UseMultiColumnFeatures { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public List<WebsitePackageFeatureVM> Features { get; set; }
        public List<WebsitePackagePricingVM> Pricing { get; set; }

        // For admin list view
        public List<WebsitePackageVM> List { get; set; }

        public WebsitePackageVM()
        {
            Features = new List<WebsitePackageFeatureVM>();
            Pricing = new List<WebsitePackagePricingVM>();
            BuyButtonUrl = "/Account/Register";
            BuyButtonText = "Buy";
            LeftColumnWidth = 6;
            RightColumnWidth = 6;
            IsActive = true;
        }
    }

    public class WebsitePackageFeatureVM
    {
        public int ID { get; set; }
        public int PackageID { get; set; }
        public string FeatureText { get; set; }
        public int SortOrder { get; set; }
    }

    public class WebsitePackagePricingVM
    {
        public int ID { get; set; }
        public int PackageID { get; set; }
        public string DurationText { get; set; }
        public string PricePKR { get; set; }
        public string PriceUSD { get; set; }
        public int SortOrder { get; set; }
    }
}
