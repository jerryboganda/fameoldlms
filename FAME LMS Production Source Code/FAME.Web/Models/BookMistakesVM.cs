using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace First_Aid_Made_Easy.Models
{
    public class BookMistakesVM
    {
        public int Id { get; set; }
        public string BookEdition { get; set; }
        public string BookName { get; set; }
        public string QuestionNo { get; set; }
        public string PageNo { get; set; }
        public string Subject { get; set; }
        public string Detail { get; set; }
        public string PicPath { get; set; }
        public string Correction { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        public Nullable<System.DateTime> CreatedDT { get; set; }
        public List<BookMistakesVM> List { get; set; }
    }
}