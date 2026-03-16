using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using System;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface IChatRepository
    {
        List<ChatVM> GroupList();
        List<sp_GetConversations_Result> GetList(string userID);
        Tuple<List<string>, ChatReplyVM> SaveReply(ChatReplyVM model);
        bool SaveFile(string FileName, long ID);
        int Save(ChatVM model);
        bool ChangeStatus(int ID, string Status);
        ChatVM GetByID(int? id, string userID);
        ChatVM LoadMore(int? LoadAbove, int convID, string userID);
        ChatVM GetByUserID(string FriendID, string userID);
        Friend GetFrindList(string UserID);
        bool SendRequest(string Fid, string UserID);
        bool UndoRequest(string Fid, string UserID);
        bool AcceptRequest(int id, bool IsAccepted);
        List<sp_GetEnrollmentByCourse_Result> GetGroupInfo(int ConvID, string UserID);
    }
}
