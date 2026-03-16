using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace First_Aid_Made_Easy.BLL
{
    public class ChatRepository : IChatRepository
    {
        #region Chat Crud
        public List<ChatVM> GroupList()
        {
            using (FAMEEntities db = new FAMEEntities())
            {

                var list = db.tbl_Conversation.Where(x => x.UserID_Two == null && x.Title != null).ToList();
                return list.ConvertAll(x => new ChatVM
                {
                    ConvID = x.ConvID,
                    UserID_One = x.UserID_One,
                    Title = x.Title,
                    Descriptiom = x.Descriptiom
                });
            }
        }
        public List<sp_GetConversations_Result> GetList(string userID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.sp_GetConversations(userID).ToList();
            }
        }
        public Tuple<List<string>, ChatReplyVM> SaveReply(ChatReplyVM model)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var rep = new tbl_ConversationReply
                {
                    ReplyBody = model.ReplyBody,
                    ConvID = model.ConvID,
                    SentAt = Common.GetCurrentDate(),
                    Status = MsgStatus.Sent.ToString(),
                    UserID = model.UserID,
                };
                db.tbl_ConversationReply.Add(rep);
                db.SaveChanges();
                List<string> UserIDs = new List<string>();
                var conv = db.tbl_Conversation.Find(model.ConvID);
                if (conv.CourseID > 0 || conv.PackageID > 0)
                {
                    // ===== Make USerID optional if gives error ========
                    UserIDs = db.sp_GetEnrollmentByCourse(conv.CourseID.ToString(), conv.PackageID.ToString(), true).Select(x => x.Id).ToList();
                    UserIDs.Add(conv.UserID_One);
                }
                else
                    UserIDs.Add(conv.UserID_One == model.UserID ? conv.UserID_Two : conv.UserID_One);
                model.Name = db.tbl_User.FirstOrDefault(y => y.User_AspUser == model.UserID)?.User_Name;
                model.Pic = db.tbl_User.FirstOrDefault(y => y.User_AspUser == model.UserID)?.User_Pic;
                model.ID = rep.ID;
                UserIDs.Remove(model.UserID);
                return new Tuple<List<string>, ChatReplyVM>(UserIDs, model);
            }
        }
        public bool SaveFile(string FileName, long ID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var qp = db.tbl_ConversationMedia.Add(new tbl_ConversationMedia
                {
                    ReplyID = ID,
                    Path = FileName,
                });
                db.SaveChanges();
                return true;
            }
        }

        public int Save(ChatVM model)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_Conversation table = new tbl_Conversation();
                if (model.ConvID > 0)
                {
                    table = db.tbl_Conversation.Find(model.ConvID);
                }
                table.UserID_One = model.UserID_One;
                table.UserID_Two = model.UserID_Two;
                table.Title = model.Title;
                table.Descriptiom = model.Descriptiom;
                table.CourseID = model.CourseID;
                table.PackageID = model.PackageID;
                table.Status = Status.Open.ToString();
                if (model.ConvID == 0)
                {
                    db.tbl_Conversation.Add(table);
                }
                db.SaveChanges();
                return table.ConvID;
            }
        }
        public bool ChangeStatus(int ID, string Status)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var table = db.tbl_Conversation.Find(ID);
                table.Status = Status;
                db.SaveChanges();
                return true;
            }
        }
        public ChatVM GetByID(int? id, string userID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_Conversation model = db.tbl_Conversation.Find(id);

                ChatVM s = new ChatVM()
                {
                    ConvID = model.ConvID,
                    UserID_One = model.UserID_One,
                    UserID_Two = model.UserID_Two,
                    UserID = userID,
                    LoadMore = model.tbl_ConversationReply.Count() > 15,
                    Descriptiom = model.Descriptiom,
                    Title = model.Title,
                    CourseID = model.CourseID,
                    PackageID = model.PackageID,
                    LastReply = model.tbl_ConversationReply.Max(x => x.SentAt)
                };
                if (s.FriendID != null)
                {
                    s.ConvName = db.tbl_User.FirstOrDefault(x => x.User_AspUser == s.FriendID)?.User_Name;
                    s.FriendPic = db.tbl_User.FirstOrDefault(x => x.User_AspUser == s.FriendID)?.User_Pic ?? "userLogo.jpg";
                }
                else
                {
                    s.ConvName = s.Title;
                    s.FriendPic = "groupLogo.jpg";
                }
                s.Replies = model.tbl_ConversationReply.OrderByDescending(x => x.ID).Take(15).ToList().ConvertAll(x => new ChatReplyVM
                {
                    ID = x.ID,
                    ReplyBody = x.ReplyBody,
                    SentAt = x.SentAt,
                    Status = x.Status,
                    UserID = x.UserID,
                    ReadAt = x.ReadAt,
                    Side = x.UserID == userID ? "right" : "left",
                    Name = x.UserID == userID ? "You" : db.tbl_User.FirstOrDefault(y => y.User_AspUser == x.UserID)?.User_Name,
                    Pic = db.tbl_User.FirstOrDefault(y => y.User_AspUser == x.UserID)?.User_Pic ?? "userLogo.jpg",
                    FilePath = x.tbl_ConversationMedia.FirstOrDefault()?.Path
                }).OrderBy(x => x.ID).ToList();
                return s;
            }
        }
        public ChatVM LoadMore(int? LoadAbove, int convID, string userID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_Conversation model = db.tbl_Conversation.Find(convID);

                ChatVM s = new ChatVM()
                {
                    LoadMore = model.tbl_ConversationReply.Where(x => x.ID < LoadAbove).Count() > 15,
                };
                s.Replies = model.tbl_ConversationReply.Where(x => x.ID < LoadAbove).OrderByDescending(x => x.ID).Take(15).ToList().ConvertAll(x => new ChatReplyVM
                {
                    ID = x.ID,
                    ReplyBody = x.ReplyBody,
                    SentAt = x.SentAt,
                    Status = x.Status,
                    UserID = x.UserID,
                    ReadAt = x.ReadAt,
                    Side = x.UserID == userID ? "right" : "left",
                    Name = x.UserID == userID ? "You" : db.tbl_User.FirstOrDefault(y => y.User_AspUser == x.UserID)?.User_Name,
                    Pic = db.tbl_User.FirstOrDefault(y => y.User_AspUser == x.UserID)?.User_Pic ?? "userLogo.jpg",
                    FilePath = x.tbl_ConversationMedia.FirstOrDefault()?.Path
                }).OrderBy(x => x.ID).ToList();
                return s;
            }
        }
        public ChatVM GetByUserID(string FriendID, string userID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_Conversation model = db.tbl_Conversation.FirstOrDefault(x => (x.UserID_One == FriendID && x.UserID_Two == userID) || (x.UserID_One == userID && x.UserID_Two == FriendID));

                if (model == null)
                {
                    var data = new ChatVM
                    {
                        UserID_One = FriendID,
                        UserID_Two = userID,
                        UserID = userID,
                        LoadMore = false,
                        Replies = new List<ChatReplyVM>(),
                    };
                    data.ConvName = db.tbl_User.FirstOrDefault(x => x.User_AspUser == FriendID)?.User_Name;
                    data.FriendPic = db.tbl_User.FirstOrDefault(x => x.User_AspUser == FriendID)?.User_Pic;
                    data.ConvID = Save(data);
                    return data;
                }
                ChatVM s = new ChatVM()
                {
                    ConvID = model.ConvID,
                    UserID_One = model.UserID_One,
                    UserID_Two = model.UserID_Two,
                    UserID = userID,
                    LoadMore = model.tbl_ConversationReply.Count() > 15,
                    Descriptiom = model.Descriptiom,
                    Title = model.Title,
                };
                s.ConvName = db.tbl_User.FirstOrDefault(x => x.User_AspUser == s.FriendID)?.User_Name;
                s.FriendPic = db.tbl_User.FirstOrDefault(x => x.User_AspUser == s.FriendID)?.User_Pic;
                s.Replies = model.tbl_ConversationReply.OrderByDescending(x => x.ID).Take(15).ToList().ConvertAll(x => new ChatReplyVM
                {
                    ID = x.ID,
                    ReplyBody = x.ReplyBody,
                    SentAt = x.SentAt,
                    Status = x.Status,
                    UserID = x.UserID,
                    ReadAt = x.ReadAt,
                    Side = x.UserID == userID ? "right" : "left",
                    Name = x.UserID == userID ? "You" : db.tbl_User.FirstOrDefault(y => y.User_AspUser == x.UserID)?.User_Name,
                    Pic = db.tbl_User.FirstOrDefault(y => y.User_AspUser == x.UserID)?.User_Pic ?? "userLogo.jpg",
                    FilePath = x.tbl_ConversationMedia.FirstOrDefault()?.Path
                }).OrderBy(x => x.ID).ToList();
                return s;
            }
        }

        #endregion

        #region Friends



        public Friend GetFrindList(string UserID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                Friend f = new Friend();
                var s = ReqStatus.Pending.ToString();
                f.FriendList = db.sp_GetFriendList(UserID).ToList();
                f.RequestList = db.tbl_FriendRequest.Where(x => x.FutureFriendID == UserID && x.Status == s).ToList().ConvertAll(x => new FriendRequest
                {
                    ReqID = x.ReqID,
                    FriendName = db.tbl_User.FirstOrDefault(u => u.User_AspUser == x.UserID).User_Name,
                    FriendPic = db.tbl_User.FirstOrDefault(u => u.User_AspUser == x.UserID).User_Pic,
                    UserID = x.UserID,
                    FutureFriendID = x.FutureFriendID,
                });
                f.AllUser = db.sp_GetAllUsersForFriendList(UserID, (int)Roles.Student).ToList();
                f.Instructors = db.sp_GetAllUsersForFriendList(UserID, (int)Roles.Teacher).ToList();
                return f;
            }
        }
        public bool SendRequest(string Fid, string UserID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                if (!db.tbl_FriendRequest.Any(x => x.UserID == UserID && x.FutureFriendID == Fid))
                {
                    db.tbl_FriendRequest.Add(new tbl_FriendRequest
                    {
                        UserID = UserID,
                        FutureFriendID = Fid,
                        ExpiresDate = Common.GetCurrentDate(),
                        Status = ReqStatus.Pending.ToString(),
                    });
                    db.SaveChanges();
                }
                return true;
            }
        }
        public bool UndoRequest(string Fid, string UserID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var req = db.tbl_FriendRequest.FirstOrDefault(x => x.UserID == UserID && x.FutureFriendID == Fid);
                db.tbl_FriendRequest.Remove(req);
                db.SaveChanges();
                return true;
            }
        }
        public bool AcceptRequest(int id, bool IsAccepted)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var req = db.tbl_FriendRequest.Find(id);
                if (IsAccepted)
                {
                    req.Status = ReqStatus.Accepted.ToString();
                    db.tbl_Friends.Add(new tbl_Friends
                    {
                        FriendOne = req.UserID,
                        FriendTwo = req.FutureFriendID,
                        Status = FriendStatus.Continued.ToString(),
                        Type = FriendType.Friend.ToString(),
                        FriendshipDate = Common.GetCurrentDate()
                    });
                }
                else
                    req.Status = ReqStatus.Rejected.ToString();

                db.SaveChanges();
                return true;
            }
        }
        #endregion

        #region Group

        public List<sp_GetEnrollmentByCourse_Result> GetGroupInfo(int ConvID, string UserID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var conv = db.tbl_Conversation.Find(ConvID);
                return db.sp_GetEnrollmentByCourse(conv.CourseID.ToString(), conv.PackageID.ToString(), true).OrderByDescending(x => x.IsFriend).ToList();
            }
        }
        #endregion
    }
}