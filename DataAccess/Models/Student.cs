using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Student
{
    public int StudentId { get; set; }

    public int UserId { get; set; }

    public string Phone { get; set; } = null!;

    public string Gender { get; set; } = null!;

    public DateOnly DateOfBirth { get; set; }

    public DateOnly EnrollmentDate { get; set; }

    public virtual ICollection<CourseRegistration> CourseRegistrations { get; set; } = new List<CourseRegistration>();

    public virtual ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();

    public virtual UserAccount User { get; set; } = null!;
}
