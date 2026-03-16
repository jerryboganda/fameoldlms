using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface ITicketRepository
    {
        List<sp_lstTickets_Result> GetList(string status, string datef, string datet, string studentID, string IssuedTo = "");
        bool SaveReply(TicketReplyVM model);
        bool DeleteQuestion(int id);
        int Save(TicketVM model);
        bool ChangePopular(int ID, bool Status);
        bool ChangeStatus(int ID, string Status);
        TicketVM GetByID(int? id, string ID);
        int GetCourseFid(int id);
    }
}
