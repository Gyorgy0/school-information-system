using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Microsoft.AspNetCore.Mvc;
using SchoolAPI.Models;
using SchoolAPI.Models.Subject;

namespace SchoolAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class TimetableController : Controller
    {
        [HttpPost]
        public IActionResult GetTimetable([FromForm] string timetableID)
        {
            List<TimetableEntry?> timetable = new List<TimetableEntry?>();
            string sql =
                @"SELECT * FROM Timetable 
                WHERE TimetableID = @TimetableID 
                ORDER BY CASE 
                WHEN Day = 'Hétfő' THEN 1 
                WHEN Day = 'Kedd' THEN 2 
                WHEN Day = 'Szerda' THEN 3 
                WHEN Day = 'Csütörtök' THEN 4 
                WHEN Day = 'Péntek' THEN 5 
                END ASC, 
                Hour ASC";
            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@TimetableID", timetableID);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        timetable.Add(
                            new TimetableEntry
                            {
                                Day = Convert.ToString(reader["Day"]),
                                Hour = Convert.ToInt64(reader["Hour"]),
                                Subject = Convert.ToString(reader["Subject"]),
                                Classroom = Convert.ToString(reader["Classroom"]),
                                TeacherID = Convert.ToInt64(reader["TeacherID"]),
                            }
                        );
                    }
                }
            }

            if (timetable == null)
                return NotFound("Keresett órarend nem található!");
            else
                return Json(timetable);
        }

        [HttpPost]
        public IActionResult CreateTimetable(
            [FromForm] string timetableID,
            [FromForm] string day,
            [FromForm] int hour,
            [FromForm] string subject,
            [FromForm] string classroom,
            [FromForm] int teacherID
        )
        {
            if (CheckForConflicts(day, hour, classroom, teacherID))
            {
                return BadRequest("Az adott időpontban ütközés van más tanórával!");
            }

            string sql =
                @"INSERT INTO Timetable (TimetableID, Day, Hour, Subject, Classroom, TeacherID) 
                VALUES (@TimetableID, @Day, @Hour, @Subject, @Classroom, @TeacherID)";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@TimetableID", timetableID);
                    cmd.Parameters.AddWithValue("@Day", day);
                    cmd.Parameters.AddWithValue("@Hour", hour);
                    cmd.Parameters.AddWithValue("@Subject", subject);
                    cmd.Parameters.AddWithValue("@Classroom", classroom);
                    cmd.Parameters.AddWithValue("@TeacherID", teacherID);
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok("Órarend sikeresen létrehozva!");
        }

        [HttpPost]
        public IActionResult DeleteTimetableAction(
            [FromForm] string timetableID,
            [FromForm] string day,
            [FromForm] int hour,
            [FromForm] string subject,
            [FromForm] string classroom,
            [FromForm] int teacherID
        )
        {
            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                string sql =
                    @"DELETE FROM Timetable 
                    WHERE TimetableID = @TimetableID 
                    AND Day = @Day 
                    AND Hour = @Hour 
                    AND Subject = @Subject 
                    AND Classroom = @Classroom 
                    AND TeacherID = @TeacherID";
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@TimetableID", timetableID);
                    cmd.Parameters.AddWithValue("@Day", day);
                    cmd.Parameters.AddWithValue("@Hour", hour);
                    cmd.Parameters.AddWithValue("@Subject", subject);
                    cmd.Parameters.AddWithValue("@Classroom", classroom);
                    cmd.Parameters.AddWithValue("@TeacherID", teacherID);
                    if (cmd.ExecuteNonQuery() == 0)
                        return NotFound("Keresett órarendi bejegyzés nem található.");

                    return Ok("Órarendi bejegyzés sikeresen törölve!");
                }
            }
        }

        public bool CheckForConflicts(string day, long hour, string classroom, long teacherID)
        {
            string sql =
                @"SELECT COUNT(*)
                FROM Timetable
                WHERE Day = @Day
                AND Hour = @Hour
                AND (
                    TeacherID = @TeacherID OR
                    Classroom = @Classroom
                )";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Day", day);
                    cmd.Parameters.AddWithValue("@Hour", Convert.ToInt64(hour));
                    cmd.Parameters.AddWithValue("@TeacherID", Convert.ToInt64(teacherID));
                    cmd.Parameters.AddWithValue("@Classroom", classroom);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        [HttpPost]
        public IActionResult CreateSubject([FromForm] string subjectname)
        {
            string sql = $"INSERT INTO Subjects (Name) VALUES ('{subjectname}')";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok();
        }

        [HttpGet]
        public IActionResult GetSubjects()
        {
            using var conn = DatabaseConnector.CreateNewConnection();

            var subjects = new List<string>();

            using (var cmd = new SQLiteCommand("SELECT Name FROM Subjects", conn))
            using (var reader = cmd.ExecuteReader())
                while (reader.Read())
                    subjects.Add(Convert.ToString(reader["Name"]));

            return Ok(new { Subjects = subjects });
        }

        [HttpPost]
        public IActionResult DeleteSubject([FromForm] string subjectname)
        {
            string sql = $"DELETE FROM Subjects WHERE Name = '{subjectname}'";
            Console.WriteLine("elér!!");

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.ExecuteNonQuery();
                }
                sql = $"DELETE FROM Timetable WHERE Subject = '???'";
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.ExecuteNonQuery();
                }
                return Ok("Tantárgy sikeresen törölve!");
            }
        }

        [HttpPost]
        public IActionResult CreateClassroom([FromForm] string classroomname)
        {
            string sql = $"INSERT INTO Classrooms (Name) VALUES ('{classroomname}')";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok();
        }

        [HttpGet]
        public IActionResult GetClassrooms()
        {
            using var conn = DatabaseConnector.CreateNewConnection();

            var classrooms = new List<string>();

            using (var cmd = new SQLiteCommand("SELECT Name FROM Classrooms", conn))
            using (var reader = cmd.ExecuteReader())
                while (reader.Read())
                    classrooms.Add(Convert.ToString(reader["Name"]));

            return Ok(new { Classrooms = classrooms });
        }

        [HttpPost]
        public IActionResult DeleteClassroom([FromForm] string classroomname)
        {
            string sql = $"DELETE FROM Classrooms WHERE Name = '{classroomname}'";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    if (cmd.ExecuteNonQuery() == 0)
                        return NotFound("Törölni kívánt terem nem található.");
                }
                sql = $"DELETE FROM Timetable WHERE Classroom = '???'";
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.ExecuteNonQuery();
                }
                return Ok("Terem sikeresen törölve!");
            }
        }

        [HttpPost]
        public IActionResult CreateClass([FromForm] int year)
        {
            string[] alphabet =
            {
                "A",
                "B",
                "C",
                "D",
                "E",
                "F",
                "G",
                "H",
                "I",
                "J",
                "K",
                "L",
                "M",
                "N",
                "O",
                "P",
                "Q",
                "R",
                "S",
                "T",
                "U",
                "V",
                "W",
                "X",
                "Y",
                "Z",
            };
            int count = 0;
            int groupcount = 0;

            string groupname = "";

            string sql = $"SELECT COUNT(*) FROM Classes WHERE Year = @Year";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Year", Convert.ToInt32(year));
                    groupcount = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            int digit = groupcount % alphabet.Length;
            groupcount = groupcount / alphabet.Length;
            groupname += alphabet[digit % alphabet.Length];

            while (groupcount > 0)
            {
                digit = groupcount % alphabet.Length;
                groupcount = groupcount / alphabet.Length;
                groupname += alphabet[digit % alphabet.Length];
            }

            string classname = year + "." + groupname;

            sql =
                $"INSERT INTO Classes ( Year, GroupName, ClassName ) VALUES ( @Year, @GroupName, @ClassName )";
            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Year", Convert.ToInt32(year));
                    cmd.Parameters.AddWithValue("@GroupName", groupname);
                    cmd.Parameters.AddWithValue("@ClassName", classname);
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok();
        }

        [HttpGet]
        public IActionResult GetClasses()
        {
            using var conn = DatabaseConnector.CreateNewConnection();

            var classes = new List<string>();

            using (var cmd = new SQLiteCommand("SELECT ClassName FROM Classes", conn))
            using (var reader = cmd.ExecuteReader())
                while (reader.Read())
                    classes.Add(Convert.ToString(reader["ClassName"]));

            return Ok(new { Classes = classes });
        }

        [HttpPost]
        public IActionResult DeleteClass([FromForm] string classname)
        {
            string sql = $"DELETE FROM Classes WHERE ClassName = @ClassName";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@ClassName", classname);
                    if (cmd.ExecuteNonQuery() == 0)
                        return NotFound("Törölni kívánt osztály nem található.");
                }
                sql = $"DELETE FROM Timetable WHERE TimetableID = '???'";
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.ExecuteNonQuery();
                }
                return Ok("Osztály sikeresen törölve!");
            }
        }
    }
}
