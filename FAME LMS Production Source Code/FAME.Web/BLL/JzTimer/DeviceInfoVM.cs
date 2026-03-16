using First_Aid_Made_Easy.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace First_Aid_Made_Easy.BLL.JzTimer
{
    public class EmailReportVM
    {
        public List<DeviceInfoVM> LogInfo { get; set; }
        public List<ResultInfoVM> Result { get; set; }
        public List<WatchInfoVM> Watch { get; set; }
        public EmailSettingsVM Settings { get; set; }
        public sp_GetStudentsWithLastOnline_Result Student { get; set; }
        public int LastOnline { get; set; }
    }
    public class ResultInfoVM
    {
        public Nullable<System.DateTime> Datetime { get; set; }
        public string QuestionPaper { get; set; }
        public Nullable<int> ObtainedMarks { get; set; }
        public Nullable<int> TotalMarks { get; set; }
        public string Mode { get; set; }
    }
    public class DeviceInfoVM
    {
        public string UserAgent { get; set; }
        public Nullable<System.DateTime> DateTime { get; set; }
    }
    public class WatchInfoVM
    {
        public Nullable<System.DateTime> DateTime { get; set; }
        public int? Watched { get; set; }
    }
}