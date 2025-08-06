using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Subject
{
    public int SubjectId { get; set; }

    public string Title { get; set; } = null!;

    public int Credit { get; set; }

    public string Description { get; set; } = null!;

    public virtual ICollection<CourseSubject> CourseSubjects { get; set; } = new List<CourseSubject>();

    public virtual ICollection<GradeItem> GradeItems { get; set; } = new List<GradeItem>();
}
