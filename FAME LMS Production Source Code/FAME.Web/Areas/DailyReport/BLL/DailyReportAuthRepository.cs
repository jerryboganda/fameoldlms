using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace First_Aid_Made_Easy.BLL
{
    public class DailyReportAuthRepository : IDailyReportAuthRepository
    {
        private readonly FAMEEntities db = new FAMEEntities();

        public DailyReportUserVM ValidateUser(string username, string password)
        {
            var user = db.tbl_DailyReportUsers
                .FirstOrDefault(x => x.UserName == username && x.Password == password);

            if (user == null) return null;

            return new DailyReportUserVM
            {
                ID = user.ID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Role = user.Role ?? "User", // Default to User if Role is null
                CreatedDT = user.CreatedDT,
                ModifyDT = user.ModifyDT
            };
        }

        public bool IsUsernameTaken(string username)
        {
            return db.tbl_DailyReportUsers.Any(x => x.UserName == username);
        }

        public bool RegisterUser(DailyReportRegisterVM model)
        {
            try
            {
                if (IsUsernameTaken(model.UserName))
                    return false;

                var user = new tbl_DailyReportUsers
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.UserName,
                    Password = model.Password, // Plain password as requested
                    Role = "User", // Default role for self-registration
                    CreatedDT = DateTime.Now
                };

                db.tbl_DailyReportUsers.Add(user);
                return db.SaveChanges() > 0;
            }
            catch
            {
                return false;
            }
        }

        public List<DailyReportUserVM> GetAllUsers()
        {
            return db.tbl_DailyReportUsers.ToList()
                .ConvertAll(x => new DailyReportUserVM
                {
                    ID = x.ID,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    UserName = x.UserName,
                    Role = x.Role ?? "User",
                    CreatedDT = x.CreatedDT,
                    ModifyDT = x.ModifyDT
                });
        }

        public DailyReportUserVM GetUserById(int id)
        {
            var user = db.tbl_DailyReportUsers.Find(id);
            if (user == null) return null;

            return new DailyReportUserVM
            {
                ID = user.ID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Role = user.Role ?? "User",
                CreatedDT = user.CreatedDT,
                ModifyDT = user.ModifyDT
            };
        }

        public bool UpdateUser(DailyReportUserVM model)
        {
            try
            {
                var user = db.tbl_DailyReportUsers.Find(model.ID);
                if (user == null) return false;

                // Check if username is taken by another user
                if (db.tbl_DailyReportUsers.Any(x => x.UserName == model.UserName && x.ID != model.ID))
                    return false;

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.UserName = model.UserName;
                user.Role = model.Role;
                user.ModifyDT = DateTime.Now;

                // Only update password if it's provided
                if (!string.IsNullOrEmpty(model.Password))
                {
                    user.Password = model.Password;
                }

                return db.SaveChanges() > 0;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteUser(int id)
        {
            try
            {
                var user = db.tbl_DailyReportUsers.Find(id);
                if (user == null) return false;

                // Check if user has reports - prevent deletion if they do
                if (db.tbl_DailyReport.Any(x => x.UserFid == id))
                    return false;

                db.tbl_DailyReportUsers.Remove(user);
                return db.SaveChanges() > 0;
            }
            catch
            {
                return false;
            }
        }

        public bool CreateAdminUser(DailyReportUserVM model)
        {
            try
            {
                if (IsUsernameTaken(model.UserName))
                    return false;

                var user = new tbl_DailyReportUsers
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.UserName,
                    Password = model.Password,
                    Role = "Admin",
                    CreatedDT = DateTime.Now
                };

                db.tbl_DailyReportUsers.Add(user);
                return db.SaveChanges() > 0;
            }
            catch
            {
                return false;
            }
        }

        public bool HasAnyUsers()
        {
            return db.tbl_DailyReportUsers.Any();
        }

        public bool IsAdmin(int userId)
        {
            var user = db.tbl_DailyReportUsers.Find(userId);
            return user != null && user.Role == "Admin";
        }
    }
}