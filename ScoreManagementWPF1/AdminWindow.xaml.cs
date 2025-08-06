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
using Service;

namespace ScoreManagementWPF1
{
    /// <summary>
    /// Interaction logic for AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        private ScoreManagementSystemContext _context = new();
        private readonly UserAccountService _userService = new();
        private readonly CourseService _courseService = new();
        private readonly CourseSubjectService _courseSubjectService = new();
        private readonly StudentService _studentService = new();
        private readonly StudentCourseService _studentCourseService = new();
        private readonly SubjectService _subjectService = new();
        private readonly GradeItemService _gradeItemService = new();

        private List<Student> _allStudents = new();
        private int _editingStudentId;
        private int _editingCourseSubjectId;
        private bool _isEditing = false;

        private Subject _editingSubject;
        private GradeItem _editingGradeItem;

        private List<GradeItem> _currentGradeItems = new();

        private Course _editingCourse;
        private bool _isEditingCourse = false;
        private List<CourseSubject> _selectedSubjectsInCourse = new();
        private List<Subject> _subjects;
        private List<UserAccount> _teachers;


        private List<UserAccount> _allUsers = new();
        private int _editingUserId = 0;
        private bool _isEditingUser = false;



        public AdminWindow(int userId)
        {
            InitializeComponent();
            LoadCourses();
            LoadStudentsToComboBox();
            LoadFilterComboBox();
            //LoadStudentCourseGrid();
            LoadSubjects();
            LoadStudentCourseDisplay();
            LoadUserList();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Đảm bảo sự kiện chỉ xử lý khi người dùng chọn tab mới
            if (e.Source is TabControl tabControl && e.OriginalSource == tabControl)
            {
                if (tabControl.SelectedItem is TabItem selectedTab)
                {
                    string tabHeader = selectedTab.Header.ToString();
                    switch (tabHeader)
                    {
                        case "Quản lý sinh viên":
                            LoadCourses();
                            LoadStudentsToComboBox();
                            LoadStudentCourseDisplay();
                            break;

                        case "Quản lý môn học":
                            LoadSubjects();
                            break;

                        case "Quản lý lớp học phần":
                            LoadCourses();
                            LoadSubjects();
                            break;
                        case "Thêm thành viên mới":
                            txtUserSearch.Text = "";            
                            cbRoleFilter.SelectedIndex = 0;     
                            LoadUserList();
                            break;
                    }
                }
            }
        }


        // ==================== TAB 1 ====================
        private void LoadCourses()
        {
            CourseComboBox.ItemsSource = _courseService.GetAll();
            dgCourses.ItemsSource = _courseService.GetAll();
            _subjects = _subjectService.GetAll();
            cbxCourseSubject.ItemsSource = _subjects;
            cbxCourseSubject.DisplayMemberPath = "Title";
            cbxCourseSubject.SelectedValuePath = "SubjectId";

            _teachers = _userService.GetTeachers();
            cbxTeacher.ItemsSource = _teachers;
            cbxTeacher.DisplayMemberPath = "FullName";
            cbxTeacher.SelectedValuePath = "UserId";
            var result = (from course in _context.Courses
                          join cs in _context.CourseSubjects on course.CourseId equals cs.CourseId into csGroup
                          select new
                          {
                              course.CourseId,
                              course.Title,
                              course.StartDate,
                              course.EndDate,
                              SubjectInfo = string.Join("\n",
                                  from cs in csGroup
                                  join subj in _context.Subjects on cs.SubjectId equals subj.SubjectId
                                  join teacher in _context.UserAccounts on cs.TeacherId equals teacher.UserId
                                  select $"{subj.Title} - GV: {teacher.FullName}")
                          }).ToList();

            dgCourses.ItemsSource = result;


        }

