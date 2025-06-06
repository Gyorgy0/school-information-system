using Microsoft.AspNetCore.Mvc;
using SchoolAPI.Models.Lunch;
using SchoolAPI.Models;
using System.Collections.Generic;
using System.Data.SQLite;

namespace SchoolAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class LunchController : Controller
    {
        [HttpGet]
        public IActionResult GetMenu()
        {
            List<LunchItem> menu = new List<LunchItem>();
            string sql = "SELECT * FROM Lunch";

            using (var connection = DatabaseConnector.CreateNewConnection())
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    menu.Add(new LunchItem
                    {
                        LunchID = reader.GetInt32(0),
                        Day = reader.GetString(1),
                        Meal = reader.GetString(2)
                    });
                }
            }

            return Json(menu);
        }

        [HttpPost]
        public IActionResult CreateLunch([FromForm] string day, [FromForm] string meal)
        {
            string sql = "INSERT INTO Lunch (Day, Meal) VALUES (@Day, @Meal)";

            using (var connection = DatabaseConnector.CreateNewConnection())
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@Day", day);
                cmd.Parameters.AddWithValue("@Meal", meal);
                cmd.ExecuteNonQuery();
            }

            return Ok("Lunch entry created successfully");
        }

        [HttpPost]
        public IActionResult UpdateLunch([FromForm] int lunchID, [FromForm] string? day, [FromForm] string? meal)
        {
            string sql = @"
                UPDATE Lunch
                SET
                    Day = COALESCE(@Day, Day),
                    Meal = COALESCE(@Meal, Meal)
                WHERE LunchID = @LunchID";

            using (var connection = DatabaseConnector.CreateNewConnection())
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@LunchID", lunchID);
                cmd.Parameters.AddWithValue("@Day", day ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Meal", meal ?? (object)DBNull.Value);

                if (cmd.ExecuteNonQuery() == 0)
                    return NotFound("Lunch entry not found");

                return Ok("Lunch updated successfully");
            }
        }

        [HttpPost]
        public IActionResult DeleteLunch([FromForm] int lunchID)
        {
            string sql = "DELETE FROM Lunch WHERE LunchID = @LunchID";

            using (var connection = DatabaseConnector.CreateNewConnection())
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@LunchID", lunchID);
                if (cmd.ExecuteNonQuery() == 0)
                    return NotFound("Lunch entry not found");

                return Ok("Lunch deleted successfully");
            }
        }
    }
}