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
        public IActionResult GetTimetableAction()
        {
            List<TimetableEntry> timetable = new List<TimetableEntry>();
            string sql = "SELECT * FROM Timetable";
            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        timetable.Add(
                            new TimetableEntry
                            {
                                TimetableID = reader.GetInt64(0),
                                Day = reader.GetString(1),
                                Hour = reader.GetString(2),
                                Subject = reader.GetString(3),
                                Classroom = reader.GetString(4),
                                TeacherID = reader.GetInt64(5),
                            }
                        );
                    }
                }
            }

            return Json(timetable);
        }

        [HttpGet]
        public IActionResult GetTimetable(string timetableID)
        {
            TimetableEntry? entry = null;
            string sql = "SELECT * FROM Timetable WHERE TimetableID = @TimetableID";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@TimetableID", timetableID);
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        entry = new TimetableEntry
                        {
                            TimetableID = reader.GetInt32(0),
                            Day = reader.GetString(1),
                            Hour = reader.GetString(2),
                            Subject = reader.GetString(3),
                            Classroom = reader.GetString(4),
                            TeacherID = reader.GetInt32(5),
                        };
                    }
                }
            }

            if (entry == null)
                return NotFound("Timetable entry not found");

            return Json(entry);
        }

        [HttpPost]
        public IActionResult CreateSubject([FromForm] int subjectId, [FromForm] string subjectname)
        {
            string sql = "INSERT INTO Subjects (SubjectID, Name) VALUES (@SubjectID, @Name)";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@SubjectID", subjectId);
                    cmd.Parameters.AddWithValue("@Name", subjectname);
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok();
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
                "INSERT INTO Timetable (TimetableID, Day, Hour, Subject, Room, TeacherID) VALUES (@TimetableID, @Day, @Hour, @Subject, @Classroom, @TeacherID)";

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

            return Ok("Órarend sikeresen létrehozva");
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
                @"
                UPDATE Timetable
                SET
                    Day = COALESCE(@Day, Day),
                    Hour = COALESCE(@Hour, Hour),
                    Subject = COALESCE(@Subject, Subject),
                    Classroom = COALESCE(@Classroom, Classroom),
                    TeacherID = COALESCE(@TeacherID, TeacherID)
                WHERE TimetableID = @TimetableID";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@TimetableID", timetableID);
                    cmd.Parameters.AddWithValue("@Day", day ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Hour", hour ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Subject", subject ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Classroom", classroom ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TeacherID", teacherID ?? (object)DBNull.Value);

                    if (cmd.ExecuteNonQuery() == 0)
                        return NotFound("Órarend bejegyzés nem található");

                    return Ok("Órarend bejegyzés sikeresen frissítve");
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
                        return NotFound("Timetable entry not found");

                    return Ok("Timetable entry deleted successfully");
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
                    Room = {room} OR
                    Subject = {subject}
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

        [HttpGet]
        public IActionResult GetSubjects()
        {
            using var conn = DatabaseConnector.CreateNewConnection();

            var subjects = new List<string>();

            using (var cmd = new SQLiteCommand("SELECT Name FROM Subjects", conn))
            using (var reader = cmd.ExecuteReader())
                while (reader.Read())
                    subjects.Add(reader["Name"].ToString());

            return Ok(new { Subjects = subjects });
        }
    }
}
