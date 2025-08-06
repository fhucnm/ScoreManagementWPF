
-- ============================================
-- DATABASE: ScoreManagementSystem
-- ============================================
CREATE DATABASE ScoreManagementSystem;
GO
USE ScoreManagementSystem;
GO

-- 1. Tài khoản người dùng
CREATE TABLE UserAccount (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Student', 'Teacher', 'Admin'))
);

-- 2. Sinh viên
CREATE TABLE Student (
    StudentID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT UNIQUE NOT NULL,
    Phone NVARCHAR(15) NOT NULL,
    Gender NVARCHAR(10) NOT NULL,
    DateOfBirth DATE NOT NULL,
    EnrollmentDate DATE NOT NULL,
    FOREIGN KEY (UserID) REFERENCES UserAccount(UserID) ON DELETE CASCADE
);

-- 3. Môn học
CREATE TABLE Subject (
    SubjectID INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(100) NOT NULL,
    Credit INT NOT NULL,
    Description NVARCHAR(255) NOT NULL
);

-- 4. Lớp học phần
CREATE TABLE Course (
    CourseID INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(100) NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL
);

-- 5. Gán môn học vào lớp học phần với giáo viên phụ trách
CREATE TABLE CourseSubject (
    CourseSubjectID INT PRIMARY KEY IDENTITY(1,1),
    CourseID INT NOT NULL,
    SubjectID INT NOT NULL,
    TeacherID INT NOT NULL,
    FOREIGN KEY (CourseID) REFERENCES Course(CourseID) ON DELETE CASCADE,
    FOREIGN KEY (SubjectID) REFERENCES Subject(SubjectID),
    FOREIGN KEY (TeacherID) REFERENCES UserAccount(UserID)
   );

-- 6. Thành phần điểm
CREATE TABLE GradeItem (
    GradeID INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(100) NOT NULL,
    Value DECIMAL(5,2) NOT NULL,
    SubjectID INT NOT NULL,
    FOREIGN KEY (SubjectID) REFERENCES Subject(SubjectID)
);

-- 7. Sinh viên đăng ký học 1 môn trong lớp học phần
CREATE TABLE StudentCourse (
    StudentID INT NOT NULL,
    CourseSubjectID INT NOT NULL,
    PRIMARY KEY (StudentID, CourseSubjectID),
    FOREIGN KEY (StudentID) REFERENCES Student(StudentID),
    FOREIGN KEY (CourseSubjectID) REFERENCES CourseSubject(CourseSubjectID) ON DELETE CASCADE
);

-- 8. Bảng điểm
CREATE TABLE Mark (
    StudentID INT NOT NULL,
    CourseSubjectID INT NOT NULL,
    GradeID INT NOT NULL,
    Value DECIMAL(5,2) NOT NULL,
    Note NVARCHAR(255),
    PRIMARY KEY (StudentID, CourseSubjectID, GradeID),
    FOREIGN KEY (StudentID, CourseSubjectID) REFERENCES StudentCourse(StudentID, CourseSubjectID) ON DELETE CASCADE,
    FOREIGN KEY (GradeID) REFERENCES GradeItem(GradeID)
);

-- 9. Đăng ký lớp-môn chờ duyệt
CREATE TABLE CourseRegistration (
    RegistrationID INT PRIMARY KEY IDENTITY(1,1),
    StudentID INT NOT NULL,
    CourseSubjectID INT NOT NULL,
    RegistrationDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    FOREIGN KEY (StudentID) REFERENCES Student(StudentID),
    FOREIGN KEY (CourseSubjectID) REFERENCES CourseSubject(CourseSubjectID)
);

-- ============================================
-- INSERT DATA
-- ============================================

-- UserAccount (password: 123456 -> SHA256)
INSERT INTO UserAccount (Username, PasswordHash, FullName, Email, Role) VALUES 
('admin01', '123456', N'Nguyễn Văn A', 'admin01@admin.edu.vn', 'Admin'),
('teacher01', '123456', N'Trần Thị B', 'teacher01@teacher.edu.vn', 'Teacher'),
('teacher02', '123456', N'Lê Văn C', 'teacher02@teacher.edu.vn', 'Teacher'),
('student01', '123456', N'Phạm Thị D', 'student01@student.edu.vn', 'Student'),
('student02', '123456', N'Hồ Văn E', 'student02@student.edu.vn', 'Student'),
('student03', '123456', N'Vũ Thị F', 'student03@student.edu.vn', 'Student');

INSERT INTO UserAccount (Username, PasswordHash, FullName, Email, Role) VALUES 
('teacher03', '123456', N'Nguyễn Minh P', 'teacher03@teacher.edu.vn', 'Teacher');

-- Subject
INSERT INTO Subject (Title, Credit, Description) VALUES 
(N'Lập trình C#', 3, N'Môn học về ngôn ngữ C# và .NET'),
(N'Cơ sở dữ liệu', 3, N'Môn học về quản lý dữ liệu và SQL Server'),
(N'Lập trình Java', 3, N'Môn học về Java và ứng dụng');

-- Course
INSERT INTO Course (Title, StartDate, EndDate) VALUES 
(N'Kỳ 1 - Năm 2025', '2025-09-01', '2026-01-10');

-- CourseSubject
INSERT INTO CourseSubject (CourseID, SubjectID, TeacherID) VALUES 
(1, 1, 2), -- C# - teacher01
(1, 2, 3); -- CSDL - teacher02

-- GradeItem
INSERT INTO GradeItem (Title, Value, SubjectID) VALUES 
(N'Midterm', 30.00, 1),
(N'Final', 70.00, 1),
(N'Midterm', 40.00, 2),
(N'Final', 60.00, 2);

-- Student
INSERT INTO Student (UserID, Phone, Gender, DateOfBirth, EnrollmentDate) VALUES 
(4, '0901111111', N'Nữ', '2004-03-21', '2025-08-15'),
(5, '0902222222', N'Nam', '2003-12-12', '2025-08-15'),
(6, '0903333333', N'Nữ', '2004-05-10', '2025-08-15');

-- StudentCourse
INSERT INTO StudentCourse (StudentID, CourseSubjectID) VALUES 
(1, 1),
(1, 2),
(2, 1),
(3, 2);

-- CourseRegistration
INSERT INTO CourseRegistration (StudentID, CourseSubjectID, Status) VALUES 
(1, 1, 'Approved'),
(1, 2, 'Approved'),
(2, 1, 'Pending'),
(3, 2, 'Pending');

-- Mark
INSERT INTO Mark (StudentID, CourseSubjectID, GradeID, Value, Note) VALUES 
(1, 1, 1, 7.5, N'Tốt'),
(1, 1, 2, 8.0, N'Tốt'),
(1, 2, 3, 6.5, N'Khá'),
(1, 2, 4, 7.0, N'Trung bình');
