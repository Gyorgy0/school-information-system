using Microsoft.AspNetCore.Mvc;
using SchoolAPI.Models;
using SchoolAPI.Models.Timetable;
using System.Data.SQLite;
using System.Diagnostics;
using System.Security.Claims;

[ApiController]
[Route("[controller]/[action]")]
public class SchoolEventController : Controller
{
    [HttpGet]
    public IActionResult GetEvents()
    {
        List<Timetable> events = new List<Timetable>();

        string sql = "SELECT * FROM SchoolEvent";

        using (var connection = DatabaseConnector.CreateNewConnection())
        {
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        events.Add(new Timetable
                        {
                            EventID = reader.GetInt32(0),
                            TimetableID = reader.GetInt32(1),
                            EventType = reader.GetString(2),
                            EventDate = reader.GetDateTime(3),
                            Description = reader.GetString(4)
                        });
                    }
                }
            }
        }

        return Ok(events);
    }

    [HttpPost]
    public IActionResult AddEvent([FromForm] int timetableID, [FromForm] string eventType, [FromForm] DateTime eventDate, [FromForm] string description)
    {
        if (!IsAdmin())
        {
            return Unauthorized("Only admins can add events.");
        }

        string sql = "INSERT INTO SchoolEvent (TimetableID, EventType, EventDate, Description) VALUES (@TimetableID, @EventType, @EventDate, @Description)";

        using (var connection = DatabaseConnector.CreateNewConnection())
        {
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@TimetableID", timetableID);
                cmd.Parameters.AddWithValue("@EventType", eventType);
                cmd.Parameters.AddWithValue("@EventDate", eventDate);
                cmd.Parameters.AddWithValue("@Description", description);
                cmd.ExecuteNonQuery();
            }
        }

        return Ok("School event added.");
    }

    [HttpPost]
    public IActionResult EditEvent(int eventID, string eventType, DateTime eventDate, string description)
    {
        if (!IsAdmin())
        {
            return Unauthorized("Only admins can edit events.");
        }

        string sql = "UPDATE SchoolEvent SET EventType = @EventType, EventDate = @EventDate, Description = @Description WHERE EventID = @EventID";

        using (var connection = DatabaseConnector.CreateNewConnection())
        {
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@EventID", eventID);
                cmd.Parameters.AddWithValue("@EventType", eventType);
                cmd.Parameters.AddWithValue("@EventDate", eventDate);
                cmd.Parameters.AddWithValue("@Description", description);
                cmd.ExecuteNonQuery();
            }
        }

        return Ok("School event updated.");
    }

    [HttpPost]
    public IActionResult DeleteEvent(int eventID)
    {
        if (!IsAdmin())
        {
            return Unauthorized("Only admins can delete events.");
        }

        string sql = "DELETE FROM SchoolEvent WHERE EventID = @EventID";

        using (var connection = DatabaseConnector.CreateNewConnection())
        {
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@EventID", eventID);
                cmd.ExecuteNonQuery();
            }
        }

        return Ok("School event deleted.");
    }

    private bool IsAdmin()
    {
        var sessionId = Request.Cookies["id"];
        if (string.IsNullOrEmpty(sessionId))
        {
            return false;
        }

        Int64 userId = SessionManager.GetUserID(sessionId);
        if (userId == -1)
        {
            return false;
        }

        using var connection = DatabaseConnector.CreateNewConnection();
        using var cmd = new SQLiteCommand("SELECT Role FROM User WHERE UserID = @userId", connection);
        cmd.Parameters.AddWithValue("@userId", userId);

        var role = cmd.ExecuteScalar() as string;
        return role != null && role.Equals("admin", StringComparison.OrdinalIgnoreCase);
    }
}