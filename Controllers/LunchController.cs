using Microsoft.AspNetCore.Mvc;
using SchoolAPI.Models.Lunch;
using SchoolAPI.Models;
using System.Collections.Generic;
using System.Data.SQLite;
using SchoolAPI.Controllers;
using System.Data.Entity;
using System.Globalization;

namespace SchoolAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class LunchController : Controller
    {
        [HttpPost]
        public IActionResult Regenerate([FromBody] RegenerateRequest request)
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
                WHEN Lunch.Day = 'Hétfő' THEN 1
                WHEN Lunch.Day = 'Kedd' THEN 2
                WHEN Lunch.Day = 'Szerda' THEN 3
                WHEN Lunch.Day = 'Csütörtök' THEN 4
                WHEN Lunch.Day = 'Péntek' THEN 5
                ELSE 6 END";

            using (var connection = DatabaseConnector.CreateNewConnection())
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    menu.Add(new
                    {
                        LunchID = Convert.ToString(reader["LunchID"]),
                        Day = Convert.ToString(reader["Day"]),
                        Soup = Convert.ToString(reader["Soup"]),
                        MainDish = Convert.ToString(reader["MainDish"]),
                        Dessert = Convert.ToString(reader["Dessert"]),
                    });
                }
            }

            LunchGenerator.GenerateLunchForWeek(conn, request.StartDate);

            return Ok("Új menü generálva.");
        }

        public class RegenerateRequest
        {
            public DateTime StartDate { get; set; }
        }

        /*[HttpPost]
        public IActionResult GenerateFullYearMenu([FromQuery] int year = 2025) //not used rn
        {
            using var conn = DatabaseConnector.CreateNewConnection();
            LunchGenerator.GenerateFullYearLunch(conn, year);
            return Ok($"Éves menü generálva a {year} évre.");
        }*/

        /*[HttpPost]
        public IActionResult UpdateDayMenu([FromBody] DayMenuUpdateDto update)
        {
            using var conn = DatabaseConnector.CreateNewConnection();

            int? soupId = GetIdByName(conn, "Soup", update.Soup);
            int? mainDishId = GetIdByName(conn, "MainDish", update.MainDish);
            int? dessertId = GetIdByName(conn, "Dessert", update.Dessert);

            if (soupId == null || mainDishId == null || dessertId == null)
            {
                return BadRequest("Étel nem található az adatbázisban.");
            }

            using var cmd = new SQLiteCommand(@"
            UPDATE Lunch 
            SET SoupID = @soupId, MainDishID = @mainId, DessertID = @dessertId 
            WHERE Date = @date", conn);

            cmd.Parameters.AddWithValue("@soupId", soupId);
            cmd.Parameters.AddWithValue("@mainId", mainDishId);
            cmd.Parameters.AddWithValue("@dessertId", dessertId);
            cmd.Parameters.AddWithValue("@date", update.Date); // itt string formátumban "yyyy-MM-dd"

            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                return NotFound("A megadott nap nem található.");
            }

            return Ok("Mentés sikeres.");
        }*/

        [HttpPost]
        public IActionResult UpdateDayMenu([FromBody] DayMenuUpdateDto update)
        {
            using var conn = DatabaseConnector.CreateNewConnection();

            int? soupId = GetIdByName(conn, "Soup", update.Soup);
            int? mainDishId = GetIdByName(conn, "MainDish", update.MainDish);
            int? dessertId = GetIdByName(conn, "Dessert", update.Dessert);

            if (soupId == null || mainDishId == null || dessertId == null)
            {
                return BadRequest("Étel nem található az adatbázisban.");
            }

            var formattedDate = DateTime.Parse(update.Date).ToString("yyyy-MM-dd");

            using var cmd = new SQLiteCommand(@"
        UPDATE Lunch 
        SET SoupID = @soupId, MainDishID = @mainId, DessertID = @dessertId 
        WHERE ""Date"" = @date", conn);

            cmd.Parameters.AddWithValue("@soupId", soupId);
            cmd.Parameters.AddWithValue("@mainId", mainDishId);
            cmd.Parameters.AddWithValue("@dessertId", dessertId);
            cmd.Parameters.AddWithValue("@date", formattedDate);

            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                return NotFound("A megadott nap nem található.");
            }

            return Ok("Mentés sikeres.");
        }

        [HttpPost]
        public IActionResult AddFoodItem([FromBody] AddFoodDto newItem)
        {
            using var conn = DatabaseConnector.CreateNewConnection();

            SQLiteCommand cmd;

            if (newItem.Type == "Soup")
                cmd = new SQLiteCommand("INSERT INTO Soup (Name) VALUES (@name)", conn);
            else if (newItem.Type == "MainDish")
                cmd = new SQLiteCommand("INSERT INTO MainDish (Name) VALUES (@name)", conn);
            else if (newItem.Type == "Dessert")
                cmd = new SQLiteCommand("INSERT INTO Dessert (Name) VALUES (@name)", conn);
            else
                return BadRequest("Érvénytelen kategória.");

            cmd.Parameters.AddWithValue("@name", newItem.Name);

            try
            {
                cmd.ExecuteNonQuery();
                return Ok("Sikeres hozzáadás.");
            }
            catch (SQLiteException ex) when (ex.ResultCode == SQLiteErrorCode.Constraint)
            {
                return BadRequest("Ez az étel már létezik.");
            }
            catch (Exception ex)
            {
                return BadRequest("Hiba: " + ex.Message);
            }
        }

        [HttpPost]
        public IActionResult DeleteFoodItem([FromBody] AddFoodDto itemToDelete)
        {
            using var conn = DatabaseConnector.CreateNewConnection();

            string? table = itemToDelete.Type switch
            {
                "Soup" => "Soup",
                "MainDish" => "MainDish",
                "Dessert" => "Dessert",
                _ => null
            };

            if (table == null)
                return BadRequest("Érvénytelen kategória.");

            using var cmd = new SQLiteCommand($"DELETE FROM {table} WHERE Name = @name", conn);
            cmd.Parameters.AddWithValue("@name", itemToDelete.Name);

            try
            {
                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected == 0)
                    return NotFound("Nem található az étel a megadott kategóriában.");

                return Ok("Sikeres törlés.");
            }
            catch (SQLiteException ex)
            {
                return BadRequest("Hiba történt törlés közben: " + ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetFoodOptions()
        {
            using var conn = DatabaseConnector.CreateNewConnection();

            var soups = new List<string>();
            var mainDishes = new List<string>();
            var desserts = new List<string>();

            using (var cmd = new SQLiteCommand("SELECT Name FROM Soup", conn))
            using (var reader = cmd.ExecuteReader())
                while (reader.Read())
                    soups.Add(reader.GetString(0));

            using (var cmd = new SQLiteCommand("SELECT Name FROM MainDish", conn))
            using (var reader = cmd.ExecuteReader())
                while (reader.Read())
                    mainDishes.Add(reader.GetString(0));

            using (var cmd = new SQLiteCommand("SELECT Name FROM Dessert", conn))
            using (var reader = cmd.ExecuteReader())
                while (reader.Read())
                    desserts.Add(reader.GetString(0));

            return Ok(new
            {
                soups = soups,
                mainDishes = mainDishes,
                desserts = desserts
            });
        }

        [HttpGet]
        public IActionResult GetMenuForWeek([FromQuery] DateTime startDate)
        {
            if (startDate == DateTime.MinValue)
            {
                DateTime today = DateTime.Today;
                int daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;
                startDate = today.AddDays(-daysSinceMonday);
            }
            else
            {
                // Normalize startDate to Monday of that week
                int daysSinceMonday = ((int)startDate.DayOfWeek + 6) % 7;
                startDate = startDate.AddDays(-daysSinceMonday);
                Console.WriteLine($"GetMenuForWeek called with startDate: {startDate:yyyy-MM-dd}");
            }

            return GetMenuForWeekInternal(startDate);
        }
    }

    public class DayMenuUpdateDto
        {
            public string? Day { get; set; }
            public string? Soup { get; set; }
            public string? MainDish { get; set; }
            public string? Dessert { get; set; }
        }

    public class AddFoodDto
    {
        public string? Type { get; set; }
        public string? Name { get; set; }
    }
}