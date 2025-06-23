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
        [HttpGet]
        public IActionResult GetTimetable(string timetableID)
        {
            TimetableEntry? timetable = null;
            string sql = $"SELECT * FROM Timetable WHERE TimetableID = {timetableID}";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        timetable = new TimetableEntry
                        {
                            TimetableID = Convert.ToInt64(reader["TimetableID"]),
                            Day = Convert.ToString(reader["Day"]),
                            Hour = Convert.ToString(reader["Hour"]),
                            Subject = Convert.ToString(reader["Subject"]),
                            Classroom = Convert.ToString(reader["Classroom"]),
                            TeacherID = Convert.ToInt64(reader["TeacherID"]),
                        };
                    }
                }
            }

            if (timetable == null)
                return NotFound("Keresett órarend nem található!");
            else
                return Json(timetable);
        }

        [HttpGet]
        public IActionResult GetTimetables()
        {
            using var conn = DatabaseConnector.CreateNewConnection();

            var timetables = new List<string>();

            using (var cmd = new SQLiteCommand("SELECT TimetableID FROM Timetable", conn))
            using (var reader = cmd.ExecuteReader())
                while (reader.Read())
                    timetables.Add(Convert.ToString(reader["TimetableID"]));

            return Ok(new { Timetables = timetables });
        }

        [HttpPost]
        public IActionResult CreateTimetable(
            [FromForm] string timetableID,
            [FromForm] string day,
            [FromForm] string hour,
            [FromForm] string subject,
            [FromForm] string classroom,
            [FromForm] int teacherID
        )
        {
            if (CheckForConflicts(day, hour, subject, classroom, teacherID))
            {
                return BadRequest(
                    "Az adott időpontban ütközés van a tanár, tantárgy, vagy terem miatt!"
                );
            }

            string sql =
                $"INSERT INTO Timetable (TimetableID, Day, Hour, Subject, Room, TeacherID) VALUES ({timetableID}, {day}, {hour}, {subject}, {classroom}, {teacherID})";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok("Órarend sikeresen létrehozva!");
        }

        [HttpPost]
        public IActionResult UpdateTimetable(
            [FromForm] int timetableID,
            [FromForm] string? day,
            [FromForm] string? hour,
            [FromForm] string? subject,
            [FromForm] string? classroom,
            [FromForm] int? teacherID
        )
        {
            if (
                (
                    day != null
                    || hour != null
                    || subject != null
                    || classroom != null
                    || teacherID != null
                )
                && CheckForConflicts(
                    day ?? "",
                    hour ?? "",
                    subject ?? "",
                    classroom ?? "",
                    teacherID ?? 0
                )
            )
            {
                return BadRequest("Az új beállítások ütközést okoznak!");
            }

            string sql =
                $@"
                UPDATE Timetable
                SET
                    Day = COALESCE({day ?? (object)DBNull.Value}, Day),
                    Hour = COALESCE({hour ?? (object)DBNull.Value}, Hour),
                    Subject = COALESCE({subject ?? (object)DBNull.Value}, Subject),
                    Classroom = COALESCE({classroom ?? (object)DBNull.Value}, Classroom),
                    TeacherID = COALESCE({teacherID ?? (object)DBNull.Value}, TeacherID)
                WHERE TimetableID = {timetableID}";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    if (cmd.ExecuteNonQuery() == 0)
                        return NotFound("Órarend bejegyzés nem található.");

                    return Ok("Órarend bejegyzés sikeresen frissítve!");
                }
            }
        }

        [HttpPost]
        public IActionResult DeleteTimetable([FromForm] int timetableID)
        {
            string sql = $"DELETE FROM Timetable WHERE TimetableID = {timetableID}";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    if (cmd.ExecuteNonQuery() == 0)
                        return NotFound("Keresett órarend nem található.");

                    return Ok("Órarend sikeresen törölve!");
                }
            }
        }

        public bool CheckForConflicts(
            string day,
            string hour,
            string subject,
            string room,
            int teacherID
        )
        {
            string sql =
                $@"
                SELECT COUNT(*)
                FROM Timetable
                WHERE Day = {day}
                AND Hour = {hour}
                AND (
                    TeacherID = {teacherID} OR
                    Room = {room}
                )";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    var count = Convert.ToInt32(cmd.ExecuteScalar());

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
