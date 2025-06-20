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
            string sql = "SELECT * FROM Timetable ORDER BY DayCount";
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
                                DayCount = reader.GetInt64(2),
                                Hour = reader.GetString(3),
                                Subject = reader.GetString(4),
                                Room = reader.GetString(5),
                                TeacherID = reader.GetInt64(6),
                                ClassID = Convert.ToInt64(reader["ClassID"]),
                            }
                        );
                    }
                }
            }

            return Json(timetable);
        }

        [HttpGet]
        public IActionResult GetTimetable(int id)
        {
            TimetableEntry? entry = null;
            string sql = "SELECT * FROM Timetable WHERE TimetableID = @TimetableID";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@TimetableID", id);
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        entry = new TimetableEntry
                        {
                            TimetableID = reader.GetInt32(0),
                            Day = reader.GetString(1),
                            Hour = reader.GetString(2),
                            Subject = reader.GetString(3),
                            Room = reader.GetString(4),
                            TeacherID = reader.GetInt32(5),
                            ClassID = reader.GetInt32(6),
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
            [FromForm] string day,
            [FromForm] int dayCount,
            [FromForm] string hour,
            [FromForm] string subject,
            [FromForm] string room,
            [FromForm] int teacherID,
            [FromForm] int classID
        )
        {
            if (CheckForConflicts(day, hour, subject, room, teacherID))
            {
                return BadRequest(
                    "Az adott időpontban ütközés van a tanár, tantárgy, vagy terem miatt!"
                );
            }

            string sql =
                "INSERT INTO Timetable (Day, DayCount, Hour, Subject, Room, TeacherID, ClassID) VALUES (@Day, @DayCount, @Hour, @Subject, @Room, @TeacherID, @ClassID)";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Day", day);
                    cmd.Parameters.AddWithValue("@DayCount", dayCount);
                    cmd.Parameters.AddWithValue("@Hour", hour);
                    cmd.Parameters.AddWithValue("@Subject", subject);
                    cmd.Parameters.AddWithValue("@Room", room);
                    cmd.Parameters.AddWithValue("@TeacherID", teacherID);
                    cmd.Parameters.AddWithValue("@ClassID", classID);
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
            [FromForm] string? room,
            [FromForm] int? teacherID
        )
        {
            if (
                (
                    day != null
                    || hour != null
                    || subject != null
                    || room != null
                    || teacherID != null
                )
                && CheckForConflicts(
                    day ?? "",
                    hour ?? "",
                    subject ?? "",
                    room ?? "",
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
                    Room = COALESCE(@Room, Room),
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
                    cmd.Parameters.AddWithValue("@Room", room ?? (object)DBNull.Value);
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
