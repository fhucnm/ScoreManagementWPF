using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace ScoreManagementWPF1
{
    public partial class StudentWindow : Window
    {
        private readonly ScoreManagementSystemContext _context = new();
        private readonly int _studentId;

        public StudentWindow(int studentId)
        {
            InitializeComponent();
            _studentId = studentId;
            LoadSubjects();
        }

        private void LoadSubjects()
        {
            var subjects = _context.StudentCourses
                .Include(sc => sc.CourseSubject)
                .ThenInclude(cs => cs.Course)
                .Include(sc => sc.CourseSubject.Subject)
                .Where(sc => sc.StudentId == _studentId)
                .Select(sc => new
                {
                    sc.CourseSubjectId,
                    SubjectTitle = $"{sc.CourseSubject.Course.Title} - {sc.CourseSubject.Subject.Title}"
                })
                .Distinct()
                .ToList();

            cbSubjects.ItemsSource = subjects;
        }

        private void cbSubjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbSubjects.SelectedValue is int courseSubjectId)
            {
                LoadMarks(courseSubjectId);
            }
        }

        private void LoadMarks(int courseSubjectId)
        {
            var marks = _context.Marks
                .Include(m => m.Grade)
                .Where(m => m.StudentId == _studentId && m.CourseSubjectId == courseSubjectId)
                .ToList();

            var markDisplay = marks.Select(m => new
            {
                GradeTitle = m.Grade.Title,
                Score = m.Value
            }).ToList();

            dgStudentMarks.ItemsSource = markDisplay;

            var note = marks.FirstOrDefault()?.Note ?? "";
            txtNote.Text = string.IsNullOrWhiteSpace(note) ? "" : $"Ghi chú: {note}";
        }
    }
}