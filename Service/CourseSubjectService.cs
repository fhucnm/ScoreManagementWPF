using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace Service
{
    public class CourseSubjectService
    {
        ScoreManagementSystemContext _context;
        public CourseSubjectService() {
            _context = new ScoreManagementSystemContext();
        }

        public CourseSubject? GetById(int courseSubjectId)
        {
            return _context.CourseSubjects.FirstOrDefault(cs => cs.CourseSubjectId == courseSubjectId);
        }
        public List<CourseSubject> GetByCourse(int courseId)
        {
            return _context.CourseSubjects.Include(cs => cs.Subject)
                                           .Where(cs => cs.CourseId == courseId)
                                           .ToList();
        }

        public List<Subject> GetSubjectsByCourse(int courseId)
        {
            return _context.CourseSubjects
                .Include(cs => cs.Subject)
                .Where(cs => cs.CourseId == courseId)
                .Select(cs => cs.Subject)
                .Distinct()
                .ToList();
        }


        public CourseSubject GetByCourseAndSubject(int courseId, int subjectId)
        {
            return _context.CourseSubjects
                .Include(cs => cs.Subject)
                .Include(cs => cs.Teacher)
                .Include(cs => cs.Course)
                .FirstOrDefault(cs => cs.CourseId == courseId && cs.SubjectId == subjectId);
        }


        public List<CourseSubject> GetSubjectsInCourse(int courseId)
        {
            return _context.CourseSubjects
                .Where(cs => cs.CourseId == courseId)
                .Select(cs => new CourseSubject
                {
                    CourseSubjectId = cs.CourseSubjectId,
                    CourseId = cs.CourseId,
                    SubjectId = cs.SubjectId,
                    Subject = cs.Subject
                })
                .ToList();
        }
        public void UpdateCourseAndSubjects(Course course, List<CourseSubject> newCourseSubjects)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var existingCourse = _context.Courses.FirstOrDefault(c => c.CourseId == course.CourseId);
                if (existingCourse == null)
                    throw new Exception("Không tìm thấy lớp học.");

                // Cập nhật lớp
                existingCourse.Title = course.Title;
                existingCourse.StartDate = course.StartDate;
                existingCourse.EndDate = course.EndDate;
                _context.SaveChanges();

                // Lấy danh sách CourseSubject hiện tại
                var existingSubjects = _context.CourseSubjects
                    .Where(cs => cs.CourseId == course.CourseId)
                    .ToList();

                // Cập nhật hoặc thêm mới
                foreach (var newCS in newCourseSubjects)
                {
                    var existing = existingSubjects.FirstOrDefault(cs => cs.SubjectId == newCS.SubjectId);
                    if (existing != null)
                    {
                        // ✅ Nếu đã tồn tại -> cập nhật TeacherId
                        existing.TeacherId = newCS.TeacherId;
                    }
                    else
                    {
                        // ✅ Nếu chưa tồn tại -> thêm mới
                        _context.CourseSubjects.Add(new CourseSubject
                        {
                            CourseId = course.CourseId,
                            SubjectId = newCS.SubjectId,
                            TeacherId = newCS.TeacherId
                        });
                    }
                }

                // Xóa các môn học không còn trong danh sách mới
                foreach (var oldCS in existingSubjects)
                {
                    bool stillExists = newCourseSubjects.Any(cs => cs.SubjectId == oldCS.SubjectId);
                    if (!stillExists)
                    {
                        _context.CourseSubjects.Remove(oldCS);
                    }
                }

                _context.SaveChanges();
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }


        public List<CourseSubject> GetByTeacher(int teacherId)
        {
            return _context.CourseSubjects
                .Include(cs => cs.Course)
                .Include(cs => cs.Subject)
                .Include(cs => cs.Teacher)
                .Where(cs => cs.TeacherId == teacherId)
                .ToList();
        }




    }
}
