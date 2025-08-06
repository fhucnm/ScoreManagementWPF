using System;
using System.Collections.Generic;
using System.Linq;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace Service
{
    public class StudentCourseService
    {
        private readonly ScoreManagementSystemContext _context = new();

        public List<StudentCourse> GetAll()
        {
            return _context.StudentCourses
                .Include(sc => sc.Student).ThenInclude(s => s.User)
                .Include(sc => sc.CourseSubject).ThenInclude(cs => cs.Subject)
                .Include(sc => sc.CourseSubject).ThenInclude(cs => cs.Teacher)
                .Include(sc => sc.CourseSubject).ThenInclude(cs => cs.Course) // 👈 thêm dòng này
                .ToList();
        }



        public void Add(int studentId, int courseSubjectId)
        {
            if (!_context.StudentCourses.Any(sc => sc.StudentId == studentId && sc.CourseSubjectId == courseSubjectId))
            {
                _context.StudentCourses.Add(new StudentCourse { StudentId = studentId, CourseSubjectId = courseSubjectId });
                _context.SaveChanges();
            }
        }

        public void Update(int oldStudentId, int oldCourseSubjectId, int newStudentId, int newCourseSubjectId)
        {
            var existing = _context.StudentCourses
                .FirstOrDefault(sc => sc.StudentId == oldStudentId && sc.CourseSubjectId == oldCourseSubjectId);

            if (existing != null)
            {
                _context.StudentCourses.Remove(existing);
                _context.SaveChanges();

                var newStudentCourse = new StudentCourse
                {
                    StudentId = newStudentId,
                    CourseSubjectId = newCourseSubjectId
                };

                _context.StudentCourses.Add(newStudentCourse);
                _context.SaveChanges();
            }
        }



        public void Delete(int studentId, int courseSubjectId)
        {
            var sc = _context.StudentCourses.FirstOrDefault(s => s.StudentId == studentId && s.CourseSubjectId == courseSubjectId);
            if (sc != null)
            {
                _context.StudentCourses.Remove(sc);
                _context.SaveChanges();
            }
        }

        public List<StudentCourse> GetStudentsInCourseSubject(int courseId, int subjectId)
        {
            return _context.StudentCourses
                .Include(sc => sc.Student)
                    .ThenInclude(s => s.User) // 👈 Bắt buộc phải có
                .Include(sc => sc.CourseSubject)
                .Where(sc => sc.CourseSubject.CourseId == courseId && sc.CourseSubject.SubjectId == subjectId)
                .ToList();
        }

        public List<StudentCourse> GetAllByTeacher(int teacherId)
        {
            return _context.StudentCourses
                .Include(sc => sc.Student).ThenInclude(s => s.User)
                .Include(sc => sc.CourseSubject).ThenInclude(cs => cs.Course)
                .Include(sc => sc.CourseSubject).ThenInclude(cs => cs.Subject)
                .Include(sc => sc.CourseSubject).ThenInclude(cs => cs.Teacher)
                .Where(sc => sc.CourseSubject.TeacherId == teacherId)
                .ToList();
        }



        public List<StudentCourse> GetByCourseSubject(int courseSubjectId)
        {
            return _context.StudentCourses
                           .Include(sc => sc.Student)
                               .ThenInclude(s => s.User)
                           .Include(sc => sc.CourseSubject)
                               .ThenInclude(cs => cs.Course)
                           .Include(sc => sc.CourseSubject)
                               .ThenInclude(cs => cs.Subject)
                           .Where(sc => sc.CourseSubjectId == courseSubjectId)
                           .ToList();
        }


    }
}