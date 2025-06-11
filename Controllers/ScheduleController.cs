using Microsoft.AspNetCore.Mvc;
using SchoolAPI.Models;
using SchoolAPI.Models.Subject;
using System.Collections.Generic;
using System.Data.SQLite;
using System;

namespace SchoolAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ScheduleController : Controller
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
                            TeacherID = reader.GetInt32(5),
                            ClassID = reader.GetInt32(6)
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
                            TeacherID = reader.GetInt32(5),
                            ClassID = reader.GetInt32(6)
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
        public IActionResult CreateTimetable([FromForm] string day, [FromForm] string hour, [FromForm] string subject, [FromForm] string room, [FromForm] int teacherID, [FromForm] int classID)
        {
            if (CheckForConflicts(day, hour, subject, room, teacherID))
            {
                return BadRequest("Az adott id�pontban �tk�z�s van a tan�r, tant�rgy, vagy terem miatt!");
            }

            string sql = "INSERT INTO Timetable (Day, Hour, Subject, Room, TeacherID, ClassID) VALUES (@Day, @Hour, @Subject, @Room, @TeacherID, @ClassID)";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Day", day);
                    cmd.Parameters.AddWithValue("@Hour", hour);
                    cmd.Parameters.AddWithValue("@Subject", subject);
                    cmd.Parameters.AddWithValue("@Room", room);
                    cmd.Parameters.AddWithValue("@TeacherID", teacherID);
                    cmd.Parameters.AddWithValue("@ClassID", classID);
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok("�rarend sikeresen l�trehozva");
        }

        [HttpPost]
        public IActionResult UpdateTimetable([FromForm] int timetableID, [FromForm] string? day, [FromForm] string? hour, [FromForm] string? subject, [FromForm] string? room, [FromForm] int? teacherID)
        {
            if ((day != null || hour != null || subject != null || room != null || teacherID != null) &&
                CheckForConflicts(day ?? "", hour ?? "", subject ?? "", room ?? "", teacherID ?? 0))
            {
                return BadRequest("Az �j be�ll�t�sok �tk�z�st okoznak!");
            }

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
                        return NotFound("�rarend bejegyz�s nem tal�lhat�");

                    return Ok("�rarend bejegyz�s sikeresen friss�tve");
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

        public bool CheckForConflicts(string day, string hour, string subject, string room, int teacherID)
        {
            string sql = @"
        SELECT COUNT(*)
        FROM Timetable
        WHERE Day = @Day
        AND Hour = @Hour
        AND (
            TeacherID = @TeacherID OR
            Room = @Room OR
            Subject = @Subject
        )";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Day", day);
                    cmd.Parameters.AddWithValue("@Hour", hour);
                    cmd.Parameters.AddWithValue("@Subject", subject);
                    cmd.Parameters.AddWithValue("@Room", room);
                    cmd.Parameters.AddWithValue("@TeacherID", teacherID);

                    var count = Convert.ToInt32(cmd.ExecuteScalar());

                    return count > 0;
                }
            }
        }

        [HttpGet]
        public IActionResult GetSubjectsFromTimetableDistinct()
        {
            List<string> subjects = new List<string>();
            string sql = "SELECT DISTINCT Subject FROM Timetable";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        subjects.Add(reader.GetString(0));
                    }
                }
            }

            return Json(subjects);
        }

        [HttpGet]
        public IActionResult GetSubjects()
        {
            List<SubjectModel> subjects = new List<SubjectModel>();
            string sql = "SELECT SubjectID, Name FROM Subjects ORDER BY SubjectID";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        subjects.Add(new SubjectModel
                        {
                            SubjectID = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }
            return Json(subjects);
        }

        [HttpGet]
        public IActionResult GetSubjectIdByName(string name)
        {
            // Mivel nincs Subject t�bla, megkeress�k, hogy szerepel-e a tant�rgy a Timetable-ben
            string sql = "SELECT DISTINCT Subject FROM Timetable WHERE Subject = @Name";

            using (var connection = DatabaseConnector.CreateNewConnection())
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@Name", name);
                var result = cmd.ExecuteScalar();

                if (result == null)
                    return NotFound("Nincs ilyen tant�rgy");

                // Az ID-t gener�lni kell ugyan�gy, mint az el�z� met�dusban � itt egy egyszer� megold�s, hogy lek�rdezz�k az �sszes tant�rgyat �s megtal�ljuk az index�t
                string sqlAll = "SELECT DISTINCT Subject FROM Timetable ORDER BY Subject";
                List<string> allSubjects = new List<string>();

                using (var cmdAll = new SQLiteCommand(sqlAll, connection))
                {
                    var reader = cmdAll.ExecuteReader();
                    while (reader.Read())
                    {
                        allSubjects.Add(reader.GetString(0));
                    }
                }

                int subjectId = allSubjects.IndexOf(name) + 1; // +1 mert az ID-k 1-t�l kezd�dnek

                return Json(new { subjectID = subjectId });
            }
        }
    }
}