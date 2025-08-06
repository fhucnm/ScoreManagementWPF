using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Models;

public partial class ScoreManagementSystemContext : DbContext
{
    public ScoreManagementSystemContext()
    {
    }

    public ScoreManagementSystemContext(DbContextOptions<ScoreManagementSystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseRegistration> CourseRegistrations { get; set; }

    public virtual DbSet<CourseSubject> CourseSubjects { get; set; }

    public virtual DbSet<GradeItem> GradeItems { get; set; }

    public virtual DbSet<Mark> Marks { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentCourse> StudentCourses { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<UserAccount> UserAccounts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=LAPTOP-F97RKI4T;Database=ScoreManagementSystem;User Id=sa;Password=123;TrustServerCertificate=true;Trusted_Connection=SSPI;Encrypt=false;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Course__C92D7187B040C9B4");

            entity.ToTable("Course");

            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.Title).HasMaxLength(100);
        });

        modelBuilder.Entity<CourseRegistration>(entity =>
        {
            entity.HasKey(e => e.RegistrationId).HasName("PK__CourseRe__6EF58830D5353617");

            entity.ToTable("CourseRegistration");

            entity.Property(e => e.RegistrationId).HasColumnName("RegistrationID");
            entity.Property(e => e.CourseSubjectId).HasColumnName("CourseSubjectID");
            entity.Property(e => e.RegistrationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.StudentId).HasColumnName("StudentID");

            entity.HasOne(d => d.CourseSubject).WithMany(p => p.CourseRegistrations)
                .HasForeignKey(d => d.CourseSubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CourseReg__Cours__6A30C649");

            entity.HasOne(d => d.Student).WithMany(p => p.CourseRegistrations)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CourseReg__Stude__693CA210");
        });

        modelBuilder.Entity<CourseSubject>(entity =>
        {
            entity.HasKey(e => e.CourseSubjectId).HasName("PK__CourseSu__8E8409568E3218D4");

            entity.ToTable("CourseSubject");

            entity.HasIndex(e => new { e.CourseId, e.SubjectId }, "UQ_Course_Subject").IsUnique();

            entity.HasIndex(e => new { e.CourseId, e.TeacherId }, "UQ_Course_Teacher").IsUnique();

            entity.Property(e => e.CourseSubjectId).HasColumnName("CourseSubjectID");
            entity.Property(e => e.CourseId).HasColumnName("CourseID");
            entity.Property(e => e.SubjectId).HasColumnName("SubjectID");
            entity.Property(e => e.TeacherId).HasColumnName("TeacherID");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseSubjects)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__CourseSub__Cours__5812160E");

            entity.HasOne(d => d.Subject).WithMany(p => p.CourseSubjects)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CourseSub__Subje__59063A47");

            entity.HasOne(d => d.Teacher).WithMany(p => p.CourseSubjects)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CourseSub__Teach__59FA5E80");
        });

        modelBuilder.Entity<GradeItem>(entity =>
        {
            entity.HasKey(e => e.GradeId).HasName("PK__GradeIte__54F87A376A4EEAE8");

            entity.ToTable("GradeItem");

            entity.Property(e => e.GradeId).HasColumnName("GradeID");
            entity.Property(e => e.SubjectId).HasColumnName("SubjectID");
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.Value).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Subject).WithMany(p => p.GradeItems)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GradeItem__Subje__5CD6CB2B");
        });

        modelBuilder.Entity<Mark>(entity =>
        {
            entity.HasKey(e => new { e.StudentId, e.CourseSubjectId, e.GradeId }).HasName("PK__Mark__7C7992968B6C4D8E");

            entity.ToTable("Mark");

            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.CourseSubjectId).HasColumnName("CourseSubjectID");
            entity.Property(e => e.GradeId).HasColumnName("GradeID");
            entity.Property(e => e.Note).HasMaxLength(255);
            entity.Property(e => e.Value).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Grade).WithMany(p => p.Marks)
                .HasForeignKey(d => d.GradeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Mark__GradeID__6477ECF3");

            entity.HasOne(d => d.StudentCourse).WithMany(p => p.Marks)
                .HasForeignKey(d => new { d.StudentId, d.CourseSubjectId })
                .HasConstraintName("FK__Mark__6383C8BA");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Student__32C52A79905D37BD");

            entity.ToTable("Student");

            entity.HasIndex(e => e.UserId, "UQ__Student__1788CCADEC089680").IsUnique();

            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.Phone).HasMaxLength(15);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithOne(p => p.Student)
                .HasForeignKey<Student>(d => d.UserId)
                .HasConstraintName("FK__Student__UserID__4F7CD00D");
        });

        modelBuilder.Entity<StudentCourse>(entity =>
        {
            entity.HasKey(e => new { e.StudentId, e.CourseSubjectId }).HasName("PK__StudentC__4A2D6AEC3128EDC7");

            entity.ToTable("StudentCourse");

            entity.Property(e => e.StudentId).HasColumnName("StudentID");
            entity.Property(e => e.CourseSubjectId).HasColumnName("CourseSubjectID");

            entity.HasOne(d => d.CourseSubject).WithMany(p => p.StudentCourses)
                .HasForeignKey(d => d.CourseSubjectId)
                .HasConstraintName("FK__StudentCo__Cours__60A75C0F");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentCourses)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentCo__Stude__5FB337D6");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.SubjectId).HasName("PK__Subject__AC1BA388245C8992");

            entity.ToTable("Subject");

            entity.Property(e => e.SubjectId).HasColumnName("SubjectID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(100);
        });

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__UserAcco__1788CCAC9AF91D1A");

            entity.ToTable("UserAccount");

            entity.HasIndex(e => e.Username, "UQ__UserAcco__536C85E4AC905916").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(20);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
