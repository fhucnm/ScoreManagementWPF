using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class CourseSubjectDisplay
    {
        public int SubjectId { get; set; }
        public string Title { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
    }

}
