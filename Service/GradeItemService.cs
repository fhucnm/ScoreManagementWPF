using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Models;

namespace Service
{
    public class GradeItemService
    {
        private readonly ScoreManagementSystemContext _context = new();

        public List<GradeItem> GetBySubject(int subjectId)
        {
            return _context.GradeItems
                .Where(g => g.SubjectId == subjectId)
                .ToList();
        }

        public void Add(GradeItem item)
        {
            if (!_context.GradeItems.Any(g => g.SubjectId == item.SubjectId && g.Title.ToLower() == item.Title.ToLower()))
            {
                _context.GradeItems.Add(item);
                _context.SaveChanges();
            }
            else
            {
                throw new Exception("Thành phần điểm đã tồn tại trong môn học này.");
            }
        }

        public void Update(GradeItem item)
        {
            var existing = _context.GradeItems.FirstOrDefault(g => g.GradeId == item.GradeId);
            if (existing != null)
            {
                existing.Title = item.Title;
                existing.Value = item.Value;
                _context.SaveChanges();
            }
            else
            {
                throw new Exception("Không tìm thấy thành phần điểm để cập nhật.");
            }
        }

        public void Delete(int gradeItemId)
        {
            try
            {
                var grade = _context.GradeItems.FirstOrDefault(g => g.GradeId == gradeItemId);
                if (grade == null)
                {
                    throw new Exception("Không tìm thấy thành phần điểm để xóa.");
                }

                // Kiểm tra xem có đang bị dùng trong bảng Mark không
                bool isUsedInMark = _context.Marks.Any(m => m.GradeId == gradeItemId);
                if (isUsedInMark)
                {
                    throw new InvalidOperationException("Không thể xóa vì thành phần điểm đang được sử dụng để nhập điểm cho sinh viên.");
                }

                _context.GradeItems.Remove(grade);
                _context.SaveChanges();
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception(ex.Message); // truyền tiếp thông báo cụ thể
            }
            catch (Exception)
            {
                throw new Exception("Xóa thành phần điểm thất bại. Vui lòng kiểm tra ràng buộc dữ liệu.");
            }
        }

    }
}
