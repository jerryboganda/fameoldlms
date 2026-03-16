using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace First_Aid_Made_Easy.Models
{
    public class DailyReportVM
    {
        public int ID { get; set; }
        
        [Required]
        public string Body { get; set; }
        
        [Display(Name = "Date")]
        public string Date { get; set; }
        public DateTime? DateDT { get; set; }
        
        public int? UserFid { get; set; }
        
        public string FilePath { get; set; }
        
        public HttpPostedFileBase AttachmentFile { get; set; }
        
        public DateTime? CreatedDT { get; set; }
        
        public List<DailyReportVM> List { get; set; }
    }
}