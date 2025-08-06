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
using Service;
using ClosedXML.Excel;
using Microsoft.Win32;



namespace ScoreManagementWPF1
{
    public partial class TeacherWindow : Window
    {
        private ScoreManagementSystemContext _context = new();
        private readonly UserAccountService _userService = new();
        private readonly CourseService _courseService = new();
        private readonly CourseSubjectService _courseSubjectService = new();
        private readonly StudentService _studentService = new();
        private readonly StudentCourseService _studentCourseService = new();
        private readonly SubjectService _subjectService = new();
        private readonly GradeItemService _gradeItemService = new();
        private readonly MarkService _markService = new();

        private List<Student> _allStudents = new();
        private int _editingStudentId;
        private int _editingCourseSubjectId;
        private bool _isEditing = false;
        private readonly int _loggedInTeacherId;

        private Subject _editingSubject;
        private GradeItem _editingGradeItem;

        private List<GradeItem> _currentGradeItems = new();

        private Course _editingCourse;
        private bool _isEditingCourse = false;
        private List<CourseSubject> _selectedSubjectsInCourse = new();
        private List<Subject> _subjects;
        private List<UserAccount> _teachers;

        // Add these to the top of your TeacherWindow class
        private List<GradeItem> _currentGradeItemsMarkTab = new();
        private List<MarkRowDisplay> _markRows = new();





        public TeacherWindow(int userId)
        {
            InitializeComponent();
            _loggedInTeacherId = userId;
            LoadCourses();
            LoadStudentsToComboBox();
            LoadFilterComboBox();
            LoadStudentCourseDisplay();
            LoadCoursesAndSubjects();
            LoadMarkTable();

        }

        //========== tab 1 =================

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Xử lý khi chuyển tab nếu cần (VD: reset form, reload dữ liệu...)
            if (e.Source is TabControl tabControl)
            {
                if (tabControl.SelectedItem is TabItem selectedTab)
                {
                    string tabHeader = selectedTab.Header.ToString();
                    switch (tabHeader)
                    {
                        case "Quản lý sinh viên":
                            LoadCourses();
                            LoadStudentsToComboBox();
                            //LoadStudentCourseGrid();
                            LoadStudentCourseDisplay();
                            break;
                        case "Quản lý điểm":
                           //LoadMarkTable();
                            break;
                        case "Quản lý lớp học phần":
                            LoadCourses();
                            break;
                    }
                }
            }
        }

        // ==================== TAB 1 ====================
        private void LoadCourses()
        {
            var courseSubjects = _courseSubjectService.GetByTeacher(_loggedInTeacherId);
            var teacherCourses = courseSubjects
                .Select(cs => cs.Course)
                .Distinct()
                .ToList();

            CourseComboBox.ItemsSource = teacherCourses;
            cbCourseFilter.ItemsSource = teacherCourses;
        }

