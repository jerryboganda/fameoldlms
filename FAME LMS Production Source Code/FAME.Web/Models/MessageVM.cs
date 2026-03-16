using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Models
{
    public class MessageVM
    {
        public string Subject { get; set; }
        [AllowHtml]
        public string Body { get; set; }
        public string Destination { get; set; }
        public List<string> Destinations { get; set; }
    }
}