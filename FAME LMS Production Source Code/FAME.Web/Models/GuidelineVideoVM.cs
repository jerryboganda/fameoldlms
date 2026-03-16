using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace First_Aid_Made_Easy.Models
{
    public class GuidelineVideoVM
    {
        public int GuidelineVideoID { get; set; }

        [Required]
        public int PackageID { get; set; }

        [Required]
        [StringLength(255)]
        public string VideoTitle { get; set; }

        [StringLength(500)]
        public string VideoDescription { get; set; }

        [Required]
        [StringLength(500)]
        public string VideoPath { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedDT { get; set; }

        // For displaying package name
        public string PackageName { get; set; }

        // For list views
        public List<GuidelineVideoVM> List { get; set; }
    }
}
