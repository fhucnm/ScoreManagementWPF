using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace Service
{
    public class MarkService
    {
        private readonly ScoreManagementSystemContext _context = new();
        public List<Mark> GetByStudentCourse(int studentId, int courseSubjectId)
        {
            return _context.Marks
                .Where(m => m.StudentId == studentId && m.CourseSubjectId == courseSubjectId)
                .ToList();
        }

        public Mark? GetMark(int studentId, int courseId, int subjectId, int gradeId)
        {
            return _context.Marks
                .FirstOrDefault(m => m.StudentId == studentId
                                  && m.GradeId == gradeId
                                  && m.StudentCourse.CourseSubject.CourseId == courseId
                                  && m.StudentCourse.CourseSubject.SubjectId == subjectId);
        }


        public void SaveOrUpdateMark(int studentId, int courseId, int subjectId, int gradeId, decimal? score, string? note)
        {
            // Kiểm tra điểm có hợp lệ không (phải là số trong khoảng 0-10)
            if (score is null || score < 0 || score > 10)
            {
                throw new ArgumentException($"Điểm không hợp lệ: {score}. Vui lòng nhập giá trị từ 0 đến 10.");
            }

            var courseSubjectId = _context.CourseSubjects
                .Where(cs => cs.CourseId == courseId && cs.SubjectId == subjectId)
                .Select(cs => cs.CourseSubjectId)
                .FirstOrDefault();

            if (courseSubjectId == 0) return;

            var existing = _context.Marks.FirstOrDefault(m =>
                m.StudentId == studentId &&
                m.CourseSubjectId == courseSubjectId &&
                m.GradeId == gradeId);

            if (existing != null)
            {
                existing.Value = (decimal)score;
                existing.Note = note;
            }
            else
            {
                _context.Marks.Add(new Mark
                {
                    StudentId = studentId,
                    CourseSubjectId = courseSubjectId,
                    GradeId = gradeId,
                    Value = (decimal)score,
                    Note = note
                });
            }

            _context.SaveChanges();
        }


    }
}