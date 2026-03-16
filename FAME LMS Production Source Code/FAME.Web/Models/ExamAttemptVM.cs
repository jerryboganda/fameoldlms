using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace First_Aid_Made_Easy.Models
{
    public class ExamAttemptVM
    {
        public int ExamAttemptID { get; set; }

        [Required(ErrorMessage = "Please select a package")]
        [Display(Name = "Package")]
        public int PackageID { get; set; }

        [Required(ErrorMessage = "Attempt name is required")]
        [StringLength(100)]
        [Display(Name = "Attempt Name")]
        public string AttemptName { get; set; }

        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        public DateTime? CreatedDT { get; set; }

        // For display
        public string PackageName { get; set; }

        // For listing
        public List<ExamAttemptVM> List { get; set; }
    }
}
