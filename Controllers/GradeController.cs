using Microsoft.AspNetCore.Mvc;
using SchoolAPI.Models;
using System.Collections.Generic;
using System.Data.SQLite;

namespace SchoolAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class GradeController : Controller
    {
        [HttpGet]
        public IActionResult GetGradesAction()
        {
            List<Grade> gradesList = new List<Grade>();
            string sql = "SELECT * FROM Grade";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        gradesList.Add(new Grade
                        {
                            GradeID = reader.GetInt32(0),
                            UserID = reader.GetInt32(1),
                            Subject = reader.GetString(2),
                            GradeValue = reader.GetInt32(3),
                            Date = reader.GetDateTime(4)
                        });
                    }
                }
            }

            return Json(gradesList);
        }

        [HttpGet]
        public IActionResult GetGrade(int id)
        {
            Grade? grade = null;
            string sql = "SELECT * FROM Grade WHERE GradeID = @GradeID";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@GradeID", id);
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        grade = new Grade
                        {
                            GradeID = reader.GetInt32(0),
                            UserID = reader.GetInt32(1),
                            Subject = reader.GetString(2),
                            GradeValue = reader.GetInt32(3),
                            Date = reader.GetDateTime(4)
                        };
                    }
                }
            }

            if (grade == null)
                return NotFound("Grade not found");

            return Json(grade);
        }

        [HttpPost]
        public IActionResult CreateGradeAction([FromForm] int userID, [FromForm] string subject, [FromForm] int gradeValue, [FromForm] string date)
        {
            string sql = "INSERT INTO Grade (UserID, Subject, GradeValue, Date) VALUES (@UserID, @Subject, @GradeValue, @Date)";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@UserID", userID);
                    cmd.Parameters.AddWithValue("@Subject", subject);
                    cmd.Parameters.AddWithValue("@GradeValue", gradeValue);
                    cmd.Parameters.AddWithValue("@Date", date);
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok("Grade created successfully");
        }

        [HttpPost]
        public IActionResult UpdateGrade([FromForm] int gradeID, [FromForm] int? userID, [FromForm] string? subject, [FromForm] int? gradeValue, [FromForm] string? date)
        {
            string sql = @"
                UPDATE Grade
                SET
                    UserID = COALESCE(@UserID, UserID),
                    Subject = COALESCE(@Subject, Subject),
                    GradeValue = COALESCE(@GradeValue, GradeValue),
                    Date = COALESCE(@Date, Date)
                WHERE GradeID = @GradeID";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@GradeID", gradeID);
                    cmd.Parameters.AddWithValue("@UserID", userID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Subject", subject ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@GradeValue", gradeValue ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Date", date ?? (object)DBNull.Value);

                    if (cmd.ExecuteNonQuery() == 0)
                        return NotFound("Grade not found");

                    return Ok("Grade updated successfully");
                }
            }
        }

        [HttpPost]
        public IActionResult DeleteGrade([FromForm] int gradeID)
        {
            string sql = "DELETE FROM Grade WHERE GradeID = @GradeID";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@GradeID", gradeID);
                    if (cmd.ExecuteNonQuery() == 0)
                        return NotFound("Grade not found");

                    return Ok("Grade deleted successfully");
                }
            }
        }
    }
}