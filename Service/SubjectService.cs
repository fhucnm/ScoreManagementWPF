using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DataAccess.Models;

namespace Service
{
    public class SubjectService
    {
        private readonly ScoreManagementSystemContext _context = new();

        public List<Subject> GetAll()
        {
            return _context.Subjects.ToList();
        }

        public void AddWithGradeItems(Subject subject, List<GradeItem> gradeItems)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    if (_context.Subjects.Any(s => s.Title.ToLower() == subject.Title.ToLower()))
                        throw new Exception("Tên môn học đã tồn tại.");

                    decimal totalWeight = gradeItems.Sum(g => g.Value);
                    if (totalWeight != 100)
                        throw new Exception("Tổng trọng số các thành phần điểm phải bằng 100%.");
 
                    _context.Subjects.Add(subject);
                    _context.SaveChanges();

                    foreach (var item in gradeItems)
                    {
                        if (!_context.GradeItems.Any(g => g.SubjectId == subject.SubjectId && g.Title.ToLower() == item.Title.ToLower()))
                        {
                            item.SubjectId = subject.SubjectId;
                            _context.GradeItems.Add(item);
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
        }


        public void UpdateWithGradeItems(Subject subject, List<GradeItem> gradeItems)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    decimal totalWeight = gradeItems.Sum(g => g.Value);
                    if (totalWeight != 100)
                        throw new Exception("Tổng trọng số các thành phần điểm phải bằng 100%.");

                    var existing = _context.Subjects.Find(subject.SubjectId);
                    if (existing != null)
                    {
                        if (_context.Subjects.Any(s => s.SubjectId != subject.SubjectId && s.Title.ToLower() == subject.Title.ToLower()))
                            throw new Exception("Tên môn học đã tồn tại.");

                        existing.Title = subject.Title;
                        existing.Credit = subject.Credit;
                        existing.Description = subject.Description;
                        _context.SaveChanges();
                    }

                    var existingItems = _context.GradeItems.Where(g => g.SubjectId == subject.SubjectId).ToList();
                    foreach (var item in gradeItems)
                    {
                        var match = existingItems.FirstOrDefault(x => x.GradeId == item.GradeId);
                        if (match != null)
                        {
                            match.Title = item.Title;
                            match.Value = item.Value;
                        }
                        else if (!_context.GradeItems.Any(g => g.SubjectId == subject.SubjectId && g.Title.ToLower() == item.Title.ToLower()))
                        {
                            item.SubjectId = subject.SubjectId;
                            _context.GradeItems.Add(item);
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
        }


        public void DeleteWithTransaction(int subjectId)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Kiểm tra có đang được dùng ở CourseSubject
                    bool isUsedInCourse = _context.CourseSubjects.Any(cs => cs.SubjectId == subjectId);
                    if (isUsedInCourse)
                        throw new InvalidOperationException("Môn học đang trong quá trình giảng dạy, không thể xóa.");

                    // Xóa GradeItems trước
                    var gradeItems = _context.GradeItems.Where(g => g.SubjectId == subjectId).ToList();
                    _context.GradeItems.RemoveRange(gradeItems);
                    _context.SaveChanges();

                    var subject = _context.Subjects.Find(subjectId);
                    if (subject != null)
                    {
                        _context.Subjects.Remove(subject);
                    }

                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch (InvalidOperationException ex)
                {
                    transaction.Rollback();
                    throw new Exception(ex.Message); // Truyền lại lỗi cụ thể
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw new Exception("Xóa môn học thất bại. Vui lòng kiểm tra ràng buộc dữ liệu.");
                }
            }
        }




        public bool ExistsByTitle(string title)
        {
            return _context.Subjects.Any(s => s.Title.ToLower() == title.ToLower());
        }

        public bool ExistsByTitle(string title, int exceptId)
        {
            return _context.Subjects.Any(s => s.Title.ToLower() == title.ToLower() && s.SubjectId != exceptId);
        }

        public List<Subject> SearchByTitle(string keyword)
        {
            return _context.Subjects
                .Where(s => s.Title.ToLower().Contains(keyword.ToLower()))
                .ToList();
        }
    }
}