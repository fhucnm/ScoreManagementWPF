using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Course
{
    public int CourseId { get; set; }

    public string Title { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public virtual ICollection<CourseSubject> CourseSubjects { get; set; } = new List<CourseSubject>();
}
