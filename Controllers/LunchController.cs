using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SQLite;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using SchoolAPI.Controllers;
using SchoolAPI.Models;
using SchoolAPI.Models.Lunch;

namespace SchoolAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class LunchController : Controller
    {
        [HttpPost]
        public IActionResult Regenerate([FromBody] RegenerateRequest request)
        {
            if (request == null || request.StartDate == default)
                return BadRequest("Érvénytelen dátum.");

            var sessionId = Request.Cookies["id"];
            if (string.IsNullOrEmpty(sessionId))
                return Unauthorized("Nincs bejelentkezve.");

            var userId = SessionManager.GetUserID(sessionId);
            if (userId == -1)
                return Unauthorized("Nincs bejelentkezve vagy lejárt a munkamenet.");

            using var conn = DatabaseConnector.CreateNewConnection();

            var deleteCmd = conn.CreateCommand();
            deleteCmd.CommandText =
                @"
        DELETE FROM Lunch 
        WHERE DATE(Date) BETWEEN DATE(@startDate) AND DATE(@startDate, '+4 day')";
            deleteCmd.Parameters.AddWithValue(
                "@startDate",
                request.StartDate.ToString("yyyy-MM-dd")
            );
            deleteCmd.ExecuteNonQuery();

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

            using var cmd = new SQLiteCommand(
                @"
        UPDATE Lunch 
        SET SoupID = @soupId, MainDishID = @mainId, DessertID = @dessertId 
        WHERE ""Date"" = @date",
                conn
            );

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
                _ => null,
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

            return Ok(
                new
                {
                    soups = soups,
                    mainDishes = mainDishes,
                    desserts = desserts,
                }
            );
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
                //Console.WriteLine($"GetMenuForWeek called with startDate: {startDate:yyyy-MM-dd}");
            }

            return GetMenuForWeekInternal(startDate);
        }

        [NonAction]
        public IActionResult GetMenuForWeekInternal(DateTime startDate)
        {
            var weekMenu = new List<object>();

            using var conn = DatabaseConnector.CreateNewConnection();

            var sql =
                @"
                SELECT 
                    Lunch.LunchID,
                    Lunch.Date,
                    Soup.Name AS Soup,
                    MainDish.Name AS MainDish,
                    Dessert.Name AS Dessert
                FROM Lunch
                JOIN Soup ON Lunch.SoupID = Soup.SoupID
                JOIN MainDish ON Lunch.MainDishID = MainDish.MainDishID
                JOIN Dessert ON Lunch.DessertID = Dessert.DessertID
                WHERE DATE(Lunch.Date) BETWEEN DATE(@startDate) AND DATE(@startDate, '+4 day')
                ORDER BY DATE(Lunch.Date) ASC
                ";

            using var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@startDate", startDate.ToString("yyyy-MM-dd"));

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var date = DateTime.Parse(reader.GetString(1));
                weekMenu.Add(
                    new
                    {
                        date = date.ToString("yyyy-MM-dd"),
                        day = date.ToString("dddd", new CultureInfo("hu-HU")),
                        soup = reader.GetString(2),
                        mainDish = reader.GetString(3),
                        dessert = reader.GetString(4),
                    }
                );
            }
            return Json(
                new
                {
                    startDate = startDate.ToString("yyyy-MM-dd"), // a pontos hétfő dátuma
                    weekMenu = weekMenu,
                }
            );
        }

        private int? GetIdByName(SQLiteConnection conn, string tableName, string? name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            using var cmd = new SQLiteCommand(
                $"SELECT {tableName}ID FROM {tableName} WHERE Name = @name",
                conn
            );
            cmd.Parameters.AddWithValue("@name", name);
            var result = cmd.ExecuteScalar();
            return result == null ? (int?)null : Convert.ToInt32(result);
        }

        /*[HttpPost]
        public IActionResult SignUp([FromBody] SignUpDto data)
        {
            var sessionId = Request.Cookies["id"];
            if (string.IsNullOrEmpty(sessionId))
                return Unauthorized("Nincs bejelentkezve.");

            var userId = SessionManager.GetUserID(sessionId);
            if (userId == -1)
                return Unauthorized("Nincs bejelentkezve vagy lejárt a munkamenet.");

            using var conn = DatabaseConnector.CreateNewConnection();

            using var cmdGetLunchId = new SQLiteCommand("SELECT LunchID FROM Lunch WHERE Date = @date", conn);
            cmdGetLunchId.Parameters.AddWithValue("@date", data.Date);

            var lunchIdObj = cmdGetLunchId.ExecuteScalar();
            if (lunchIdObj == null)
                return NotFound("Nem található ebéd a megadott napra.");

            int lunchId = Convert.ToInt32(lunchIdObj);

            using var cmd = new SQLiteCommand("INSERT INTO LunchSignup (UserID, LunchID, Day) VALUES (@userId, @lunchId, @day)", conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@lunchId", lunchId);
            cmd.Parameters.AddWithValue("@day", data.Date);

            cmd.ExecuteNonQuery();

            return Ok("Feljelentkezés sikeres.");
        }
        [HttpPost]
        public IActionResult SignOut([FromBody] SignUpDto data)
        {
            var sessionId = Request.Cookies["id"];
            if (string.IsNullOrEmpty(sessionId))
                return Unauthorized("Nincs bejelentkezve.");

            var userId = SessionManager.GetUserID(sessionId);
            if (userId == -1)
                return Unauthorized("Nincs bejelentkezve vagy lejárt a munkamenet.");

            using var conn = DatabaseConnector.CreateNewConnection();

            using var cmdGetLunchId = new SQLiteCommand("SELECT LunchID FROM Lunch WHERE Date = @date", conn);
            cmdGetLunchId.Parameters.AddWithValue("@date", data.Date);

            var lunchIdObj = cmdGetLunchId.ExecuteScalar();
            if (lunchIdObj == null)
                return NotFound("Nem található ebéd a megadott napra.");

            int lunchId = Convert.ToInt32(lunchIdObj);

            using var cmd = new SQLiteCommand("DELETE FROM LunchSignup WHERE UserID = @userId AND LunchID = @lunchId", conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@lunchId", lunchId);

            int affected = cmd.ExecuteNonQuery();

            return affected > 0 ? Ok("Lejelentkezés sikeres.") : NotFound("Nem volt ilyen jelentkezés.");
        }*/
    }

    public class DailyMenu
    {
        public DateTime Date { get; set; }
        public string Soup { get; set; }
        public string MainDish { get; set; }
        public string Dessert { get; set; }
    }

    public class WeeklyMenu
    {
        public int WeekNumber { get; set; }
        public List<DailyMenu> DailyMenus { get; set; } = new List<DailyMenu>();
    }

    public class YearlyMenu
    {
        public int Year { get; set; }
        public List<WeeklyMenu> WeeklyMenus { get; set; } = new List<WeeklyMenu>();
    }

    public class DayMenuUpdateDto
    {
        public string? Date { get; set; }
        public string? Soup { get; set; }
        public string? MainDish { get; set; }
        public string? Dessert { get; set; }
    }

    public class AddFoodDto
    {
        public string? Type { get; set; }
        public string? Name { get; set; }
    }

    /*public class SignUpDto
    {
        public string? Date { get; set; }
    }*/
}
