using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Service;

namespace ScoreManagementWPF1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly UserAccountService _userAccountService = new UserAccountService();
        public MainWindow()
        {
            InitializeComponent();
            // Set the initial focus to the email textbox
            txtEmail.Focus();
            txtPassword.Focus();
            // Set the password box to use asterisks for masking
            txtPassword.PasswordChar = '*';
            
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Vui lòng nhập cả email và mật khẩu.", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!email.EndsWith("@student.edu.vn") && !email.EndsWith("@teacher.edu.vn") && !email.EndsWith("@admin.edu.vn"))
            {
                MessageBox.Show("Email phải kết thúc bằng @student.edu.vn, @teacher.edu.vn hoặc @admin.edu.vn", "Cảnh báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Mật khẩu phải có ít nhất 6 ký tự.", "Cảnh báo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var user = _userAccountService.Login(email, password);
                if (user == null)
                {
                    MessageBox.Show("Đăng nhập thất bại, sai email hoặc mật khẩu.", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Window nextWindow;

                if (email.EndsWith("@student.edu.vn"))
                {
                    nextWindow = new StudentWindow(user.UserId); // truyền StudentId nếu cần
                }
                else if (email.EndsWith("@teacher.edu.vn"))
                {
                    nextWindow = new TeacherWindow(user.UserId); // truyền TeacherId nếu cần
                }
                else
                {
                    nextWindow = new AdminWindow(user.UserId); // truyền admin id
                }

                this.Hide();
                nextWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đăng nhập: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}