using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Mark
{
    public int StudentId { get; set; }

    public int CourseSubjectId { get; set; }

    public int GradeId { get; set; }

    public decimal Value { get; set; }

    public string? Note { get; set; }

    public virtual GradeItem Grade { get; set; } = null!;

    public virtual StudentCourse StudentCourse { get; set; } = null!;
    public virtual CourseSubject CourseSubject { get; set; } = null!;
}
