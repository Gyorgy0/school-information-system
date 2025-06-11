using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using SchoolAPI.Models;
using SchoolAPI.Models.Grade;
using SchoolAPI.Models.Timetable;

[ApiController]
[Route("[controller]/[action]")]
public class StatisticsController : Controller
{
    [HttpGet]
    public IActionResult GetByRole(string role, string username)
    {
        using var connection = DatabaseConnector.CreateNewConnection();

        if (role == "student")
        {
            var grades = new List<GradeModel>();
            var absences = new List<Timetable>();

            int studentID = GetUserID(username, connection);
            if (studentID == -1)
                return NotFound("Felhaszn�l� nem tal�lhat�.");

            // Jegyek lek�rdez�se
            string sqlGrades = @"
                SELECT g.TaskID, g.StudentGrade, g.Date, t.Title AS TaskTitle
                FROM Grade g
                JOIN Task t ON g.TaskID = t.TaskID
                WHERE g.StudentID = @StudentID";

            using (var cmd = new SQLiteCommand(sqlGrades, connection))
            {
                cmd.Parameters.AddWithValue("@StudentID", studentID);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    grades.Add(new GradeModel
                    {
                        StudentID = studentID,
                        TaskID = reader.GetInt32(0),
                        StudentGrade = reader.GetFloat(1),
                        Date = reader.GetDateTime(2),
                        TaskTitle = reader.GetString(3),
                        StudentName = username
                    });
                }
            }

            // Hi�nyz�sok lek�rdez�se
            string sqlAbsences = @"
                SELECT AbsenceID, TimetableID, Date
                FROM Absence
                WHERE StudentID = @StudentID";

            using (var cmd = new SQLiteCommand(sqlAbsences, connection))
            {
                cmd.Parameters.AddWithValue("@StudentID", studentID);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    absences.Add(new Timetable
                    {
                        AbsenceID = reader.GetInt32(0),
                        TimetableID = reader.GetInt32(1),
                        StudentID = studentID,
                        Date = reader.GetDateTime(2)
                    });
                }
            }

            return Json(new { grades, absences });
        }
        else if (role == "teacher")
        {
            var students = new List<object>();
            int teacherID = GetUserID(username, connection);
            if (teacherID == -1)
                return NotFound("Tan�r nem tal�lhat�.");

            string sql = @"
                SELECT DISTINCT u.UserID, u.Username
                FROM User u
                JOIN Grade g ON g.StudentID = u.UserID
                JOIN Task t ON g.TaskID = t.TaskID
                WHERE t.TeacherID = @TeacherID";

            using (var cmd = new SQLiteCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@TeacherID", teacherID);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int studentID = reader.GetInt32(0);
                    string studentName = reader.GetString(1);

                    var studentGrades = new List<GradeModel>();
                    int absenceCount = 0;

                    // Jegyek lek�rdez�se
                    string gradeSql = @"
                        SELECT g.TaskID, g.StudentGrade, g.Date, t.Title
                        FROM Grade g
                        JOIN Task t ON g.TaskID = t.TaskID
                        WHERE g.StudentID = @StudentID AND t.TeacherID = @TeacherID";

                    using (var gradeCmd = new SQLiteCommand(gradeSql, connection))
                    {
                        gradeCmd.Parameters.AddWithValue("@StudentID", studentID);
                        gradeCmd.Parameters.AddWithValue("@TeacherID", teacherID);
                        using var gradeReader = gradeCmd.ExecuteReader();
                        while (gradeReader.Read())
                        {
                            studentGrades.Add(new GradeModel
                            {
                                StudentID = studentID,
                                TaskID = gradeReader.GetInt32(0),
                                StudentGrade = gradeReader.GetFloat(1),
                                Date = gradeReader.GetDateTime(2),
                                TaskTitle = gradeReader.GetString(3),
                                StudentName = studentName
                            });
                        }
                    }

                    // Hi�nyz�sok sz�ma
                    string absenceSql = "SELECT COUNT(*) FROM Absence WHERE StudentID = @StudentID";
                    using (var absenceCmd = new SQLiteCommand(absenceSql, connection))
                    {
                        absenceCmd.Parameters.AddWithValue("@StudentID", studentID);
                        absenceCount = Convert.ToInt32(absenceCmd.ExecuteScalar());
                    }

                    students.Add(new
                    {
                        name = studentName,
                        grades = studentGrades,
                        absences = new string[absenceCount]
                    });
                }
            }

            return Json(new { students });
        }
        else if (role == "admin")
        {
            int teachersCount = 0;
            int studentsCount = 0;
            var classes = new List<object>();

            using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM User WHERE Role = 'teacher'", connection))
            {
                teachersCount = Convert.ToInt32(cmd.ExecuteScalar());
            }

            using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM User WHERE Role = 'student'", connection))
            {
                studentsCount = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // ClassID alapj�n csoportos�tott lek�rdez�s a User t�bl�b�l
            string classSql = @"
                SELECT ClassID, COUNT(*) AS StudentCount
                FROM User
                WHERE Role = 'student' AND ClassID IS NOT NULL
                GROUP BY ClassID";

            using (var cmd = new SQLiteCommand(classSql, connection))
            {
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int classID = reader.GetInt32(0);
                    int studentCount = reader.GetInt32(1);

                    classes.Add(new
                    {
                        name = $"Class {classID}",
                        studentCount = studentCount
                    });
                }
            }

            return Json(new { teachersCount, studentsCount, classes });
        }

        return BadRequest("Ismeretlen szerepk�r.");
    }

    private int GetUserID(string username, SQLiteConnection connection)
    {
        string sql = "SELECT UserID FROM User WHERE Username = @Username LIMIT 1";
        using var cmd = new SQLiteCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Username", username);
        var result = cmd.ExecuteScalar();
        return result != null ? Convert.ToInt32(result) : -1;
    }
}