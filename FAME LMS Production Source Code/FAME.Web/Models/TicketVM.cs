using First_Aid_Made_Easy.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace First_Aid_Made_Easy.Models
{
    public class TicketVM
    {
        public int Ticket_ID { get; set; }
        public int? CategoryID { get; set; }
        public string Subject { get; set; }
        public string Category { get; set; }
        public string Body { get; set; }
        public string IssuedTo { get; set; }
        public string IssuedToName { get; set; }
        public string Status { get; set; }
        public string StudentName { get; set; }
        public Nullable<System.DateTime> CreatedDT { get; set; }
        public string CreatedBy { get; set; }
        public List<sp_lstTickets_Result> List { get; set; }
        public List<TicketReplyVM> Reply { get; set; }
    }

    public class TicketReplyVM
    {
        public int ID { get; set; }
        public Nullable<int> Ticket_ID { get; set; }
        public string FilePath { get; set; }
        public Nullable<bool> IsRead { get; set; }
        public string ReplyBody { get; set; }
        public string SentTo { get; set; }
        public Nullable<System.DateTime> SendDT { get; set; }
        public string SendBy { get; set; }
        public HttpPostedFileBase File { get; set; }
        public bool IsMine { get; set; }
    }
    }