using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class CourseRegistration
{
    public int RegistrationId { get; set; }

    public int StudentId { get; set; }

    public int CourseSubjectId { get; set; }

    public DateTime? RegistrationDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual CourseSubject CourseSubject { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
