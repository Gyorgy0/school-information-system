using Microsoft.AspNetCore.Mvc;
using SchoolAPI.Models.Lunch;
using SchoolAPI.Models;
using System.Collections.Generic;
using System.Data.SQLite;
using SchoolAPI.Controllers;

namespace SchoolAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class LunchController : Controller
    {
        [HttpGet]
        public IActionResult GetMenu()
        {
            var menu = new List<object>();

            string sql = @"
                SELECT 
                    Lunch.LunchID,
                    Lunch.Day,
                    Soup.Name AS Soup,
                    MainDish.Name AS MainDish,
                    Dessert.Name AS Dessert
                FROM Lunch
                JOIN Soup ON Lunch.SoupID = Soup.SoupID
                JOIN MainDish ON Lunch.MainDishID = MainDish.MainDishID
                JOIN Dessert ON Lunch.DessertID = Dessert.DessertID
                ORDER BY CASE 
                    WHEN Lunch.Day = 'H�tf�' THEN 1
                    WHEN Lunch.Day = 'Kedd' THEN 2
                    WHEN Lunch.Day = 'Szerda' THEN 3
                    WHEN Lunch.Day = 'Cs�t�rt�k' THEN 4
                    WHEN Lunch.Day = 'P�ntek' THEN 5
                    ELSE 6 END";

            using (var connection = DatabaseConnector.CreateNewConnection())
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    menu.Add(new
                    {
                        LunchID = reader.GetInt32(0),
                        Day = reader.GetString(1),
                        Soup = reader.GetString(2),
                        MainDish = reader.GetString(3),
                        Dessert = reader.GetString(4)
                    });
                }
            }

            return Json(menu);
        }

        [HttpPost]
        public IActionResult Regenerate()
        {
            using var conn = DatabaseConnector.CreateNewConnection();
            LunchGenerator.GenerateWeeklyLunch(conn);
            return Ok("Lunch menu regenerated");
        }

        [HttpPost]
        public IActionResult UpdateDayMenu([FromBody] DayMenuUpdateDto update)
        {
            using var conn = DatabaseConnector.CreateNewConnection();

            int? soupId = GetIdByName(conn, "Soup", update.Soup);
            int? mainDishId = GetIdByName(conn, "MainDish", update.MainDish);
            int? dessertId = GetIdByName(conn, "Dessert", update.Dessert);

            if (soupId == null || mainDishId == null || dessertId == null)
            {
                return BadRequest("�tel nem tal�lhat� az adatb�zisban.");
            }

            var cmd = new SQLiteCommand(@"
            UPDATE Lunch 
            SET SoupID = @soupId, MainDishID = @mainId, DessertID = @dessertId 
            WHERE Day = @day", conn);

            cmd.Parameters.AddWithValue("@soupId", soupId);
            cmd.Parameters.AddWithValue("@mainId", mainDishId);
            cmd.Parameters.AddWithValue("@dessertId", dessertId);
            cmd.Parameters.AddWithValue("@day", update.Day);

            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                return NotFound("A megadott nap nem tal�lhat�.");
            }

            return Ok("Ment�s sikeres.");
        }

        private int? GetIdByName(SQLiteConnection conn, string table, string name)
        {
            var cmd = new SQLiteCommand($"SELECT {table}ID FROM {table} WHERE Name = @name", conn);
            cmd.Parameters.AddWithValue("@name", name);
            var result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : (int?)null;
        }
    }
    public class DayMenuUpdateDto
    {
        public string Day { get; set; }
        public string Soup { get; set; }
        public string MainDish { get; set; }
        public string Dessert { get; set; }
    }
}