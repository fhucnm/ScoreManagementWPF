using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Service
{
    public class CourseService
    {
        private readonly ScoreManagementSystemContext _context;

        public CourseService()
        {
            _context = new ScoreManagementSystemContext();
        }

        public List<Course> GetAll()
        {
            return _context.Courses.ToList();
        }

        public void AddCourse(Course course, List<CourseSubject> courseSubjects)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                _context.Courses.Add(course);
                _context.SaveChanges();

                foreach (var cs in courseSubjects)
                {
                    cs.CourseId = course.CourseId;
                    _context.CourseSubjects.Add(cs);
                }

                _context.SaveChanges();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void UpdateCourse(Course course)
        {
            var existing = _context.Courses.Find(course.CourseId);
            if (existing != null)
            {
                existing.Title = course.Title;
                existing.StartDate = course.StartDate;
                existing.EndDate = course.EndDate;
                _context.SaveChanges();
            }
        }

        public void DeleteCourse(int courseId)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var courseSubjects = _context.CourseSubjects.Where(cs => cs.CourseId == courseId);
                _context.CourseSubjects.RemoveRange(courseSubjects);

                var course = _context.Courses.Find(courseId);
                if (course != null)
                {
                    _context.Courses.Remove(course);
                }

                _context.SaveChanges();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<Course> SearchByTitle(string keyword)
        {
            return _context.Courses
                .Where(c => c.Title.ToLower().Contains(keyword.ToLower()))
                .ToList();
        }

    }
}
