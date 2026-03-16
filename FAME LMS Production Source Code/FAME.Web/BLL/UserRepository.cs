using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;

using First_Aid_Made_Easy.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.BLL
{
    public class UserRepository : IUserRepository
    {
        public List<UserVM> GetList()
        {
            List<UserVM> list = new List<UserVM>();
            return list;
        }
        public UserVM GetProfile(string id)
        {
            bool? NotComplete = null;
            UserVM user;
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_User model = new tbl_User();
                if (!string.IsNullOrEmpty(id))
                {
                    model = db.tbl_User.Where(x => x.User_AspUser == id).FirstOrDefault();
                }
                else
                {
                    NotComplete = true;
                    model = new tbl_User()
                    {
                        User_AspUser = id,
                        User_Pic = "userLogo.jpg",
                        User_Id = 0,
                    };
                }
                user = new UserVM()
                {
                    FatherEmail = model.FatherEmail,
                    User_FatherName = model.User_FatherName,
                    FatherProfession = model.FatherProfession,
                    SponserProfession = model.SponserProfession,
                    User_Mobile = model.User_Mobile,
                    OccupationID = model.OccupationID,
                    CountryID = model.CountryID,
                    InstituteID = model.InstituteID,
                    YearOfMBBS = model.YearOfMBBS,
                    User_AspUser = model.User_AspUser,
                    User_Id = model.User_Id,
                    User_Name = model.User_Name,
                    User_Pic = model.User_Pic,
                    CNICFront = model.CNICFront,
                    CNICBack = model.CNICBack,
                    CNIC = model.CNIC,
                    Institute = model.Institute,
                    Type = model.Type,
                    StuCardFront = model.StuCardFront,
                    JobLocation = model.JobLocation,
                    City = model.City,
                    ExamType = model.ExamType,
                    MockTestType = model.MockTestType,
                    NotComplete = NotComplete,
                    CategoryID = model.CategoryID,
                };
                return user;
            }
        }
        public UserVM GetByID(string id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    tbl_User user = db.tbl_User.Where(x => x.User_AspUser == id).FirstOrDefault();
                    UserVM userVM = new UserVM()
                    {
                        User_Id = user.User_Id,
                        User_FatherName = user.User_FatherName,
                        User_Name = user.User_Name,
                        User_Pic = user.User_Pic ?? "userLogo.jpg",
                        Institute = user.Institute,
                        User_AspUser = user.User_AspUser,
                        FatherEmail = user.FatherEmail,
                    };
                    return userVM;
                }
                catch
                {
                    throw;
                }
            }

        }
        public UserVM GetByID(int id, string UserID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    tbl_User user = db.tbl_User.Find(id);
                    UserVM userVM = new UserVM()
                    {
                        User_Id = user.User_Id,
                        User_FatherName = user.User_FatherName,
                        User_Name = user.User_Name,
                        User_Pic = user.User_Pic ?? "userLogo.jpg",
                        Institute = user.Institute,
                        FatherEmail = user.FatherEmail,
                        User_AspUser = user.User_AspUser,
                        DisableRequest = db.tbl_FriendRequest.Any(x => x.FutureFriendID == UserID && x.UserID == user.User_AspUser) ||
                                         db.tbl_FriendRequest.Any(x => x.FutureFriendID == user.User_AspUser && x.UserID == UserID) ||
                                         db.tbl_Friends.Any(x => x.FriendOne == UserID && x.FriendTwo == user.User_AspUser) ||
                                         db.tbl_Friends.Any(x => x.FriendOne == user.User_AspUser && x.FriendTwo == UserID) ||
                                         UserID == user.User_AspUser,
                    };
                    return userVM;
                }
                catch
                {
                    return null;
                }
            }

        }
        public bool Save(UserVM user)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_User table = new tbl_User();
                if (user.User_Id > 0)
                {
                    // Edit User
                    table = db.tbl_User.Find(user.User_Id);
                    table.AspNetUsers.ShouldChangeInfo = false;
                }
                else
                {
                    // Re-register User
                    var u = db.tbl_User.FirstOrDefault(x => x.User_AspUser == user.User_AspUser);
                    if (u != null)
                    {
                        table = u;
                        Common.DeleteFile(table.CNICBack);
                        Common.DeleteFile(table.CNICFront);
                        Common.DeleteFile(table.User_Pic);

                    }

                    table.CNICBack = user.CNICBack;
                    table.CNICFront = user.CNICFront;
                    table.User_Pic = user.User_Pic ?? "userLogo.jpg";


                    table.StuCardBack = user.StuCardBack;
                    table.StuCardFront = user.StuCardFront;

                    table.User_AspUser = user.User_AspUser;
                    table.Allowed_PC_Dev = 1;
                    table.Allowed_Mob_Dev = 1;
                    table.CreateDT = Common.GetCurrentDate();
                }

                if (user.User_PicFile != null)
                {
                    if (table.User_Pic == "userLogo.jpg") table.User_Pic = "";
                    user.User_Pic = Common.SavePic(user.User_PicFile, "Comp/", table.User_Pic);
                    table.User_Pic = new ImageProcessing(user.User_Pic).Compress();
                }
                else if (table.User_Pic == null)
                {
                    table.User_Pic = user.User_Pic ?? "userLogo.jpg";
                }

                if (user.CNICFrontFile != null)
                {
                    user.CNICFront = Common.SavePic(user.CNICFrontFile, "Comp/", table.CNICFront);
                    new ImageProcessing(user.CNICFront).Process();
                    table.CNICFront = user.CNICFront;
                }
                if (user.CNICBackFile != null)
                {
                    user.CNICBack = Common.SavePic(user.CNICBackFile, "Comp/", table.CNICBack);
                    new ImageProcessing(user.CNICBack).Process();
                    table.CNICBack = user.CNICBack;
                }
                if (user.StuCardFrontFile != null)
                {
                    user.StuCardFront = Common.SavePic(user.StuCardFrontFile, "Comp/", table.StuCardFront);
                    new ImageProcessing(user.StuCardFront).Process();
                    table.StuCardFront = user.StuCardFront;
                }
                if (user.StuCardBackFile != null)
                {
                    user.StuCardBack = Common.SavePic(user.StuCardBackFile, "Comp/", table.StuCardBack);
                    new ImageProcessing(user.StuCardBack).Process();
                    table.StuCardBack = user.StuCardBack;
                }



                table.User_FatherName = user.User_FatherName;
                table.FatherEmail = user.FatherEmail;
                table.FatherProfession = user.FatherProfession;
                table.SponserProfession = user.SponserProfession;
                table.OccupationID = user.OccupationID;
                table.CountryID = user.CountryID;
                table.InstituteID = user.InstituteID;
                table.YearOfMBBS = user.YearOfMBBS;
                table.Institute = user.Institute;
                table.User_Name = user.User_Name;
                table.CNIC = user.CNIC;
                table.JobLocation = user.JobLocation;
                table.CategoryID = user.CategoryID;
                table.City = user.City;
                table.ExamType = user.ExamType;
                table.User_Mobile = user.User_Mobile;
                table.Type = user.Type;

                if (table.User_Id == 0)
                {
                    db.tbl_User.Add(table);
                }
                db.SaveChanges();
                return true;
            }

        }
        public StudentDetail GetStudentDetail(int iD, string ID = null)
        {
            using (DAL.FAMEEntities db = new DAL.FAMEEntities())
            {
                var rt = RequestType.Register.ToString();

                var user = new tbl_User();
                if (string.IsNullOrEmpty(ID))
                    user = db.tbl_User.Find(iD);
                else
                    user = db.tbl_User.FirstOrDefault(x => x.User_AspUser == ID);

                var model = new StudentDetail()
                {
                    User = new StudentVM()
                    {
                        FatherEmail = user.FatherEmail,
                        User_Name = user.User_Name,
                        User_FatherName = user.User_FatherName,
                        SponserProfession = user.SponserProfession,
                        FatherProfession = user.FatherProfession,
                        City = user.City,
                        Institute = user.Institute,
                        JobLocation = user.JobLocation,
                        Email = user.AspNetUsers.Email,
                        Student_Mobile = user.User_Mobile,
                        YearOfMBBS = user.YearOfMBBS,
                        Occupation = user.tbl_Master?.Master_Value,
                        User_Pic = user.User_Pic ?? "userLogo.jpg",
                        User_Id = user.User_Id,
                        Notes = user.Notes,
                        CNICBack = user.CNICBack,
                        CNICFront = user.CNICFront,
                        User_AspUser = user.User_AspUser,
                        StuCardBack = user.StuCardBack,
                        StuCardFront = user.StuCardFront,
                        RequestID = user.AspNetUsers.tbl_Request.FirstOrDefault(x => x.RequestFor == rt)?.RequestID ?? 0,
                        IsAccepted = user.AspNetUsers.tbl_Request.FirstOrDefault(x => x.RequestFor == rt)?.IsAccepted ?? false,
                    },
                };
                return model;
            }
        }


        public bool SaveRoles(string UserID, int?[] Roles)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    db.sp_DeleteUserRoles(UserID);
                    foreach (var r in Roles)
                        if (r > 0)
                            db.sp_AddUserRoles(UserID, r);
                    return true;
                }
                catch { return false; }
            }
        }
        public List<string> GetEmailsByRole(string roleName)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.vw_Roles.Where(x => x.Name == roleName).Select(x => x.Email).ToList();
            }
        }

        public bool Delete(string id)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    var user = db.tbl_User.FirstOrDefault(x => x.User_AspUser == id);
                    if (user != null)
                    {
                        db.tbl_User.Remove(user);
                        db.SaveChanges();
                        return true;
                    }
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public List<SelectListItem> GetStudentsByType(int type)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.vw_StudetsList.Where(x => x.Type == type)
                    .OrderBy(x => x.Text)
                    .ToList()
                    .ConvertAll(x => new SelectListItem { Text = x.Text + " - " + x.User_Name, Value = x.Value });
            }
        }

        public List<tbl_Master> GetAgentCategories()
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.tbl_Master.Where(x => x.Master_Group == (int)MasterGroup.AgentCategory).ToList();
            }
        }
        public int GetLoginCount(string userId)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.tbl_UserLogins.Count(x => x.UserID == userId);
            }
        }

        public List<string> GetUserPics(System.DateTime since)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.tbl_User.Where(x => x.CreateDT > since && x.User_Pic != null).Select(x => x.User_Pic).ToList();
            }
        }
    }
}