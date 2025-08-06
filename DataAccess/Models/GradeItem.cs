using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class GradeItem
{
    public int GradeId { get; set; }

    public string Title { get; set; } = null!;

    public decimal Value { get; set; }

    public int SubjectId { get; set; }

    public virtual ICollection<Mark> Marks { get; set; } = new List<Mark>();

    public virtual Subject Subject { get; set; } = null!;
}
