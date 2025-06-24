using System.Data.SQLite;
using Microsoft.AspNetCore.Mvc;
using SchoolAPI.Models;

[ApiController]
[Route("[controller]/[action]")]
public class UserController : Controller
{
    [HttpPost]
    public IActionResult Create(
        [FromForm] string username,
        [FromForm] string password,
        [FromForm] string role
    )
    {
        using (var connection = DatabaseConnector.CreateNewConnection())
        {
            // Ellenőrizzük, hogy a felhasználónév már létezik-e
            string checkUserSql = "SELECT COUNT(*) FROM User WHERE Username = @Username";
            using (var checkCmd = new SQLiteCommand(checkUserSql, connection))
            {
                checkCmd.Parameters.AddWithValue("@Username", username);
                long count = (long)checkCmd.ExecuteScalar();
                if (count > 0)
                {
                    return Conflict("Ez a felhasználónév már foglalt.");
                }
            }

            // Jelszó hashelése és mentése
            string salt = PasswordManager.GenerateSalt();
            string hashedPassword = PasswordManager.GeneratePasswordHash(password, salt);

            string insertSql =
                "INSERT INTO User (Username, PasswordHash, PasswordSalt, Role) VALUES (@Username, @PasswordHash, @PasswordSalt, @Role)";
            using (SQLiteCommand cmd = new SQLiteCommand(insertSql, connection))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                cmd.Parameters.AddWithValue("@PasswordSalt", salt);
                cmd.Parameters.AddWithValue("@Role", role);
                cmd.ExecuteNonQuery();
            }
        }

