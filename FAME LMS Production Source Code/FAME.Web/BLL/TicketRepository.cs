using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;

using First_Aid_Made_Easy.Models;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace First_Aid_Made_Easy.BLL
{
    public class TicketRepository : ITicketRepository
    {
        public List<sp_lstTickets_Result> GetList(string status, string datef, string datet, string studentID, string IssuedTo = "")
        {
            using (FAMEEntities db = new FAMEEntities())
            {

                DateTime? datefrom = Common.TryStringToDate(datef);
                DateTime? dateto = Common.TryStringToDate(datet);
                return db.sp_lstTickets(datefrom, dateto, status, studentID, IssuedTo).ToList();
            }
        }

        public bool SaveReply(TicketReplyVM model)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                db.tbl_TicketReply.Add(new tbl_TicketReply
                {
                    ReplyBody = model.ReplyBody,
                    FilePath = model.File != null ? Common.SavePic(model.File, "ticket/", null) : model.FilePath,
                    SendBy = model.SendBy,
                    SentTo = model.SentTo,
                    Ticket_ID = model.Ticket_ID,
                    IsRead = false,
                    SendDT = Common.GetCurrentDate(),
                });
                db.SaveChanges();
                return true;
            }
        }

        public bool DeleteQuestion(int id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    var v = db.tbl_Ticket.Find(id);
                    db.tbl_Ticket.Remove(v);
                    db.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        public int Save(TicketVM model)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_Ticket table = new tbl_Ticket();
                if (model.Ticket_ID > 0)
                {
                    table = db.tbl_Ticket.Find(model.Ticket_ID);
                }

                table.Ticket_ID = model.Ticket_ID;
                table.Subject = model.Subject;
                table.Body = model.Body;
                table.CategoryID = model.CategoryID;
                table.Status = Status.Open.ToString();

                if (!(model.Ticket_ID > 0))
                {
                    table.CreatedBy = model.CreatedBy;
                    try
                    { table.IssuedTo = db.sp_GetFreeSupportAgent(model.CategoryID).First().ID; }
                    catch { return 0; }
                    table.CreatedDT = Common.GetCurrentDate();
                    db.tbl_Ticket.Add(table);
                }
                db.SaveChanges();
                return table.Ticket_ID;
            }
        }
        public bool ChangePopular(int ID, bool Status)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var table = db.tbl_Ticket.Find(ID);
                table.IsPopular = Status;
                db.SaveChanges();
                return true;
            }
        }
        public bool ChangeStatus(int ID, string Status)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var table = db.tbl_Ticket.Find(ID);
                table.Status = Status;
                db.SaveChanges();
                return true;
            }
        }

        public TicketVM GetByID(int? id, string ID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_Ticket model = db.tbl_Ticket.Find(id);

                TicketVM s = new TicketVM()
                {
                    Ticket_ID = model.Ticket_ID,
                    CategoryID = model.CategoryID,
                    Body = model.Body,
                    CreatedBy = model.CreatedBy,
                    CreatedDT = model.CreatedDT,
                    IssuedTo = model.IssuedTo,
                    Status = model.Status,
                    Subject = model.Subject,
                    Category = model.tbl_Master?.Master_Value,
                    StudentName = db.tbl_User.FirstOrDefault(x => x.User_AspUser == model.CreatedBy).User_Name,
                    Reply = model.tbl_TicketReply.OrderByDescending(x => x.SendDT).ToList().ConvertAll(x => new TicketReplyVM
                    {
                        FilePath = "~/Images/" + x.FilePath,
                        IsRead = x.IsRead,
                        ReplyBody = x.ReplyBody,
                        SendBy = x.SendBy,
                        SendDT = x.SendDT,
                        SentTo = x.SentTo,
                        IsMine = x.SendBy == ID
                    }),
                };


                return s;
            }
        }
        public int GetCourseFid(int id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    tbl_Section model = db.tbl_Section.Find(id);
                    return model.Course_Fid;
                }
                catch
                {
                    return 0;
                }
            }
        }
    }
}