using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class StudentCourse
{
    public int StudentId { get; set; }

    public int CourseSubjectId { get; set; }

    public virtual CourseSubject CourseSubject { get; set; } = null!;

    public virtual ICollection<Mark> Marks { get; set; } = new List<Mark>();

    public virtual Student Student { get; set; } = null!;
}
