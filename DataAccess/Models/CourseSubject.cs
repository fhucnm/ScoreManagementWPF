using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class CourseSubject
{
    public int CourseSubjectId { get; set; }

    public int CourseId { get; set; }

    public int SubjectId { get; set; }

    public int TeacherId { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<CourseRegistration> CourseRegistrations { get; set; } = new List<CourseRegistration>();

    public virtual ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();

    public virtual Subject Subject { get; set; } = null!;

    public virtual UserAccount Teacher { get; set; } = null!;
}