        private void CourseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CourseComboBox.SelectedValue is int courseId)
            {
                var courseSubjects = _courseSubjectService.GetByTeacher(_loggedInTeacherId)
                                                          .Where(cs => cs.CourseId == courseId)
                                                          .ToList();
                SubjectComboBox.ItemsSource = courseSubjects;
                SubjectComboBox.DisplayMemberPath = "Subject.Title";
                SubjectComboBox.SelectedValuePath = "CourseSubjectId";
            }
        }

        private void LoadStudentsToComboBox()
        {
            _allStudents = _studentService.GetAllStudents();
            StudentSelectComboBox.ItemsSource = _allStudents;
            StudentSelectComboBox.DisplayMemberPath = "User.FullName";
            StudentSelectComboBox.SelectedValuePath = "StudentId";
        }


        private void LoadStudentCourseDisplay()
        {
            var studentCourses = _studentCourseService.GetAllByTeacher(_loggedInTeacherId);

            var rows = studentCourses.Select(sc => new StudentCourseDisplay
            {
                StudentId = sc.StudentId,
                FullName = sc.Student.User.FullName,
                Email = sc.Student.User.Email,
                CourseTitle = sc.CourseSubject.Course.Title,
                SubjectTitle = sc.CourseSubject.Subject.Title
            }).ToList();

            StudentDataGrid.ItemsSource = rows;
        }






        private void AddStudent_Click(object sender, RoutedEventArgs e)
        {
            if (StudentSelectComboBox.SelectedItem is not Student student)
            {
                MessageBox.Show("Vui lòng chọn sinh viên.");
                return;
            }

            if (SubjectComboBox.SelectedValue is not int courseSubjectId)
            {
                MessageBox.Show("Vui lòng chọn môn học trong lớp học phần.");
                return;
            }

            // Lấy CourseSubject từ DB
            var courseSubject = _courseSubjectService.GetById(courseSubjectId);
            if (courseSubject == null)
            {
                MessageBox.Show("Không tìm thấy môn học trong lớp học phần.");
                return;
            }

            // Kiểm tra xem sinh viên đã có trong lớp/môn chưa
            bool alreadyExists = _studentCourseService.GetAll()
                .Any(sc => sc.StudentId == student.StudentId && sc.CourseSubjectId == courseSubjectId);

            if (alreadyExists)
            {
                MessageBox.Show("Sinh viên đã được thêm vào môn học trong lớp này.");
                return;
            }

            // Thêm vào bảng StudentCourse
            _studentCourseService.Add(student.StudentId, courseSubjectId);
            LoadStudentCourseDisplay(); // Gợi ý: dùng lại hàm load danh sách
            MessageBox.Show($"✅ Đã thêm sinh viên: {student.User.FullName} vào lớp \"{courseSubject.Course.Title}\" - môn \"{courseSubject.Subject.Title}\".");
        }


    




        private void CancelEditStudent_Click(object sender, RoutedEventArgs e)
        {
            _isEditing = false;
            StudentSelectComboBox.SelectedIndex = -1;
            CourseComboBox.SelectedIndex = -1;
            SubjectComboBox.SelectedIndex = -1;
        }

        private void DeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is StudentCourseDisplay display)
            {
                var result = MessageBox.Show("Bạn có chắc muốn xóa sinh viên khỏi lớp học?", "Xác nhận", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    var studentCourses = _studentCourseService.GetAll()
                        .Where(sc => sc.StudentId == display.StudentId && sc.CourseSubject.Course.Title == display.CourseTitle)
                        .ToList();

                    foreach (var sc in studentCourses)
                    {
                        _studentCourseService.Delete(sc.StudentId, sc.CourseSubjectId);
                    }

                    LoadStudentCourseDisplay();
                }
            }
        }


        private void SearchStudent_Click(object sender, RoutedEventArgs e)
        {
            string keyword = StudentSearchBox.Text.Trim().ToLower();
            int? selectedCourseId = cbCourseFilter.SelectedValue as int?;
            int? selectedSubjectId = cbSubjectFilter.SelectedValue as int?;

            var studentCourses = _studentCourseService.GetAllByTeacher(_loggedInTeacherId);

            var filtered = studentCourses
                .Where(sc =>
                    (!selectedCourseId.HasValue || sc.CourseSubject.CourseId == selectedCourseId) &&
                    (!selectedSubjectId.HasValue || sc.CourseSubject.SubjectId == selectedSubjectId) &&
                    (string.IsNullOrWhiteSpace(keyword) ||
                     sc.Student.User.FullName.ToLower().Contains(keyword) ||
                     sc.Student.User.Email.ToLower().Contains(keyword)))
                .Select(sc => new StudentCourseDisplay
                {
                    StudentId = sc.StudentId,
                    FullName = sc.Student.User.FullName,
                    Email = sc.Student.User.Email,
                    CourseTitle = sc.CourseSubject.Course.Title,
                    SubjectTitle = sc.CourseSubject.Subject.Title
                })
                .ToList();

            StudentDataGrid.ItemsSource = filtered;
        }


        // ==================== filter ====================

        private void LoadFilterComboBox()
        {
            var courseSubjects = _courseSubjectService.GetByTeacher(_loggedInTeacherId);

            var courses = courseSubjects.Select(cs => cs.Course).Distinct().ToList();
            var subjects = courseSubjects.Select(cs => cs.Subject).Distinct().ToList();

            cbCourseFilter.ItemsSource = courses;
            cbSubjectFilter.ItemsSource = subjects;

            cbCourseFilter.DisplayMemberPath = "Title";
            cbCourseFilter.SelectedValuePath = "CourseId";
            cbSubjectFilter.DisplayMemberPath = "Title";
            cbSubjectFilter.SelectedValuePath = "SubjectId";
        }

        private void cbCourseFilter_Selection(object sender, SelectionChangedEventArgs e)
        {
            if (cbCourseFilter.SelectedValue is int courseId)
            {
                var subjects = _courseSubjectService.GetByTeacher(_loggedInTeacherId)
                    .Where(cs => cs.CourseId == courseId)
                    .Select(cs => cs.Subject)
                    .Distinct()
                    .ToList();

                cbSubjectFilter.ItemsSource = subjects;
            }

            FilterStudentList();
        }

        private void SubjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterStudentList();
        }

        private void FilterStudentList()
        {
            string keyword = StudentSearchBox.Text.Trim().ToLower();
            int? selectedCourseId = cbCourseFilter.SelectedValue as int?;
            int? selectedSubjectId = cbSubjectFilter.SelectedValue as int?;

            var studentCourses = _studentCourseService.GetAllByTeacher(_loggedInTeacherId);

            var filtered = studentCourses
                .Where(sc =>
                    (!selectedCourseId.HasValue || sc.CourseSubject.CourseId == selectedCourseId) &&
                    (!selectedSubjectId.HasValue || sc.CourseSubject.SubjectId == selectedSubjectId) &&
                    (string.IsNullOrWhiteSpace(keyword) ||
                     sc.Student.User.FullName.ToLower().Contains(keyword) ||
                     sc.Student.User.Email.ToLower().Contains(keyword)))
                .Select(sc => new StudentCourseDisplay
                {
                    StudentId = sc.StudentId,
                    FullName = sc.Student.User.FullName,
                    Email = sc.Student.User.Email,
                    CourseTitle = sc.CourseSubject.Course.Title,
                    SubjectTitle = sc.CourseSubject.Subject.Title
                })
                .ToList();

            StudentDataGrid.ItemsSource = filtered;
        }


        //===================== TAB 2 ====================

        private void LoadCoursesAndSubjects()
        {
            var courseSubjects = _courseSubjectService.GetByTeacher(_loggedInTeacherId);

            var courses = courseSubjects.Select(cs => cs.Course).Distinct().ToList();
            var subjects = courseSubjects.Select(cs => cs.Subject).Distinct().ToList();

            cbMarkCourse.ItemsSource = courses;
            cbMarkCourse.DisplayMemberPath = "Title";
            cbMarkCourse.SelectedValuePath = "CourseId";

            cbMarkSubject.ItemsSource = subjects;
            cbMarkSubject.DisplayMemberPath = "Title";
            cbMarkSubject.SelectedValuePath = "SubjectId";
        }

        private void cbCourseGrade_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbMarkCourse.SelectedValue is int selectedCourseId)
            {
                
                var filteredSubjects = _courseSubjectService
                    .GetByTeacher(_loggedInTeacherId)
                    .Where(cs => cs.CourseId == selectedCourseId)
                    .Select(cs => cs.Subject)
                    .Distinct()
                    .ToList();

                cbMarkSubject.ItemsSource = filteredSubjects;
                cbMarkSubject.DisplayMemberPath = "Title";
                cbMarkSubject.SelectedValuePath = "SubjectId";
            }

            
            if (cbMarkCourse.SelectedValue != null && cbMarkSubject.SelectedValue != null)
            {
                LoadMarkTable();
            }
        }


        private void cbSubjectGrade_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if (cbMarkCourse.SelectedValue != null && cbMarkSubject.SelectedValue != null)
            {
                LoadMarkTable();
            }
        }


        private void LoadMarkTable()
        {
            if (cbMarkCourse.SelectedValue is not int courseId || cbMarkSubject.SelectedValue is not int subjectId) return;

            var courseSubject = _courseSubjectService.GetByCourseAndSubject(courseId, subjectId);
            if (courseSubject == null) return;

            int courseSubjectId = courseSubject.CourseSubjectId;

            _currentGradeItemsMarkTab = _gradeItemService.GetBySubject(subjectId);
            var studentCourses = _studentCourseService.GetByCourseSubject(courseSubjectId);
            var allMarks = _context.Marks.Where(m => m.CourseSubjectId == courseSubjectId).ToList();

            _markRows = studentCourses.Select(sc =>
            {
                var student = sc.Student;
                var scores = new Dictionary<int, double>();
                foreach (var item in _currentGradeItemsMarkTab)
                {
                    var mark = allMarks.FirstOrDefault(m => m.StudentId == student.StudentId && m.GradeId == item.GradeId);
                    scores[item.GradeId] = mark?.Value != null ? (double)mark.Value : 0.0;
                }

                var firstNote = allMarks.FirstOrDefault(m => m.StudentId == student.StudentId)?.Note;

                return new MarkRowDisplay
                {
                    StudentId = student.StudentId,
                    FullName = student.User.FullName,
                    Email = student.User.Email,
                    CourseId = courseSubject.Course.CourseId,
                    CourseTitle = courseSubject.Course.Title,
                    SubjectId = courseSubject.Subject.SubjectId,
                    SubjectTitle = courseSubject.Subject.Title,
                    Scores = scores,
                    Note = firstNote
                };
            }).ToList();

            dgMarks.Columns.Clear();

            dgMarks.Columns.Add(new DataGridTextColumn { Header = "Mã SV", Binding = new Binding("StudentId") });
            dgMarks.Columns.Add(new DataGridTextColumn { Header = "Họ tên", Binding = new Binding("FullName") });
            dgMarks.Columns.Add(new DataGridTextColumn { Header = "Email", Binding = new Binding("Email") });
            dgMarks.Columns.Add(new DataGridTextColumn { Header = "Lớp học phần", Binding = new Binding("CourseTitle") });
            dgMarks.Columns.Add(new DataGridTextColumn { Header = "Môn học", Binding = new Binding("SubjectTitle") });

            foreach (var gradeItem in _currentGradeItemsMarkTab)
            {
                dgMarks.Columns.Add(new DataGridTextColumn
                {
                    Header = gradeItem.Title,
                    Binding = new Binding($"Scores[{gradeItem.GradeId}]") { Mode = BindingMode.TwoWay },
                });
            }


            dgMarks.Columns.Add(new DataGridTextColumn
            {
                Header = "Ghi chú",
                Binding = new Binding("Note") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged }
            });


            dgMarks.Columns.Add(new DataGridTemplateColumn
            {
                Header = "Hành động",
                CellTemplate = (DataTemplate)this.Resources["MarkActionTemplate"]
            });

            dgMarks.ItemsSource = _markRows;

            cbStudentGrade.ItemsSource = _markRows;
            cbStudentGrade.SelectedValuePath = "StudentId";
            cbStudentGrade.DisplayMemberPath = "FullName";
            cbStudentGrade.SelectedIndex = -1;

        }


        private void SearchMark_Click(object sender, RoutedEventArgs e)
        {
            string keyword = txtSearchMark.Text.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(keyword)) { LoadMarkTable(); return; }

            var filtered = _markRows
                .Where(row => row.FullName.ToLower().Contains(keyword) || row.Email.ToLower().Contains(keyword))
                .ToList();

            dgMarks.ItemsSource = filtered;
        }

      

       


        private void dgMarks_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            
        }


        private void SaveEditedMarks_Click(object sender, RoutedEventArgs e)
        {
            if (cbMarkCourse.SelectedValue is not int courseId || cbMarkSubject.SelectedValue is not int subjectId)
            {
                MessageBox.Show("Vui lòng chọn lớp học phần và môn học trước.");
                return;
            }

            var courseSubject = _courseSubjectService.GetByCourseAndSubject(courseId, subjectId);
            if (courseSubject == null)
            {
                MessageBox.Show("Không tìm thấy lớp-môn.");
                return;
            }

            int courseSubjectId = courseSubject.CourseSubjectId;
            bool hasError = false;

            foreach (var row in _markRows)
            {
                foreach (var scoreEntry in row.Scores)
                {
                    int gradeId = scoreEntry.Key;
                    double value = scoreEntry.Value;

                    // Kiểm tra điểm có hợp lệ không
                    if (value < 0 || value > 10)
                    {
                        MessageBox.Show($"Điểm không hợp lệ: {value}\n(Sinh viên: {row.FullName}, GradeId: {gradeId})\nĐiểm phải nằm trong khoảng 0 đến 10.", "Lỗi điểm", MessageBoxButton.OK, MessageBoxImage.Warning);
                        hasError = true;
                    }
                }
            }

            if (hasError)
            {
                MessageBox.Show("Có điểm không hợp lệ. Vui lòng sửa trước khi lưu.", "Lỗi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Nếu không có lỗi, bắt đầu lưu
            foreach (var row in _markRows)
            {
                foreach (var scoreEntry in row.Scores)
                {
                    int gradeId = scoreEntry.Key;
                    double value = scoreEntry.Value;

                    var mark = _context.Marks.FirstOrDefault(m =>
                        m.StudentId == row.StudentId &&
                        m.CourseSubjectId == courseSubjectId &&
                        m.GradeId == gradeId);

                    if (mark != null)
                    {
                        mark.Value = (decimal)value;
                    }
                    else
                    {
                        var newMark = new Mark
                        {
                            StudentId = row.StudentId,
                            CourseSubjectId = courseSubjectId,
                            GradeId = gradeId,
                            Value = (decimal)value,
                            Note = row.Note
                        };
                        _context.Marks.Add(newMark);
                    }
                }

                // Ghi chú chung (chỉ cập nhật 1 lần cho mỗi student)
                var firstMark = _context.Marks.FirstOrDefault(m =>
                    m.StudentId == row.StudentId &&
                    m.CourseSubjectId == courseSubjectId);
                if (firstMark != null)
                {
                    firstMark.Note = row.Note;
                }
            }

            _context.SaveChanges();
            MessageBox.Show("Đã lưu các chỉnh sửa điểm.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadMarkTable();
        }




        private void ImportMarksFromFile_Click(object sender, RoutedEventArgs e)
        {
            if (cbMarkCourse.SelectedValue is not int courseId || cbMarkSubject.SelectedValue is not int subjectId)
            {
                MessageBox.Show("Vui lòng chọn lớp học phần và môn học trước.");
                return;
            }

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".xlsx",
                Filter = "Excel Files (*.xlsx)|*.xlsx"
            };

            bool? result = dlg.ShowDialog();

            if (result != true) return;

            string filePath = dlg.FileName;

            try
            {
                using var workbook = new XLWorkbook(filePath);
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed().Skip(1); // bỏ dòng tiêu đề

                bool hasError = false;

                foreach (var row in rows)
                {
                    int rowIndex = row.RowNumber();

                    if (!int.TryParse(row.Cell(1).GetValue<string>(), out int studentId))
                    {
                        MessageBox.Show($"Lỗi ở dòng {rowIndex}: Không thể đọc mã sinh viên.", "Lỗi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                        hasError = true;
                        continue;
                    }

                    var courseSubject = _courseSubjectService.GetByCourseAndSubject(courseId, subjectId);
                    if (courseSubject == null) continue;

                    int courseSubjectId = courseSubject.CourseSubjectId;

                    // Ghi chú (ở cuối hàng)
                    var note = row.Cell(6 + _currentGradeItemsMarkTab.Count).GetValue<string>();

                    for (int i = 0; i < _currentGradeItemsMarkTab.Count; i++)
                    {
                        var gradeItem = _currentGradeItemsMarkTab[i];
                        var cell = row.Cell(6 + i);
                        string raw = cell.GetValue<string>();

                        if (!double.TryParse(raw, out double value))
                        {
                            MessageBox.Show($"Dòng {rowIndex} - Sinh viên ID {studentId}, cột \"{gradeItem.Title}\": không phải số hợp lệ.\nGiá trị: \"{raw}\"", "Lỗi định dạng", MessageBoxButton.OK, MessageBoxImage.Warning);
                            hasError = true;
                            continue;
                        }

                        if (value < 0 || value > 10)
                        {
                            MessageBox.Show($"Dòng {rowIndex} - Sinh viên ID {studentId}, cột \"{gradeItem.Title}\": điểm ngoài khoảng 0-10.\nGiá trị: {value}", "Lỗi điểm", MessageBoxButton.OK, MessageBoxImage.Warning);
                            hasError = true;
                            continue;
                        }

                        var mark = _context.Marks.FirstOrDefault(m =>
                            m.StudentId == studentId &&
                            m.CourseSubjectId == courseSubjectId &&
                            m.GradeId == gradeItem.GradeId);

                        if (mark != null)
                        {
                            mark.Value = (decimal)value;
                        }
                        else
                        {
                            _context.Marks.Add(new Mark
                            {
                                StudentId = studentId,
                                CourseSubjectId = courseSubjectId,
                                GradeId = gradeItem.GradeId,
                                Value = (decimal)value,
                                Note = note
                            });
                        }
                    }

                    // Gán note cho bản ghi đầu tiên
                    var firstMark = _context.Marks.FirstOrDefault(m =>
                        m.StudentId == studentId &&
                        m.CourseSubjectId == courseSubjectId);
                    if (firstMark != null)
                    {
                        firstMark.Note = note;
                    }
                }

                if (hasError)
                {
                    MessageBox.Show("Có lỗi dữ liệu trong file. Không thể lưu điểm.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    _context.SaveChanges();
                    LoadMarkTable();
                    MessageBox.Show("Đã nhập điểm thành công.", "Hoàn tất", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi đọc file: " + ex.Message, "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void ExportByStudent_Click(object sender, RoutedEventArgs e)
        {
            if (cbStudentGrade.SelectedValue is not int selectedStudentId)
            {
                MessageBox.Show("Vui lòng chọn một sinh viên để xuất file.");
                return;
            }

            var selectedStudent = _markRows.FirstOrDefault(r => r.StudentId == selectedStudentId);
            if (selectedStudent == null)
            {
                MessageBox.Show("Không tìm thấy dữ liệu sinh viên.");
                return;
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Student_Mark");

            int col = 1;
            worksheet.Cell(1, col++).Value = "Mã SV";
            worksheet.Cell(1, col++).Value = "Họ tên";
            worksheet.Cell(1, col++).Value = "Email";
            worksheet.Cell(1, col++).Value = "Lớp học phần";
            worksheet.Cell(1, col++).Value = "Môn học";

            foreach (var gradeItem in _currentGradeItemsMarkTab)
            {
                worksheet.Cell(1, col++).Value = gradeItem.Title;
            }

            worksheet.Cell(1, col++).Value = "Ghi chú";

            int rowIndex = 2;
            col = 1;
            worksheet.Cell(rowIndex, col++).Value = selectedStudent.StudentId;
            worksheet.Cell(rowIndex, col++).Value = selectedStudent.FullName;
            worksheet.Cell(rowIndex, col++).Value = selectedStudent.Email;
            worksheet.Cell(rowIndex, col++).Value = selectedStudent.CourseTitle;
            worksheet.Cell(rowIndex, col++).Value = selectedStudent.SubjectTitle;

            foreach (var gradeItem in _currentGradeItemsMarkTab)
            {
                worksheet.Cell(rowIndex, col++).Value =
                    selectedStudent.Scores.TryGetValue(gradeItem.GradeId, out double val) ? val : "";
            }

            worksheet.Cell(rowIndex, col++).Value = selectedStudent.Note ?? "";
            worksheet.Columns().AdjustToContents();

            
            var dlg = new SaveFileDialog
            {
                FileName = $"Diem_{selectedStudent.FullName.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                Filter = "Excel Files (*.xlsx)|*.xlsx"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    workbook.SaveAs(dlg.FileName);
                    MessageBox.Show($"Đã xuất file:\n{dlg.FileName}", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xuất file:\n" + ex.Message);
                }
            }
        }


        private void ExportBySubject_Click(object sender, RoutedEventArgs e)
        {
            if (_markRows == null || !_markRows.Any())
            {
                MessageBox.Show("Không có dữ liệu để xuất.");
                return;
            }

            if (_currentGradeItemsMarkTab == null || !_currentGradeItemsMarkTab.Any())
            {
                MessageBox.Show("Không có thành phần điểm để xuất.");
                return;
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Marks_By_Subject");

            int col = 1;

            
            worksheet.Cell(1, col++).Value = "Mã SV";
            worksheet.Cell(1, col++).Value = "Họ tên";
            worksheet.Cell(1, col++).Value = "Email";
            worksheet.Cell(1, col++).Value = "Lớp học phần";
            worksheet.Cell(1, col++).Value = "Môn học";

            
            foreach (var gradeItem in _currentGradeItemsMarkTab)
            {
                worksheet.Cell(1, col++).Value = gradeItem.Title;
            }

            worksheet.Cell(1, col++).Value = "Ghi chú";

            
            for (int i = 0; i < _markRows.Count; i++)
            {
                var row = _markRows[i];
                int rowIndex = i + 2;
                int dataCol = 1;

                worksheet.Cell(rowIndex, dataCol++).Value = row.StudentId;
                worksheet.Cell(rowIndex, dataCol++).Value = row.FullName;
                worksheet.Cell(rowIndex, dataCol++).Value = row.Email;
                worksheet.Cell(rowIndex, dataCol++).Value = row.CourseTitle;
                worksheet.Cell(rowIndex, dataCol++).Value = row.SubjectTitle;

                foreach (var gradeItem in _currentGradeItemsMarkTab)
                {
                    if (row.Scores.TryGetValue(gradeItem.GradeId, out double score))
                        worksheet.Cell(rowIndex, dataCol++).Value = score;
                    else
                        worksheet.Cell(rowIndex, dataCol++).Value = "";
                }

                worksheet.Cell(rowIndex, dataCol++).Value = row.Note ?? "";
            }

            worksheet.Columns().AdjustToContents();

            
            var dlg = new SaveFileDialog
            {
                FileName = $"Diem_Mon_{_markRows[0].SubjectTitle}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                Filter = "Excel Files (*.xlsx)|*.xlsx"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    workbook.SaveAs(dlg.FileName);
                    MessageBox.Show($"Đã xuất file:\n{dlg.FileName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi lưu file:\n" + ex.Message);
                }
            }
        }



        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // Mở lại màn hình đăng nhập (MainWindow)
            MainWindow loginWindow = new MainWindow();
            loginWindow.Show();

            // Đóng cửa sổ admin hiện tại
            this.Close();
        }


    }
}
