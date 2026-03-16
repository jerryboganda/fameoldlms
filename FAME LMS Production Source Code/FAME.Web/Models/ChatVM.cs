using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using First_Aid_Made_Easy.DAL;

namespace First_Aid_Made_Easy.Models
{
    public class ChatVM
    {

        public int ConvID { get; set; }
        public string UserID { get; set; } // = The User that is Currently Accessing That Converstaion ==
        public string UserID_Two { get; set; }
        public string UserID_One { get; set; }
        public string Status { get; set; }
        public bool LoadMore { get; set; }

        public string Title { get; set; }
        public string Descriptiom { get; set; }
        public int? CourseID { get; set; }
        public int? PackageID { get; set; }

        public DateTime? LastReply { get; set; }

        public List<ChatReplyVM> Replies { get; set; }
        public List<sp_GetConversations_Result> List { get; set; }
        public List<ChatVM> GroupList { get; set; }

        public string FriendPic { get; set; }
        public string ConvName { get; set; }
        public string FriendID
        {
            get
            {

                if (Title != null) return null;
                else if(UserID == UserID_Two) return UserID_One;
                else return UserID_Two;
            }
        }
    }
    public class ChatReplyVM
    {

        public long ID { get; set; }
        public Nullable<int> ConvID { get; set; }
        public string UserID { get; set; }
        public string ReplyBody { get; set; }
        public string Side { get; set; }
        public string Name { get; set; }
        public string Pic { get; set; }
        public string FilePath { get; set; }
        public Nullable<System.DateTime> SentAt { get; set; }
        public Nullable<System.DateTime> ReadAt { get; set; }
        public string Status { get; set; }
        public HttpPostedFileBase File { get; set; }

    }
    public class Friend
    {

        public int ID { get; set; }
        public string FriendName { get; set; }
        public string FriendPic { get; set; }
        public string FriendEmail { get; set; }
        public string FriendID { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public Nullable<System.DateTime> FriendshipDate { get; set; }
        public List<sp_GetFriendList_Result> FriendList { get; set; }
        public List<FriendRequest> RequestList { get; set; }
        public List<sp_GetAllUsersForFriendList_Result> AllUser { get; set; }
        public List<sp_GetAllUsersForFriendList_Result> Instructors { get; set; }
    }
    public class FriendRequest
    {

        public int ReqID { get; set; }
        public string FutureFriendID { get; set; }
        public string FriendName { get; set; }
        public string FriendEmail { get; set; }
        public string FriendPic { get; set; }
        public string Status { get; set; }
        public Nullable<System.DateTime> ExpiresDate { get; set; }
        public string UserID { get; set; }
        public List<FriendRequest> RequestList { get; set; }
    }
}