        return Ok("Felhasználó sikeresen létrehozva!");
    }

    [HttpPost]
    public IActionResult ChangePassword(
        [FromForm] string oldpassword,
        [FromForm] string newpassword
    )
    {
        string sessionId = Request.Cookies["id"];
        long userID = -1;
        string classname = "";
        string sql = $"SELECT UserID From Session WHERE SessionCookie = '{sessionId}'";
        using (var connection = DatabaseConnector.CreateNewConnection())
        {
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    userID = Convert.ToInt64(reader["UserID"]);
                }
            }
            if (userID == -1)
            {
                return Unauthorized("Jelszócseréhez először be kell jelentkezned!");
            }
            // Jelszó ellenőrzése
            sql =
                $"SELECT UserID, PasswordHash, PasswordSalt, Role FROM User WHERE UserID = '{userID}'";
            using (SQLiteCommand cmd = new SQLiteCommand(sql, connection))
            {
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string? storedPasswordHash = reader["PasswordHash"].ToString();
                        string? storedSalt = reader["PasswordSalt"].ToString();

                        if (
                            !string.IsNullOrEmpty(storedPasswordHash)
                            && !string.IsNullOrEmpty(storedSalt)
                            && PasswordManager.Verify(oldpassword, storedSalt, storedPasswordHash)
                        )
                        {
                            userID = Convert.ToInt64(reader["UserID"]);
                        }
                        else
                        {
                            userID = -1;
                        }
                    }
                }
            }
            if (userID == -1)
            {
                return Unauthorized("Helytelen jelszó!");
            }
        }
        using (var connection = DatabaseConnector.CreateNewConnection())
        {
            // Jelszó hashelése és mentése
            string salt = PasswordManager.GenerateSalt();
            string hashedPassword = PasswordManager.GeneratePasswordHash(newpassword, salt);

            string insertSql =
                "UPDATE User SET PasswordHash = @PasswordHash, PasswordSalt = @PasswordSalt WHERE UserID = @UserID";
            using (SQLiteCommand cmd = new SQLiteCommand(insertSql, connection))
            {
                cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                cmd.Parameters.AddWithValue("@PasswordSalt", salt);
                cmd.Parameters.AddWithValue("@UserID", userID);
                cmd.ExecuteNonQuery();
            }
        }

        return Ok("Felhasználó jelszava sikeresen kicserélve!");
    }

    [HttpPost]
    public IActionResult Login([FromForm] string username, [FromForm] string password)
    {
        long userID = -1;
        string? role = null;

        // Csak az olvasásra használjuk a DB kapcsolatot, majd bezárjuk
        using (SQLiteConnection connection = DatabaseConnector.CreateNewConnection())
        {
            // Megnézzük, hogy be van-e jelentkezve már az éppen bejelentkezni kívánó felhasználó
            string selectSql = $"SELECT UserID FROM User WHERE Username = '{username}'";
            using (SQLiteCommand cmd = new SQLiteCommand(selectSql, connection))
            {
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        userID = Convert.ToInt64(reader["UserID"]);
                    }
                }
            }
            if (userID == -1)
            {
                selectSql = $"SELECT UserID, SessionCookie FROM Session WHERE UserID = '{userID}'";
                using (SQLiteCommand cmd = new SQLiteCommand(selectSql, connection))
                {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string? sessionCookieValue = reader["SessionCookie"]?.ToString();
                            if (!string.IsNullOrEmpty(sessionCookieValue))
                            {
                                SessionManager.InvalidateSession(sessionCookieValue);
                            }
                        }
                    }
                }
            }
            // Jelszó ellenőrzése
            selectSql =
                $"SELECT UserID, PasswordHash, PasswordSalt, Role FROM User WHERE Username = '{username}'";
            using (SQLiteCommand cmd = new SQLiteCommand(selectSql, connection))
            {
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string? storedPasswordHash = reader["PasswordHash"].ToString();
                        string? storedSalt = reader["PasswordSalt"].ToString();
                        role = reader["Role"].ToString();

                        if (
                            !string.IsNullOrEmpty(storedPasswordHash)
                            && !string.IsNullOrEmpty(storedSalt)
                            && PasswordManager.Verify(password, storedSalt, storedPasswordHash)
                        )
                        {
                            userID = Convert.ToInt64(reader["UserID"]);
                        }
                        else
                        {
                            userID = -1;
                        }
                    }
                }
            }
        }
        if (userID == -1)
        {
            return Unauthorized("Helytelen felhasználónév vagy jelszó!");
        }
        // Itt már nincs megnyitva másik kapcsolat, mehet az írás
        SessionManager.InvalidateAllSessions(userID);
        string sessionCookie = SessionManager.CreateSession(userID);

        Response.Cookies.Append(
            "id",
            sessionCookie,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1),
            }
        );

        return Ok(new { message = "Bejelentkezés sikeres!", role = role });
    }

    [HttpPost]
    public IActionResult Logout()
    {
        var currentsessioncookie = Request.Cookies["id"];
        if (string.IsNullOrEmpty(currentsessioncookie))
        {
            return new UnauthorizedResult();
        }
        SessionManager.InvalidateSession(currentsessioncookie);
        Response.Cookies.Delete("id");
        return Ok("Kijelentkezés sikeres!");
    }

    public static bool IsLoggedIn(string SessionCookie)
    {
        long userID = SessionManager.GetUserID(SessionCookie);
        return userID != -1;
    }

    [HttpGet]
    public IActionResult GetUser()
    {
        try
        {
            var sessionId = Request.Cookies["id"];
            long userID = SessionManager.ValidateSession(sessionId);
            return Json(userID);
        }
        catch (UnauthorizedAccessException)
        {
            Response.Cookies.Delete("id");
            return Unauthorized("Munkamenet lejárt, vagy érvénytelen.");
        }
    }

    [HttpGet]
    public IActionResult CheckSession()
    {
        var sessionId = Request.Cookies["id"];
        if (string.IsNullOrEmpty(sessionId))
        {
            return Json(
                new
                {
                    userID = -1,
                    username = (string?)null,
                    role = (string?)null,
                }
            );
        }
        long userID = SessionManager.GetUserID(sessionId);
        if (userID == -1)
        {
            return Json(
                new
                {
                    userID = -1,
                    username = (string?)null,
                    role = (string?)null,
                }
            );
        }

        string? role = null;
        string? username = null;
        using (var connection = DatabaseConnector.CreateNewConnection())
        {
            string selectSql = "SELECT Role, Username FROM User WHERE UserID = @UserID";
            using (var cmd = new SQLiteCommand(selectSql, connection))
            {
                cmd.Parameters.AddWithValue("@UserID", userID);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        role = reader["Role"].ToString();
                        username = reader["Username"].ToString();
                    }
                }
            }
        }

        return Json(
            new
            {
                userID,
                username,
                role,
            }
        );
    }

    [HttpGet]
    public IActionResult GetTeacherNames()
    {
        List<string> teachers = new List<string>();
        List<long> teacherIDs = new List<long>();
        try
        {
            using (var connection = DatabaseConnector.CreateNewConnection())
            {
                string selectSql =
                    @"SELECT ScientificRank, Firstname, Middlename, Surname, UserID FROM User WHERE role='teacher'";
                using (var cmd = new SQLiteCommand(selectSql, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string rank = Convert.ToString(reader["ScientificRank"]);
                            string firstname = Convert.ToString(reader["Firstname"]);
                            string middlename = Convert.ToString(reader["Middlename"]);
                            string surname = Convert.ToString(reader["Surname"]);
                            long ID = Convert.ToInt64(reader["UserID"]);
                            string fullname =
                                rank
                                + (rank is null ? "" : " ")
                                + firstname
                                + (firstname is null ? "" : " ")
                                + middlename
                                + (middlename is null ? "" : " ")
                                + surname;
                            teachers.Add(fullname);
                            teacherIDs.Add(ID);
                        }
                    }
                }
            }
            return Json(new { Teachers = teachers, TeacherIDs = teacherIDs });
        }
        catch (Exception ex)
        {
            // Log the exception details
            Console.Error.WriteLine($"An error occurred: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet]
    public IActionResult GetClass()
    {
        string sessionId = Request.Cookies["id"];
        long UserID = -1;
        string classname = "";
        using (var connection = DatabaseConnector.CreateNewConnection())
        {
            string sql = $"SELECT UserID From Session WHERE SessionCookie = '{sessionId}'";
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    UserID = Convert.ToInt64(reader["UserID"]);
                }
            }
            if (UserID == -1)
            {
                return Ok("Nem tartozol egyik osztálycsoporthoz sem!");
            }
            sql = $"SELECT ClassName From User WHERE UserID = '{Convert.ToString(UserID)}'";
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    classname = Convert.ToString(reader["ClassName"]);
                }
            }
        }
        return Ok(classname);
    }
}
