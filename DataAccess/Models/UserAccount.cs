using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class UserAccount
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Role { get; set; } = null!;

    public virtual ICollection<CourseSubject> CourseSubjects { get; set; } = new List<CourseSubject>();

    public virtual Student? Student { get; set; }
}
