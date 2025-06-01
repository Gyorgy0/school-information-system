using Microsoft.AspNetCore.Mvc;
using SchoolAPI.Models;
using System.Collections.Generic;
using System.Data.SQLite;

namespace SchoolAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class MessageController : Controller
    {
        [HttpGet]
        public IActionResult GetMessagesAction()
        {
            List<Message> messagesList = new List<Message>();
            string sql = "SELECT * FROM Message";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        messagesList.Add(new Message
                        {
                            MessageID = reader.GetInt32(0),
                            SenderID = reader.GetInt32(1),
                            ReceiverID = reader.GetInt32(2),
                            MessageText = reader.GetString(3),
                            Timestamp = reader.GetDateTime(4)
                        });
                    }
                }
            }

            return Json(messagesList);
        }

        [HttpGet]
        public IActionResult GetMessage(int id)
        {
            Message? message = null;
            string sql = "SELECT * FROM Message WHERE MessageID = @MessageID";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@MessageID", id);
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        message = new Message
                        {
                            MessageID = reader.GetInt32(0),
                            SenderID = reader.GetInt32(1),
                            ReceiverID = reader.GetInt32(2),
                            MessageText = reader.GetString(3),
                            Timestamp = reader.GetDateTime(4)
                        };
                    }
                }
            }

            if (message == null)
                return NotFound("Message not found");

            return Json(message);
        }

        [HttpPost]
        public IActionResult CreateMessage([FromForm] int senderID, [FromForm] int receiverID, [FromForm] string messageText, [FromForm] string timestamp)
        {
            string sql = "INSERT INTO Message (SenderID, ReceiverID, MessageText, Timestamp) VALUES (@SenderID, @ReceiverID, @MessageText, @Timestamp)";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@SenderID", senderID);
                    cmd.Parameters.AddWithValue("@ReceiverID", receiverID);
                    cmd.Parameters.AddWithValue("@MessageText", messageText);
                    cmd.Parameters.AddWithValue("@Timestamp", timestamp);
                    cmd.ExecuteNonQuery();
                }
            }

            return Ok("Message created successfully");
        }

        [HttpPost]
        public IActionResult UpdateMessage([FromForm] int messageID, [FromForm] int? senderID, [FromForm] int? receiverID, [FromForm] string? messageText, [FromForm] string? timestamp)
        {
            string sql = @"
                UPDATE Message
                SET
                    SenderID = COALESCE(@SenderID, SenderID),
                    ReceiverID = COALESCE(@ReceiverID, ReceiverID),
                    MessageText = COALESCE(@MessageText, MessageText),
                    Timestamp = COALESCE(@Timestamp, Timestamp)
                WHERE MessageID = @MessageID";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@MessageID", messageID);
                    cmd.Parameters.AddWithValue("@SenderID", senderID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ReceiverID", receiverID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@MessageText", messageText ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Timestamp", timestamp ?? (object)DBNull.Value);

                    if (cmd.ExecuteNonQuery() == 0)
                        return NotFound("Message not found");

                    return Ok("Message updated successfully");
                }
            }
        }

        [HttpPost]
        public IActionResult DeleteMessage([FromForm] int messageID)
        {
            string sql = "DELETE FROM Message WHERE MessageID = @MessageID";

            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@MessageID", messageID);
                    if (cmd.ExecuteNonQuery() == 0)
                        return NotFound("Message not found");

                    return Ok("Message deleted successfully");
                }
            }
        }
    }
}