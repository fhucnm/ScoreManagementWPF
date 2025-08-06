using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace Service
{
    public class StudentService
    {
        private readonly ScoreManagementSystemContext _context = new();

        public List<Student> GetAllStudents()
        {
            return _context.Students.Include(s => s.User).ToList();
        }

        public List<Student> SearchStudents(string keyword)
        {
            return _context.Students.Include(s => s.User)
                .Where(s => s.User.FullName.Contains(keyword) || s.User.Email.Contains(keyword))
                .ToList();
        }

        public void Add(Student s)
        {
            _context.Students.Add(s);
            _context.SaveChanges();
        }

    }
}
