using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class MarkRowDisplay
    {
        public int StudentId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; }
        public int SubjectId { get; set; }
        public string SubjectTitle { get; set; }
        public Dictionary<int, double> Scores { get; set; } = new();
        public string? Note { get; set; }
    }
}
