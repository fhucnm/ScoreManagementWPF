using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class StudentCourseDisplay
    {
        public int StudentId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string CourseTitle { get; set; }

        public string Subjects { get; set; } // Chuỗi "Toán - GV: A\nLý - GV: B"
        public string SubjectTitle { get; set; }
    }
}
