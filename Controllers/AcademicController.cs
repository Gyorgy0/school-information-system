using Microsoft.AspNetCore.Mvc;
using SchoolAPI.Models;
using SchoolAPI.Models.Timetable;
using SchoolAPI.Models.Grade;
using SchoolAPI.Models.Task;
using SchoolAPI.Models.Topic;
using SchoolAPI.Models.Users;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Security.Claims;

namespace SchoolAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]

    public class AcademicController : Controller
    {
        [HttpGet]
        public IActionResult GetAbsences(int? timetableID = null)
        {
            List<Timetable> absences = new List<Timetable>();
            string sql = timetableID.HasValue
                ? "SELECT * FROM Absence WHERE TimetableID = @TimetableID"
                : "SELECT * FROM Absence";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    if (timetableID.HasValue)
                        cmd.Parameters.AddWithValue("@TimetableID", timetableID.Value);

                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        absences.Add(new Timetable
                        {
                            AbsenceID = reader.GetInt32(0),
                            TimetableID = reader.GetInt32(1),
                            StudentID = reader.GetInt32(2),
                            Date = reader.GetDateTime(3)
                        });
                    }
                }
            }

            return Json(absences);
        }

        [HttpPost]
        public IActionResult MarkAbsence([FromForm] int timetableID, [FromForm] int studentID, [FromForm] DateTime date)
        {
            string sql = "INSERT INTO Absence (TimetableID, StudentID, Date) VALUES (@TimetableID, @StudentID, @Date)";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@TimetableID", timetableID);
                    cmd.Parameters.AddWithValue("@StudentID", studentID);
                    cmd.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok("Hiányzás sikeresen rögzítve");
        }

        [HttpPost]
        public IActionResult AddGrade(int studentID, int taskID, float grade)
        {
            string sql = "INSERT INTO Grade (StudentID, TaskID, Grade, Date) VALUES (@StudentID, @TaskID, @Grade, @Date)";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@StudentID", studentID);
                    cmd.Parameters.AddWithValue("@TaskID", taskID);
                    cmd.Parameters.AddWithValue("@Grade", grade);
                    cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok("Értékelés hozzáadva");
        }

        [HttpGet]
        public IActionResult GetStudentAverage(int studentID)
        {
            string sql = "SELECT AVG(Grade) FROM Grade WHERE StudentID = @StudentID";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@StudentID", studentID);
                    var result = cmd.ExecuteScalar();

                    if (result == DBNull.Value)
                        return Ok(0.0);

                    return Ok(Convert.ToDouble(result));
                }
            }
        }

        [HttpGet]
        public IActionResult GetClassAverage(int classID)
        {
            string sql = @"
        SELECT AVG(Grade) 
        FROM Grade 
        WHERE StudentID IN (
            SELECT UserID FROM User WHERE ClassID = @ClassID AND Role = 'student'
        )";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@ClassID", classID);
                    var result = cmd.ExecuteScalar();
                    return Ok(result);
                }
            }
        }

        [HttpGet]
        public IActionResult GenerateReportCard(int studentID)
        {
            var averageResponse = GetStudentAverage(studentID) as OkObjectResult;
            if (averageResponse == null)
                return BadRequest("Hiba történt az átlag kiszámításakor.");

            double average = Convert.ToDouble(averageResponse.Value);

            string grade = "";
            if (average >= 4.5)
                grade = "A";
            else if (average >= 3.5)
                grade = "B";
            else if (average >= 2.5)
                grade = "C";
            else if (average >= 1.5)
                grade = "D";
            else
                grade = "F";

            return Ok(new { ReportCard = grade, Average = average });
        }

        [HttpGet]
        public IActionResult GetGrades(string role, int? studentID = null, int? teacherID = null)
        {
            string sql;

            if (role == "student" && studentID.HasValue)
            {
                sql = @"
            SELECT g.StudentID, u.Name AS StudentName, g.TaskID, t.Title AS TaskTitle, g.Grade, g.Date
            FROM Grade g
            JOIN User u ON g.StudentID = u.UserID AND u.Role = 'student'
            JOIN Task t ON g.TaskID = t.TaskID
            WHERE g.StudentID = @StudentID";
            }
            else if (role == "teacher" && teacherID.HasValue)
            {
                sql = @"
            SELECT g.StudentID, u.Name AS StudentName, g.TaskID, t.Title AS TaskTitle, g.Grade, g.Date
            FROM Grade g
            JOIN User u ON g.StudentID = u.UserID AND u.Role = 'student'
            JOIN Task t ON g.TaskID = t.TaskID
            WHERE g.StudentID IN (
                SELECT StudentID FROM ClassAssignments WHERE TeacherID = @TeacherID
            )";
            }
            else if (role == "admin")
            {
                sql = @"
            SELECT g.StudentID, u.Name AS StudentName, g.TaskID, t.Title AS TaskTitle, g.Grade, g.Date
            FROM Grade g
            JOIN User u ON g.StudentID = u.UserID AND u.Role = 'student'
            JOIN Task t ON g.TaskID = t.TaskID";
            }
            else
            {
                return BadRequest("Érvénytelen szerepkör vagy hiányzó azonosító.");
            }

            var grades = new List<GradeModel>();
            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    if (role == "student")
                        cmd.Parameters.AddWithValue("@StudentID", studentID);
                    else if (role == "teacher")
                        cmd.Parameters.AddWithValue("@TeacherID", teacherID);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            grades.Add(new GradeModel
                            {
                                StudentID = reader.GetInt32(0),
                                StudentName = reader.GetString(1),
                                TaskID = reader.GetInt32(2),
                                TaskTitle = reader.GetString(3),
                                StudentGrade = reader.GetFloat(4),
                                Date = reader.GetDateTime(5)
                            });
                        }
                    }
                }
            }

            return Ok(grades);
        }

        [HttpPost]
        public IActionResult AddWarningOrPraise(int studentID, string type, string description)
        {
            if (type != "Warning" && type != "Praise")
                return BadRequest("Érvénytelen típus");

            string sql = "INSERT INTO WarningsAndPraises (StudentID, Type, Description, Date) VALUES (@StudentID, @Type, @Description, @Date)";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@StudentID", studentID);
                    cmd.Parameters.AddWithValue("@Type", type);
                    cmd.Parameters.AddWithValue("@Description", description);
                    cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok("Megrovó vagy dícséret hozzáadva");
        }

        [HttpPost]
        public IActionResult AddTask([FromForm] string title, [FromForm] string description, [FromForm] int subjectID, [FromForm] int teacherID, [FromForm] DateTime dueDate)
        {
            string sql = "INSERT INTO Task (Title, Description, SubjectID, TeacherID, DueDate) VALUES (@Title, @Description, @SubjectID, @TeacherID, @DueDate)";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Description", description);
                    cmd.Parameters.AddWithValue("@SubjectID", subjectID);
                    cmd.Parameters.AddWithValue("@TeacherID", teacherID);
                    cmd.Parameters.AddWithValue("@DueDate", dueDate.ToString("yyyy-MM-dd"));
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok("Feladat sikeresen hozzáadva");
        }

        [HttpGet]
        public IActionResult GetTasksForSubject(int subjectID)
        {
            List<TaskModel> tasks = new();

            string sql = @"
            SELECT 
                Task.TaskID,
                Task.Title,
                Task.Description,
                Task.SubjectID,
                Task.TeacherID,
                Task.DueDate,
                Subject.Name AS SubjectName
            FROM Task
            JOIN Subject ON Task.SubjectID = Subject.SubjectID
            WHERE Task.SubjectID = @SubjectID";

            using var connection = DatabaseConnector.CreateNewConnection();
            using var cmd = new SQLiteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@SubjectID", subjectID);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                tasks.Add(new TaskModel
                {
                    TaskID = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Description = reader.GetString(2),
                    SubjectID = reader.GetInt32(3),
                    TeacherID = reader.GetInt32(4),
                    DueDate = reader.GetDateTime(5),
                    SubjectName = reader.GetString(6)
                });
            }

            return Json(tasks);
        }

        [HttpGet]
        public IActionResult GetTasks()
        {
            List<TaskModel> tasks = new List<TaskModel>();
            string sql = @"
            SELECT 
                Task.TaskID,
                Task.Title,
                Task.Description,
                Task.SubjectID,
                Task.TeacherID,
                Task.DueDate,
                Subject.Name AS SubjectName
            FROM Task
            JOIN Subject ON Task.SubjectID = Subject.SubjectID";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        tasks.Add(new TaskModel
                        {
                            TaskID = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Description = reader.GetString(2),
                            SubjectID = reader.GetInt32(3),
                            TeacherID = reader.GetInt32(4),
                            DueDate = reader.GetDateTime(5),
                            SubjectName = reader.GetString(6)
                        });
                    }
                }
            }

            return Json(tasks);
        }

        [HttpPost]
        public IActionResult AddTaskByName([FromForm] string title, [FromForm] string description, [FromForm] string subjectName, [FromForm] int teacherID, [FromForm] DateTime dueDate)
        {
            string sqlGetTimetableId = @"
            SELECT TimetableID FROM Timetable
            WHERE Subject = @SubjectName AND TeacherID = @TeacherID
            LIMIT 1";

            int timetableID;

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sqlGetTimetableId, connection))
                {
                    cmd.Parameters.AddWithValue("@SubjectName", subjectName);
                    cmd.Parameters.AddWithValue("@TeacherID", teacherID);
                    var result = cmd.ExecuteScalar();

                    if (result == null)
                        return BadRequest("Nem található ilyen tantárgy ezzel a tanárral az órarendben.");

                    timetableID = Convert.ToInt32(result);
                }

                string sqlInsert = "INSERT INTO Task (Title, Description, SubjectID, TeacherID, DueDate) VALUES (@Title, @Description, @SubjectID, @TeacherID, @DueDate)";
                using (var cmd = new SQLiteCommand(sqlInsert, connection))
                {
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Description", description);
                    cmd.Parameters.AddWithValue("@SubjectID", timetableID);
                    cmd.Parameters.AddWithValue("@TeacherID", teacherID);
                    cmd.Parameters.AddWithValue("@DueDate", dueDate);
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok("Feladat sikeresen hozzáadva");
        }

        [HttpGet]
        public IActionResult GetTopics()
        {
            List<TopicModel> topics = new List<TopicModel>();
            string sql = "SELECT TaskID as TopicID, Title as Subject, Description FROM Task";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        topics.Add(new TopicModel
                        {
                            TopicID = reader.GetInt32(0),
                            Subject = reader.GetString(1),
                            Description = reader.GetString(2)
                        });
                    }
                }
            }

            return Json(topics);
        }

        [HttpPost]
        public IActionResult AddTopic(
            [FromForm] string subject,
            [FromForm] string description,
            [FromForm] int subjectID,
            [FromForm] int teacherID,
            [FromForm] DateTime dueDate)
        {
            string sql = "INSERT INTO Task (Title, Description, SubjectID, TeacherID, DueDate) " +
                         "VALUES (@Title, @Description, @SubjectID, @TeacherID, @DueDate)";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Title", subject);
                    cmd.Parameters.AddWithValue("@Description", description);
                    cmd.Parameters.AddWithValue("@SubjectID", subjectID);
                    cmd.Parameters.AddWithValue("@TeacherID", teacherID);
                    cmd.Parameters.AddWithValue("@DueDate", dueDate);
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok("Témakör sikeresen hozzáadva");
        }
    }
}