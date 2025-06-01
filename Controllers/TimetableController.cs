using Microsoft.AspNetCore.Mvc;
using SchoolAPI.Models;
using System.Collections.Generic;
using System.Data.SQLite;

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
                        timetable.Add(new TimetableEntry
                        {
                            TimetableID = reader.GetInt32(0),
                            Day = reader.GetString(1),
                            Hour = reader.GetString(2),
                            Subject = reader.GetString(3),
                            Room = reader.GetString(4),
                            TeacherID = reader.GetInt32(5)
                        });
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
                            TeacherID = reader.GetInt32(5)
                        };
                    }
                }
            }

            if (entry == null)
                return NotFound("Timetable entry not found");

            return Json(entry);
        }

        [HttpPost]
        public IActionResult CreateTimetable([FromForm] string day, [FromForm] string hour, [FromForm] string subject, [FromForm] string room, [FromForm] int teacherID)
        {
            string sql = "INSERT INTO Timetable (Day, Hour, Subject, Room, TeacherID) VALUES (@Day, @Hour, @Subject, @Room, @TeacherID)";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Day", day);
                    cmd.Parameters.AddWithValue("@Hour", hour);
                    cmd.Parameters.AddWithValue("@Subject", subject);
                    cmd.Parameters.AddWithValue("@Room", room);
                    cmd.Parameters.AddWithValue("@TeacherID", teacherID);
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok("Timetable entry created successfully");
        }

        [HttpPost]
        public IActionResult UpdateTimetable([FromForm] int timetableID, [FromForm] string? day, [FromForm] string? hour, [FromForm] string? subject, [FromForm] string? room, [FromForm] int? teacherID)
        {
            string sql = @"
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
                        return NotFound("Timetable entry not found");

                    return Ok("Timetable entry updated successfully");
                }
            }
        }

        [HttpPost]
        public IActionResult DeleteTimetable([FromForm] int timetableID)
        {
            string sql = "DELETE FROM Timetable WHERE TimetableID = @TimetableID";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@TimetableID", timetableID);
                    if (cmd.ExecuteNonQuery() == 0)
                        return NotFound("Timetable entry not found");

                    return Ok("Timetable entry deleted successfully");
                }
            }
        }
    }
}