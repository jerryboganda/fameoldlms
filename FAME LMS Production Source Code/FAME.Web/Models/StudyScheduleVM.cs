using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace First_Aid_Made_Easy.Models
{
    public class StudyScheduleVM
    {
        public int StudyScheduleID { get; set; }

        [Required(ErrorMessage = "Please select an exam attempt")]
        [Display(Name = "Exam Attempt")]
        public int ExamAttemptID { get; set; }

        [Required(ErrorMessage = "Schedule type is required")]
        [Display(Name = "Schedule Type")]
        public int ScheduleType { get; set; } // 1=Structured, 2=Text/HTML, 3=File, 4=Calendar

        [Display(Name = "Week Number")]
        public int? WeekNumber { get; set; }

        [Required(ErrorMessage = "Topic title is required")]
        [StringLength(255)]
        [Display(Name = "Topic Title")]
        public string TopicTitle { get; set; }

        [Display(Name = "Description (HTML)")]
        public string TopicDescription { get; set; }

        [StringLength(500)]
        [Display(Name = "File Path")]
        public string FilePath { get; set; }

        [Display(Name = "Schedule Date")]
        [DataType(DataType.Date)]
        public DateTime? ScheduleDate { get; set; }

        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        public DateTime? CreatedDT { get; set; }

        // For display
        public string AttemptName { get; set; }
        public string PackageName { get; set; }
        public int PackageID { get; set; }

        // For file upload
        public HttpPostedFileBase ScheduleFile { get; set; }

        // For listing
        public List<StudyScheduleVM> List { get; set; }

        // Schedule type display helper
        public string ScheduleTypeName
        {
            get
            {
                switch (ScheduleType)
                {
                    case 1: return "Structured (Weekly)";
                    case 2: return "Text/HTML";
                    case 3: return "File Attachment";
                    case 4: return "Calendar Date";
                    default: return "Unknown";
                }
            }
        }
    }
}
