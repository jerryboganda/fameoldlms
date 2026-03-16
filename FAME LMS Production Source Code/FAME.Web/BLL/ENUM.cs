using System.ComponentModel.DataAnnotations;

namespace First_Aid_Made_Easy.BLL
{
    public enum FriendType { Friend, Teacher }
    public enum FriendStatus { Continued, Blocked }
    public enum ReqStatus { Pending, Accepted, Rejected }
    public enum Status { Open, Close, }
    public enum MeetStatus { Active, UpComing, Completed, }
    public enum MsgStatus { Sent, Read }
    public enum EnrollStatus { Pending, Approved, Rejected, Continue, Expired, Override }
    public enum MasterGroup { Occupation = 1, College = 2, AgentCategory = 3, WelComeMsg = 4, DashBoardAd = 5 }

    public enum Roles { Student = 1, Teacher = 2, Admin = 3, Assistant = 4, SuppAgent = 5, }

    public enum RequestType { Extension, Register, }
    public enum NotesType { Tags, Notes, }
    public enum UserType { Local = 1, AMI = 2, AMIAmb = 4, /*AMIFree = 3,AMISec = 5, JLLBD = 6,*/ NRETest = 7, AppInaug = 8, FreePlab = 9, FreeCNS = 10, FreeNre2 = 11, FreeNre1 = 12 }
    public enum QuestionType { ALL = 0, Mcq = 1, Short = 2, Long = 3, FAQ = 4, FAQVideo = 5 }
    public enum ContentType { Video = 1, Audio = 2, File = 3, Trial = 4 }
    public enum Difficulty { Easy = 1, Medium = 2, Hard = 3, }
    public enum SolveMode { Exam, Self }
    public enum SMSApi { Mocean, Telesign, m4sms }
    public enum CodeType { SMS, Email }
    public enum MockTestTypes
    {
        [Display(Name = "FCPS-1 Mock Test")] FCPS1,
        [Display(Name = "NRE Mock Test")] NRE,
        [Display(Name = "USMLE Mock Test")] USMLE,
        [Display(Name = "PLAB Mock Test")] PLAB,
        [Display(Name = "SMLE | DHA | HAAD | MOH Q BANK MCQS")] HAAD,
        [Display(Name = "MD/MS JCAT")] JCAT,
        [Display(Name = "AMC-1 Mock Test")] AMC1,
    }
    public enum DocForm { Meeting }
    public enum EmailType { None, FirstMessage, Warning, Parent }

}