        private void CourseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CourseComboBox.SelectedValue is int courseId)
            {
                var subjects = _courseSubjectService.GetByCourse(courseId);
                SubjectComboBox.ItemsSource = subjects;
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


        private void LoadStudentCourseGrid()
        {
            var data = _studentCourseService.GetAll()
    .Where(sc => sc.Student != null
              && sc.Student.User != null
              && sc.CourseSubject != null
              && sc.CourseSubject.Course != null
              && sc.CourseSubject.Subject != null
              && sc.CourseSubject.Teacher != null)
    .GroupBy(sc => new { sc.StudentId, sc.CourseSubject.CourseId })
    .Select(g => new
    {
        StudentId = g.Key.StudentId,
        FullName = g.First().Student.User.FullName,
        Email = g.First().Student.User.Email,
        CourseTitle = g.First().CourseSubject.Course.Title,
        Subjects = string.Join("\n", g.Select(x =>
            $"{x.CourseSubject.Subject.Title} - GV: {x.CourseSubject.Teacher.FullName}"))
    })
    .ToList();


            StudentDataGrid.ItemsSource = data;
        }

        private void LoadStudentCourseDisplay()
        {
            var studentCourses = _studentCourseService.GetAll();

            var rows = studentCourses
                .Where(sc => sc.Student != null
                    && sc.Student.User != null
                    && sc.CourseSubject != null
                    && sc.CourseSubject.Course != null
                    && sc.CourseSubject.Subject != null
                    && sc.CourseSubject.Teacher != null)
                .Select(sc => new StudentCourseDisplay
                {
                    StudentId = sc.StudentId,
                    FullName = sc.Student.User.FullName,
                    Email = sc.Student.User.Email,
                    CourseTitle = sc.CourseSubject.Course.Title,
                    Subjects = $"{sc.CourseSubject.Subject.Title} - {sc.CourseSubject.Teacher.FullName}",
                })
                .ToList();

            StudentDataGrid.ItemsSource = rows;
        }





        private void AddStudent_Click(object sender, RoutedEventArgs e)
        {
            if (StudentSelectComboBox.SelectedItem is Student student && SubjectComboBox.SelectedValue is int courseSubjectId)
            {
                var courseSubject = _courseSubjectService.GetById(courseSubjectId);
                if (courseSubject == null)
                {
                    MessageBox.Show("Không tìm thấy môn học trong lớp học phần.");
                    return;
                }

                // Kiểm tra trùng môn học trong cùng lớp học phần
                var existing = _studentCourseService.GetAll()
                    .FirstOrDefault(sc => sc.StudentId == student.StudentId &&
                                          sc.CourseSubject != null &&
                                          sc.CourseSubject.CourseId == courseSubject.CourseId &&
                                          sc.CourseSubject.SubjectId == courseSubject.SubjectId);
                if (existing != null)
                {
                    MessageBox.Show("Sinh viên đã học môn học này trong lớp học phần đã chọn.");
                    return;
                }

                _studentCourseService.Add(student.StudentId, courseSubjectId);
                LoadStudentCourseGrid();
                MessageBox.Show($"Đã thêm sinh viên: {student.User.FullName} vào lớp học thành công.");
            }
            else
            {
                MessageBox.Show("Vui lòng chọn sinh viên và môn học để thêm.");
            }
        }

        private void EditStudent_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is StudentCourseDisplay display)
            {
                var student = _allStudents.FirstOrDefault(s => s.StudentId == display.StudentId);
                StudentSelectComboBox.SelectedItem = student;

                var course = _courseService.GetAll().FirstOrDefault(c => c.Title == display.CourseTitle);
                if (course != null)
                {
                    CourseComboBox.SelectedValue = course.CourseId;
                    var courseSubjects = _courseSubjectService.GetByCourse(course.CourseId);
                    SubjectComboBox.ItemsSource = courseSubjects;
                    SubjectComboBox.DisplayMemberPath = "Subject.Title";
                    SubjectComboBox.SelectedValuePath = "SubjectId";

                    var firstSubject = courseSubjects.FirstOrDefault(cs =>
                        display.Subjects.Contains(cs.Subject.Title));
                    if (firstSubject != null)
                    {
                        SubjectComboBox.SelectedValue = firstSubject.SubjectId;
                    }

                    _editingStudentId = display.StudentId;
                    _editingCourseSubjectId = firstSubject?.CourseSubjectId ?? 0;
                    _isEditing = true;
                }
            }
        }


        private void UpdateStudent_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditing)
            {
                MessageBox.Show("Bạn chưa chọn mục để cập nhật.");
                return;
            }

            if (CourseComboBox.SelectedValue is int courseId &&
                SubjectComboBox.SelectedValue is int subjectId &&
                StudentSelectComboBox.SelectedValue is int newStudentId)
            {
                var newCourseSubject = _courseSubjectService.GetByCourseAndSubject(courseId, subjectId);
                if (newCourseSubject == null)
                {
                    MessageBox.Show("Không tìm thấy môn học trong lớp học phần.");
                    return;
                }

                // Kiểm tra trùng
                var duplicate = _studentCourseService.GetAll()
                    .Any(sc => sc.StudentId == newStudentId && sc.CourseSubjectId == newCourseSubject.CourseSubjectId);
                if (duplicate)
                {
                    MessageBox.Show("Sinh viên đã học môn học này.");
                    return;
                }

                _studentCourseService.Update(_editingStudentId, _editingCourseSubjectId, newStudentId, newCourseSubject.CourseSubjectId);
                _isEditing = false;
                LoadStudentCourseDisplay();
                MessageBox.Show("Cập nhật thành công.");
            }
            else
            {
                MessageBox.Show("Vui lòng chọn đầy đủ thông tin.");
            }
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

            var filtered = _studentCourseService.GetAll()
                .Where(sc => sc.Student != null
                          && sc.Student.User != null
                          && sc.CourseSubject != null
                          && sc.CourseSubject.Course != null
                          && sc.CourseSubject.Subject != null
                          && sc.CourseSubject.Teacher != null
                          && (sc.Student.User.FullName.ToLower().Contains(keyword)
                              || sc.Student.User.Email.ToLower().Contains(keyword)))
                .Select(sc => new StudentCourseDisplay
                {
                    StudentId = sc.StudentId,
                    FullName = sc.Student.User.FullName,
                    Email = sc.Student.User.Email,
                    CourseTitle = sc.CourseSubject.Course.Title,
                    Subjects = $"{sc.CourseSubject.Subject.Title} - GV: {sc.CourseSubject.Teacher.FullName}",
                })
                .ToList();

            StudentDataGrid.ItemsSource = filtered;
        }

        // ==================== filter ====================

        private void LoadFilterComboBox()
        {
            var courses = _courseService.GetAll();
            cbCourseFilter.ItemsSource = courses;
            cbCourseFilter.SelectedIndex = -1;

            cbSubjectFilter.ItemsSource = null;
        }

        private void cbCourseFilter_Selection(object sender, SelectionChangedEventArgs e)
        {
            if (cbCourseFilter.SelectedValue is int courseId)
            {
                var subjects = _courseSubjectService.GetSubjectsByCourse(courseId);
                cbSubjectFilter.ItemsSource = subjects;
                cbSubjectFilter.SelectedIndex = -1;
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

            var studentCourses = _studentCourseService.GetAll();

            var filtered = studentCourses
                .Where(sc => sc.Student != null
                          && sc.Student.User != null
                          && sc.CourseSubject != null
                          && sc.CourseSubject.Course != null
                          && sc.CourseSubject.Subject != null
                          && sc.CourseSubject.Teacher != null)
                .Where(sc =>
                    (string.IsNullOrWhiteSpace(keyword)
                        || sc.Student.User.FullName.ToLower().Contains(keyword)
                        || sc.Student.User.Email.ToLower().Contains(keyword)) &&
                    (!selectedCourseId.HasValue || sc.CourseSubject.CourseId == selectedCourseId) &&
                    (!selectedSubjectId.HasValue || sc.CourseSubject.SubjectId == selectedSubjectId))
                .Select(sc => new StudentCourseDisplay
                {
                    StudentId = sc.StudentId,
                    FullName = sc.Student.User.FullName,
                    Email = sc.Student.User.Email,
                    CourseTitle = sc.CourseSubject.Course.Title,
                    Subjects = $"{sc.CourseSubject.Subject.Title} - GV: {sc.CourseSubject.Teacher.FullName}"
                })
                .ToList();

            StudentDataGrid.ItemsSource = filtered;
        }



        // ==================== TAB 2 ====================


        private void LoadSubjects()
        {
            var subjects = _subjectService.GetAll();
            dgSubjects.ItemsSource = subjects;
        }

        private void AddSubject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newSubject = new Subject
                {
                    Title = txtSubjectTitle.Text.Trim(),
                    Credit = int.Parse(txtSubjectCredit.Text),
                    Description = txtSubjectDesc.Text
                };

                _subjectService.AddWithGradeItems(newSubject, _currentGradeItems);
                dgSubjects.ItemsSource = _subjectService.GetAll();
                MessageBox.Show("Thêm môn học thành công.");
                ClearSubjectForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
            LoadSubjects();
        }


        private void UpdateSubject_Click(object sender, RoutedEventArgs e)
        {
            if (_editingSubject == null) return;

            try
            {
                _editingSubject.Title = txtSubjectTitle.Text.Trim();
                _editingSubject.Credit = int.Parse(txtSubjectCredit.Text);
                _editingSubject.Description = txtSubjectDesc.Text;

                _subjectService.UpdateWithGradeItems(_editingSubject, _currentGradeItems);
                dgSubjects.ItemsSource = _subjectService.GetAll();
                MessageBox.Show("Cập nhật môn học thành công.");
                ClearSubjectForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }


        private void DeleteSubject_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var subject = btn?.Tag as Subject;
            if (subject == null) return;

            if (MessageBox.Show($"Xóa môn học {subject.Title}?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    _subjectService.DeleteWithTransaction(subject.SubjectId);
                    dgSubjects.ItemsSource = _subjectService.GetAll();
                    MessageBox.Show("Xóa môn học thành công.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể xóa môn học: " + ex.Message);
                }
            }
        }



        private void EditSubject_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var selectedSubject = btn?.Tag as Subject;
            if (selectedSubject == null) return;

            _editingSubject = selectedSubject;
            txtSubjectId.Text = selectedSubject.SubjectId.ToString();
            txtSubjectTitle.Text = selectedSubject.Title;
            txtSubjectCredit.Text = selectedSubject.Credit.ToString();
            txtSubjectDesc.Text = selectedSubject.Description;

            _currentGradeItems = _gradeItemService.GetBySubject(selectedSubject.SubjectId);
            lstGradeItems.ItemsSource = _currentGradeItems;
        }


        private void CancelEditSubject_Click(object sender, RoutedEventArgs e)
        {
            // Reset biến đang sửa
            _editingSubject = null;
            _editingGradeItem = null;

            // Xóa nội dung form môn học
            txtSubjectId.Text = string.Empty;
            txtSubjectTitle.Text = string.Empty;
            txtSubjectCredit.Text = string.Empty;
            txtSubjectDesc.Text = string.Empty;

            // Xóa danh sách thành phần điểm
            _currentGradeItems.Clear();
            lstGradeItems.ItemsSource = null;

            // Reset input thành phần điểm
            txtGradeItemTitle.Text = "Tên thành phần";
            txtGradeItemWeight.Text = "Trọng số %";
        }


        private void AddGradeItem_Click(object sender, RoutedEventArgs e)
        {
            if (!float.TryParse(txtGradeItemWeight.Text, out float weight)) return;

            var title = txtGradeItemTitle.Text.Trim();
            if (_currentGradeItems.Any(g => g.Title.ToLower() == title.ToLower()))
            {
                MessageBox.Show("Tên thành phần điểm đã tồn tại.");
                return;
            }

            _currentGradeItems.Add(new GradeItem { Title = title, Value = (decimal)weight });
            lstGradeItems.ItemsSource = null;
            lstGradeItems.ItemsSource = _currentGradeItems;
        }

        private void UpdateGradeItem_Click(object sender, RoutedEventArgs e)
        {
            if (_editingGradeItem == null)
            {
                MessageBox.Show("Vui lòng chọn thành phần điểm cần sửa.");
                return;
            }

            if (!float.TryParse(txtGradeItemWeight.Text, out float weight))
            {
                MessageBox.Show("Trọng số không hợp lệ.");
                return;
            }

            var title = txtGradeItemTitle.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Tên thành phần không được để trống.");
                return;
            }

            var duplicate = _currentGradeItems.FirstOrDefault(g =>
                g.Title.ToLower() == title.ToLower() && g.GradeId != _editingGradeItem.GradeId);
            if (duplicate != null)
            {
                MessageBox.Show("Tên thành phần điểm bị trùng.");
                return;
            }

            _editingGradeItem.Title = title;
            _editingGradeItem.Value = (decimal)weight;

            lstGradeItems.ItemsSource = null;
            lstGradeItems.ItemsSource = _currentGradeItems;

            MessageBox.Show("Đã cập nhật thành phần điểm.");

            _editingGradeItem = null;
            txtGradeItemTitle.Text = "";
            txtGradeItemWeight.Text = "";
        }

        private void EditGradeItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is GradeItem item)
            {
                _editingGradeItem = item;
                txtGradeItemTitle.Text = item.Title;
                txtGradeItemWeight.Text = item.Value.ToString();
            }
        }

        private void DeleteGradeItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is GradeItem item)
            {
                try
                {
                    _gradeItemService.Delete(item.GradeId);
                    lstGradeItems.ItemsSource = _gradeItemService.GetBySubject(item.SubjectId);
                    MessageBox.Show("Đã xóa thành phần điểm.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể xóa thành phần điểm: " + ex.Message);
                }
            }
        }


        private void SearchSubject_Click(object sender, RoutedEventArgs e)
        {
            var keyword = txtSearchSubject.Text.Trim();
            dgSubjects.ItemsSource = _subjectService.SearchByTitle(keyword);
        }

        private void ClearSubjectForm()
        {
            txtSubjectId.Text = string.Empty;
            txtSubjectTitle.Text = string.Empty;
            txtSubjectCredit.Text = string.Empty;
            txtSubjectDesc.Text = string.Empty;

            txtGradeItemTitle.Text = "Tên thành phần";
            txtGradeItemWeight.Text = "Trọng số %";
            lstGradeItems.ItemsSource = null;

            _editingSubject = null;
            _editingGradeItem = null;
        }






        // -------------------COURSE--------------------------------



        private void AddCourse_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(txtCourseTitle.Text) ||
                dpStartDate.SelectedDate == null || dpEndDate.SelectedDate == null)
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tên lớp và ngày bắt đầu, kết thúc.");
                return;
            }

            // Tạo đối tượng lớp học phần
            var newCourse = new Course
            {
                Title = txtCourseTitle.Text.Trim(),
                StartDate = dpStartDate.SelectedDate.HasValue
             ? DateOnly.FromDateTime(dpStartDate.SelectedDate.Value)
             : DateOnly.FromDateTime(DateTime.Now),
                EndDate = dpEndDate.SelectedDate.HasValue
           ? DateOnly.FromDateTime(dpEndDate.SelectedDate.Value)
           : DateOnly.FromDateTime(DateTime.Now.AddMonths(3))
            };

            //// Kiểm tra ràng buộc 1 giáo viên không dạy >1 môn trong 1 lớp
            //var duplicatedTeacher = _selectedSubjectsInCourse
            //    .Select(g => g.TeacherId)
            //    .ToList();

            //if (duplicatedTeacher > 0)
            //{
            //    var teacherName = _teachers.FirstOrDefault(t => t.UserId == duplicatedTeacher)?.FullName;
            //    MessageBox.Show($"Giáo viên '{teacherName}' chỉ được dạy một môn trong một lớp.");
            //    return;
            //}

            // Gọi service để thêm Course + danh sách môn học + giáo viên
            try
            {
                _courseService.AddCourse(newCourse, _selectedSubjectsInCourse);
                LoadCourses();
                ClearCourseForm();
                MessageBox.Show("Thêm lớp học phần thành công.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm lớp học phần: {ex.Message}");
            }
        }






        private void UpdateCourse_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditingCourse)
            {
                MessageBox.Show("Vui lòng chọn lớp để sửa.");
                return;
            }

            _editingCourse.Title = txtCourseTitle.Text.Trim();
            _editingCourse.StartDate = DateOnly.FromDateTime(dpStartDate.SelectedDate ?? DateTime.Now);
            _editingCourse.EndDate = DateOnly.FromDateTime(dpEndDate.SelectedDate ?? DateTime.Now.AddMonths(3));


            _courseSubjectService.UpdateCourseAndSubjects(_editingCourse, _selectedSubjectsInCourse);

            LoadCourses();
            ClearCourseForm();
            MessageBox.Show("Cập nhật lớp học phần thành công.");
        }




        private void DeleteCourse_Click(object sender, RoutedEventArgs e)
{
    var btn = sender as Button;
    if (btn?.Tag is int courseId)
    {
        var course = _context.Courses.FirstOrDefault(c => c.CourseId == courseId);
        if (course != null)
        {
            var confirm = MessageBox.Show("Bạn có chắc muốn xóa lớp này không?", "Xác nhận", MessageBoxButton.YesNo);
            if (confirm == MessageBoxResult.Yes)
            {
                _context.CourseSubjects.RemoveRange(_context.CourseSubjects.Where(cs => cs.CourseId == courseId));
                _context.Courses.Remove(course);
                _context.SaveChanges();
                LoadCourses(); // Cập nhật lại danh sách
            }
        }
    }
}


        private void EditCourse_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag is int courseId)
            {
                var course = _context.Courses.FirstOrDefault(c => c.CourseId == courseId);
                if (course != null)
                {
                    txtCourseId.Text = course.CourseId.ToString();
                    txtCourseTitle.Text = course.Title;
                    dpStartDate.SelectedDate = course.StartDate.ToDateTime(TimeOnly.MinValue);
                    dpEndDate.SelectedDate = course.EndDate.ToDateTime(TimeOnly.MinValue);

                    _editingCourse = course;
                    _isEditingCourse = true;

                    // ✨ Nạp lại danh sách môn học - giáo viên đã gán vào lớp
                    var selectedSubjects = (from cs in _context.CourseSubjects
                                            where cs.CourseId == courseId
                                            select new CourseSubject
                                            {
                                                SubjectId = cs.SubjectId,
                                                TeacherId = cs.TeacherId
                                            }).ToList();

                    _selectedSubjectsInCourse = selectedSubjects;

                    RefreshCourseSubjectListBox(); // Hiển thị lại lên ListBox
                }
            }
        }




        private void CancelCourse_Click(object sender, RoutedEventArgs e)
        {
            ClearCourseForm();
        }


        //private void ResetCourseForm()
        //{
        //    txtCourseId.Text = "";
        //    txtCourseTitle.Text = "";
        //    dpStartDate.SelectedDate = null;
        //    dpEndDate.SelectedDate = null;
        //    lstCourseSubjects.ItemsSource = null;
        //    _editingCourse = null;
        //    _isEditingCourse = false;
        //}



        private void AddCourseSubject_Click(object sender, RoutedEventArgs e)
        {
            if (cbxCourseSubject.SelectedValue is int subjectId && cbxTeacher.SelectedValue is int teacherId)
            {
                // Nếu môn học đã tồn tại trong lớp
                if (_selectedSubjectsInCourse.Any(cs => cs.SubjectId == subjectId))
                {
                    MessageBox.Show("Môn học này đã được thêm vào lớp.");
                    return;
                }

                //// ✅ Kiểm tra nếu giáo viên đã dạy môn khác trong lớp
                //if (_selectedSubjectsInCourse.Any(cs => cs.TeacherId == teacherId))
                //{
                //    MessageBox.Show("Giáo viên này đã phụ trách một môn khác trong lớp. Mỗi giáo viên chỉ được dạy 1 môn trong một lớp.");
                //    return;
                //}

                // Nếu hợp lệ thì thêm
                _selectedSubjectsInCourse.Add(new CourseSubject
                {
                    SubjectId = subjectId,
                    TeacherId = teacherId
                });

                RefreshCourseSubjectListBox();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn môn học và giáo viên.");
            }
        }



        private void RefreshCourseSubjectListBox()
        {
            var displayList = _selectedSubjectsInCourse
                .Select(cs => new CourseSubjectDisplay
                {
                    SubjectId = cs.SubjectId,
                    Title = _subjects.FirstOrDefault(s => s.SubjectId == cs.SubjectId)?.Title ?? "",
                    TeacherId = cs.TeacherId,
                    TeacherName = _teachers.FirstOrDefault(t => t.UserId == cs.TeacherId)?.FullName ?? ""
                }).ToList();

            lstCourseSubjects.ItemsSource = displayList;
        }




        private void RemoveCourseSubject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CourseSubjectDisplay displayItem)
            {
                // Hiển thị thông báo xác nhận
                var result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa môn '{displayItem.Title}' (GV: {displayItem.TeacherName}) khỏi lớp?",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    var itemToRemove = _selectedSubjectsInCourse
                        .FirstOrDefault(cs => cs.SubjectId == displayItem.SubjectId && cs.TeacherId == displayItem.TeacherId);

                    if (itemToRemove != null)
                    {
                        _selectedSubjectsInCourse.Remove(itemToRemove);
                        RefreshCourseSubjectListBox();
                    }
                }
            }
        }



        private void ClearCourseForm()
        {
            txtCourseId.Text = "";
            txtCourseTitle.Text = "";
            dpStartDate.SelectedDate = null;
            dpEndDate.SelectedDate = null;
            cbxCourseSubject.SelectedIndex = -1;
            cbxTeacher.SelectedIndex = -1;

            _editingCourse = null;
            _isEditingCourse = false;

            _selectedSubjectsInCourse.Clear();
            RefreshCourseSubjectListBox();
        }


        private void SearchCourse_Click(object sender, RoutedEventArgs e)
        {
            var keyword = txtSearchCourse.Text.Trim();
            dgCourses.ItemsSource = _courseService.SearchByTitle(keyword);
        }

        private void EditCourseSubject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CourseSubjectDisplay displayItem)
            {
                cbxCourseSubject.SelectedValue = displayItem.SubjectId;
                cbxTeacher.SelectedValue = displayItem.TeacherId;

                // Xóa khỏi danh sách tạm để sửa rồi thêm lại
                var itemToEdit = _selectedSubjectsInCourse
                    .FirstOrDefault(cs => cs.SubjectId == displayItem.SubjectId && cs.TeacherId == displayItem.TeacherId);
                if (itemToEdit != null)
                {
                    _selectedSubjectsInCourse.Remove(itemToEdit);
                    RefreshCourseSubjectListBox();
                }
            }
        }


        //--------------- add new user ----------------



        private void LoadUserList()
        {
            if (dgUsers == null) return;

            _allUsers = _userService.GetAll();
            FilterUserList(null, null);
        }


        // Lọc theo keyword + role
        private void FilterUserList(object sender, RoutedEventArgs e)
        {
            if (dgUsers == null) return; // 🔐 Tránh lỗi nếu tab chưa khởi tạo

            string keyword = txtUserSearch.Text.Trim().ToLower();

            var selectedItem = cbRoleFilter.SelectedItem as ComboBoxItem;
            string selectedRole = selectedItem?.Content?.ToString();

            var filtered = _allUsers
                .Where(u =>
                    (string.IsNullOrWhiteSpace(keyword)
                     || u.FullName.ToLower().Contains(keyword)
                     || u.Email.ToLower().Contains(keyword))
                    && (selectedRole == "Tất cả" || u.Role == selectedRole))
                .ToList();

            dgUsers.ItemsSource = filtered;
        }


        // Thêm hoặc cập nhật
        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            string username = txtNewUsername.Text.Trim();
            string password = txtNewPassword.Password.Trim();
            string fullname = txtNewFullName.Text.Trim();
            string email = txtNewEmail.Text.Trim();
            string role = (cbxNewRole.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)
                || string.IsNullOrEmpty(fullname) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin.");
                return;
            }

            try
            {
                if (_isEditingUser)
                {
                    var user = new UserAccount
                    {
                        UserId = _editingUserId,
                        Username = username,
                        PasswordHash = password,
                        FullName = fullname,
                        Email = email,
                        Role = role
                    };

                    _userService.Update(user);
                    MessageBox.Show("Cập nhật tài khoản thành công.");
                }
                else
                {
                    var user = new UserAccount
                    {
                        Username = username,
                        PasswordHash = password,
                        FullName = fullname,
                        Email = email,
                        Role = role
                    };

                    _userService.Add(user);

                    
                    if (role == "Student")
                    {
                        var student = new Student
                        {
                            UserId = user.UserId,
                            Phone = "0123456789", // hoặc thêm TextBox để nhập
                            Gender = "Nam",       // hoặc dùng ComboBox
                            DateOfBirth = DateOnly.FromDateTime(new DateTime(2004, 1, 1)),
                            EnrollmentDate = DateOnly.FromDateTime(DateTime.Now)

                        };
                        _studentService.Add(student);
                    }

                    MessageBox.Show("Thêm tài khoản thành công.");
                }

                ClearNewUserForm();
                LoadUserList();
                _isEditingUser = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu tài khoản: " + ex.Message);
            }
        }

        // Sửa
        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is UserAccount user)
            {
                txtNewUsername.Text = user.Username;
                txtNewPassword.Password = user.PasswordHash;
                txtNewFullName.Text = user.FullName;
                txtNewEmail.Text = user.Email;
                cbxNewRole.SelectedItem = cbxNewRole.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Content.ToString() == user.Role);

                _editingUserId = user.UserId;
                _isEditingUser = true;
            }
        }

        // Xóa
        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is UserAccount user)
            {
                if (MessageBox.Show($"Xóa tài khoản {user.Username}?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _userService.Delete(user.UserId);
                    LoadUserList();
                    MessageBox.Show("Đã xóa tài khoản.");
                }
            }
        }

        // Reset form
        private void ClearNewUserForm()
        {
            txtNewUsername.Text = "";
            txtNewPassword.Password = "";
            txtNewFullName.Text = "";
            txtNewEmail.Text = "";
            cbxNewRole.SelectedIndex = 0;
            _editingUserId = 0;
            _isEditingUser = false;
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
