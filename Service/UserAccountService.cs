using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Models;

namespace Service
{
    public class UserAccountService
    {
        private readonly ScoreManagementSystemContext _context;

        public UserAccountService()
        {
            _context = new ScoreManagementSystemContext();
        }

        public UserAccount? Login(string email, string password)
        {
            return _context.UserAccounts
                .FirstOrDefault(u => u.Email == email && u.PasswordHash == password);
        }

        public List<UserAccount> GetTeachers()
        {
            return _context.UserAccounts.Where(u => u.Role == "Teacher").ToList();
        }

        public List<UserAccount> GetAll()
        {
            return _context.UserAccounts.ToList();
        }

        public void Add(UserAccount user)
        {
            if (_context.UserAccounts.Any(u => u.Email.ToLower() == user.Email.ToLower()))
                throw new Exception("Email đã được sử dụng.");

            _context.UserAccounts.Add(user);
            _context.SaveChanges();
        }

        public void Delete(int userId)
        {
            var user = _context.UserAccounts.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                _context.UserAccounts.Remove(user);
                _context.SaveChanges();
            }
        }

        public void Update(UserAccount updatedUser)
        {
            var existing = _context.UserAccounts.FirstOrDefault(u => u.UserId == updatedUser.UserId);
            if (existing == null)
                throw new Exception("Không tìm thấy tài khoản để cập nhật.");

            if (_context.UserAccounts.Any(u => u.Email.ToLower() == updatedUser.Email.ToLower()
                                             && u.UserId != updatedUser.UserId))
                throw new Exception("Email đã được sử dụng.");

            existing.Username = updatedUser.Username;
            existing.PasswordHash = updatedUser.PasswordHash;
            existing.FullName = updatedUser.FullName;
            existing.Email = updatedUser.Email;
            existing.Role = updatedUser.Role;

            _context.SaveChanges();
        }
    }
}
