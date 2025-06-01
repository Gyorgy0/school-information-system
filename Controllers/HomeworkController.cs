using Microsoft.AspNetCore.Mvc;
using SchoolAPI.Models;
using System.Collections.Generic;
using System.Data.SQLite;

namespace SchoolAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class HomeworkController : Controller
    {
        [HttpGet]
        public IActionResult GetHomeworksAction()
        {
            List<Homework> homeworkList = new List<Homework>();
            string sql = "SELECT * FROM Homework";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        homeworkList.Add(new Homework
                        {
                            HomeworkID = reader.GetInt32(0),
                            UserID = reader.GetInt32(1),
                            Subject = reader.GetString(2),
                            Description = reader.GetString(3),
                            DueDate = reader.GetDateTime(4)
                        });
                    }
                }
            }

            return Json(homeworkList);
        }

        [HttpGet]
        public IActionResult GetHomework(int id)
        {
            Homework? homework = null;
            string sql = "SELECT * FROM Homework WHERE HomeworkID = @HomeworkID";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@HomeworkID", id);
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        homework = new Homework
                        {
                            HomeworkID = reader.GetInt32(0),
                            UserID = reader.GetInt32(1),
                            Subject = reader.GetString(2),
                            Description = reader.GetString(3),
                            DueDate = reader.GetDateTime(4)
                        };
                    }
                }
            }

            if (homework == null)
                return NotFound("Homework not found");

            return Json(homework);
        }

        [HttpPost]
        public IActionResult CreateHomeworkAction([FromForm] int userID, [FromForm] string subject, [FromForm] string description, [FromForm] string dueDate)
        {
            string sql = "INSERT INTO Homework (UserID, Subject, Description, DueDate) VALUES (@UserID, @Subject, @Description, @DueDate)";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@UserID", userID);
                    cmd.Parameters.AddWithValue("@Subject", subject);
                    cmd.Parameters.AddWithValue("@Description", description);
                    cmd.Parameters.AddWithValue("@DueDate", dueDate);
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok("Homework created successfully");
        }

        [HttpPost]
        public IActionResult UpdateHomework([FromForm] int homeworkID, [FromForm] int? userID, [FromForm] string? subject, [FromForm] string? description, [FromForm] string? dueDate)
        {
            string sql = @"
                UPDATE Homework
                SET
                    UserID = COALESCE(@UserID, UserID),
                    Subject = COALESCE(@Subject, Subject),
                    Description = COALESCE(@Description, Description),
                    DueDate = COALESCE(@DueDate, DueDate)
                WHERE HomeworkID = @HomeworkID";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@HomeworkID", homeworkID);
                    cmd.Parameters.AddWithValue("@UserID", userID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Subject", subject ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", description ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DueDate", dueDate ?? (object)DBNull.Value);

                    if (cmd.ExecuteNonQuery() == 0)
                        return NotFound("Homework not found");

                    return Ok("Homework updated successfully");
                }
            }
        }

        [HttpPost]
        public IActionResult DeleteHomework([FromForm] int homeworkID)
        {
            string sql = "DELETE FROM Homework WHERE HomeworkID = @HomeworkID";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@HomeworkID", homeworkID);
                    if (cmd.ExecuteNonQuery() == 0)
                        return NotFound("Homework not found");

                    return Ok("Homework deleted successfully");
                }
            }
        }
    }
}